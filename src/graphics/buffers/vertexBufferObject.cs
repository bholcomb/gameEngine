using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{     
   public abstract class BaseVertexBufferObject : BufferObject
   {
      public BaseVertexBufferObject(BufferUsageHint hint)
         : base(BufferTarget.ArrayBuffer, hint)
      {
      }

      public abstract Type GetVertexType();
   }

   public class VertexBufferObject : BufferObject
   {
      public VertexBufferObject(BufferUsageHint hint)
         : base(BufferTarget.ArrayBuffer, hint)
      {
      }
   }
}
