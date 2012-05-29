#region File Description
//-----------------------------------------------------------------------------
// CReaderPK3.cs version 0.1b
//
// Reader CPK3Model
//
// Created by Spear (http://www.codernet.es), spear@codernet.es
// Coordinator of XNA Community (http://www.codeplex.com/XNACommunity)
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace PK3Model
{
  public class CReaderPK3 : ContentTypeReader<CPK3Model>
  {
    protected override CPK3Model Read(ContentReader input, CPK3Model existingInstance)
    {
      CPK3Model model = new CPK3Model();
      model.load(input);
      return model;
    }
  }
}
