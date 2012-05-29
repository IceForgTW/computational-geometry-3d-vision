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

using PK3Model;

namespace ModuleModel
{
    public class CMD3Model : IModel
    {
        #region Attributes
        private List<SModelMesh> _meshes;
        private PK3Model.CPK3Model _coreModel;
        private VertexDeclaration _vertexDeclaration;
        private int _vertexSizeInBytes;
        
        private string[] _animations;
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

        public BoundingBox BoundingBox
        {
            get { return _coreModel.getBoundingBox(); }
        }

        public BoundingBox BoundingBoxHead
        {
            get { return _coreModel.getBoundingBoxHead(); }
        }

        public BoundingBox BoundingBoxLegs
        {
            get { return _coreModel.getBoundingBoxLegs(); }
        }

        public BoundingBox BoundingBoxUpper
        {
            get { return _coreModel.getBoundingBoxUpper(); }
        }

        public Matrix WeaponMatrix
        {
            get { return _coreModel.getWeaponMatrix(); }
        }
        #endregion Properties

        #region Constructors
        public CMD3Model(string lstAnimationXMLFilename)
        {
            LoadListAnimationNames(lstAnimationXMLFilename);
        }
        #endregion Constructors

        #region Methods
        #region Interface_Implemented_Methods
        public void Load(string modelName, string textureName, GraphicsDevice device, ContentManager content)
        {
            _coreModel = content.Load<PK3Model.CPK3Model>(modelName);
            _coreModel.initialize(device);
            _coreModel.useLighting = true;
            
            _vertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
            _vertexSizeInBytes = VertexPositionNormalTexture.SizeInBytes;
            
        }

        private void LoadListAnimationNames(string XMLFileName)
        {
            XmlDocument doc = new XmlDocument();
            
            doc.Load(XMLFileName);
            
            XmlNodeList animations = doc.GetElementsByTagName("Animations")[0].ChildNodes;
            _animations = new string[animations.Count];
            for (int i = 0; i < animations.Count; i++)
            {
                _animations[i] = animations[i].InnerText;
            }
        }

        public string[] GetListAnimationNames()
        {
            return _animations;
        }

        public bool SetAnimation(string animationName)
        {
            if (_animations.Contains(animationName))
            {
                _coreModel.setAnimation(animationName);
                return true;
            }
            return false;
        }

        public bool SetAnimationNoLoop(string animationName)
        {
            return true;
        }

        public void SetAnimationsSequence(string firstAnimation, string secondAnimation)
        {
            if (_animations.Contains(firstAnimation) && _animations.Contains(secondAnimation))
            {
                _coreModel.setAnimation(firstAnimation);
                _coreModel.setAnimation(secondAnimation);
            }
        }
        
        public void Update(GraphicsDevice device, GameTime gameTime)
        {
            _coreModel.update(gameTime);
            _meshes = new List<SModelMesh>();
            
            for (int i = 0; i < _coreModel.HeadVertices.Length; i++)
            {
                _meshes.Add(convertToMyMesh(device, _coreModel.HeadVertices[i]));
            }
            for (int i = 0; i < _coreModel.UpperVertices.Length; i++)
            {
                _meshes.Add(convertToMyMesh(device, _coreModel.UpperVertices[i]));
            }
            for (int i = 0; i < _coreModel.LowerVertices.Length; i++)
            {
                _meshes.Add(convertToMyMesh(device, _coreModel.LowerVertices[i]));
            }
        }

        private SModelMesh convertToMyMesh(GraphicsDevice device, SRenderInfoMesh renderMess)
        {
            SModelMesh result = new SModelMesh();

            result.vertexBuffer = new VertexBuffer(device, _vertexSizeInBytes, BufferUsage.WriteOnly);
            result.vertexBuffer.SetData(renderMess.vertex_info);
            result.texture = _coreModel.getTexture(renderMess.texture_name);
            result.numPrimitives = renderMess.vertex_info.Length / 3;

            return result;
        }

        public void Draw(GameTime gameTime, GraphicsDevice device, Effect effect, Matrix worldViewProjectionMatrix, int resolutionWidth, int resolutionHeight)
        {
            effect.Parameters["xWorldViewProjection"].SetValue(worldViewProjectionMatrix);
            _coreModel.renderShadowEffect(device, Matrix.Identity, effect);
        }

        public void Draw(GraphicsDevice device, Matrix world, Matrix view, Matrix projection, GameTime gameTime)
        {
            _coreModel.render(device, world, view, projection);
        }
        #endregion Interface_Implemented_Methods

        #region Particular_Class_Methods
        public bool isFinishAnimationTorso()
        {
            return _coreModel.isFinishAnimationTorso();
        }

        public bool isFinishAnimationLegs()
        {
            return _coreModel.isFinishAnimationLegs();
        }
        #endregion Particular_Class_Methods
        #endregion Methods
    }
}
