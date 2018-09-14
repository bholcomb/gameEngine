using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class SkinnedModelRenderable : Renderable
   {
      public SkinnedModel model;
      public UniformBufferObject mySkinningBuffer = new UniformBufferObject(BufferUsageHint.DynamicDraw);

      public SkinnedModelRenderable()
         : base("skinnedModel")
      {
      }

      public override bool isVisible(Camera c)
      {
         return c.containsSphere(position, model.size);
      }
   }
}