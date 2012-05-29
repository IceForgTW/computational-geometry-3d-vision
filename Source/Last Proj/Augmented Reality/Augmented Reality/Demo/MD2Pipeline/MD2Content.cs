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
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.IO;

namespace MD2Importer
{
    public class Face
    {
        public int[] posIndex = new int[3];
        public int[] uvIndex  = new int[3];
        public       Face() { }
    }

    public class Frame
    {
        public Face[] face;

        public int       numVerts;
        public int       numUVs;
        public int       numFaces;
        public float[,]  vertex;      // frame's vertices
        public Vector3[] normal;      // frame's normals
        public Vector2[] UV;          // texture's UV coordinates       
    }

    public class Quake2Model
    {
        public int[,,]   positionIndex;
        public int[,,]   frameUVIndex;
        public float[,,] framePosition;
        public float[,,] normal;
        public float[,,] UV;

        public int[]     startFrame;
        public int[]     endFrame;

        public byte[]    byteBuffer;
        public int       numBytes;
        public const int HEADERSIZE = 17;
        public int[]     headerInfo = new int[HEADERSIZE];

        public int       numFaces;
        public int       numFrames;
        public int       numVerts;
        public int       numUVs;
        public int       numAnim;

        private ArrayList triList       = new ArrayList();
        private ArrayList indexList     = new ArrayList();
        private ArrayList uvList        = new ArrayList();
        public  ArrayList frameList     = new ArrayList();
        private ArrayList tempFrameList = new ArrayList();


        private const int FILEID = 0;           // identifies file
        private const int VERSION = 1;          // version must be 8 for md2
        private const int TEXW = 2;             // texture pixel w
        private const int TEXH = 3;             // texture pixel h
        private const int FRAMESIZE = 4;          // bytes per frame
        private const int NUM_SKINS = 5;        // total textures
        private const int NUM_VERTS = 6;        // total vertices (constant for each frame)
        private const int NUMTEXCOORDS = 7;     // total texture coordinates
        private const int NUM_TRI = 8;          // total faces (polygons)
        private const int NUM_GLCOMMANDS = 9;   // triangle list or triangle fans
        private const int NUM_FRAMES = 10;      // total animation frames
        private const int OFFSET_SKINS = 11;    // texture name
        private const int OFFSET_TEX_COORDS = 12;   // texture data offset
        private const int OFFSET_TRIANGLES = 13;// triangle (face) data offset
        private const int OFFSET_FRAMES = 14;   // frame data offset
        private const int OFFSET_GLCOMMANDS = 15;   // gl commands data offset
        private const int OFFSET_END = 16;      // The end of the file offset

        // stores references to vertex and texture arrays
        public class Indices
        {
            public short[] posIndex;
            public short[] uvIndex;
        }

        public class Cuv
        {
            public short u, v;   // texture coordinates
        }

        // vertices for frames
        public class Triangle
        {
            public float[] vertex = new float[3];
        }

        // stores frame name and vertices 
        public class AnimationFrame
        {
            public char[]       name;
            public Triangle[]   face;
        }

        public class Animation
        {
            public String animName;     // animation name
            public int    startFrame;   // first frame in animation
            public int    endFrame;		// last frame in animation
        }
        public ArrayList animationList = new ArrayList();
        public Quake2Model() { }        // default constructor


        public Quake2Model(byte[] bytes)
        {
            byteBuffer = bytes;
            numBytes = byteBuffer.Length;


            // read summary of data stored in the md2 file
            MemoryStream ms = new MemoryStream(byteBuffer);
            BinaryReader br = new BinaryReader(ms);
            ms.Position = 0;

            // read the faces
            for (int i = 0; i < 17; i++)
                headerInfo[i] = br.ReadInt32();

            br.Close();
            ms.Close();

            numFaces    = headerInfo[NUM_TRI];
            numFrames   = headerInfo[NUM_FRAMES];
            numUVs      = headerInfo[NUMTEXCOORDS];
            readFaceIndexes();
            readUVs();
            readXYZ();
            convertDataStructures();
            storePerVertexNormal();
        }

