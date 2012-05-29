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

namespace ModuleModel
{
    public struct SModelMesh
    {
        public VertexBuffer vertexBuffer;
        public Texture2D texture;
        public int numPrimitives;
    }

    public interface IModel
    {
        SModelMesh[] Meshes { get; }
        VertexDeclaration vertexDeclaration { get; }
        int vertexSizeInBytes { get; }
        
        void Load(string modelName, string textureName, GraphicsDevice device, ContentManager content);
        bool SetAnimation(string animationName);
        bool SetAnimationNoLoop(string animationName);
        void SetAnimationsSequence(string firstAnimation, string secondAnimation);
        void Update(GraphicsDevice device, GameTime gameTime);
        void Draw(GameTime gameTime, GraphicsDevice device, Effect effect, Matrix worldViewProjectionMatrix, int resolutionWidth, int resolutionHeight);
        void Draw(GraphicsDevice device, Matrix world, Matrix view, Matrix projection, GameTime gameTime);
        string[] GetListAnimationNames();
    }
}
