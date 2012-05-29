#region File Description
//-----------------------------------------------------------------------------
// PK3Loader.cs version 0.1b
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
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using MD3Data;
#endregion

namespace PK3Loader
{

  public class CPK3Loader 
  {
    public class TextureInfo
    {
      public string mesh_name;
      public string texture_name;
    };

    public class SkinInfo
    {
      public List<TextureInfo> textures_info;
    };

    Dictionary<string, SkinInfo> skin_list;
    private List<CMD3Loader> md3_list;

    private Dictionary<string, ExternalReference<TextureContent>> materials_list;
    private Dictionary<string, string> skin_default;
    private List<MD3animation> animations_list;

    private string path;

    private int torso_frames; //animation offset, for legs

    #region Read File PK3
    public void loadPK3(string filename)
    {
      //System.Diagnostics.Debugger.Launch();
      md3_list = new List<CMD3Loader>();
      skin_list = new Dictionary<string, SkinInfo>();
      materials_list = new Dictionary<string, ExternalReference<TextureContent>>();
      skin_default = new Dictionary<string, string>();
      animations_list = new List<MD3animation>();

  		ZipInputStream s = new ZipInputStream(File.OpenRead(filename));

      path = Path.GetDirectoryName(filename);
      path = path + "/textures/" + Path.GetFileNameWithoutExtension(filename);
      Directory.CreateDirectory(path);
 			ZipEntry theEntry;
      string entry_name;
      string directory_md3 = "";
			while ((theEntry = s.GetNextEntry()) != null) {
 				Console.WriteLine("Name : {0}", theEntry.Name);
				Console.WriteLine("Date : {0}", theEntry.DateTime);
				Console.WriteLine("Size : (-1, if the size information is in the footer)");
				Console.WriteLine("      Uncompressed : {0}", theEntry.Size);
				Console.WriteLine("      Compressed   : {0}", theEntry.CompressedSize);
        entry_name = theEntry.Name.ToLower();
        string extension = Path.GetExtension(entry_name);
        if (extension == ".md3")
        {
          if (directory_md3 == "")
            directory_md3 = Path.GetDirectoryName(entry_name);
          else if (Path.GetDirectoryName(entry_name) != directory_md3) //more models to pk3, not suported
            break;
          //read md3 stream
          MemoryStream stream = readData(s, theEntry.Size);
          CMD3Loader md3 = new CMD3Loader();
          string file_type = Path.GetFileNameWithoutExtension(entry_name);
          if (file_type.IndexOf("head") >= 0 ) 
            md3.md3_type = CMD3Loader.TMD3Part.HEAD;
          else if (file_type.IndexOf("upper") >= 0)
            md3.md3_type = CMD3Loader.TMD3Part.UPPER;
          else
            md3.md3_type = CMD3Loader.TMD3Part.LOWER;
          md3.loadMD3(stream); 
          md3_list.Add(md3);
        }
        else if (extension == ".skin")
        {
          if (directory_md3 == "")
            directory_md3 = Path.GetDirectoryName(entry_name);
          else if (Path.GetDirectoryName(entry_name) != directory_md3) //more models to pk3, not suported
            break;

          MemoryStream stream = readData(s, theEntry.Size);
          string key = Path.GetFileNameWithoutExtension(entry_name);
          loadSkin(key,stream);
        }
        else if (extension == ".cfg")
        {
          if (directory_md3 == "")
            directory_md3 = Path.GetDirectoryName(entry_name);
          else if (Path.GetDirectoryName(entry_name) != directory_md3) //more models to pk3, not suported
            break;
          //animacion
          MemoryStream stream = readData(s, theEntry.Size);
          loadAnimation(stream);
        }
        else if (extension == ".jpg" || extension == ".tga" || extension == ".bmp" || extension == ".png")
        {
          if (directory_md3 == "")
            directory_md3 = Path.GetDirectoryName(entry_name);
          else if (Path.GetDirectoryName(entry_name) != directory_md3) //more models to pk3, not suported
            break;
          unZipFile(path, s, theEntry);
        }
      }

      loadMaterials(path);
    }
    #endregion

    #region Animation Region
    private void loadAnimation(MemoryStream stream)
    {
      torso_frames = 0;
      StreamReader reader = new StreamReader(stream);
      while (!reader.EndOfStream)
      {
        string line_text = reader.ReadLine();
        if (line_text.Length > 0)
        {
          char[] c = line_text.Substring(0, 1).ToCharArray();
          if (Char.IsNumber(c[0]))
          {
            //info a animation, found
            char[] separator = { ' ','\t' };
            string[] s = line_text.Split(separator);
            addNewAnimation(s);
          }
        }
      }
      reader.Close();
      stream.Close();
    }

    private void addNewAnimation(string[] s) 
    {
      MD3animation anim = new MD3animation();
      int count = 0;
      for (int i = 0; i < s.Length; ++i) 
      {
        if (s[i].Length > 0)
        {
          int result;
          if (int.TryParse(s[i], out result))
          {
            switch (count)
            {
              case 0:
                anim.startFrame = result;
                break;
              case 1:
                anim.numFrames = result;
                break;
              case 2:
                anim.loopingFrames = result;
                break;
              case 3:
                anim.framesPerSecond = result;
                break;
            }
            ++count;
          }
          else if (count > 3 && s[i].Length > 4 )
          {
            anim.strName = s[i];
            if (anim.strName.IndexOf("TORSO") >= 0)
            {
              torso_frames += anim.numFrames;
            }
            else if (anim.strName.IndexOf("LEGS") >= 0)
            {
              anim.startFrame -= torso_frames;
            }
            break;
          }
        }
      }
      animations_list.Add(anim);
    }
    #endregion