        /// <summary>
        /// Read frame name from *.md2 and parse it to extract animation name,
        /// frame start, and frame end. 
        /// </summary>
        /// <param name="tMD2Model">Stores total frames, animation count, 
        /// current aniamtion index, current frame</param>
        private void parseAnimations(int numFrames)
        {
            String      prevFrame        = "";
            String      currFrame        = "";
            int         startFrameNum    = 0;
            Animation   prevAnimation    = new Animation();

            // num of frames is same as total frames
            for (int frameNum = 0; frameNum<numFrames; frameNum++)
            {
                char[]          name  = new char[16];
                AnimationFrame frame = (AnimationFrame)tempFrameList[frameNum];
                                name  = frame.name;
                String animationName  = "";

                // remove number from the name
                for (int j = 0; j < name.Length; j++)
                {
                    // enter this next portion of the loop when  first number is found in the name
                    string n = name[j].ToString();
                    if (   n == "0" || n == "1" || n == "2" || n == "3"
                        || n == "4" || n == "5" || n == "6"
                        || n == "7" || n == "8" || n == "9")
                    {

                        // store all letters up until the first number is found
                        for (int k = 0; k < j; k++)
                            animationName = animationName + name[k].ToString();

                        // search through remainder of string which stores numbers and extract
                        // the frame number
                        for (int k = j; k < name.Length; k++)
                        {
                            // get current character and enter loop if number is found in name
                            string endChar = name[k].ToString();
                            if(endChar == "0" || endChar == "1" || endChar == "2" || endChar == "3"
                            || endChar == "4" || endChar == "5" || endChar == "6"
                            || endChar == "7" || endChar == "8" || endChar == "9")
                            {
                                // after 1st num found in name store all chars in string till 
                                // end is reached
                                int l = k + 1;
                                while (name[l] != '\0')
                                {
                                    endChar = endChar + name[l].ToString();
                                    l++;
                                }
                                // skip last two characters because name could have a
                                // number in it. ie. 'pain1' and to avoid the null byte '\0'
                                if (endChar.Length > 2)
                                {
                                    // add numbers to the string as characters in the string
                                    for (int m = 0; m < endChar.Length - 2; m++)
                                    {
                                        animationName = animationName + endChar[0].ToString();
                                    }
                                    endChar = endChar[endChar.Length - 2].ToString() + endChar[endChar.Length - 1].ToString();
                                }
                                break; // got frame number so break
                            }
                        }
                        break;         // got frame name so break
                    }
                }

                // new frame name found or finshed all frames in list
                if (animationName != prevFrame || frameNum == numFrames - 1)
                {
                    // assign end frame number after 1st frame 
                    if (prevFrame != "")
                    {
                        Animation tempAnimation  = new Animation();
                        tempAnimation.endFrame   = frameNum;
                        tempAnimation.animName   = currFrame;
                        tempAnimation.startFrame = startFrameNum;
                        animationList.Add(tempAnimation);
                    }
                    // set up new animation
                    prevFrame     = animationName; // store current name for comparison later
                    currFrame     = animationName; // store current frame name
                    startFrameNum = frameNum;      // store starting frame number
                }
            }

            // set up three arrays to store animation frame count, start frame, and end frame
            numAnim     = animationList.Count;
            startFrame  = new int[numAnim];
            endFrame    = new int[numAnim];

            // populate three arrays for animation frame count, start frame, and end frame
            for (int i = 0; i < numAnim; i++)
            {
                Animation tempAnimation = (Animation)animationList[i];
                startFrame[i]           = tempAnimation.startFrame;
                endFrame[i]             = tempAnimation.endFrame;
            }
        }

