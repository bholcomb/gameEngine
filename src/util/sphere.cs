using System;

using OpenTK;

namespace Util
{
   public class Sphere
   {
      public Vector3 myCenter;
      public float myRadius;

      public Sphere(Vector3 origin, float radius)
      {
         myCenter = origin;
         myRadius = radius;
      }
   }
}