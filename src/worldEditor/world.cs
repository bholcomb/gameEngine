using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WorldEditor
{
   public class World
   {
      public int myWidth;
      public int myHeight;
      int myRegionsX;
      int myRegionsY;

      public Generator myGenerator;
      public Region[,] myRegions;

      public World(int X, int Y)
      {
         width = X;
         height = Y;
         myGenerator = new Generator();

         myRegionsX = width / WorldParameters.theRegionSize;
         myRegionsY = height / WorldParameters.theRegionSize;
         myRegions = new Region[myRegionsX, myRegionsY];

         for (int y = 0; y < myRegionsY; y++)
         {
            for (int x = 0; x < myRegionsX; x++)
            {
               Region r = new Region(this, x * WorldParameters.theRegionSize, y * WorldParameters.theRegionSize);
               myRegions[x, y] = r;
            }
         }

         for (int y = 0; y < myRegionsY; y++)
         {
            for (int x = 0; x < myRegionsX; x++)
            {
               myRegions[x, y].updateNeighbors();
            }
         }
      }

      public int width
      {
         get { return myWidth; }
         set
         {
            int rem = value % WorldParameters.theRegionSize;
            if (rem == 0)
               myWidth = value;
            else
               myWidth = value + (WorldParameters.theRegionSize - rem);
         }
      }

      public int height
      {
         get { return myHeight; }
         set
         {
            int rem = value % WorldParameters.theRegionSize;
            if (rem == 0)
               myHeight = value;
            else
               myHeight = value + (WorldParameters.theRegionSize - rem);
         }
      }

      public void generate()
      {
         Stopwatch sw = new Stopwatch();
         sw.Start();

         //height data
         Console.WriteLine("Generating data...");
         for (int y = 0; y < myRegionsY; y++)
         {
            for (int x = 0; x < myRegionsX; x++)
            {
               Console.Write("Generating Region {0}:{1}....", x, y);
               myRegions[x, y].generateData();
               Console.WriteLine("Done");
            }
         }
         Console.WriteLine("done {0}", sw.Elapsed);

         //update bitmasks
         Console.WriteLine("Update bitmasks..."); sw.Restart();
         for (int y = 0; y < myRegionsY; y++)
         {
            for (int x = 0; x < myRegionsX; x++)
            {
               Console.Write("Updateing Bitmasks Region {0}:{1}....", x, y);
               myRegions[x, y].updateBitmasks();
               Console.WriteLine("Done");
            }
         }
         Console.WriteLine("done {0}", sw.Elapsed);

         //floodfill
         Console.WriteLine("Floodfill..."); sw.Restart();
         for (int y = 0; y < myRegionsY; y++)
         {
            for (int x = 0; x < myRegionsX; x++)
            {
               Console.Write("Floodfill Region {0}:{1}....", x, y);
               myRegions[x, y].floodFill();
               Console.WriteLine("Done");
            }
         }
         Console.WriteLine("done {0}", sw.Elapsed);

         //generate biomes
         Console.WriteLine("Generating biomes..."); sw.Restart();
         for (int y = 0; y < myRegionsY; y++)
         {
            for (int x = 0; x < myRegionsX; x++)
            {
               Console.Write("Generate Biomes Region {0}:{1}....", x, y);
               myRegions[x, y].generateBiomes();
               myRegions[x, y].updateBiomeBitmasks();
               Console.WriteLine("Done");
            }
         }
         Console.WriteLine("done {0}", sw.Elapsed);

         //generate textures
         Console.WriteLine("Generating textures..."); sw.Restart();
         for (int y = 0; y < myRegionsY; y++)
         {
            for (int x = 0; x < myRegionsX; x++)
            {
               Console.Write("Floodfill Region {0}:{1}....", x, y);
               myRegions[x, y].generateTextures();
               Console.WriteLine("Done");
            }
         }
         Console.WriteLine("done {0}", sw.Elapsed);

      }

      int wrap(int a, int n)
      {
         return ((a % n) + n) % n;
      }

      public Tile findTile(int x, int y)
      {
         int trueX = wrap(x, myWidth);
         int trueY = wrap(y, myHeight);

         int regionX = trueX / WorldParameters.theRegionSize;
         int regionY = trueY / WorldParameters.theRegionSize;

         int tileX = trueX % WorldParameters.theRegionSize;
         int tileY = trueY % WorldParameters.theRegionSize;

         return myRegions[regionX, regionY].myTiles[tileX, tileY];
      }
   }
}