        private void convertDataStructures()
        {
            parseAnimations(headerInfo[NUM_FRAMES]);

            // Create a local i to store the first i of animation's data
            // for each i
            for (int i = 0; i < headerInfo[NUM_FRAMES]; i++)
            {
                Frame tempFrame = new Frame();

                // store total verts, uv's, and triangles from header info
                tempFrame.numVerts = headerInfo[NUM_VERTS];
                tempFrame.numUVs   = headerInfo[NUMTEXCOORDS];
                tempFrame.numFaces = headerInfo[NUM_TRI];

                // create an array big enough to the total verts and norms each i
                tempFrame.vertex   = new float[tempFrame.numVerts, 3];
                tempFrame.normal   = new Vector3[tempFrame.numVerts];

                // get i info from array list
                AnimationFrame animationFrame = (AnimationFrame)tempFrameList[i];

                // assign x,y,z to i from structure in array list
                for (int j = 0; j < tempFrame.numVerts; j++)
                {
                    tempFrame.vertex[j, 0] = animationFrame.face[j].vertex[0];
                    tempFrame.vertex[j, 1] = animationFrame.face[j].vertex[1];
                    tempFrame.vertex[j, 2] = animationFrame.face[j].vertex[2];
                }

                // create UV array for current i with enough space for all tex coords
                tempFrame.UV = new Vector2[headerInfo[NUMTEXCOORDS]];

                // md2 stores pixels for uv's. to map them onto the surface this needs
                // to be converted to a ratio that ranges between 0 and 1. To do this,
                // divide each U and V by the pixel width and height.
                for (int j = 0; j < headerInfo[NUMTEXCOORDS]; j++)
                {
                    Cuv cpx = (Cuv)uvList[j];
                    tempFrame.UV[j].X = cpx.u / (float)headerInfo[TEXW];
                    tempFrame.UV[j].Y = cpx.v / (float)headerInfo[TEXH];
                }

                // each flat surface (or triangle) in the model is a face
                // declare an array to store all faces in the i
                tempFrame.face = new Face[headerInfo[NUM_TRI]];

                for (int tri = 0; tri < headerInfo[NUM_TRI]; tri++)
                {
                    // store xyz indices for each of the three triangle coordinates
                    Face tempFace = new Face();
                    
                    Indices indices = (Indices)indexList[tri];

                    tempFace.posIndex[0] = indices.posIndex[0];
                    tempFace.posIndex[1] = indices.posIndex[1];
                    tempFace.posIndex[2] = indices.posIndex[2];

                    // store all uv indices in a triangle
                    tempFace.uvIndex[0]  = indices.uvIndex[0];
                    tempFace.uvIndex[1]  = indices.uvIndex[1];
                    tempFace.uvIndex[2]  = indices.uvIndex[2];

                    tempFrame.face[tri]  = new Face();
                    tempFrame.face[tri]  = tempFace;
                }
                frameList.Add(tempFrame);
            }
            tempFrameList.Clear();
            indexList.Clear();
            uvList.Clear();
        }

        /// <summary>
        /// Computes and stores normals for each vertex each frame.
        /// </summary>
        private void storePerVertexNormal()
        {
            for (int frameNum = 0; frameNum < headerInfo[NUM_FRAMES]; frameNum++)
            {
                Frame tempFrame      = new Frame();
                tempFrame            = (Frame)frameList[frameNum];

                // calculate and store normals for each vertex
                Vector3[] tempNormal = new Vector3[headerInfo[NUM_TRI]];
                for (int tri = 0; tri < headerInfo[NUM_TRI]; tri++)
                {
                    // get the normal for the immediate face
                    Vector3
                    A = new Vector3(tempFrame.vertex[tempFrame.face[tri].posIndex[0], 0],
                                    tempFrame.vertex[tempFrame.face[tri].posIndex[0], 1],
                                    tempFrame.vertex[tempFrame.face[tri].posIndex[0], 2]);
                    Vector3
                    B = new Vector3(tempFrame.vertex[tempFrame.face[tri].posIndex[1], 0],
                                    tempFrame.vertex[tempFrame.face[tri].posIndex[1], 1],
                                    tempFrame.vertex[tempFrame.face[tri].posIndex[1], 2]);
                    Vector3
                    C = new Vector3(tempFrame.vertex[tempFrame.face[tri].posIndex[2], 0],
                                    tempFrame.vertex[tempFrame.face[tri].posIndex[2], 1],
                                    tempFrame.vertex[tempFrame.face[tri].posIndex[2], 2]);
                    Vector3 vector1 = A - C;
                    Vector3 vector2 = C - B;
                    Vector3 normal  = Vector3.Cross(vector1, vector2);
                    tempNormal[tri] = normal;
                }

                // go through all vertices
                for (int vert = 0; vert < headerInfo[NUM_VERTS]; vert++)
                {
                    Vector3 summedNormals = Vector3.Zero;
                    float counter = 0.0f;

                    // find surrounding faces for each vertex and calculate the 
                    // average normal
                    for (int tri = 0; tri < headerInfo[NUM_TRI]; tri++)
                    {
                        // check if any face coordinates uses the current vertex
                        if (
                            tempFrame.face[tri].posIndex[0] == vert ||
                            tempFrame.face[tri].posIndex[1] == vert ||
                            tempFrame.face[tri].posIndex[2] == vert)
                        {
                            // sum normals for all surrounding faces
                            summedNormals   += tempNormal[tri];
                            counter         += 1.0f;
                        }
                    }

                    // get average of normals for all surrounding faces
                    Vector3 normalAverage = summedNormals/counter;
                    normalAverage.Normalize();
                    tempFrame.normal[vert] = normalAverage;
                }
            }
        }

