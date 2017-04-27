using System;

using OpenTK;

namespace Util
{
   public class LineSegment
   {
      public Vector3 myA;
      public Vector3 myB;

      public LineSegment(Vector3 a, Vector3 b)
      {
         myA = a;
         myB = b;
      }
   }
}