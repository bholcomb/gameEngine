using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class PixelBufferObject : BufferObject
   {
      protected int myWidth;
      protected int myHeight;
      protected OpenTK.Graphics.OpenGL.PixelInternalFormat myInternalForamt;

      public PixelBufferObject(BufferUsageHint hint)
         : base(BufferTarget.TextureBuffer, hint)
      {
      }

      public void setSize(int x, int y, PixelInternalFormat format)
      {
         myWidth = x;
         myHeight = y;

         switch (format)
         {
            case PixelInternalFormat.Rgb8:
            case PixelInternalFormat.Rgb:
               {
                  resize(myWidth * myHeight * 3);
               }
               break;
            case PixelInternalFormat.Rgba8:
            case PixelInternalFormat.Rgba:
               {
                  resize(myWidth * myHeight * 4);
               }
               break;
            case PixelInternalFormat.Luminance:
               {
                  resize(myWidth * myHeight * 1);
               }
               break;
            case PixelInternalFormat.Rgb32f:
               {
                  resize(myWidth * myHeight * 3 * 4);
               }
               break;
            case PixelInternalFormat.Rgba32f:
               {
                  resize(myWidth * myHeight * 4 * 4);
               }
               break;
         }
      }

      /*
      public void copyFromBitmap(Bitmap bitmap)
      {
         
         Bitmap lockedBitmap;
         if (BitmapAlgorithms.RowOrder(bitmap) == ImageRowOrder.TopToBottom)
         {
            //
            // OpenGL wants rows bottom to top.
            //
            Bitmap flippedBitmap = (Bitmap)bitmap.Clone();
            flippedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            lockedBitmap = flippedBitmap;
         }
         else
         {
            lockedBitmap = bitmap;
         }

         BitmapData lockedPixels = lockedBitmap.LockBits(new Rectangle(
             0, 0, lockedBitmap.Width, lockedBitmap.Height),
             ImageLockMode.ReadOnly, lockedBitmap.PixelFormat);

         sizeInBytes = lockedPixels.Stride * lockedPixels.Height;

         bind();
         GL.BufferSubData(myBufferTarget, new IntPtr(), new IntPtr(sizeInBytes), lockedPixels.Scan0);
         unbind();

         lockedBitmap.UnlockBits(lockedPixels);
         
      }

      public Bitmap copyToBitmap()
      {
      }
      */
   }
}
