using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class SkyboxRenderable : Renderable
   {
      public SkyBox model;

      public SkyboxRenderable() : base()
      {
         myType = "skybox";
      }

      public override bool isVisible(Camera c)
      {
         return true;
      }
   }
}