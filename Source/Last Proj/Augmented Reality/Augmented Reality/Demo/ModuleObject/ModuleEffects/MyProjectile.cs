using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ParticleSystemCore;
using ParticleSystemCore.ParticleSystems;

namespace ModuleEffects
{
    public abstract class MyProjectile
    {
        #region Attributes

        const float trailParticlesPerSecond = 200;
        const int numExplosionParticles = 30;
        const int numExplosionSmokeParticles = 50;
        
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem trailParticles;
        ParticleEmitter trailEmitter;

        Vector3 position;
        Vector3 velocity;
        float acceleration = 0;

        Vector3 sourPosition;
        Vector3 destPosition;
        float maxDistance = 0;
        
        Game game;
        bool isExecuting = false;

        public bool IsFinish
        {
            get { return !isExecuting; }
        }
        #endregion Attributes

        public MyProjectile(Game game)
        {
            this.game = game;
        }

        public void Initialize()
        {
            explosionParticles = new ExplosionParticleSystem(game, game.Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(game, game.Content);
            trailParticles = new ProjectileTrailParticleSystem(game, game.Content);

            explosionParticles.Settings = CreateSettingsForExplosion();
            explosionSmokeParticles.Settings = CreateSettingsForExplosionSmoke();
            trailParticles.Settings = CreateSettingsForTrail();

            explosionSmokeParticles.DrawOrder = 200;
            trailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;

            game.Components.Add(explosionParticles);
            game.Components.Add(explosionSmokeParticles);
            game.Components.Add(trailParticles);

            trailEmitter = new ParticleEmitter(trailParticles, trailParticlesPerSecond, Vector3.Zero);
        }

        #region Settings
        protected abstract ParticleSettings CreateSettingsForExplosion();
        protected abstract ParticleSettings CreateSettingsForExplosionSmoke();
        protected abstract ParticleSettings CreateSettingsForTrail();
        #endregion Settings

        public void Execute(Vector3 sourPos, Vector3 destPos, float velocity, float acceleration)
        {
            this.velocity = Vector3.Normalize(destPos - sourPos) * velocity;
            position = sourPos;
            sourPosition = sourPos;
            destPosition = destPos;
            this.acceleration = acceleration;
            CreateMaxDistance();
         
            isExecuting = true;
        }

        protected void CreateMaxDistance()
        {
            maxDistance = Vector3.Distance(sourPosition, destPosition);
        }

        private bool NeedExplode()
        {
            return (Vector3.Distance(sourPosition, position) >= maxDistance);
            
        }

        public virtual void Update(GameTime gameTime)
        {
            if (isExecuting)
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                position += velocity * elapsedTime;

                trailEmitter.Update(gameTime, position);

                
                if (NeedExplode())
                {
                    Vector3 explosionVelocity = new Vector3(0, 0.1f, 0);
                    
                    for (int i = 0; i < numExplosionParticles; i++)
                        explosionParticles.AddParticle(position, explosionVelocity);

                    for (int i = 0; i < numExplosionSmokeParticles; i++)
                        explosionSmokeParticles.AddParticle(position, explosionVelocity);

                    isExecuting = false;
                }
            }
        }

        public void Draw(GameTime gameTime, Matrix viewMatrix, Matrix projectionMatrix)
        {
            explosionParticles.SetCamera(viewMatrix, projectionMatrix);
            explosionSmokeParticles.SetCamera(viewMatrix, projectionMatrix);
            trailParticles.SetCamera(viewMatrix, projectionMatrix);
        }

        public void UnloadContent()
        {
            game.Components.Remove(explosionParticles);
            game.Components.Remove(explosionSmokeParticles);
            game.Components.Remove(trailParticles);
        }
    }
}
