using System;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class PersistentBufferObject : BufferObject
   {
      IntPtr myPtr;
      UInt32 mySize;

      public PersistentBufferObject(BufferUsageHint hint, UInt32 size)
         : base(BufferTarget.ArrayBuffer,  hint)
      {
         mySize = size;
         bind();
         BufferStorageFlags flags = BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit;
         GL.BufferStorage(BufferTarget.ArrayBuffer, new IntPtr(size), IntPtr.Zero, flags);
         BufferAccessMask flags2 = BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit;
         myPtr = GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, new IntPtr(size), flags2);
         unbind();
      }

      public IntPtr ptr { get { return myPtr; } }

      public UInt32 size { get { return mySize; } }

   }
}
