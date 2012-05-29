using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace MD2Runtime
{

    public class MD2Model
    {
        // these variables store values that are accessible in game class
        public int          numFaces;
        public int          numFrames;
        public int          numBytes;
        public int[,,]      positionIndex;
        public int[,,]      frameUVIndex;
        public int          numVerts, numUVs;
        public float[,,]    framePosition;// = new float[numFrames, numVerts, 3];
        public float[,,]    UV;
        public float[,,]    normal;
        public int          numAnim;
        public int[]        startFrame;
        public int[]        endFrame;

        // reads from XNB file
        internal MD2Model(ContentReader cr)
        {
            int size        = cr.ReadInt32();
            numBytes        = size;
            numFrames       = cr.ReadInt32();
            numFaces        = cr.ReadInt32();
            positionIndex   = new int[numFrames, numFaces, 3];
            frameUVIndex    = new int[numFrames, numFaces, 3];
            
            // read position and UV indices
            for (int frameNum = 0; frameNum < numFrames; frameNum++)
            {
                for (int tri = 0; tri < numFaces; tri++)
                {
                    positionIndex[frameNum, tri, 0] = cr.ReadInt32();
                    positionIndex[frameNum, tri, 1] = cr.ReadInt32();
                    positionIndex[frameNum, tri, 2] = cr.ReadInt32();
                    frameUVIndex[frameNum, tri, 0]  = cr.ReadInt32();
                    frameUVIndex[frameNum, tri, 1]  = cr.ReadInt32();
                    frameUVIndex[frameNum, tri, 2]  = cr.ReadInt32();
                }
            }

            // read in the position coordinates
            numVerts        = cr.ReadInt32();
            framePosition   = new float[numFrames, numVerts, 3];

            for (int frameNum = 0; frameNum < numFrames; frameNum++)
            {
                for (int vert = 0; vert < numVerts; vert++)
                {
                    framePosition[frameNum, vert, 0] = cr.ReadSingle();
                    framePosition[frameNum, vert, 1] = cr.ReadSingle();
                    framePosition[frameNum, vert, 2] = cr.ReadSingle();
                }
            }

            // read texture coordinates
            numUVs          = cr.ReadInt32();
            UV              = new float[numFrames, numUVs, 2];

            for (int frameNum = 0; frameNum < numFrames; frameNum++)
            {
                for (int iUV = 0; iUV < numUVs; iUV++)
                {
                    UV[frameNum, iUV, 0] = cr.ReadSingle();
                    UV[frameNum, iUV, 1] = cr.ReadSingle();
                }
            }

            // read normal vector data
            normal          = new float[numFrames, numVerts, 3];
            for (int frameNum = 0; frameNum < numFrames; frameNum++)
            {
                for (int vert = 0; vert < numVerts; vert++)
                {
                    normal[frameNum, vert, 0] = cr.ReadSingle();
                    normal[frameNum, vert, 1] = cr.ReadSingle();
                    normal[frameNum, vert, 2] = cr.ReadSingle();
                }
            }

            // read animation info
            numAnim         = cr.ReadInt32();
            startFrame      = new int[numAnim];
            endFrame        = new int[numAnim];

            // read start and end frames
            for (int i = 0; i < numAnim; i++)
            {
                startFrame[i] = cr.ReadInt32();
                endFrame[i]   = cr.ReadInt32();
            }
        }
    }


    // reads XNB file
    public class MD2Reader : ContentTypeReader<MD2Model>
    {
        protected override MD2Model Read(ContentReader input,
                                       MD2Model existingInstance)
        {
            return new MD2Model(input);
        }
    }
}
