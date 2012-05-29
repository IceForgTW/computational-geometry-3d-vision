#region Using Statements
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
#endregion Using Statements

namespace ModuleEffects
{
    public abstract class MyRing
    {
        #region Atrributes
        private Game owner;
        private ParticleSystem fireParticles;
        private ParticleSystem smokePlumeParticles;
        private Random random;

        private float radius = 0.3f;
        private Vector3 position = Vector3.Zero;
        private Vector3 rotation = new Vector3(MathHelper.PiOver2, 0, 0);
        private int fireOverSmoke = 1;
        private int fireAmount = 0;
        protected float scale = 1.0f;
        const int fireParticlesPerFrame = 20;

        #endregion Attributes

        #region Properties
        /// <summary>
        /// The radius of the ring.
        /// </summary>
        public float Radius
        {
            set { if (radius > 0) { radius = value; } }
            get { return radius; }
        }

        /// <summary>
        /// The origin of the ring.
        /// </summary>
        public Vector3 Position
        {
            set { position = value; }
            get { return position; }
        }

        /// <summary>
        /// Rotation around the X Axis - Pitch Rotation (Radians).
        /// </summary>
        public float RotationX
        {
            set { rotation.Y = value; }
            get { return rotation.Y; }
        }

        /// <summary>
        /// Rotation around the Y Axis - Yaw Rotation (Radians).
        /// </summary>
        public float RotationY
        {
            set { rotation.X = value; }
            get { return rotation.X; }
        }

        /// <summary>
        /// Rotation around the Z Axis - Roll Rotation (Radians).
        /// </summary>
        public float RotationZ
        {
            set { rotation.Z = value; }
            get { return rotation.Z; }
        }

        /// <summary>
        /// Ratio fire particles over smoke particles. Control how much smoke.
        /// </summary>
        public int FireOverSmoke
        {
            set { if (value > 0) { fireOverSmoke = value; } }
            get { return fireOverSmoke; }
        }

        #endregion Properties

        #region Initialization
        /// <summary>
        /// Constructor of FireRing Class.
        /// </summary>
        public MyRing(Game game, float objectScale)
        {
            owner = game;
            random = new Random();
            scale = objectScale;

            // Construct our particle system components.
            fireParticles = new FireParticleSystem(game, game.Content);
            smokePlumeParticles = new SmokePlumeParticleSystem(game, game.Content);

            // Scale particle by current radius / 30.0f (default radius).
            fireParticles.Settings = CreateSettingsForFire();
            smokePlumeParticles.Settings = CreateSettingsForSmokePlume();

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            smokePlumeParticles.DrawOrder = 100;
            fireParticles.DrawOrder = 200;

            // Register the particle system components.
            game.Components.Add(smokePlumeParticles);
            game.Components.Add(fireParticles);
        }

        protected abstract ParticleSettings CreateSettingsForSmokePlume();
        
        protected abstract ParticleSettings CreateSettingsForFire();
        
        #endregion Initialization

        #region Update
        /// <summary>
        /// Updates the Fire Ring.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            UpdateFire();
        }

        /// <summary>
        /// Helper for updating the fire effect.
        /// </summary>
        private void UpdateFire()
        {
            // Create a number of fire particles, randomly positioned around a circle.
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
            }

            // Create one smoke particle per frmae, too.
            if (fireAmount > fireOverSmoke)
            {
                smokePlumeParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
                fireAmount = 0;
            }
            else
            {
                fireAmount += fireParticlesPerFrame;
            }
        }

        /// <summary>
        /// Helper used by the UpdateFire method. Chooses a random location
        /// around a circle, at which a fire particle will be created.
        /// </summary>
        private Vector3 RandomPointOnCircle()
        {
            double angle = random.NextDouble() * MathHelper.TwoPi;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            Vector3 originalPoint = new Vector3(x * radius, y * radius, 0);
            Vector3 rotatedPoint = Vector3.Transform(originalPoint, Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z));
            Vector3 resultPoint = position + rotatedPoint;

            return resultPoint;
        }

        #endregion Update

        #region Draw
        /// <summary>
        /// Draws the Fire Ring.
        /// </summary>
        public void Draw(GameTime gameTime, Matrix associateMatrix)
        {
            fireParticles.SetCamera(Matrix.Identity, associateMatrix);
            smokePlumeParticles.SetCamera(Matrix.Identity, associateMatrix);
        }
        #endregion Draw

        #region Destruction
        public void UnloadContent()
        {
            owner.Components.Remove(fireParticles);
            owner.Components.Remove(smokePlumeParticles);
        }
        #endregion Destruction
    }
}
