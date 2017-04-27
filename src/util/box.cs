using System;

using OpenTK;

namespace Util
{
   public class AABox
   {
      public Vector3 myOrigin;
      public Vector3 mySize;
      public Vector3 myMin;
      public Vector3 myMax;

      public AABox(Vector3 min, Vector3 max)
      {
         myOrigin = (min+max)/2;
         mySize = max-min;
         myMin = min;
         myMax = max;
      }
   }
}