using System;

using OpenTK;

namespace Util
{
   public class Ray
   {
      public Vector3 myOrigin;
      public Vector3 myDirection;
      public Vector3 myInvDirection;
      public int[] mySigns = new int[3];

      public Ray(Vector3 origin, Vector3 direction)
      {
         myOrigin = origin;
         myDirection = direction;
         myDirection.Normalize();
         myInvDirection = new Vector3(1.0f / myDirection.X, 1.0f / myDirection.Y, 1.0f / myDirection.Z);
         mySigns[0] = myInvDirection.X < 0 ? 1 : 0;
         mySigns[1] = myInvDirection.Y < 0 ? 1 : 0;
         mySigns[2] = myInvDirection.Z < 0 ? 1 : 0;
      }
   }
}