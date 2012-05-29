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
    public class CBuilding
    {
        private Effect _effect;
        private Model _model;
        private Texture2D[] _textures;

        private float _scaleRatio;
        private float _heightShift;
        private Vector3 _position = Vector3.Zero;
        private float _rotation = 0;
        private Matrix _worldMatrix;

        #region Lighting
        private float _Ambient = 0.0f;
        private Vector3 _LightDirection = Vector3.Zero;
        private bool _EnableLighting = false;
        #endregion Lighting
        private EffectsManager effectManager = new EffectsManager();
        

        public float ScaleRatio
        {
            get { return _scaleRatio; }
        }

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public float Radius
        {
            get 
            {
                float max = 0;
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    float radius = Vector3.Distance(_position, mesh.BoundingSphere.Center) + mesh.BoundingSphere.Radius;
                    if (radius > max)
                    {
                        max = radius;
                    }
                }
                return max * _scaleRatio;
            }
        }

        public CBuilding()
        {
            
        }

        public void InitializeObject(GraphicsDevice device, ContentManager content,
            string modelAssetName,
            Vector3 position, 
            float scaleRatio, 
            float heightShift)
        {
            _effect = content.Load<Effect>(@"Effects\Series4Effects");
            _model = LoadModel(device, content, modelAssetName, out _textures);
            _heightShift = heightShift;
            _scaleRatio = scaleRatio;
            _position = position;
        }

        private Model LoadModel(GraphicsDevice device, ContentManager Content, string assetName, out Texture2D[] textures)
        {
            Model newModel = Content.Load<Model>(assetName);
            
            List<Texture2D> lstTextures = new List<Texture2D>();
            
            foreach (ModelMesh mesh in newModel.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    lstTextures.Add(currentEffect.Texture);
                }
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = _effect.Clone(device);
                }
            }
            
            textures = lstTextures.ToArray();

            return newModel;
        }

        private void SetPosition(Vector3 position)
        {
            _position = position;
            float shift = _heightShift * _scaleRatio;
            _position.Y += shift;
        }

        private void UpdateScale(float scale)
        {
            float oldShift = _heightShift * _scaleRatio;
            _scaleRatio = scale;
            float newShift = _heightShift * _scaleRatio;
            _position.Y -= (oldShift - newShift);
        }

        public void EnableLighting(Vector3 lightDirection, float ambient)
        {
            _EnableLighting = true;
            _LightDirection = lightDirection;
            _Ambient = ambient;
        }

        public void Update(GraphicsDevice device, GameTime gameTime)
        {
            Matrix worldRotation = Matrix.CreateRotationY(_rotation);
            Matrix worldTranslation = Matrix.CreateTranslation(_position);
            Matrix worldScale = Matrix.CreateScale(_scaleRatio);

            _worldMatrix = worldScale * worldRotation * worldTranslation;

            effectManager.Update(gameTime);
        }

        public void Draw(GraphicsDevice device, GameTime gameTime, Matrix associateMatrix, int resolutionWidth, int resolutionHeight)
        {
            device.RenderState.AlphaBlendEnable = false;
            device.RenderState.DepthBufferWriteEnable = true;
            device.RenderState.AlphaTestEnable = false;
            CullMode preMode = device.RenderState.CullMode;
            device.RenderState.CullMode = CullMode.None;
            int i = 0;
            Matrix mm2 = Matrix.CreateRotationX((float)Math.PI / 2);

            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorldViewProjection"].SetValue(mm2 *_worldMatrix * associateMatrix);
                    currentEffect.Parameters["xWidth"].SetValue(resolutionWidth);
                    currentEffect.Parameters["xHeight"].SetValue(resolutionHeight);
                    currentEffect.Parameters["xTexture"].SetValue(_textures[i++]);
                    currentEffect.Parameters["xEnableLighting"].SetValue(_EnableLighting);
                    currentEffect.Parameters["xAmbient"].SetValue(_Ambient);
                    currentEffect.Parameters["xLightDirection"].SetValue(_LightDirection);
                }
                mesh.Draw();
            }
            device.RenderState.CullMode = preMode;

            effectManager.Draw(gameTime, associateMatrix);
        }

        public void Draw(GraphicsDevice device, GameTime gameTime, Matrix associateMatrix)
        {
            Draw(device, gameTime, associateMatrix, 800, 600);
        }

        #region Effects
        public void AddEffect(MyEffect newEffect, float timeMillisecond)
        {
            if (newEffect != null)
            {
                effectManager.AddEffect(newEffect, timeMillisecond);
            }
        }
        #endregion Effects
    }
}
