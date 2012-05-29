#region File Description
//-----------------------------------------------------------------------------
// PK3Processor.cs version 0.1b
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
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace PK3Loader
{
  [ContentProcessor]
  class CPK3Processor : ContentProcessor<CPK3Loader,CPK3Loader>
  {
    public override CPK3Loader Process(CPK3Loader input, ContentProcessorContext context)
    {
      input.buildTextures(context);
      return input;
    }
  }

  [ContentImporter(".pk3", CacheImportedData = true, DefaultProcessor = "CPK3Processor")]
  public class CPK3Importer : ContentImporter<CPK3Loader>
  {
    
    public override CPK3Loader Import(string filename, ContentImporterContext context)
    {

      CPK3Loader loader = new CPK3Loader();
      loader.loadPK3(filename);
      return loader;
    }     
  }


  [ContentTypeWriter]
  public class CWriterPK3: ContentTypeWriter<CPK3Loader>
  {
    protected override void Write(ContentWriter output, CPK3Loader value)
    {
      value.write(output);
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "PK3Model.CReaderPK3, PK3Model, Version=1.0, Culture=neutral";
    }

    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(CPK3Loader).AssemblyQualifiedName;
    }
  }

}
