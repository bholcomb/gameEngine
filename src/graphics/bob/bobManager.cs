using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public class BobManager
   {
      struct Location
      {
         String filename;
         int offset;
      }

      Dictionary<String, Location> myRegistry = new Dictionary<String, Location>();

      public BobManager()
      {

      }

      public byte[] getResource(String name)
      {
         return null;
      }

      public bool addResource(String name)
      {
         return true;
      }

      public bool addDirectory(String name)
      {
         return true;
      }

      protected bool scanFile(String filename)
      {
         return true;
      }

   }
}