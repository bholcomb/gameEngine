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
      public Model model;

      public StaticModelRenderable() 
         : base("staticModel")
      {
      }

      public override bool isVisible(Camera c)
      {
         return c.containsSphere(position, model.size);
      }
   }
}