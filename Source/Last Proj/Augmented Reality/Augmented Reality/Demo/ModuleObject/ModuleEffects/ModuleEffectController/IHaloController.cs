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
    public interface IHaloController
    {
        float Scale { set; get; }
        MyRing UpdateRing(GameTime gameTime, MyRing ring, Vector3 effectPosition);
    }
}
