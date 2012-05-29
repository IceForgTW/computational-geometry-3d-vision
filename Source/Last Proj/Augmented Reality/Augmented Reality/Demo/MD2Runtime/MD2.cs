//------------------------------------------------------------
// Microsoft® XNA Game Studio Creator's Guide  
// by Stephen Cawood and Pat McGee 
// Copyright (c) McGraw-Hill/Osborne. All rights reserved.
// http://www.mhprofessional.com/product.php?isbn=007149071X
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using System.IO;
using MD2Runtime;

namespace MD2Animation
{
    // reads managed binary format
    

    public class MD2
    {
        MD2Model                                md2;
        private VertexPositionNormalTexture[]   modelVerts;
        private Texture2D                       texture;
        private int                             numFaces;
     
        //------------------------------------------------------------
        // M O D U L E   D E C L A R A T I O N S
        //------------------------------------------------------------
        private float       frameTime;
        private float       previousFrameTime;
        private float       animationSpeed        = 5.0f;
        public DynamicVertexBuffer vertexBuffer;
        private int         frameNum              = 0;
        private int         nextFrameNum          = 1;
        private int         animationNum;
        private bool        sequenced             = false;
        private int         nextAnimation;
        public  bool        paused                = false;

        private bool isLoop = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MD2()
        {}

        /// <summary>
        /// Sets value to scale the animation time.
        /// </summary>
        /// <param name="fSpeed">Amount of time scale.</param>
        public void SetAnimationSpeed(float fSpeed)
        {
            animationSpeed = fSpeed;
        }

        /// <summary>
        /// Total triangles in md2 model. This is the same amount for each frame.
        /// </summary>
        public int GetNumTriangles()
        {
            return numFaces;
        }

        /// <summary>
        /// Returns the texture that skins the model.
        /// </summary>
        public Texture2D GetTexture()
        {
            return texture;
        }

        /// <summary>
        /// Initiates loading of the model through the content pipeline using
        /// the content importer and processor.  Also loads texture through
        /// content pipeline. (Must use an XNA friendly texture format like
        /// *.tga, *.png, *.bmp, *.jpg. Native Quake *.pcx files won't load.
        /// </summary>
        /// <param name="graphics">Graphics device handle needed to initialize vertex buffer.</param>
        /// <param name="model">Name of model and file path.</param>
        /// <param name="skin">Name of texture and file path.</param>
        /// <param name="content">Content manager object for loading through pipeline.</param>
        public void LoadModel(GraphicsDevice graphics, string model, 
                              string skin, ContentManager content)
        {
            md2         = content.Load<MD2Model>(model);
            numFaces    = md2.numFaces; 
            texture     = content.Load<Texture2D>(skin);
            InitializeVertexBuffer(graphics);
        }

        /// <summary>
        /// Sets up dynamic storage variable for xyz, normal, and texture data. A dynamic
        /// area is needed to update the vertex buffer.
        /// </summary>
        /// <param name="graphics">Graphics device handle needed to initialize vertex buffer.</param>
        private void InitializeVertexBuffer(GraphicsDevice graphics)
        {
            modelVerts   = new VertexPositionNormalTexture[md2.numFaces*3];
            vertexBuffer = new DynamicVertexBuffer(graphics,
                                            VertexPositionNormalTexture.SizeInBytes*modelVerts.Length,
                                            BufferUsage.WriteOnly | BufferUsage.Points);
        }

        /// <summary>
        /// Sets current and next frame in current animation sequence.
        /// </summary>
        /// <param name="animNum">Current animation.</param>
        private void SetCurrentAndNextFrames(int animNum)
        {
            // get start and end frames of animation
            int endFrame   = md2.endFrame[animNum];
            int startFrame = md2.startFrame[animNum];

            // set current frame
            frameNum = startFrame;

            // set next frame - if animation more than 1
            // the nextFrame = currentFrame + 1 else nextFrame = currentFrame
            if (endFrame > startFrame)
                nextFrameNum = frameNum + 1;
            else
                nextFrameNum = frameNum;
        }

