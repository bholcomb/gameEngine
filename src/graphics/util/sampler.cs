using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public enum Sample { Nearest, Linear};
   public enum Boundary { Wrap, Clamp};

   public class Sampler2D
   {
      float[] myData;
      Texture myTexture;

      public Sample sample { get; set; }
      public Boundary boundary { get; set; }

      public Sampler2D(Texture t)
      {
         myTexture = t;
         updateData();
      }

      public void updateData()
      {
         int w = myTexture.width;
         int h = myTexture.height;
         int comp = 3;
         PixelFormat pf = PixelFormat.Rgb;
         switch(myTexture.pixelFormat)
         {
            case PixelInternalFormat.R32f: comp = 1; pf = PixelFormat.Red; break;
            case PixelInternalFormat.Rgb: comp = 3; pf = PixelFormat.Rgb; break;
            case PixelInternalFormat.Rgb32f: comp = 3; pf = PixelFormat.Rgb; break;
            case PixelInternalFormat.Rgba: comp = 4; pf = PixelFormat.Rgba; break;
            case PixelInternalFormat.Rgba32f: comp = 4; pf = PixelFormat.Rgba; break;
         }

         myData = new float[w * h * comp];

         myTexture.bind();
         GL.GetTexImage(myTexture.target, 0, pf, PixelType.Float, myData);
         myTexture.unbind();
      }

      public float get(float x, float y, int channel = 0)
      {
         if(myData == null)
         {
            updateData();
         }

         x = clampWrap(x, boundary);
         y = clampWrap(y, boundary);

         int comp = 3;
         switch (myTexture.pixelFormat)
         {
            case PixelInternalFormat.R32f: comp = 1; break;
            case PixelInternalFormat.Rgb: comp = 3; break;
            case PixelInternalFormat.Rgb32f: comp = 3; break;
            case PixelInternalFormat.Rgba: comp = 4; break;
            case PixelInternalFormat.Rgba32f: comp = 4; break;
         }

         if (channel >= comp)
         {
            channel = comp - 1;
         }

         int px = (int)(Math.Floor(x * myTexture.width));
         int py = (int)(Math.Floor(y * myTexture.height));
         int offset = (px + (py * myTexture.width)) * comp;

         return myData[offset + channel];
      }

      float clampWrap(float v, Boundary b)
      {
         if (v < 0.0f)
         {
            if (b == Boundary.Clamp)
            {
               v = 0.0f;
            }
            else
            {
               while (v < 0.0f)
               {
                  v += 1.0f;
               }
            }
         }

         if (v > 1.0f)
         {
            if (b == Boundary.Clamp)
            {
               v = 1.0f;
            }
            else
            {
               while (v > 1.0f)
               {
                  v -= 1.0f;
               }
            }
         }

         return v;
      }
   }  
   
   public class Sampler3D
   {

   }

   public class SamplerCubemap
   {
      /*
      From: https://scalibq.wordpress.com/2013/06/23/cubemaps/
       
       1) The face is selected by looking at the absolute values of the components of the 3d vector (|x|, |y|, |z|). The component with the absolute value of the largest magnitude determines the major axis. The sign of the component selects the positive or negative direction.

      2) The selected face is addressed as a regular 2D texture with U, V coordinates within a (0..1) range. The U and V are calculated from the two components that were not the major axis. So for example, if  we have +x as our cubemap face, then Y and Z will be used to calculate U and V:

      U = ((-Z/|X|) + 1)/2

      V = ((-Y/|X|) + 1)/2 
     */
   }
}