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
      public class AnimationChunk : Chunk
      {
         public int framerate;
         public int numFrames;
         public string skeletonName;
         public List<AnimationEvent> events = new List<AnimationEvent>();
         public List<List<Matrix4>> poses = new List<List<Matrix4>>();

         public AnimationChunk()
            : base()
         {
            myType = ChunkType.ANIMATION;
         }
      }
   }
}
 