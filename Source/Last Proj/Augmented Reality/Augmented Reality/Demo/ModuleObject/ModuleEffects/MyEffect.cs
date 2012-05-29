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

namespace ModuleEffects
{
    public interface MyEffect
    {
        string Name { get; }
        void Update(GameTime gameTime, Vector3 newPosition);
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime, Matrix associateMatrix);
        void Destroy();
    }
}
