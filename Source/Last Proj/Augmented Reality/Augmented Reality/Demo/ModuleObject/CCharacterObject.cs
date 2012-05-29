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
using ModuleModel;
using ModuleEffects;

namespace ModuleObject
{
    public enum CharacterAction
    {
        stand,
        run,
        attack,
        pain,
        jump,
        flip,
        salute,
        taunt,
        wave,
        point,
        crstnd,
        crwalk,
        crattk,
        crpain,
        crdeath,
        death
    }
    public class CCharacterObject
    {
        private IModel _characterModel;
        private IModel _weaponModel;

        private float _speed = 1.0f;
        private float _scaleRatio = 1.0f;
        private float _heightShift = 25f;
        private Vector3 _position = Vector3.Zero;
        private float _originalRotation = MathHelper.ToRadians(-90);
        private float _rotation = 0;
        
        protected Matrix _worldMatrix = Matrix.Identity;
        
        private bool _isDrawCharacter = true;
        private bool _isDrawWeapon = true;
        Random random = new Random();
        string[] lstAnimationNames;

        #region Lighting
        private float _Ambient = 0.0f;
        private Vector3 _LightDirection = Vector3.Zero;
        private bool _EnableLighting = false;
        #endregion Lighting

        Effect effect;

        private EffectsManager effectManager = new EffectsManager();

        public float speed
        {
            set { if (value > 0) { _speed = value; } }
            get { return _speed; }
        }

        public float Scale
        {
            set { if (value > 0) { UpdateScale(value); } }
            get { return _scaleRatio; }
        }

        public Vector3 Position
        {
            get { return _position; }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public float Radius
        {
            get { return _heightShift * _scaleRatio; }
        }

        public CCharacterObject()
        {
        
        }

        public void EnableLighting(Vector3 lightDirection, float ambient)
        {
            _EnableLighting = true;
            _LightDirection = lightDirection;
            _Ambient = ambient;
        }

        private string[] SelectAnimation(string name)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < lstAnimationNames.Length; i++)
            {
                if (lstAnimationNames[i].Contains(name))
                {
                    result.Add(lstAnimationNames[i]);
                }
            }
            return result.ToArray();
        }

        private string SelectAnimationSingle(string name)
        {
            return SelectAnimation(name)[0];
        }

        public void InitializeObject(GraphicsDevice device, ContentManager content,
            string modelCharacterAssetName, string textureCharacterAssetName, 
            string modelWeaponAssetName, string textureWeaponAssetName,
            string xmlAnimationsList,
            Vector3 position, float scaleRatio, float heightShift)
        {
            effect = content.Load<Effect>(@"Effects\Series4Effects");
            _characterModel = LoadModel(modelCharacterAssetName, textureCharacterAssetName, device, content, xmlAnimationsList);
            _weaponModel = LoadModel(modelWeaponAssetName, textureWeaponAssetName, device, content, xmlAnimationsList);
            lstAnimationNames = _characterModel.GetListAnimationNames();
            SetAnimation(SelectAnimationSingle("stand"));
            _heightShift = heightShift;
            _scaleRatio = scaleRatio;
            SetPosition(position);
        }

        private IModel LoadModel(string modelAssetName, string textureAssetName, GraphicsDevice device, ContentManager content, string xmlAnimationsList)
        {
            IModel result = new CMD2Model(xmlAnimationsList);
            result.Load(modelAssetName, textureAssetName, device, content);
            
            return result;
        }

        private void SetAnimation(string animationName)
        {
            _characterModel.SetAnimation(animationName);
            _weaponModel.SetAnimation(animationName);
        }

        private void SetAnimationNoLoop(string animationName)
        {
            _isDrawCharacter = _characterModel.SetAnimationNoLoop(animationName);
            _isDrawWeapon = _weaponModel.SetAnimationNoLoop(animationName);
        }

        private void SetSequenceAnimations(string animationName1, string animationName2)
        {
            _characterModel.SetAnimationsSequence(animationName1, animationName2);
            _weaponModel.SetAnimationsSequence(animationName1, animationName2);
        }

        private void SetPosition(Vector3 position)
        {
            Vector3 newPos = position;
            float shift = _heightShift * _scaleRatio;
            newPos.Y += shift;
            _position = newPos;
        }

        private void UpdateScale(float scale)
        {
            float oldShift = _heightShift * _scaleRatio;
            _scaleRatio = scale;
            float newShift = _heightShift * _scaleRatio;
            _position.Y -= (oldShift - newShift);
        }

        public void Update(GraphicsDevice device, GameTime gameTime)
        {
            Matrix worldRotation = Matrix.CreateRotationY(_originalRotation + _rotation);
            Matrix worldTranslation = Matrix.CreateTranslation(_position);
            Matrix worldScale = Matrix.CreateScale(_scaleRatio);

            _worldMatrix = worldScale * worldRotation * worldTranslation;

            _characterModel.Update(device, gameTime);
            _weaponModel.Update(device, gameTime);

            effectManager.Update(gameTime);
        }

        public void Draw(GraphicsDevice device, GameTime gameTime, Matrix associateMatrix)
        {
            Draw(device, gameTime, associateMatrix, 800, 600);
        }

        public void Draw(GraphicsDevice device, GameTime gameTime, Matrix associateMatrix, int resolutionWidth, int resolutionHeight)
        {
            device.RenderState.AlphaBlendEnable = false;
            device.RenderState.DepthBufferWriteEnable = true;
            device.RenderState.AlphaTestEnable = false;
            effect.Parameters["xEnableLighting"].SetValue(_EnableLighting);
            effect.Parameters["xAmbient"].SetValue(_Ambient);
            effect.Parameters["xLightDirection"].SetValue(_LightDirection);
            if (_isDrawCharacter)
            {
                _characterModel.Draw(gameTime, device, effect, _worldMatrix * associateMatrix, resolutionWidth, resolutionHeight);
            }
            if (_isDrawWeapon)
            {
                _weaponModel.Draw(gameTime, device, effect, _worldMatrix * associateMatrix, resolutionWidth, resolutionHeight);
            }

            effectManager.Draw(gameTime, associateMatrix);
        }

        public void SetObjectAnimationNoLoop(CharacterAction action)
        {
            SetSequenceAnimations(action.ToString(), "stand");
        }

        public void SetObjectAnimationLoop(CharacterAction action)
        {
            SetAnimation(action.ToString());
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
