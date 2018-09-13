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
   public class Bone
   {
      public string myName;
      public int myParent = -1;
      public Matrix4 myPose;
      public Bone()
      {

      }
   }

   public class AnimationEvent
   {
      public int frame;
      public string name;
   }

   public class Skeleton
   {
      public string myName;
      public List<Bone> myBones = new List<Bone>();
      public int boneCount { get { return myBones.Count; } }

      public Skeleton()
      {
      }

      public Bone findBone(string name)
      {
         foreach(Bone b in myBones)
         {
            if(b.myName == name)
            {
               return b;
            }
         }

         return null;
      }

      public void debugDraw(Vector3 pos, Quaternion ori)
      {
         foreach(Bone b in myBones)
         {
            Vector3 p = pos + Vector3.Transform(b.myPose.ExtractTranslation(), ori);
            DebugRenderer.addSphere(p, 0.05f, Color4.Green, Fill.TRANSPARENT, false, 0.0f);

            if (b.myParent != -1)
            {
               Vector3 p1 = pos + Vector3.Transform(b.myPose.ExtractTranslation(), ori);
               Vector3 p2 = pos + Vector3.Transform(myBones[b.myParent].myPose.ExtractTranslation(), ori);
               DebugRenderer.addLine(p1, p2, Color4.Blue, Fill.WIREFRAME, false, 0.0f);
            }
         }
      }
   }
}