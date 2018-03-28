using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public static class Bob
   {
      public enum VertexFormat { V3N3T2, V3N3T2B4W4 };
      public enum IndexFormat { USHORT, UINT };
      public enum TextureFormat { PNG, JPG, BMP, DDS, RAW };
      public enum ChunkType { MODEL, MESH, MATERIAL, SKELETON, ACTION, SCRIPT, AUDIO, SOUNDSCAPE, PARTICLE, FONT };
   }

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