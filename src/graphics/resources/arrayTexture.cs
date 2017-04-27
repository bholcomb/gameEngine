using System;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics 
{
   public class ArrayTextureDescriptor : ResourceDescriptor
   {
      bool myFlip = false;
      String[] myPaths;

      public ArrayTextureDescriptor(string[] paths) : this(paths, false) { }
      public ArrayTextureDescriptor(string[] paths, bool flip)
         : base(paths.GetHashCode().ToString())
      {
         myPaths=paths;
         myFlip = flip;
      }

      public override IResource create(ResourceManager mgr)
      {
         ArrayTexture t = new ArrayTexture(myPaths, myFlip);
         if (t.isValid == false)
         {
            return null;
         }
         return t;
      }
   }

   public class ArrayTexture : Texture
   {
      public string[] myFilenames;

      public ArrayTexture(String[] filenames, bool flip)
         : base()
      {
         target = TextureTarget.Texture2DArray;
         myFlip = flip;

         myFilenames = filenames;

         if (LoadFromDisk() == false)
         {
            GL.DeleteTexture(myId);
            myId = 0;
            System.Console.WriteLine("Cannot create texture from Texture Array: {0}", filenames);
         }
      }

      public bool LoadFromDisk()
      {
         //GL.Enable(EnableCap.Texture3DExt);
         //GL.Hint(HintTarget.TextureCompressionHint, HintMode.Nicest);
         GL.GenTextures(1, out myId);
			bind();

         for (int i = 0; i < myFilenames.Length; i++)
         {
            OpenTK.Graphics.OpenGL.PixelInternalFormat pif;
            OpenTK.Graphics.OpenGL.PixelFormat pf;
            OpenTK.Graphics.OpenGL.PixelType pt;

            System.Console.Write("Loading Texture Array {0}-{1} : with {2}...", myId, i, myFilenames[i]);

            Bitmap bm = loadBitmap(myFilenames[i], out pif, out pf, out pt);
            if (bm != null)
            {
               //load the first image to determine size of all the images
               BitmapData Data = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);

               //first time through, create a 3d texture big enough for all the images
               if (i == 0)
               {
                  GL.TexImage3D(target, 0, pif, Data.Width, Data.Height, myFilenames.Length, 0, pf, pt, IntPtr.Zero);
                  //GL.TexImage3D(target, 0, PixelInternalFormat.CompressedRgba, Data.Width, Data.Height, myFilenames.Length, 0, pf, pt, IntPtr.Zero);
               }

               GL.TexSubImage3D(target, 0, 0, 0, i, Data.Width, Data.Height, 1, pf, pt, Data.Scan0);

               bm.UnlockBits(Data);
            }

            System.Console.WriteLine("Done!");
         }

         GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

			unbind();

			return true; // success
      }
   }
}