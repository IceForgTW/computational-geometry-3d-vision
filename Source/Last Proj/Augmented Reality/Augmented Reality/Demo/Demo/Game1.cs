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
using ModuleObject;
using ModuleEffects;
using ModuleEffectController;

using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;


namespace Demo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        [DllImport("CardDetector.dll")]
        public static extern void SetCalibrationInfo(float[] intrin, float[] distor);
        [DllImport("CardDetector.dll")]
        public static extern void SetCalibrationInfoFromXML(string file);
        [DllImport("CardDetector.dll")]
        public static extern void LearnObject(string[] objFile, int[] CardSizeWidth, int[] CardSizeHeight, int nObject);
        [DllImport("CardDetector.dll")]
        public static extern void DetectObject(IntPtr frame, out int nDetectedObject, int[] ObjectID, int[] ObjectPositions, int bFromCam);
        [DllImport("CardDetector.dll")]
        public static extern void DrawBox(IntPtr frame, int nDetectedObject, int[] ObjectID, int[] ObjectPosition);
        [DllImport("CardDetector.dll")]
        public static extern void GetProjectionMatrix(int ObjectID, int[] ObjectPosition,
            out float m11, out float m12, out float m13, out float m14,
            out float m21, out float m22, out float m23, out float m24,
            out float m31, out float m32, out float m33, out float m34
            );
        Capture cap;
        int nObject;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsDevice device;
        CBuilding building;
        CCharacterObject character;
        ObjectProvider provider;
        KeyboardState preKeyboardState;
        bool flag = true;
        string sVideoPath;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        /// 

        protected override void Initialize()
        {
            // cap: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
            graphics.ApplyChanges();
            //cap = new Capture(@"c:\Documents and Settings\LENOVO 3000 Y410\My Documents\Camtasia Studio\Thuc nghiem\data1.avi");

            device = graphics.GraphicsDevice;
            provider = new ObjectProvider(device, Content);
            base.Initialize();
        }

        void ReadObjectFromXML(string file, out int nObject, out string[] ObjFileName, out int[] CardSizeWidth, out int[] CardSizeHeight)
        {
            XmlTextReader xmlReader = new XmlTextReader(file);
            xmlReader.Read();

            nObject = int.Parse(xmlReader.GetAttribute("Total"));
            ObjFileName = new string[nObject];
            CardSizeWidth = new int[nObject];
            CardSizeHeight = new int[nObject];
            int idx = 0;

            while (xmlReader.Read())
            {
                switch (xmlReader.Name)
                {
                    case "Name":
                        break;
                    case "Image":
                        ObjFileName[idx] = xmlReader.ReadElementContentAsString();
                        break;
                    case "Width":
                        CardSizeWidth[idx] = xmlReader.ReadElementContentAsInt();
                        break;
                    case "Height":
                        CardSizeHeight[idx] = xmlReader.ReadElementContentAsInt();
                        idx++;
                        break;
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            // Hoc doi tuong
            string[] ObjectName;
            int[] CardSizeWidth;
            int[] CardSizeHeight;
            ReadObjectFromXML("ObjectConfig.xml", out nObject, out ObjectName, out CardSizeWidth, out CardSizeHeight);

            LearnObject(ObjectName, CardSizeWidth, CardSizeHeight, nObject);
            SetCalibrationInfoFromXML("camera.xml");
            StreamReader sr = new StreamReader("Input.txt");
            sVideoPath = sr.ReadLine();
            sr.Close();

            //cap = new Capture(sVideoPath);
            cap = new Capture(0);


            building = provider.GetBuildingAt(0);
            character = provider.GetCharacterAt(0);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            ProcessInput(gameTime);
            UpdateObject(gameTime);

            base.Update(gameTime);
        }

        private void LoadObject(int idx)
        {
            if (flag)
            {
                building = provider.GetBuildingAt(idx);
            }
            else
            {
                character = provider.GetCharacterAt(idx);
            }
        }

        private void UpdateObject(GameTime gameTime)
        {
            if (flag)
            {
                //building.Update(device, gameTime);
                provider.GetBuildingAt(0).Update(device, gameTime);
            }
            else
            {
                character.Update(device, gameTime);
            }
        }

        private void DrawObject(GameTime gameTime, Matrix associateMatrix)
        {
            if (flag)
            {
                building.Draw(device, gameTime, associateMatrix);
            }
            else
            {
                character.Draw(device, gameTime, associateMatrix);
            }
        }

        private void AddEffect2Building(int idxBuilding, int idxStyle, int idxColor)
        {

        }

        private void AddEffect(int idxStyle, int idxColor, bool flag)
        {
            EffectCreator.CharacterHaloStyle[] charStyles =
            {
                EffectCreator.CharacterHaloStyle.Style01,
                EffectCreator.CharacterHaloStyle.Style02
            };

            EffectCreator.CharacterHaloColor[] charColors = 
            {
                EffectCreator.CharacterHaloColor.Blue,
                EffectCreator.CharacterHaloColor.Green,
                EffectCreator.CharacterHaloColor.Violet,
                EffectCreator.CharacterHaloColor.White,
                EffectCreator.CharacterHaloColor.Yellow
            };

            EffectCreator.BuildingHaloStyle[] buildingStyles =
            {
                EffectCreator.BuildingHaloStyle.Style01,
                EffectCreator.BuildingHaloStyle.Style02
            };

            EffectCreator.BuildingHaloColor[] buildingColors = 
            {
                EffectCreator.BuildingHaloColor.Blue,
                EffectCreator.BuildingHaloColor.Green,
                EffectCreator.BuildingHaloColor.Violet,
                EffectCreator.BuildingHaloColor.White,
                EffectCreator.BuildingHaloColor.Yellow
            };


            if (flag)
            {
                MyEffect halo = EffectCreator.CreateHaloForBuilding(this, building, buildingStyles[idxStyle], buildingColors[idxColor]);
                building.AddEffect(halo, 10000);
            }
            else
            {
                MyEffect halo = EffectCreator.CreateHaloForCharacter(this, character, charStyles[idxStyle], charColors[idxColor]);
                character.AddEffect(halo, 10000);
            }
        }

        private void ProcessInput(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Enter) && preKeyboardState.IsKeyUp(Keys.Enter))
            {
                flag = !flag;
            }

            #region ChangeModel
            if (keyboardState.IsKeyDown(Keys.D0) && preKeyboardState.IsKeyUp(Keys.D0))
            {
                LoadObject(0);
            }
            if (keyboardState.IsKeyDown(Keys.D1) && preKeyboardState.IsKeyUp(Keys.D1))
            {
                LoadObject(1);
            }
            if (keyboardState.IsKeyDown(Keys.D2) && preKeyboardState.IsKeyUp(Keys.D2))
            {
                LoadObject(2);
            }
            if (keyboardState.IsKeyDown(Keys.D3) && preKeyboardState.IsKeyUp(Keys.D3))
            {
                LoadObject(3);
            }
            if (keyboardState.IsKeyDown(Keys.D4) && preKeyboardState.IsKeyUp(Keys.D4))
            {
                LoadObject(4);
            }
            if (keyboardState.IsKeyDown(Keys.D5) && preKeyboardState.IsKeyUp(Keys.D5))
            {
                LoadObject(5);
            }
            if (keyboardState.IsKeyDown(Keys.D6) && preKeyboardState.IsKeyUp(Keys.D6))
            {
                LoadObject(6);
            }
            if (keyboardState.IsKeyDown(Keys.D7) && preKeyboardState.IsKeyUp(Keys.D7))
            {
                LoadObject(7);
            }
            if (keyboardState.IsKeyDown(Keys.D8) && preKeyboardState.IsKeyUp(Keys.D8))
            {
                LoadObject(8);
            }
            if (keyboardState.IsKeyDown(Keys.D9) && preKeyboardState.IsKeyUp(Keys.D9))
            {
                LoadObject(9);
            }
            #endregion ChangeModel

            #region AddEffect
            if (keyboardState.IsKeyDown(Keys.F1) && preKeyboardState.IsKeyUp(Keys.F1))
            {
                AddEffect(0, 0, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F2) && preKeyboardState.IsKeyUp(Keys.F2))
            {
                AddEffect(0, 1, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F3) && preKeyboardState.IsKeyUp(Keys.F3))
            {
                AddEffect(0, 2, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F4) && preKeyboardState.IsKeyUp(Keys.F4))
            {
                AddEffect(0, 3, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F5) && preKeyboardState.IsKeyUp(Keys.F5))
            {
                AddEffect(0, 4, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F6) && preKeyboardState.IsKeyUp(Keys.F6))
            {
                AddEffect(1, 0, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F7) && preKeyboardState.IsKeyUp(Keys.F7))
            {
                AddEffect(1, 1, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F8) && preKeyboardState.IsKeyUp(Keys.F8))
            {
                AddEffect(1, 2, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F9) && preKeyboardState.IsKeyUp(Keys.F9))
            {
                AddEffect(1, 3, flag);
            }
            if (keyboardState.IsKeyDown(Keys.F10) && preKeyboardState.IsKeyUp(Keys.F10))
            {
                AddEffect(1, 4, flag);
            }
            #endregion AddEffect

            preKeyboardState = keyboardState;
        }

        int f = 1;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            int nDetectedObject;
            int[] ObjectID = new int[nObject];
            int[] ObjectPosition = new int[nObject * 8];
            Image<Bgr, byte> frame = cap.QueryFrame();

            if (frame != null)
            {
                IntPtr framePtr = frame.Ptr;
                // Detect
                DetectObject(frame, out nDetectedObject, ObjectID, ObjectPosition, 0);
                spriteBatch.Begin();

                Texture2D texture = ConvertToTexture2D(GraphicsDevice, frame);
                spriteBatch.Draw(texture, new Vector2(0, 0), Color.White);
                //buildings[0].Draw(device, gameTime, Matrix.Identity, m);

                spriteBatch.End();

                // Projection MAtrix
                if (nDetectedObject > 0)
                {
                    for (int idx = 0; idx < nDetectedObject; idx++)
                    {
                        float m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34;
                        int[] pos = new int[8];
                        for (int i = 0; i < 8; i++)
                            pos[i] = ObjectPosition[idx * 8 + i];
                        GetProjectionMatrix(ObjectID[idx], pos, out m11, out m12, out m13, out m14, out m21, out m22, out m23, out m24, out m31, out m32, out m33, out m34);
                        Matrix m = new Matrix();
                        m.M11 = m11; m.M12 = m21; m.M13 = m31; m.M14 = 0;
                        m.M21 = m12; m.M22 = m22; m.M23 = m32; m.M24 = 0;
                        m.M31 = m13; m.M32 = m23; m.M33 = m33; m.M34 = 0;
                        m.M41 = m14; m.M42 = m24; m.M43 = m34; m.M44 = 1;

                        //if (ObjectID[idx] == 0)
                        provider.SetActive(ObjectID[idx]);
                        building = provider.GetBuildingAt(0);
                        //building.Update(device, gameTime);
                        //if (f == 1)
                        //{
                            //AddEffect(1, 0, flag);
                        //}
                        building.Draw(device, gameTime, m, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

                        //building.Update(device, gameTime);
                        //provider.GetBuildingAt(ObjectID[idx]).Draw(device, gameTime, m, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                        //character = provider.GetCharacterAt(idx);
                        //character.Update(device, gameTime);
                        //character.Draw(device, gameTime, m, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                        
                        
                        //provider.GetBuildingAt(0).Draw(device, gameTime, m, 800, 600);
                        //else
                        //    buildings[2].Draw(device, gameTime, Matrix.Identity, m);
                    }
                    f = 0;
                }
            }



           /* GraphicsDevice.Clear(Color.Black);
            int resolutionWidth = Window.ClientBounds.Width;
            int resolutionHeight = Window.ClientBounds.Height;
            Vector3 cameraPos = new Vector3(0.0f, 2f, 8f);
            Matrix associateMatrix = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up) * 
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)resolutionWidth / (float)resolutionHeight, 0.01f, 100f);
            // TODO: Add your drawing code here
            DrawObject(gameTime, associateMatrix);*/
            base.Draw(gameTime);
        }

        public static Texture2D ConvertToTexture2D(GraphicsDevice device, Image<Bgr, byte> image)
        {
            /*         Color[] colorData = new Color[image.Width * image.Height];
                     byte[] bgrData = image.Bytes;
                     for (int i = 0; i < colorData.Length; i++)
                         colorData[i] = new Color(bgrData[3 * i + 2], bgrData[3 * i + 1], bgrData[3 * i]);

                     Texture2D frame = new Texture2D(device, image.Width, image.Height);
                     frame.SetData<Color>(0, null, colorData, 0, colorData.Length, SetDataOptions.None);
                     return frame;*/

            Texture2D frame = new Texture2D(device, image.Width, image.Height);

            using (System.IO.MemoryStream s = new System.IO.MemoryStream())
            {
                image.Bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);
                s.Seek(0, System.IO.SeekOrigin.Begin);
                frame = Texture2D.FromFile(device, s);
            }
            return frame;
        }
    }
}
