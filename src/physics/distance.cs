using System;
using System.Collections.Generic;

using Util;

using OpenTK;

namespace Physics
{
   public class Distance
   {
      public static float distanceBetween(Vector3 a, Vector3 b)
      {
         return (b - a).Length;
      }

      public static Vector3 closestPointOn(LineSegment seg, Vector3 point)
      {
         Vector3 ab = seg.myB - seg.myA;
         float t = Vector3.Dot(point - seg.myA, ab);
         if (t <= 0)
         {
            return seg.myA;
         }
         else
         {
            float denom = Vector3.Dot(ab, ab); //equal to ||ab||^2
            if (t >= denom)
            {
               return seg.myB;
            }
            else
            {
               t = t / denom;
               return seg.myA + (t * ab);
            }
         }

      }

      public static float distanceBetween(LineSegment seg, Vector3 point)
      {
         Vector3 closest = closestPointOn(seg, point);
         return distanceBetween(point, closest);
      }

      public static float closestPoints(LineSegment seg1, LineSegment seg2, out float s, out float t, out Vector3 c1, out Vector3 c2)
      {
         Vector3 d1 = seg1.myB - seg1.myA;
         Vector3 d2 = seg2.myB - seg2.myA;
         Vector3 r = seg1.myA - seg2.myA;
         float a = Vector3.Dot(d1, d1);
         float e = Vector3.Dot(d2, d2);
         float f = Vector3.Dot(d2, r);

         if (a <= float.Epsilon && e <= float.Epsilon)
         {
            s = t = 0;
            c1 = seg1.myA;
            c2 = seg2.myA;
            return Vector3.Dot(c1 - c2, c1 - c2);
         }

         if (a <= float.Epsilon)
         {
            s = 0;
            t = f / e;
            t = MathExt.clamp(t, 0.0f, 1.0f);
         }
         else
         {
            float c = Vector3.Dot(d1, r);
            if (e <= float.Epsilon)
            {
               t = 0.0f;
               s = MathExt.clamp(-c / a, 0.0f, 1.0f);
            }
            else
            {
               float b = Vector3.Dot(d1, d2);
               float denom = a * e - b * b;
               if (denom != 0.0f)
               {
                  s = MathExt.clamp((b * f - c * e) / denom, 0.0f, 1.0f);
               }
               else 
               {
                  s = 0; 
               }

               t = (b * s + f) / e;

               if (t < 0.0f)
               {
                  t = 0.0f;
                  s = MathExt.clamp(-c / a, 0.0f, 1.0f);
               }
               else if (t > 1.0f)
               {
                  t = 1.0f;
                  s = MathExt.clamp((b - c) / a, 0.0f, 1.0f);
               }
            }
         }

         c1 = seg1.myA + d1 * s;
         c2 = seg2.myA + d2 * t;
         return Vector3.Dot(c1 - c2, c1 - c2);
      }

      public static Vector3 closestPointOn(AABox xBox, Vector3 Point)
      {
         Vector3 q = Vector3.Zero;
         for (int i = 0; i < 3; i++)
         {
            float v = Point[i];
            if (v < xBox.myMin[i]) v = xBox.myMin[i];
            if (v > xBox.myMax[i]) v = xBox.myMax[i];
            q[i] = v;
         }

         return q;
      }

      public static float distanceBetween(AABox box, Vector3 point)
      {
         Vector3 closest = closestPointOn(box, point);
         return distanceBetween(point, closest);
      }
   }
}