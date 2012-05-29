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
    public abstract class MyHalo : MyEffect
    {
        #region Attributes
        protected Game game;
        private Vector3 position;
        protected MyRing ring;

        private IHaloController controller;
        protected float scale = 1.0f;
        #endregion Attributes

        #region Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public float Radius
        {
            get { return ring.Radius; }
            set { ring.Radius = value; }
        }

        public virtual string Name
        {
            get { return "MyHalo"; }
        }

        public IHaloController Controller
        {
            get { return controller; }
            set { controller = value; }
        }

        #endregion Properties

        #region Constructors
        public MyHalo(Game game, IHaloController controller)
        {
            this.game = game;
            this.controller = controller;
            scale = controller.Scale;
        }

        abstract public void Initialize();
        
        #endregion Constructors

        #region Implement Methods
        public void Update(GameTime gameTime, Vector3 newPosition)
        {
            Position = newPosition;
            controller.UpdateRing(gameTime, ring, position);
        }

        public void Update(GameTime gameTime)
        {
            Update(gameTime, Position);
        }

        public void Draw(GameTime gameTime, Matrix associateMatrix)
        {
            ring.Draw(gameTime, associateMatrix);
        }
        #endregion Implement Methods

        #region Destruction
        public void Destroy()
        {
            ring.UnloadContent();
        }
        #endregion Destruction
    }
}
