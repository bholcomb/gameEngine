/*********************************************************************************

Copyright (c) 2011 Robert C. Holcomb Jr.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in the
Software without restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


*********************************************************************************/

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace Planet
{

   public class PlanetTextureManager
   {
      public float[,] myHeightArray = new float[1024, 1024];
      //IModule myModule;

      private Bitmap earthLookupBitmap;
      private Vector3[] myEarthLookupTable;

      public PlanetTextureManager()
      {
         earthLookupBitmap = new System.Drawing.Bitmap("../data/textures/EarthLookupTable.png");
         myEarthLookupTable = new Vector3[earthLookupBitmap.Width];
         for (int i = 0; i < earthLookupBitmap.Width; i++)
         {
            System.Drawing.Color c = earthLookupBitmap.GetPixel(i, 2);
            myEarthLookupTable[i] = new Vector3((float)c.R / 256, (float)c.G / 256, (float)c.B / 256);
         }
      }

      public void init()
      {
         System.Console.WriteLine("Generating Planet texture");
         /*
         FastNoise fastPlanetContinents = new FastNoise(0);
         fastPlanetContinents.Frequency = 1.5;

         FastBillow fastPlanetLowlands = new FastBillow();
         fastPlanetLowlands.Frequency = 4;
         LibNoise.Modifiers.ScaleBiasOutput fastPlanetLowlandsScaled = new ScaleBiasOutput(fastPlanetLowlands);
         fastPlanetLowlandsScaled.Scale = 0.2;
         fastPlanetLowlandsScaled.Bias = 0.5;

         FastRidgedMultifractal fastPlanetMountainsBase = new FastRidgedMultifractal(0);
         fastPlanetMountainsBase.Frequency = 4;

         ScaleBiasOutput fastPlanetMountainsScaled = new ScaleBiasOutput(fastPlanetMountainsBase);
         fastPlanetMountainsScaled.Scale = 0.4;
         fastPlanetMountainsScaled.Bias = 0.85;

         FastTurbulence fastPlanetMountains = new FastTurbulence(fastPlanetMountainsScaled);
         fastPlanetMountains.Power = 0.1;
         fastPlanetMountains.Frequency = 50;

         FastNoise fastPlanetLandFilter = new FastNoise(0 + 1);
         fastPlanetLandFilter.Frequency = 6;

         Select fastPlanetLand = new Select(fastPlanetLandFilter, fastPlanetLowlandsScaled, fastPlanetMountains);
         fastPlanetLand.SetBounds(0, 1000);
         fastPlanetLand.EdgeFalloff = 0.5;

         FastBillow fastPlanetOceanBase = new FastBillow(0);
         fastPlanetOceanBase.Frequency = 15;
         ScaleOutput fastPlanetOcean = new ScaleOutput(fastPlanetOceanBase, 0.1);

         Select fastPlanetFinal = new Select(fastPlanetContinents, fastPlanetOcean, fastPlanetLand);
         fastPlanetFinal.SetBounds(0, 1000);
         fastPlanetFinal.EdgeFalloff = 0.5;

         myModule = fastPlanetFinal;

         LibNoise.Models.Sphere sphere = new LibNoise.Models.Sphere(myModule);
         */
         for (int x = 0; x < 1024; x++)
         {
            for (int y = 0; y < 1024; y++)
            {
               double value;

               int offsetX = -(x - 512);
               int offsetY = -(y - 512);
               double longitude = offsetY / 5.6888888888;
               if (longitude > 90.0) longitude = 90.0;
               if (longitude < -90.0) longitude = -90.0;
               double latitude = offsetX / 2.844444444;
               if (latitude > 180.0) latitude = 180.0;
               if (latitude < -180.0) latitude = -180.0;
               value = 1.0f;// sphere.GetValue(longitude, latitude);

               if (value < 0) value = 0;
               if (value > 1.0) value = 1.0;

               //rescale value to something 0.5..1.5
               //value = (value+1.0f)/2.0f;
               myHeightArray[x, y] = (float)value;
            }
         }

         System.Console.WriteLine("Done Generating Planet Texture");
      }

      public double heightAt(Vector3d v)
      {
         v.Normalize();
         return heightAt(v.X, v.Y, v.Z);
      }

      public double heightAt(double x, double y, double z)
      {
         float value = 1.0f;

         float lat = (float)System.Math.Acos(z);
         float lon = (float)System.Math.Atan2(y, x);
         lat = MathHelper.RadiansToDegrees(lat) / 90.0f;
         lon = MathHelper.RadiansToDegrees(lon) / 180.0f;

         if (lat < 0.0) lat += 1.0f;
         if (lon < 0.0) lon += 1.0f;

         if (lat > 1.0) lat = 1.0f;
         if (lon > 1.0) lon = 1.0f;

         int xv, yv;
         xv = (int)(lat * 1023.0f);
         yv = (int)(lon * 1023.0f);

         value = myHeightArray[yv, xv];

         return value;
      }

      public Color4 colorAt(float height)
      {
         if (height < 0.0) height = 0.0f;
         if (height > 1.0) height = 1.0f;

         int index = (int)(height * (myEarthLookupTable.GetLength(0) - 1));
         Vector3 c =  myEarthLookupTable[index];

         return new Color4(c.X, c.Y, c.Z, 1.0f);
      }
   }
}
