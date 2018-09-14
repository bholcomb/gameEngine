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
      public class Bone
      {
         public string myName;
         public int myParent;
         public Matrix4 myPose;

         public Bone()
         {

         }
      }

      public class SkeletonChunk : Chunk
      {
         public List<Bone> myBones = new List<Bone>();

         public SkeletonChunk()
            : base()
         {
            myType = ChunkType.SKELETON;
         }
      }
   }
}