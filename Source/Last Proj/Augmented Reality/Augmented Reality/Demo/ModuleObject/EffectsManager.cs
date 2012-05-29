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

namespace ModuleObject
{
    public class EffectsManager
    {
        List<MyEffect> lstEffect = new List<MyEffect>();
        List<float> lstTime = new List<float>();

        public EffectsManager()
        {
        }

        public void AddEffect(MyEffect newEffect, float time)
        {
            lstEffect.Add(newEffect);
            lstTime.Add(time);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < lstTime.Count; i++)
            {
                if (lstTime[i] < 0)
                {
                    lstEffect[i].Destroy();
                    lstEffect.RemoveAt(i);
                    lstTime.RemoveAt(i);
                    i--;
                }
                else
                {
                    lstTime[i] -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    lstEffect[i].Update(gameTime);
                }
            }
        }

        public void Draw(GameTime gameTime, Matrix associateMatrix)
        {
            foreach (MyEffect effect in lstEffect)
            {
                effect.Draw(gameTime, associateMatrix);
            }
        }
    }
}
