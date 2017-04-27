using System;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class IndexBufferObject : BufferObject
   {
      public enum IndexBufferDatatype
      {
         UnsignedByte,
         UnsignedShort,
         UnsignedInt
      }

      protected IndexBufferDatatype myDataType;

      public IndexBufferObject(BufferUsageHint hint)
         : base(BufferTarget.ElementArrayBuffer, hint)
      {
      }

      public IndexBufferDatatype type
      {
         get { return myDataType; }
      }

      public override void setData<T>(T[] bufferInSystemMemory, int destinationOffsetInBytes, int lengthInBytes)
      {
         if (typeof(T) == typeof(byte))
         {
            myDataType = IndexBufferDatatype.UnsignedByte;
            count = lengthInBytes / 1;
         }
         else if (typeof(T) == typeof(ushort))
         {
            myDataType = IndexBufferDatatype.UnsignedShort;
            count = lengthInBytes / 2;
         }
         else if (typeof(T) == typeof(uint))
         {
            myDataType = IndexBufferDatatype.UnsignedInt;
            count = lengthInBytes / 4;
         }
         else
         {
            throw new ArgumentException("bufferInSystemMemory must be an array of byte, ushort or uint.", "bufferInSystemMemory");
         }


         base.setData(bufferInSystemMemory, destinationOffsetInBytes, lengthInBytes);
      }

   }
}
