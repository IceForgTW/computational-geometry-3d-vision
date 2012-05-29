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
using System.Xml;
using MD2Animation;

namespace ModuleModel
{
    public class CMD2Model : IModel
    {
        #region Attributes
        private MD2 _coreModel;
        private Dictionary<string, int> _animations;
        private List<SModelMesh> _meshes;
        private VertexDeclaration _vertexDeclaration;
        private int _vertexSizeInBytes;
        BasicEffect _defaultEffect;
        #endregion Attributes

        #region Properties
        public SModelMesh[] Meshes
        {
            get { return _meshes.ToArray(); }
        }

        public VertexDeclaration vertexDeclaration
        {
            get { return _vertexDeclaration; }
        }

        public int vertexSizeInBytes
        {
            get { return _vertexSizeInBytes; }
        }
        #endregion Properties

        #region Constructors
        public CMD2Model(string lstAnimationXMLFilename)
        {
            _coreModel = new MD2();
            _animations = new Dictionary<string, int>();
            LoadListAnimationNames(lstAnimationXMLFilename);
        }
        #endregion Constructors

        #region Methods
        #region Interface_Implemented_Methods
        public void Load(string modelName, string textureName, GraphicsDevice device, ContentManager content)
        {
            _coreModel.LoadModel(device, modelName, textureName, content);
            _vertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
            _vertexSizeInBytes = VertexPositionNormalTexture.SizeInBytes;
            
            InitializeDefaultEffect(device);
        }

        private void InitializeDefaultEffect(GraphicsDevice device)
        {
            _defaultEffect = new BasicEffect(device, null);
            _defaultEffect.TextureEnabled = true;
            _defaultEffect.EnableDefaultLighting();
        }

        private void LoadListAnimationNames(string XMLFileName)
        {
            XmlDocument doc = new XmlDocument();
            
            doc.Load(XMLFileName);
            
            XmlNodeList animations = doc.GetElementsByTagName("Animations")[0].ChildNodes;
            for (int i = 0; i < animations.Count; i++)
            {
                _animations.Add(animations[i].InnerText, int.Parse(animations[i].Attributes["id"].Value));
            }
        }

        public string[] GetListAnimationNames()
        {
            string[] result = new string[_animations.Count];
            for (int i = 0; i < _animations.Count; i++)
            {
                result[i] = _animations.ElementAt(i).Key;
            }

            return result;
        }

        public bool SetAnimation(string animationName)
        {
            if (_animations.ContainsKey(animationName))
            {
                return _coreModel.SetAnimation(_animations[animationName]);
            }
            return false;
        }

        public bool SetAnimationNoLoop(string animationName)
        {
            if (_animations.ContainsKey(animationName))
            {
                return _coreModel.SetAnimationNoLoop(_animations[animationName]);
            }
            return false;
        }

        public void SetAnimationsSequence(string firstAnimation, string secondAnimation)
        {
            _coreModel.SetAnimationSequence(_animations[firstAnimation], _animations[secondAnimation]);
        }

        public void Update(GraphicsDevice device, GameTime gameTime)
        {
            _coreModel.UpdateModel(device, gameTime);

            _meshes = new List<SModelMesh>();

            SModelMesh mesh = new SModelMesh();
            mesh.vertexBuffer = _coreModel.vertexBuffer;
            mesh.texture = _coreModel.GetTexture();
            mesh.numPrimitives = _coreModel.GetNumTriangles();
            _meshes.Add(mesh);
        }

        public void Draw(GameTime gameTime, GraphicsDevice device, Effect effect, Matrix worldViewProjectionMatrix, int resolutionWidth, int resolutionHeight)
        {
            effect.Parameters["xWorldViewProjection"].SetValue(worldViewProjectionMatrix);
            effect.Parameters["xWidth"].SetValue(resolutionWidth);
            effect.Parameters["xHeight"].SetValue(resolutionHeight);
            
            effect.Begin();
            foreach (SModelMesh mesh in _meshes)
            {
                effect.Parameters["xTexture"].SetValue(mesh.texture);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.VertexDeclaration = _vertexDeclaration;
                    device.Vertices[0].SetSource(mesh.vertexBuffer, 0, _vertexSizeInBytes);
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, mesh.numPrimitives);
                    pass.End();
                }
            }
            effect.End();
        }

        public void Draw(GraphicsDevice device, Matrix world, Matrix view, Matrix projection, GameTime gameTime)
        {
            _defaultEffect.View = view;
            _defaultEffect.Projection = projection;
            _defaultEffect.World = world;
            _defaultEffect.Begin();
            foreach (SModelMesh mesh in _meshes)
            {
                _defaultEffect.Texture = mesh.texture;
                foreach (EffectPass pass in _defaultEffect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.VertexDeclaration = _vertexDeclaration;
                    device.Vertices[0].SetSource(mesh.vertexBuffer, 0, _vertexSizeInBytes);
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, mesh.numPrimitives);
                    pass.End();
                }
            }
            _defaultEffect.End();
        }
        #endregion Interface_Implemented_Methods

        #region Particular_Class_Methods
        public void Pause()
        {
            _coreModel.Pause();
        }

        public void Resume()
        {
            _coreModel.Resume();
        }

        public bool IsPlaying(string animationName)
        {
            if (_animations.ContainsKey(animationName))
            {
                return _coreModel.IsPlaying(_animations[animationName]);
            }
            return false;
        }
        #endregion Particular_Class_Methods

        #endregion Methods
    }
}
