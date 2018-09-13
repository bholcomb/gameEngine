using System;
using System.IO;
using System.Collections.Generic;

using Util;
using Graphics;
using Lua;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   namespace Bob
   {
      public enum ChunkType { MODEL, SKELETON, ANIMATION, SCRIPT, AUDIO, SOUNDSCAPE, PARTICLE };

      public class Chunk
      {
         public ChunkType myType;
         public UInt32 myVersion;
         public UInt32 myFlags;
         public string myName;
         public Chunk()
         {

         }
      }
   }
}