    private void unZipFile(string path, ZipInputStream s, ZipEntry theEntry) 
    {
      string filename = path + "/" + Path.GetFileName(theEntry.Name);
      FileStream streamWriter = File.Create(filename);

      int size = 2048;
      byte[] data = new byte[2048];
      while (true)
      {
        size = s.Read(data, 0, data.Length);
        if (size > 0)
        {
          streamWriter.Write(data, 0, size);
        }
        else
        {
          break;
        }
      }
      streamWriter.Close();
    }

    private MemoryStream readData(ZipInputStream s, long fsize)
    {
      byte[] data = new byte[fsize];
      int size = s.Read(data, 0, data.Length);
      while (size > 0)
      {
        Console.Write(Encoding.ASCII.GetString(data, 0, size));
        size = s.Read(data, 0, data.Length);
      }
      MemoryStream stream = new MemoryStream(data);
      return stream;
    }


    public void loadSkin(string key, MemoryStream stream)
    {
      if (skin_list.ContainsKey(key) == false)
      {
        //System.Diagnostics.Debugger.Launch();
        StreamReader reader = new StreamReader(stream);
        string text_file = reader.ReadToEnd();
        reader.Close();
        stream.Close();

        char[] separator = { ',', '\n' };
        string[] string_parts = text_file.Split(separator);
        //push textures info
        SkinInfo skin_info = new SkinInfo();
        skin_info.textures_info = new List<TextureInfo>();
        for (int i = 0; i + 1 < string_parts.Length; i += 2)
        {
          TextureInfo t_info = new TextureInfo();
          t_info.mesh_name = string_parts[i].Replace('\r', ' ');
          t_info.texture_name = string_parts[i + 1].Replace('\r', ' ');
          //System.Diagnostics.Debugger.Launch();
          t_info.texture_name = t_info.texture_name.Trim();

          if (t_info.texture_name.Length > 1 && t_info.mesh_name.Length > 1)
          {
            t_info.texture_name = Path.GetFileName(t_info.texture_name).Trim();
            skin_info.textures_info.Add(t_info);
          }
        }
        skin_list.Add(key, skin_info);
      }
    }

    #region Textures Region
    private void loadMaterials(string path)
    {
      Dictionary<string, SkinInfo>.Enumerator skins = skin_list.GetEnumerator();
      while (skins.MoveNext()) 
      {
        string key = skins.Current.Key;
        if (key.IndexOf("_default")>=0)
        {
          //load default skin (textures)
          SkinInfo s = skins.Current.Value;
          for (int i = 0; i < s.textures_info.Count; ++i)
          {
            string material_key = Path.GetFileNameWithoutExtension(s.textures_info[i].texture_name);
            if (materials_list.ContainsKey(material_key) == false)
            {
              string filename = path + "/" + s.textures_info[i].texture_name;
              if (File.Exists(filename))
              {
                ContentIdentity mtl_file_identity = new ContentIdentity(filename);

                ExternalReference<TextureContent> Texture = new ExternalReference<TextureContent>(
                    s.textures_info[i].texture_name, mtl_file_identity);

                Texture.Name = material_key;

                materials_list.Add(material_key, Texture);
              }
            }
            if (skin_default.ContainsKey(s.textures_info[i].mesh_name) == false)
            {
              skin_default.Add(s.textures_info[i].mesh_name, material_key);
            }
          }
        }
      }
    }

    public string getMaterialByMeshName(string name)
    {
      string texture_name = " ";
      if (skin_default.ContainsKey(name))
      {
        texture_name = skin_default[name];
      }
      else
      {
        Dictionary<string,string>.Enumerator enumerator = skin_default.GetEnumerator();
        if (enumerator.MoveNext())
          texture_name = enumerator.Current.Value;
      }
      return texture_name;
    }

    List<ExternalReference<TextureContent>> built_textures;

    public void buildTextures(ContentProcessorContext context) 
    {
      Dictionary<string, ExternalReference<TextureContent>>.Enumerator e = materials_list.GetEnumerator();
      built_textures = new List<ExternalReference<TextureContent>>();
      while (e.MoveNext())
      {
        ExternalReference<TextureContent> t = e.Current.Value;
        ExternalReference<TextureContent> builtTexture = context.BuildAsset<TextureContent, TextureContent>(t, "TextureProcessor");
        builtTexture.Name = e.Current.Key;
        built_textures.Add(builtTexture);
      }
    }

    #endregion

    #region Write Data
    private void writeTextures(ContentWriter output)
    {
      output.Write(built_textures.Count);
      for (int i = 0; i < built_textures.Count; ++i)
      {
        output.Write(built_textures[i].Name);
        output.WriteExternalReference<TextureContent>(built_textures[i]);
      }
    }

    public void write(ContentWriter output)
    {
      writeTextures(output);
      output.Write(md3_list.Count);
      for (int i = 0; i < md3_list.Count; ++i)
      {
        md3_list[i].write(output,this);
      }
      output.Write(animations_list.Count);
      for (int i = 0; i < animations_list.Count; ++i)
      {
        output.Write(animations_list[i].strName);
        output.Write(animations_list[i].startFrame);
        output.Write(animations_list[i].numFrames);
        output.Write(animations_list[i].loopingFrames);
        output.Write(animations_list[i].framesPerSecond);
      }
    }
    #endregion
  }
}
