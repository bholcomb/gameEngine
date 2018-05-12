using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OGL = OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class CubemapTextureDescriptor : ResourceDescriptor
   {
      bool myFlip = false;
      List<String> myFaces;
      String myObjName = "";

      public CubemapTextureDescriptor(List<String> files, String objName=""):
         base(files.GetHashCode().ToString())
      {
         myFlip = false;
         myObjName = objName;
         myFaces = files;
      }

      public override IResource create(ResourceManager mgr)
      {
         CubemapTexture t = new CubemapTexture(myFaces, myFlip);
         if (t.isValid == false)
         {
            return null;
         }

         if (myObjName != "")
            t.setName(myObjName);

         return t;
      }

      public bool flip
      {
         get { return myFlip; }
         set { myFlip = value; }
      }
   }

   public class CubemapTexture : Texture
   {
      public TextureTarget[] myFaces ={
         TextureTarget.TextureCubeMapPositiveX,
         TextureTarget.TextureCubeMapNegativeX,
         TextureTarget.TextureCubeMapPositiveY,
         TextureTarget.TextureCubeMapNegativeY,
         TextureTarget.TextureCubeMapPositiveZ,
         TextureTarget.TextureCubeMapNegativeZ
      };

      public string[] myFilenames;


      public CubemapTexture(List<String> faces, bool flip)
         : base()
      {
         myFlip = flip;
         target = TextureTarget.TextureCubeMap;

         myFilenames = faces.ToArray();

         GL.GenTextures(1, out myId);

         if (LoadFromDisk() == false)
         {
            GL.DeleteTexture(myId);
            myId = 0;
            System.Console.WriteLine("Cannot create cubemap from file: {0}", faces.ToString());
         }

			GL.TexParameter(target, TextureParameterName.TextureBaseLevel, 0);
			GL.TexParameter(target, TextureParameterName.TextureMaxLevel, 0); //needed since no mip maps are created
			GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);  //nearest since no mip maps are created
			GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);  //nearest since no mip maps are created
			GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		}

      public CubemapTexture(int width, int height, OGL.PixelInternalFormat pif)
      {
         target = TextureTarget.TextureCubeMap;
         myPixelFormat = pif;
         OGL.PixelFormat pf;
         findPixelType(myPixelFormat, out pf, out myDataType);
         myWidth = width;
         myHeight = height;
         GL.GenTextures(1, out myId);
         GL.BindTexture(target, myId);
         GL.TexParameter(target, TextureParameterName.TextureBaseLevel, 0);
         GL.TexParameter(target, TextureParameterName.TextureMaxLevel, 0); //needed since no mip maps are created
         GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);  //nearest since no mip maps are created
         GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);  //nearest since no mip maps are created
         GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
         GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

         for (int i=0; i< 6; i++)
         {
            GL.TexImage2D(myFaces[i], 0, myPixelFormat, myWidth, myHeight, 0, pf, myDataType, IntPtr.Zero);
         }
      }

      public CubemapTexture()
      {
         target = TextureTarget.TextureCubeMap;
         GL.GenTextures(1, out myId);
      }

      public override bool bind()
      {
         if (myId == 0)
         {
            Warn.print("Failed to bind cubemap texture id 0");
            return false;
         }

         GL.Enable(EnableCap.TextureCubeMapSeamless);
         GL.BindTexture(target, myId);

         return true;
      }

      public bool LoadFromDisk()
      {
         GL.Enable(EnableCap.TextureCubeMapSeamless);
         //GL.Hint(HintTarget.TextureCompressionHint, HintMode.Nicest);
         
         GL.BindTexture(target, myId);

         for (int i = 0; i < 6; i++)
         {
            System.Console.Write("Loading Cubemap Face {0} with {1}...", i, myFilenames[i]);
            OGL.PixelFormat pf;
            Bitmap bm = loadBitmap(myFilenames[i], out myPixelFormat, out pf, out myDataType);

            if (bm != null)
            {
               //flip the image since it's backwards from what opengl expects
               if (myFlip == true)
               {
                  bm.RotateFlip(RotateFlipType.RotateNoneFlipY);
               }

               BitmapData Data = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);

               GL.TexImage2D(myFaces[i], 0, myPixelFormat, Data.Width, Data.Height, 0, pf, myDataType, Data.Scan0);
               //GL.TexImage2D(myFaces[i], 0, PixelInternalFormat.CompressedRgb, Data.Width, Data.Height, 0, pf, pt, Data.Scan0);

               myWidth = Data.Width;
               myHeight = Data.Height;
              

               bm.UnlockBits(Data);
            }

            System.Console.WriteLine("Done!");
         }

         return true; // success
      }

      public void updateFace(TextureTarget face, int width, int height, PixelData pixels)
      {
         GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
         GL.BindTexture(target, myId);
         GL.TexImage2D(face, 0, myPixelFormat, width, height, 0, pixels.pixelFormat, pixels.dataType, pixels.data);
      }

      public void updateFace(int face, Texture t)
      {
         GL.CopyImageSubData(t.id(), ImageTarget.Texture2D, 0, 0, 0, 0, myId, ImageTarget.TextureCubeMap, 0, 0, 0, face, t.width, t.height, 1);
      }

      public void updateFaces(Texture[] textures)
      {
         for(int i=0; i< 6; i++)
         {
            updateFace(i, textures[i]);
         }
      }

      public override bool saveData(string filename)
      {
         bind();
         int depth = 3;

         for(int i=0; i< 6; i++)
         {
            byte[] data = new byte[width * height * depth];
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.GetTexImage(TextureTarget.TextureCubeMapPositiveX + i, 0, OGL.PixelFormat.Rgb, PixelType.Byte, data);

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int pos = 0;
            for (int y = 0; y < height; y++)
            {
               for (int x = 0; x < width; x++)
               {
                  bmp.SetPixel(x, y, Color.FromArgb(data[pos], data[pos + 1], data[pos + 2]));
                  pos += 3;
               }
            }

            String fn = String.Format("{0}-{1}.png", filename, i);
            bmp.Save(fn);
         }

         unbind();

         return true;
      }
   }
}