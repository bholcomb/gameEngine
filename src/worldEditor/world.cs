using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WorldEditor
{
   public class World
   {
      public int myWidth;
      public int myHeight;
      public float mySeed;

      public Generator myGenerator;

      public World(int X = 1024, int Y = 1024)
      {
         myWidth = X;
         myHeight = Y;
         mySeed = WorldParameters.seed;
         myGenerator = new Generator(this);
      }

      public void generate()
      {
         Stopwatch sw = new Stopwatch();
         sw.Start();

         //height data
         Console.WriteLine("Generating data...");

         myGenerator.update();

         Console.WriteLine("done {0}", sw.Elapsed);
      }
   }
}