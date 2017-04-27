using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public abstract class BufferObject : IDisposable
   {
      protected Int32 myId;
      protected BufferTarget myBufferTarget;
      protected BufferUsageHint myBufferHint;

      public BufferObject(BufferTarget targetType, BufferUsageHint hint)
      {
         myBufferTarget = targetType;
         myBufferHint = hint;

         myId=GL.GenBuffer();
      }

      public void Dispose()
      {
         GL.DeleteBuffer(myId);
      }

      public Int32 id { get { return myId; } }

      public int sizeInBytes { get; set; }
      public int count { get; set; }

      public virtual void resize(int numBytes)
      {
         GL.BindBuffer(myBufferTarget, myId);
         GL.BufferData(myBufferTarget, numBytes, IntPtr.Zero, myBufferHint);
         GL.BindBuffer(myBufferTarget, 0);
         sizeInBytes = numBytes;
      }

		public virtual void setData(byte[] bytes)
		{
			int numBytes = bytes.Length;
			if (numBytes > sizeInBytes)
			{
				resize(numBytes);
			}

			bind();
			GL.BufferSubData(myBufferTarget, new IntPtr(0), numBytes, bytes);
			unbind();
		}

		public virtual void setData<T>(List<T> buffer) where T : struct
      {
         setData(buffer.ToArray());
      }

		public virtual void setData<T>(T bufferObject) where T: struct 
      {
         int numBytes = Marshal.SizeOf(bufferObject);
         if (numBytes > sizeInBytes)
         {
            resize(numBytes);
         }

         bind();
         GL.BufferSubData<T>(myBufferTarget, new IntPtr(0), new IntPtr(numBytes), ref bufferObject);
         unbind();
      }

      public virtual void setData<T>(List<T> buffer, int offsetInBytes) where T : struct
      {
         setData(buffer.ToArray(), offsetInBytes, buffer.Count * Marshal.SizeOf(typeof(T)));
      }

      public virtual void setData<T>(List<T> buffer, int offsetInBytes, int numBytes) where T : struct
      {
         setData(buffer.ToArray(), offsetInBytes, numBytes);
      }

      public virtual void setData<T>(T[] bufferInMemory) where T : struct
      {
         setData<T>(bufferInMemory, 0, ArraySizeInBytes(bufferInMemory));
      }

      public virtual void setData<T>(T[] bufferInMemory, int count) where T : struct
      {
         setData<T>(bufferInMemory, 0, count * Marshal.SizeOf(typeof(T)));
      }

      public virtual void setData<T>(T[] bufferInMemory, int offsetInBytes, int numBytes) where T : struct
      {
         //protect against the stupids
         if (offsetInBytes < 0)
         {
            throw new ArgumentOutOfRangeException("offsetInBytes", "offsetInBytes must be greater than or equal to zero.");
         }
         if (numBytes < 0)
         {
            throw new ArgumentOutOfRangeException("numBytes", "numBytes must be greater than or equal to zero");
         }

         if (offsetInBytes + numBytes > sizeInBytes)
         {
            resize(offsetInBytes + numBytes);
         }

         GL.BindBuffer(myBufferTarget, myId);
         GL.BufferSubData<T>(myBufferTarget, new IntPtr(offsetInBytes), new IntPtr(numBytes), bufferInMemory);
         GL.BindBuffer(myBufferTarget, 0);

         //update the count of the number of things in the buffer
         count = numBytes / Marshal.SizeOf(default(T));
      }

      public virtual T[] getData<T>() where T : struct
      {
         return getData<T>(0, sizeInBytes);
      }

      public virtual T[] getData<T>(int offsetInBytes, int numBytes) where T : struct
      {
         //protect against the stupids
         if (offsetInBytes < 0)
         {
            throw new ArgumentOutOfRangeException("offsetInBytes", "offsetInBytes must be greater than or equal to zero.");
         }
         if (numBytes < 0)
         {
            throw new ArgumentOutOfRangeException("numBytes", "numBytes must be greater than or equal to zero");
         }

         //create a new array
         T[] bufferInSystemMemory = new T[numBytes / Marshal.SizeOf(typeof(T))];

         GL.BindBuffer(myBufferTarget, myId);
         GL.GetBufferSubData(myBufferTarget, new IntPtr(offsetInBytes), new IntPtr(numBytes), bufferInSystemMemory);
         GL.BindBuffer(myBufferTarget, 0);

         return bufferInSystemMemory;
      }

      protected static int ArraySizeInBytes<T>(T[] buffer)
      {
         return buffer.Length * Marshal.SizeOf(typeof(T));
      }

      public virtual void bind()
      {
         GL.BindBuffer(myBufferTarget, myId);
      }

      public virtual void unbind()
      {
         GL.BindBuffer(myBufferTarget, 0);
      }
   }
}
