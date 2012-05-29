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
    public class CharacterHaloController_Style01 : IHaloController
    {
        private float maxHeight = 0;
        private float curHeight = 0;
        private float upScale = 100;
        private float movementPerFrame = 0.01f;
        private float millisecondPerFrame = 16.66f;
        private float timePreFrame;
        private float scale = 1.0f;

        public float MillisecondPerFrame
        {
            get { return millisecondPerFrame; }
            set { if (value > 0) { millisecondPerFrame = value; } }
        }

        public float MovementPerFrame
        {
            get { return movementPerFrame; }
            set { if (value > 0) { movementPerFrame = value; } }
        }

        public float MaxHeight
        {
            get { return maxHeight; }
            set { maxHeight = Math.Abs(value); }
        }

        public float Scale
        {
            get { return scale * upScale; }
            set { scale = value; }
        }

        public CharacterHaloController_Style01() { }

        public MyRing UpdateRing(GameTime gameTime, MyRing ring, Vector3 effectPosition)
        {
            timePreFrame += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timePreFrame >= millisecondPerFrame)
            {
                while (timePreFrame > 0)
                {
                    timePreFrame -= millisecondPerFrame;
                    UpdateHeight();
                    ring.Position = new Vector3(effectPosition.X, effectPosition.Y + curHeight, effectPosition.Z);
                    ring.Update(gameTime);
                }
            }

            return ring;
        }

        private void UpdateHeight()
        {
            if (curHeight > maxHeight)
            {
                curHeight = 0;
            }
            curHeight += movementPerFrame;
        }
    }
}
