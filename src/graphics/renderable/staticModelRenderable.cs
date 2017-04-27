using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class StaticModelRenderable : Renderable
   {
      public StaticModel model;

      public StaticModelRenderable() : base()
      {
         myType = "staticModel";
      }

      public override bool isVisible(Camera c)
      {
         return c.containsSphere(position, model.size);
      }
   }
}