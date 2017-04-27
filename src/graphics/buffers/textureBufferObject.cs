using System;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics 
{
   public class TextureBufferSampler : TextureSampler
   {
      TextureBufferObject myTbo;

      public TextureBufferSampler(TextureBufferObject tbo, int unit)
         : base(null, unit)
      {
         myTbo = tbo;
      }

      public override void bind()
      {
         GL.ActiveTexture(TextureUnit.Texture0 + myUnit);
         myTbo.bind();
      }

      public override void unbind()
      {
         GL.ActiveTexture(TextureUnit.Texture0 + myUnit);
         myTbo.unbind();
      }

      public override Int32 id()
      {
         return myTbo.textureId;
      }
   }


   //class used for non image data (vertex and stuff) for typically sampling in vertex shader
   public class TextureBufferObject : BufferObject
   {
      SizedInternalFormat myInternalFormat;
      int myTextureId;

      public TextureBufferObject(SizedInternalFormat readAs, BufferUsageHint hint)
         : base(BufferTarget.TextureBuffer, hint)
      {
         myInternalFormat = readAs;

         //create a texture
         GL.GenTextures(1, out myTextureId);
      }

      public new void Dispose()
      {
         GL.DeleteBuffer(myId);
         GL.DeleteTexture(myTextureId);
      }

      public override void setData<T>(T[] bufferInMemory)
      {
         base.setData<T>(bufferInMemory);

         //associate the texture with this buffer
         GL.BindTexture(TextureTarget.TextureBuffer, myTextureId);
         GL.TexBuffer(TextureBufferTarget.TextureBuffer, myInternalFormat, myId);
      }

      public override void setData<T>(T[] bufferInMemory, int offset, int numBytes)
      {
         base.setData<T>(bufferInMemory, offset, numBytes);

         //associate the texture with this buffer
         GL.BindTexture(TextureTarget.TextureBuffer, myTextureId);
         GL.TexBuffer(TextureBufferTarget.TextureBuffer, myInternalFormat, myId);
      }

      public override void bind()
      {
         GL.BindTexture(TextureTarget.TextureBuffer, myTextureId);
      }

      public override void unbind()
      {
         GL.BindTexture(TextureTarget.TextureBuffer, 0);
      }

      public void bindBuffer()
      {
         GL.BindBuffer(BufferTarget.TextureBuffer, myId);
      }

      public int textureId
      {
         get { return myTextureId; }
      }
   }
}