        /// <summary>
        /// Reads binary XYZ from media file.  Scales and translates each frame 
        /// according to transformations stored with each frame.
        /// </summary>
        private void readXYZ()
        {
            MemoryStream ms = new MemoryStream(byteBuffer);
            BinaryReader br = new BinaryReader(ms);

            // read all verts for all frames
            for (int i = 0; i < headerInfo[NUM_FRAMES]; i++)
            {
                ms.Position = headerInfo[OFFSET_FRAMES] + i*headerInfo[FRAMESIZE];

                // each frame is scaled and translated on XYZ
                float[] frameScale       = new float[3];
                float[] frameTranslation = new float[3];
                char[]  frameName        = new char[16];

                // read scale and tranlation information from binary file
                for (int j = 0; j < 3; j++)
                    frameScale[j]       = br.ReadSingle();
                for (int j = 0; j < 3; j++)
                    frameTranslation[j] = br.ReadSingle();

                // read frame name from media
                frameName               = br.ReadChars(16);
                //Frame[] frameList;
                AnimationFrame Frame    = new AnimationFrame();
                Frame.face              = new Triangle[headerInfo[NUM_VERTS]];

                ms.Position             = headerInfo[OFFSET_FRAMES]
                                        + 40+i*headerInfo[FRAMESIZE];
                Frame.name              = frameName;

                // read xyz, scale and tranlate them, swap Y with Z and negate Z (by design)
                for (int j = 0; j < headerInfo[NUM_VERTS]; j++)
                {
                    Triangle triangle  = new Triangle();
                    triangle.vertex[0] =   (float)br.ReadByte()* frameScale[0] + frameTranslation[0];
                    triangle.vertex[2] = -((float)br.ReadByte()* frameScale[1] + frameTranslation[1]);
                    triangle.vertex[1] =   (float)br.ReadByte()* frameScale[2] + frameTranslation[2];
                    Frame.face[j]      =          triangle;
                    float normal       =   (float)br.ReadByte();
                }
                tempFrameList.Add(Frame);
            }
            br.Close();
            ms.Close();
        }

        /// <summary>
        /// Read UV data from media file.
        /// </summary>
        private void readUVs()
        {
            MemoryStream ms = new MemoryStream(byteBuffer);
            BinaryReader br = new BinaryReader(ms);

            // read the textureUV coorindates
            ms.Position = headerInfo[OFFSET_TEX_COORDS];

            for (int i = 0; i < headerInfo[NUMTEXCOORDS]; i++)
            {
                Cuv textureUV = new Cuv();
                textureUV.u = br.ReadInt16();
                textureUV.v = br.ReadInt16();
                uvList.Add(textureUV);
            }
            br.Close();
            ms.Close();
        }

        /// <summary>
        /// Reads indexes for UV and XYZ from media file.
        /// </summary>
        private void readFaceIndexes()
        {
            MemoryStream ms = new MemoryStream(byteBuffer);
            BinaryReader br = new BinaryReader(ms);

            ms.Position = headerInfo[OFFSET_TRIANGLES];

            for (int i = 0; i < headerInfo[NUM_TRI]; i++)
            {
                Indices inidices        = new Indices();
                inidices.posIndex       = new short[3];
                inidices.uvIndex        = new short[3];
                inidices.posIndex[0]    = br.ReadInt16();
                inidices.posIndex[1]    = br.ReadInt16();
                inidices.posIndex[2]    = br.ReadInt16();
                inidices.uvIndex[0]     = br.ReadInt16();
                inidices.uvIndex[1]     = br.ReadInt16();
                inidices.uvIndex[2]     = br.ReadInt16();
                indexList.Add(inidices);
            }
            br.Close();
            ms.Close();
        }
    }

