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
         [Flags]
         public enum AnimationFlags
         {
            Looping = 0x01
         };

         public float duration;
         public string skeletonName;
         public int numBones;
         public List<AnimationEvent> events = new List<AnimationEvent>();
         public List<AnimationChannel> channels = new List<AnimationChannel>();
         public AnimationChunk()
            : base()
         {
            myType = ChunkType.ANIMATION;
         }

         public bool loop { get { return ((AnimationFlags)myFlags & AnimationFlags.Looping) != 0; } }
      }
   }
}
 