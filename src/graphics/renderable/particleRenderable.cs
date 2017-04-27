using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public abstract class ParticleRenderable : Renderable
   {
      public ParticleRenderable() : base()
      {
         myType = "particle";
      }

      public override bool isVisible(Camera c)
      {
         return true;
      }
   }
}