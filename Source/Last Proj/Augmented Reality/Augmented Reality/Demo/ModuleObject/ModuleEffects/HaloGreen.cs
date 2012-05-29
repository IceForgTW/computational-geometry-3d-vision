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

using ModuleEffectController;

namespace ModuleEffects
{
    public class HaloGreen : MyHalo
    {
        public override string Name
        {
            get { return "HaloGreen"; }
        }

        public HaloGreen(Game game, IHaloController controller) : base(game, controller) { }

        public override void Initialize()
        {
            ring = new RingGreen(game, scale);
        }
    }
}
