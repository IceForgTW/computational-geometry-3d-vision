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
    public class RingYellow : MyRing
    {
        public RingYellow(Game game, float objectScale) : base(game, objectScale) { }

        protected override ParticleSettings CreateSettingsForSmokePlume()
        {
            ParticleSettings settings = new ParticleSettings();

            settings.EffectName = @"Effects/ParticleEffect";

            settings.TextureName = @"Textures/YellowParticle";

            settings.MaxParticles = 600;

            settings.Duration = TimeSpan.FromSeconds(3);

            settings.MinHorizontalVelocity = -0.05f * scale;
            settings.MaxHorizontalVelocity = 0.05f * scale;

            settings.MinVerticalVelocity = 0.2f * scale;
            settings.MaxVerticalVelocity = 0.4f * scale;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(0, -0.05f * scale, 0);

            settings.EndVelocity = 0.75f * scale;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 0.01f * scale;
            settings.MaxStartSize = 0.02f * scale;

            settings.MinEndSize = 0.02f * scale;
            settings.MaxEndSize = 0.04f * scale;

            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;

            return settings;
        }

        protected override ParticleSettings CreateSettingsForFire()
        {
            ParticleSettings settings = new ParticleSettings();

            settings.EffectName = @"Effects/ParticleEffect";

            settings.TextureName = @"Textures/YellowParticle";

            settings.MaxParticles = 1300;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0.15f * scale;

            settings.MinVerticalVelocity = -0.1f * scale;
            settings.MaxVerticalVelocity = 0.1f * scale;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 0.15f * scale, 0);

            settings.MinColor = new Color(255, 255, 255, 10);
            settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinStartSize = 0.05f * scale;
            settings.MaxStartSize = 0.1f * scale;

            settings.MinEndSize = 0.1f * scale;
            settings.MaxEndSize = 0.4f * scale;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;

            return settings;
        }
    }
}