    // Loads Quake data from media file.  Reads all bytes and passes them
    // to the Quake2Model class to read the header and then load the media.
    [ContentImporter(".md2", DefaultProcessor = "md2Processor")]
    public class MD2Importer : ContentImporter<Quake2Model>
    {
        public override Quake2Model Import(String filename, ContentImporterContext context)
        {
            byte[]      bytes    = File.ReadAllBytes(filename);
            Quake2Model md2      = new Quake2Model(bytes);
            return      md2;
        }
    }

    // Returns media data in intermediate form
    [ContentProcessor] // <tinput, toutput>
    // ContentProcessor provides a base for developing custom processor components
    // all processor's must derive from this class
    public class md2Processor : ContentProcessor<Quake2Model, Quake2Model>
    {
        // override the Process method with our own output datatype
        public override Quake2Model Process(Quake2Model input,
                        ContentProcessorContext context)
        {
            return new Quake2Model(input.byteBuffer);
        }
    }

    // identify the components of a type writer
    [ContentTypeWriter]
    // ContentTypeWriter provides methods for converting to binary format for *.xnb file
    public class MD2Writer : ContentTypeWriter<Quake2Model>
    {
        // outputs to *.xnb file
        protected override void Write(ContentWriter output, Quake2Model md2)
        {
            output.Write(md2.numBytes);
            output.Write(md2.numFrames);
            output.Write(md2.numFaces);

            Frame tempFrame = new Frame();
            for (int frame = 0; frame < md2.numFrames; frame++)
            {
                tempFrame = (Frame)md2.frameList[frame];
                for (int tri = 0; tri < md2.numFaces; tri++)
                {

                    // store the position indices
                    output.Write(tempFrame.face[tri].posIndex[0]);
                    output.Write(tempFrame.face[tri].posIndex[1]);
                    output.Write(tempFrame.face[tri].posIndex[2]);

                    // store the uv indices
                    output.Write(tempFrame.face[tri].uvIndex[0]);
                    output.Write(tempFrame.face[tri].uvIndex[1]);
                    output.Write(tempFrame.face[tri].uvIndex[2]);
                }
            }
            tempFrame = (Frame)md2.frameList[0];
            // store vertex position data for all frames
            output.Write(tempFrame.numVerts);

            for (int frame = 0; frame < md2.numFrames; frame++)
            {
                tempFrame = (Frame)md2.frameList[frame];

                // store the position vertices
                for (int j = 0; j < tempFrame.numVerts; j++)
                {
                    output.Write(tempFrame.vertex[j, 0]);
                    output.Write(tempFrame.vertex[j, 1]);
                    output.Write(tempFrame.vertex[j, 2]);
                }
            }

            // store UV data for all frames
            output.Write(md2.numUVs);
            for (int frame = 0; frame < md2.numFrames; frame++)
            {
                tempFrame = (Frame)md2.frameList[frame];
                for (int j = 0; j < md2.numUVs; j++)
                {
                    output.Write(tempFrame.UV[j].X);
                    output.Write(tempFrame.UV[j].Y);
                }
            }

            // store normal data for all frames
            for (int frame = 0; frame < md2.numFrames; frame++)
            {
                tempFrame = (Frame)md2.frameList[frame];
                for (int j = 0; j < tempFrame.numVerts; j++)
                {
                    output.Write(tempFrame.normal[j].X);
                    output.Write(tempFrame.normal[j].Y);
                    output.Write(tempFrame.normal[j].Z);
                }
            }

            // store animation data
            output.Write(md2.numAnim);
            for (int i = 0; i < md2.numAnim; i++)
            {
                output.Write(md2.startFrame[i]);
                output.Write(md2.endFrame[i]);
            }
        }
        // Sets the CLR data type to be loaded at runtime.
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "MD2Runtime.MD2Reader, MD2Model, Version=1.0.0.0, Culture=neutral";
        }
        // specifies where to find the reader for reading from the .xnb file
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "MD2Runtime.MD2Reader, MD2Runtime, Version=1.0.0.0, Culture=neutral";

        }
    }
}
