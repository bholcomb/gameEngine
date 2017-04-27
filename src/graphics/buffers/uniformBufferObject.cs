using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class UniformBufferObject : BufferObject
   {
      int mySlot;

      public UniformBufferObject(BufferUsageHint hint)
         : base(BufferTarget.UniformBuffer, hint)
      {
      }

      public void setBufferBindPoint(int slot)
      {
         mySlot = slot;
      }
	}
}
