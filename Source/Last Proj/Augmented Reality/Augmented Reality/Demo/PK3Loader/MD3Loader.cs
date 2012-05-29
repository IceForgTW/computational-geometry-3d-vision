#region File Description
//-----------------------------------------------------------------------------
// MD3Loader.cs version 0.1b
//
// Content Pipeline PK3Loader
//
// This Content use SharpZipLib (http://www.icsharpcode.net/OpenSource/SharpZipLib/)
//
// Created by Spear (http://www.codernet.es), spear@codernet.es
// Coordinator of XNA Community (http://www.codeplex.com/XNACommunity)
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using MD3Data;
#endregion

namespace PK3Loader
{
  public class CMD3Loader
  {
    // Info md3 header
    public struct MD3header
    {
      public string fileID;		//size(4) This stores the file ID - Must be "IDP3"
      public int version;					        // This stores the file version - Must be 15
      public string strFile;	//size(68) This stores the name of the file
      public int numFrames;					      // This stores the number of animation frames
      public int numTags;					        // This stores the tag count
      public int numMeshes;					      // This stores the number of sub-objects in the mesh
      public int numMaxSkins;			      	// This stores the number of skins for the mesh
      public int headerSize;			    		// This stores the mesh header size
      public int tagStart;				    	  // This stores the offset into the file for tags
      public int tagEnd;				    		  // This stores the end offset into the file for tags
      public int fileSize;			    		  // This stores the file size
    };

    //[StructLayout(LayoutKind.Sequential, Pack=1)] 
    public struct MD3bone // bounding box
    {
      public Vector3 mins;					// This is the min (x, y, z) value for the bone
      public Vector3 maxs;					// This is the max (x, y, z) value for the bone
      public Vector3 position;			// This supposedly stores the bone position???
      public float scale;						            // This stores the scale of the bone
      public string creator;				// The modeler used to create the model (I.E. "3DS Max")
    };

    public List<MD3SubMeshes> sub_meshes;
    public MD3header header;
    public List<MD3bone> bones;
    public List<MD3tag> tag;

    public enum TMD3Part
    {
      HEAD,
      UPPER,
      LOWER
    };
    public TMD3Part md3_type = TMD3Part.HEAD;

