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
    public class ProjectileViolet : MyProjectile
    {
        public ProjectileViolet(Game game) : base(game) { }

        #region Settings
        protected override ParticleSettings CreateSettingsForExplosion()
        {
            ParticleSettings settings = new ParticleSettings();

            settings.EffectName = @"Effects/ParticleEffect";

            settings.TextureName = @"Textures/VioletParticle";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0.2f;
            settings.MaxHorizontalVelocity = 0.3f;

            settings.MinVerticalVelocity = -0.2f;
            settings.MaxVerticalVelocity = 0.2f;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 0.1f;
            settings.MaxStartSize = 0.1f;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 2;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;

            return settings;
        }

        protected override ParticleSettings CreateSettingsForExplosionSmoke()
        {
            ParticleSettings settings = new ParticleSettings();

            settings.EffectName = @"Effects/ParticleEffect";

            settings.TextureName = @"Textures/VioletParticle";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0.5f;

            settings.MinVerticalVelocity = -0.1f;
            settings.MaxVerticalVelocity = 0.5f;

            settings.Gravity = new Vector3(0, -0.2f, 0);

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 0.01f;
            settings.MaxStartSize = 0.02f;

            settings.MinEndSize = 0.05f;
            settings.MaxEndSize = 0.1f;

            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;

            return settings;
        }

        protected override ParticleSettings CreateSettingsForTrail()
        {
            ParticleSettings settings = new ParticleSettings();

            settings.EffectName = @"Effects/ParticleEffect";

            settings.TextureName = @"Textures/VioletParticle";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(3);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0.01f;

            settings.MinVerticalVelocity = -0.01f;
            settings.MaxVerticalVelocity = 0.01f;

            settings.MinColor = new Color(255, 255, 255, 50);
            settings.MaxColor = new Color(255, 255, 255, 90);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 0.05f;
            settings.MaxStartSize = 0.1f;

            settings.MinEndSize = 0.1f;
            settings.MaxEndSize = 0.4f;

            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;

            return settings;
        }
        #endregion Settings
    }
}
