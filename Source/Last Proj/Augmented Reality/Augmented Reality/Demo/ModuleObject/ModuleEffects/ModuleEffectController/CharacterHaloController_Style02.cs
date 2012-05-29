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
using ModuleEffects;

namespace ModuleEffectController
{
    public class CharacterHaloController_Style02 : IHaloController
    {
        private float upScale = 100;
        private float millisecondPerFrame = 16.66f;
        private float timePreFrame;
        private float scale = 1.0f;

        public float MillisecondPerFrame
        {
            get { return millisecondPerFrame; }
            set { if (value > 0) { millisecondPerFrame = value; } }
        }

        public float Scale
        {
            get { return scale * upScale; }
            set { scale = value; }
        }

        public CharacterHaloController_Style02() { }

        public MyRing UpdateRing(GameTime gameTime, MyRing ring, Vector3 effectPosition)
        {
            timePreFrame += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timePreFrame >= millisecondPerFrame)
            {
                while (timePreFrame > 0)
                {
                    timePreFrame -= millisecondPerFrame;
                    ring.Position = new Vector3(effectPosition.X, effectPosition.Y, effectPosition.Z);
                    ring.Update(gameTime);
                }
            }

            return ring;
        }
    }
}