        /// <summary>
        /// Let's user select the animation. ie. Walk, run, jump, crouch, crawl etc.
        /// </summary>
        /// <param name="animationIndex">Animation identifier.</param>
        public bool SetAnimation(int animationIndex)
        {
            if (animationIndex >= 0 && animationIndex < md2.numAnim)
            {
                isLoop = true;
                paused = false;
                animationNum = animationIndex;
                SetCurrentAndNextFrames(animationNum);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Let's user select the animation but no loop. ie. Walk, run, jump, crouch, crawl etc.
        /// </summary>
        /// <param name="animationIndex">Animation identifier.</param>
        public bool SetAnimationNoLoop(int animationIndex)
        {
            if (animationIndex >= 0 && animationIndex < md2.numAnim)
            {
                isLoop = false;
                paused = false;
                animationNum = animationIndex;
                SetCurrentAndNextFrames(animationNum);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Let's user select the animation. ie. Walk, run, jump, crouch, crawl etc.
        /// </summary>
        /// <param name="firstAnimation">Animation identifier.</param>
        /// <param name="secondAnimation">Animation identifier.</param>
        public void SetAnimationSequence(int firstAnimation, int secondAnimation)
        {
            sequenced       = true;
            nextAnimation   = secondAnimation;
            SetAnimation(firstAnimation);
        }

        /// <summary>
        /// Let's user determine if a specific animation is playing
        /// </summary>
        /// <param name="animationIndex">Animation identifier.</param>
        public bool IsPlaying(int animationIndex)
        {
            if (animationIndex == animationNum)
                return true;
            return false;
        }

        /// <summary>
        /// Goes to next animation in list. Starts back at beginning
        /// when the end of the list is reached.
        /// </summary>
        public void AdvanceAnimation()
        {
            // go to next animation in array
            if (animationNum + 1 < md2.numAnim)
                animationNum += 1;
            else
                animationNum = 0;
            SetCurrentAndNextFrames(animationNum);
        }

        /// <summary>
        /// Sets class level variable, paused, to freeze the animation.
        /// </summary>
        public void Pause()
        {
            paused = true;
        }

        /// <summary>
        /// Allows user to start animation from paused state.
        /// </summary>
        public void Resume()
        {
            paused = false;
        }

        /// <summary>
        /// Moves to next frame in current animation when time is up
        /// for the first animation.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private bool AdvanceFrame(GameTime gameTime)
        {
            // don't advance frame if paused
            if(paused)
                return false;

            frameTime += (float)gameTime.ElapsedGameTime.Milliseconds/20.0f; 
            frameTime  = frameTime%animationSpeed;

            // frame has been completed
            if (frameTime < previousFrameTime)
            {
                int endFrame    = md2.endFrame[animationNum];
                int startFrame  = md2.startFrame[animationNum];

                // start at beginning frame if all frames completed
                if (frameNum < endFrame-1)
                {
                    frameNum += 1;
                    nextFrameNum = frameNum + 1;

                    if (nextFrameNum > endFrame-1)
                        nextFrameNum = startFrame;
                }

                // animation is over and this anim is in a sequence
                // so advance to next animation in sequence
                else if (sequenced == true)
                {
                    sequenced     = false;
                    SetAnimation(nextAnimation);
                }
                // animation is continuous so start frame at beginning
                else
                {
                    if (isLoop)
                    {
                        frameNum = startFrame;
                        nextFrameNum = startFrame + 1;
                    }
                    else
                    {
                        paused = true;
                    }
                }
            }
            previousFrameTime = frameTime;
            return true;
        }

        /// <summary>
        /// Updates animation time for the model. Interpolates
        /// xyz's, and normals. Resets vertex buffer with 
        /// updated model updated xyz, updated normal's, and 
        /// uv's (uv's are the same for all frames).
        /// </summary>
        /// <param name="graphics">Handle to graphics device for updating vertex buffer.</param>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void UpdateModel(GraphicsDevice graphics, GameTime gameTime)
        {
            Vector2 UV; 
            Vector3 position; 
            Vector3 normal; 
            Vector3 currPos, nextPos;
            Vector3 currNorm, normNext; 
 
            int vertexCount = 0;
            float timeScale = frameTime / animationSpeed;
            normal          = new Vector3(0.0f, 1.0f, 0.0f);

            for (int i = 0; i < md2.numFaces; i++)
            {
                for(int j=0; j<3; j++)
                {
                    int posIndex   = md2.positionIndex[frameNum, i,j];
                    int uvIndex    = md2.frameUVIndex[frameNum, i, j]; 

                    UV      = new Vector2(md2.UV[frameNum, uvIndex, 0],
                                          md2.UV[frameNum, uvIndex, 1]);

                    currPos = new Vector3(md2.framePosition[frameNum, posIndex, 0],
                                          md2.framePosition[frameNum, posIndex, 1], 
                                          md2.framePosition[frameNum, posIndex, 2]);
                    nextPos = new Vector3(md2.framePosition[nextFrameNum, posIndex, 0],
                                          md2.framePosition[nextFrameNum, posIndex, 1],
                                          md2.framePosition[nextFrameNum, posIndex, 2]);
                    currNorm= new Vector3(md2.normal[frameNum, posIndex, 0],
                                          md2.normal[frameNum, posIndex, 1],
                                          md2.normal[frameNum, posIndex, 2]);

                    normNext= new Vector3(md2.normal[nextFrameNum, posIndex, 0],
                    md2.normal[nextFrameNum, posIndex, 1],
                    md2.normal[nextFrameNum, posIndex, 2]);

                    // project (interpolate) xyz by time fraction between frames
                    position  = currPos  + (nextPos - currPos) * timeScale;
                    normal    = currNorm + (normNext- currNorm)* timeScale;

                    modelVerts[vertexCount++] 
                              = new VertexPositionNormalTexture(position, normal, UV);
                }
            }

            // have to clear the graphics device before setting data
            graphics.Vertices[0].SetSource(null, 0, 0);
            vertexBuffer.SetData(modelVerts);
            AdvanceFrame(gameTime);
        }
    }
}