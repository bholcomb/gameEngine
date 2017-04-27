using System;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics 
{

   //class used for non image data used in compute shaders
   public class ShaderStorageBufferObject : BufferObject
   {
      int mySlot;

      public ShaderStorageBufferObject(BufferUsageHint hint)
         : base(BufferTarget.ShaderStorageBuffer, hint)
      {
      }

      public void setBufferBindPoint(int slot)
      {
         mySlot = slot;
      }
   }
}