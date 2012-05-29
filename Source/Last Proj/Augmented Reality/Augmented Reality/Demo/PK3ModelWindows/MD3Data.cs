#region File Description
//-----------------------------------------------------------------------------
// MD3Data.cs version 0.1b
//
// MD3 Generic structs
//
// Created by Spear (http://www.codernet.es), spear@codernet.es
// Coordinator of XNA Community (http://www.codeplex.com/XNACommunity)
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace MD3Data
{

  #region Meshes Info
  public struct MD3meshinfo  //MeshInfo, esto es por mesh.
  {
    public string meshID;			              //size(4) This stores the mesh ID (We don't care)
    public string strName;		              //size(68) This stores the mesh name (We do care)
    public int numMeshFrames;				        // This stores the mesh aniamtion frame count
    public int numSkins;					          // This stores the mesh skin count
    public int numVertices;				          // This stores the mesh vertex count
    public int numTriangles;				        // This stores the mesh face count
    public int triStart;					          // This stores the starting offset for the triangles
    public int headerSize;					        // This stores the header size for the mesh
    public int uvStart;					            // This stores the starting offset for the UV coordinates
    public int vertexStart;				          // This stores the starting offset for the vertex indices
    public int meshSize;					          // This stores the total mesh size
  };


  public struct MD3SubMeshes
  {
    public MD3meshinfo meshinfo;
    // This stores the indices into the vertex and texture coordinate arrays
    public List<int> indices;
    public List<Vector3> vertices;
    // This stores UV coordinates
    public List<Vector2> text_coord;
    public List<Vector3> normals;   //normals
    public List<string> skins; //size 68
  };

  #endregion

  #region Tag Info
  public struct MD3tag
  {
    public string strName;		  //size 64 This stores the name of the tag (I.E. "tag_torso")
    public Vector3 vPosition;	  //size 3 This stores the translation that should be performed
    public Matrix rotation;     // This stores the 3x3 rotation matrix for this frame
  };
  #endregion

  #region Animation Info
  public class MD3animation
  {
    public string strName;		                // This stores the name of the animation (I.E. "TORSO_STAND")
    public int startFrame;				            // This stores the first frame number for this animation
    public int numFrames;				              // This stores the last frame number for this animation
    public int loopingFrames;			          // This stores the looping frames for this animation (not used)
    public int framesPerSecond;		          // This stores the frames per second that this animation runs
  };
  #endregion


}