    #region Read MD3 Data
    public void loadMD3(MemoryStream stream)
    {
      BinaryReader reader = new BinaryReader(stream);
      System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
      //System.Diagnostics.Debugger.Launch();

      header = new MD3header(); 
      header.fileID = getText(enc.GetString(reader.ReadBytes(4)));
      header.version = reader.ReadInt32();
      header.strFile = getText(enc.GetString(reader.ReadBytes(68)));
      header.numFrames = reader.ReadInt32();
      header.numTags = reader.ReadInt32();
      header.numMeshes = reader.ReadInt32();
      header.numMaxSkins = reader.ReadInt32();
      header.headerSize = reader.ReadInt32();
      header.tagStart = reader.ReadInt32();
      header.tagEnd = reader.ReadInt32();
      header.fileSize = reader.ReadInt32();

      bones = new List<MD3bone>();
      for (int i = 0; i < header.numFrames; ++i)
      {
        MD3bone bone = new MD3bone();
        bone.mins.X = reader.ReadSingle();
        bone.mins.Y = reader.ReadSingle();
        bone.mins.Z = reader.ReadSingle();
        bone.maxs.X = reader.ReadSingle();
        bone.maxs.Y = reader.ReadSingle();
        bone.maxs.Z = reader.ReadSingle();
        bone.position.X = reader.ReadSingle();
        bone.position.Y = reader.ReadSingle();
        bone.position.Z = reader.ReadSingle();
        bone.scale = reader.ReadSingle();
        bone.creator = getText(enc.GetString(reader.ReadBytes(16)));
        bones.Add(bone);
      }

      tag = new List<MD3tag>();
      for (int i = 0; i < header.numFrames * header.numTags; ++i)
      {
        MD3tag data_tag= new MD3tag();
        data_tag.strName = getText(enc.GetString(reader.ReadBytes(64)));
        data_tag.vPosition.X = reader.ReadSingle();
        data_tag.vPosition.Y = reader.ReadSingle();
        data_tag.vPosition.Z = reader.ReadSingle();
        data_tag.rotation = Matrix.Identity;
        data_tag.rotation.M11 = reader.ReadSingle();
        data_tag.rotation.M12 = reader.ReadSingle();
        data_tag.rotation.M13 = reader.ReadSingle();
        data_tag.rotation.M21 = reader.ReadSingle();
        data_tag.rotation.M22 = reader.ReadSingle();
        data_tag.rotation.M23 = reader.ReadSingle();
        data_tag.rotation.M31 = reader.ReadSingle();
        data_tag.rotation.M32 = reader.ReadSingle();
        data_tag.rotation.M33 = reader.ReadSingle();
        tag.Add(data_tag);
      }

      long mesh_offset = reader.BaseStream.Position;
      sub_meshes = new List<MD3SubMeshes>();
      for (int i = 0; i < header.numMeshes; ++i)
      {
        reader.BaseStream.Seek(mesh_offset, SeekOrigin.Begin);
        //read mesh info
        MD3SubMeshes sub_mesh = new MD3SubMeshes();
        sub_mesh.meshinfo = new MD3meshinfo();
        sub_mesh.meshinfo.meshID = getText(enc.GetString(reader.ReadBytes(4)));
        sub_mesh.meshinfo.strName = getText(enc.GetString(reader.ReadBytes(68)));
        sub_mesh.meshinfo.numMeshFrames = reader.ReadInt32();
        sub_mesh.meshinfo.numSkins = reader.ReadInt32();
        sub_mesh.meshinfo.numVertices = reader.ReadInt32();
        sub_mesh.meshinfo.numTriangles = reader.ReadInt32();
        sub_mesh.meshinfo.triStart = reader.ReadInt32();
        sub_mesh.meshinfo.headerSize = reader.ReadInt32();
        sub_mesh.meshinfo.uvStart = reader.ReadInt32();
        sub_mesh.meshinfo.vertexStart = reader.ReadInt32();
        sub_mesh.meshinfo.meshSize = reader.ReadInt32();
        //Skin

        int size = sub_mesh.meshinfo.numSkins;
        sub_mesh.skins = new List<string>();
        for (int a = 0; a < size; ++a)
        {
          sub_mesh.skins.Add( getText(enc.GetString(reader.ReadBytes(68))));
        }

        //Faces
        size = sub_mesh.meshinfo.numTriangles * 3;
        sub_mesh.indices = new List<int>();
        reader.BaseStream.Seek(mesh_offset + sub_mesh.meshinfo.triStart, SeekOrigin.Begin);
        for (int a = 0; a < size; ++a)
        {
          sub_mesh.indices.Add(reader.ReadInt32());
        }

        //Vertex Texture
        size = sub_mesh.meshinfo.numVertices;
        sub_mesh.text_coord = new List<Vector2>();
        reader.BaseStream.Seek(mesh_offset + sub_mesh.meshinfo.uvStart, SeekOrigin.Begin);
        for (int a = 0; a < size; ++a)
        {
          Vector2 text_coord;
          text_coord.X = reader.ReadSingle();
          text_coord.Y = reader.ReadSingle();
          sub_mesh.text_coord.Add(text_coord);
        }

        //Vertex
        size = sub_mesh.meshinfo.numVertices * sub_mesh.meshinfo.numMeshFrames;
        sub_mesh.vertices = new List<Vector3>();
        sub_mesh.normals = new List<Vector3>();
        reader.BaseStream.Seek(mesh_offset + sub_mesh.meshinfo.vertexStart, SeekOrigin.Begin);
        for (int a = 0; a < size; ++a)
        {
          Vector3 vertex;
          vertex.X = reader.ReadInt16() / 64.0f;
          vertex.Y = reader.ReadInt16() / 64.0f;
          vertex.Z = reader.ReadInt16() / 64.0f;
          sub_mesh.vertices.Add(vertex);
          Vector3 normal;
          float lng = reader.ReadByte() / 255.0f * MathHelper.TwoPi;
          float lat = reader.ReadByte() / 255.0f * MathHelper.TwoPi;
          normal.X = (float)(Math.Cos(lat) * Math.Sin(lng));
          normal.Y = (float)(Math.Sin(lat) * Math.Sin(lng));
          normal.Z = (float)Math.Cos(lng);
          normal.Normalize();
          sub_mesh.normals.Add(normal);
        }

        mesh_offset += sub_mesh.meshinfo.meshSize;
        sub_meshes.Add(sub_mesh);
      }
      reader.Close();
      stream.Close();
    }
    #endregion

    private string getText(string text)
    {
      int index = text.IndexOf("\0");
      if (index > 0)
        text = text.Substring(0, index);
      else if (index == 0)
        text = "";

      text = XmlConvert.EncodeLocalName(text);
      return text;
    }

    #region Write Data
    public void write(ContentWriter output, CPK3Loader pk3)
    {
      int t = 0;
      if (md3_type == TMD3Part.LOWER)
        t = 1;
      else if (md3_type == TMD3Part.UPPER)
        t = 2;
      output.Write(t);
      output.Write(header.numFrames);
      output.Write(sub_meshes.Count);
      for (int i = 0; i < sub_meshes.Count; ++i) {
        string texture_name = pk3.getMaterialByMeshName(sub_meshes[i].meshinfo.strName);
        output.Write(texture_name);
        output.WriteObject(sub_meshes[i].indices);
        output.WriteObject(sub_meshes[i].vertices);
        output.WriteObject(sub_meshes[i].normals);
        output.WriteObject(sub_meshes[i].text_coord);
        //falta mesh info
      }
      output.Write(tag.Count);
      for (int i = 0; i < tag.Count; ++i)
      {
        output.Write(tag[i].strName);
        output.Write(tag[i].vPosition);
        output.Write(tag[i].rotation);
      }
      output.Write(bones.Count);
      for (int i = 0; i < bones.Count; ++i)
      {
        output.Write(bones[i].mins);
        output.Write(bones[i].maxs);
      }
    }
    #endregion

  }
}
