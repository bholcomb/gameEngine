using System;
using System.Collections.Generic;

using Util;
using OpenTK;

namespace Physics
{
   public class Intersections
   {
      public enum IntersectionResult { INSIDE, OUTSIDE, PARTIAL };

      public static bool rayBoxIntersection(Ray r, float t0, float t1, Vector3 min, Vector3 max, out float t)
      {
         //this algorithm is from "An efficient and Robust Ray-Box Intersection Algorithm"
         //Amy Williams, Steve Barrus, R. Keith Morley, Pete Shirley
         //University of Utah, unknown date
         //http://people.csail.mit.edu/amy/papers/box-jgt.ps

         t = -1.0f;

         Vector3[] bounds = new Vector3[2] { min, max };

         float tmin, tmax, tymin, tymax, tzmin, tzmax;

         tmin = (bounds[r.mySigns[0]].X - r.myOrigin.X) * r.myInvDirection.X;
         tmax = (bounds[1 - r.mySigns[0]].X - r.myOrigin.X) * r.myInvDirection.X;

         tymin = (bounds[r.mySigns[1]].Y - r.myOrigin.Y) * r.myInvDirection.Y;
         tymax = (bounds[1 - r.mySigns[1]].Y - r.myOrigin.Y) * r.myInvDirection.Y;

         if ((tmin > tymax) || (tymin > tmax))
            return false;
         if (tymin > tmin)
            tmin = tymin;
         if (tymax < tmax)
            tmax = tymax;

         tzmin = (bounds[r.mySigns[2]].Z - r.myOrigin.Z) * r.myInvDirection.Z;
         tzmax = (bounds[1 - r.mySigns[2]].Z - r.myOrigin.Z) * r.myInvDirection.Z;

         if ((tmin > tzmax) || (tzmin > tmax))
            return false;
         if (tzmin > tmin)
            tmin = tzmin;
         if (tzmax < tmax)
            tmax = tzmax;

         t = tmin;
         return ((tmin < t1) && (tmax > t0));
      }

      public static bool rayAABBIntersection(Vector3 point, Vector3 dir, AABox box, out float tmin, out Vector3 hit)
      {
         tmin = 0.0f;
         float tmax = float.MaxValue;

         for (int i = 0; i < 3; i++)
         {
            if (Math.Abs(dir[i]) < float.Epsilon)
            {
               if (point[i] < box.myMin[i] || point[i] > box.myMax[i])
               {
                  hit = Vector3.Zero;
                  return false;
               }
            }
            else
            {
               float ood = 1.0f / dir[i];
               float t1 = (box.myMin[i] - point[i]) * ood;
               float t2 = (box.myMax[i] - point[i]) * ood;

               if (t1 > t2)
               {
                  float temp = t2;
                  t2 = t1;
                  t1 = temp;
               }

               tmin = Math.Max(tmin, t1);
               tmax = Math.Min(tmax, t2);

               if(tmin > tmax)
               {
                  hit = Vector3.Zero;
                  return false;
               }
            }
         }

         hit = point + dir * tmin;
         return true;
      }

      public static bool segmentCapsuleIntersection(LineSegment seg, Vector3 c1, Vector3 c2, float radius, out float t)
      {
         t = 0;
         LineSegment capsuleSegment=new LineSegment(c1, c2);
         Vector3 h1 = Vector3.Zero;
         Vector3 h2 = Vector3.Zero;
         float s = 0;
         float tt = 0;

         if (Distance.closestPoints(seg, capsuleSegment, out s, out tt, out h1, out h2) > radius)
         {
            return false;
         }



         return true;
      }

      public static bool raySphereIntersection(Vector3 point, Vector3 dir, Sphere s, out float t, out Vector3 hit)
      {
         t = 0;
         hit = Vector3.Zero;

         Vector3 m = point - s.myCenter;
         float b = Vector3.Dot(m, dir);
         float c = Vector3.Dot(m, m) - s.myRadius * s.myRadius;

         //exit if ray origin is outside of sphere (c>0) and ray pointing away from sphere (b>0)
         if (c > 0.0f && b > 0.0f)
         {
            return false;
         }

         //a negative discriminant corresponds to ray missing sphere
         float discr = b * b - c;
         if(discr < 0.0f)
         {
            return false;
         }

         //ray found to intersect sphere, compute smallest t value of intersection
         t = -b - (float)Math.Sqrt(discr);
         
         //if t is negative, then ray was inside sphere so clamp to 0
         if (t < 0.0f)
         {
            t = 0.0f;
         }

         hit = point + t * dir;
         return true;
      }

      public static bool pointInAABox(AABox box, Vector3 point)
      {
         if (point.X >= box.myMin.X &&
             point.Y >= box.myMin.Y &&
             point.Z >= box.myMin.Z &&
             point.X < box.myMax.X &&
             point.Y < box.myMax.Y &&
             point.Z < box.myMax.Z)
         {
            return true;
         }

         return false;
      }

      public static bool pointInAABox(Vector3 min, Vector3 max, Vector3 point)
      {
         if (point.X >= min.X &&
             point.Y >= min.Y &&
             point.Z >= min.Z &&
             point.X < max.X &&
             point.Y < max.Y &&
             point.Z < max.Z)
         {
            return true;
         }

         return false;
      }

      public static bool AABoxIntersection(AABox box1, AABox box2)
      {
         Vector3 distance = box2.myOrigin - box1.myOrigin;
         return Math.Abs(distance.X) <= (box1.mySize.X + box2.mySize.X) &&
                Math.Abs(distance.Y) <= (box1.mySize.Y + box2.mySize.Y) &&
                Math.Abs(distance.Z) <= (box1.mySize.Z + box2.mySize.Z);
      }

      public static bool AABoxSweepIntersection(AABox box1, Vector3 b1Vel, AABox box2, Vector3 b2Vel, out float t)
      {
         Vector3 v = b2Vel - b1Vel; //relative velocity of box 2 to box 1
         Vector3 u0 = Vector3.Zero;
         Vector3 u1 = Vector3.One;

         if (AABoxIntersection(box1, box2) == true)
         {
            u0 = u1 = Vector3.Zero;
            t = 0;
            return true;
         }

         for (int i = 0; i < 3; i++)
         {
            if (box1.myMax[i] < box2.myMin[i] && v[i] < 0)
            {
               u0[i] = (box1.myMax[i] - box2.myMin[i]) / v[i];
            }
            else if (box2.myMax[i] < box1.myMin[i] && v[i] > 0)
            {
               u0[i] = (box1.myMin[i] - box2.myMax[i]) / v[i];
            }

            if (box2.myMax[i] > box1.myMin[i] && v[i] < 0)
            {
               u1[i] = (box1.myMin[i] - box2.myMax[i]) / v[i];
            }
            else if (box1.myMax[i] > box2.myMin[i] && v[i] > 0)
            {
               u1[i] = (box1.myMax[i] - box2.myMin[i]) / v[i];
            }
         }

         float t0 = Math.Max(u0.X, Math.Max(u0.Y, u0.Z));
         float t1 = Math.Min(u1.X, Math.Min(u1.Y, u1.Z));
         t = t0;
         return t0 <= t1;
      }

      public static bool AABBContainsSphere(Vector3 sphereCenter, float radius, AABox box)
      {
         if (sphereCenter.X - radius >= box.myMin.X &&
             sphereCenter.Y - radius >= box.myMin.Y &&
             sphereCenter.Z - radius >= box.myMin.Z &&
             sphereCenter.X + radius < box.myMax.X &&
             sphereCenter.Y + radius < box.myMax.Y &&
             sphereCenter.Z + radius < box.myMax.Z)
         {
            return true;
         }

         return false;
      }

      public static bool AABoxSphereIntersection(AABox box, Vector3 sphereCenter, float radius, out Vector3 hitNormal, out float hitDepth)
      {
         if (AABBContainsSphere(sphereCenter, radius, box))
         {
            // Do special code.
            // here, for now don't do a collision, until the center is
            // outside the box
            hitDepth = 0.0f;
            hitNormal = Vector3.Zero;
            return true;
         }

         // get closest point on box from sphere center
         Vector3 closest = Distance.closestPointOn(box, sphereCenter);

         // find the separation
         Vector3 diff = sphereCenter - closest;

         // check if points are far enough
         float dist = diff.Length;

         if (dist > radius)
         {
            hitDepth = 0.0f;
            hitNormal = Vector3.Zero;
            return false;
         }

         // collision depth
         hitDepth = radius - dist;

         // normal of collision (going towards the sphere center)
         hitNormal = diff / dist;

         return true;
      }

      public static bool AABoxSphereIntersection(AABox box1, Vector3 spherePos, float sphereRadius)
      {
         float s = 0;
         float d = 0;
         float sphereRadiusSquared = sphereRadius * sphereRadius;
         for (int i = 0; i < 3; i++)
         {
            if (spherePos[i] < box1.myMin[i])
            {
               s = spherePos[i] = box1.myMin[i];
               d += s * s;
            }
            else if (spherePos[i] > box1.myMax[i])
            {
               s = spherePos[i] - box1.myMax[i];
               d += s * s;
            }
         }

         return d <= sphereRadiusSquared;
      }

      public static bool AABoxMovingSphereIntersection(AABox box, Sphere s, Vector3 vel, out float t, out Vector3 hit)
      {
         //get bounding box of original box expanded by radius
         AABox expanded = box;
         expanded.myMin.X -= s.myRadius; expanded.myMin.Y -= s.myRadius; expanded.myMin.Z -= s.myRadius;
         expanded.myMax.X += s.myRadius; expanded.myMax.Y += s.myRadius; expanded.myMax.Z += s.myRadius; 

         //intersect ray of sphere center along vel against expanded box
         t = 0.0f;
         if (rayAABBIntersection(s.myCenter, vel, expanded, out t, out hit) ==false || t > vel.Length)
         {
            return false;
         }

         int u = 0;
         int v = 0;
         if (hit.X < box.myMin.X) u |= 1;
         if (hit.X > box.myMax.X) v |= 1;
         if (hit.Y < box.myMin.Y) u |= 2;
         if (hit.Y > box.myMax.Y) v |= 2;
         if (hit.Z < box.myMin.Z) u |= 4;
         if (hit.Z > box.myMax.Z) v |= 4;

         int m = u + v;

         if (m == 7)
         {

         }

         if ((m & (m - 1)) == 0)
         {
            return true;
         }

         return false;
      }

      #region frustum intersection tests
      public static IntersectionResult intersectCube(Vector3 cubeCenter, float cubeSize, Vector4[] frustum)
      {
         throw new Exception("not implemented");
         return IntersectionResult.INSIDE;
      }

      public static IntersectionResult pointInFrustum(Plane[] frustum, Vector3 point)
      {
         for (int p = 0; p < 6; p++)
            if (frustum[p].A * point.X + frustum[p].B * point.Y + frustum[p].C * point.Z + frustum[p].D <= 0)
               return IntersectionResult.OUTSIDE;
         return IntersectionResult.INSIDE;
      }

      public static IntersectionResult sphereInFrustum(Plane[] frustum, Vector3 point, float radius)
      {
         int c = 0;

         for (int p = 0; p < 6; p++)
         {
            float d = frustum[p].A * point.X + frustum[p].B * point.Y + frustum[p].C * point.Z + frustum[p].D;
            if (d <= -radius)
               return IntersectionResult.OUTSIDE;

            if (d > radius)
               c++;
         }

         if (c == 6)
            return IntersectionResult.INSIDE;
         else
            return IntersectionResult.PARTIAL;
      }

      public static IntersectionResult sphereInConeFrustum(Vector3 camPos, Vector3 viewDir, float far, float fov2, Vector3 point, float radius)
      {
         Vector3 backedUpPos = camPos + (viewDir * -radius);
         float radiusAdjust = 0.000f;

         Vector3 a1 = (viewDir * far) - backedUpPos;
         Vector3 a2 = point - backedUpPos;

         float angle = Vector3.CalculateAngle(a1, a2);
         if (angle > fov2 + radiusAdjust)
            return IntersectionResult.OUTSIDE;

         return IntersectionResult.INSIDE;
      }

      public static IntersectionResult cubeInFrustum(Plane[] frustum, Vector3 min, Vector3 max)
      {
         int c = 0;
         int c2 = 0;

         for (int p = 0; p < 6; p++)
         {
            c = 0;
            if (frustum[p].A * (min.X) + frustum[p].B * (min.Y) + frustum[p].C * (min.Z) + frustum[p].D > 0)
               c++;
            if (frustum[p].A * (max.X) + frustum[p].B * (min.Y) + frustum[p].C * (min.Z) + frustum[p].D > 0)
               c++;
            if (frustum[p].A * (min.X) + frustum[p].B * (max.Y) + frustum[p].C * (min.Z) + frustum[p].D > 0)
               c++;
            if (frustum[p].A * (max.X) + frustum[p].B * (max.Y) + frustum[p].C * (min.Z) + frustum[p].D > 0)
               c++;
            if (frustum[p].A * (min.X) + frustum[p].B * (min.Y) + frustum[p].C * (max.Z) + frustum[p].D > 0)
               c++;
            if (frustum[p].A * (max.X) + frustum[p].B * (min.Y) + frustum[p].C * (max.Z) + frustum[p].D > 0)
               c++;
            if (frustum[p].A * (min.X) + frustum[p].B * (max.Y) + frustum[p].C * (max.Z) + frustum[p].D > 0)
               c++;
            if (frustum[p].A * (max.X) + frustum[p].B * (max.Y) + frustum[p].C * (max.Z) + frustum[p].D > 0)
               c++;

            if (c == 0)
               return IntersectionResult.OUTSIDE;

            if (c == 8)
               c2++;
         }

         if (c2 == 6)
            return IntersectionResult.INSIDE;
         else
            return IntersectionResult.PARTIAL;
      }

      //this function is taken from http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html
      public static IntersectionResult AACubeInFrustum(Plane[] frustum, Vector3 min, Vector3 max)
      {
         IntersectionResult ret = IntersectionResult.INSIDE;

         for (int i = 0; i < 6; i++)
         {
            Vector3 p = min;
            Vector3 n = max;

            if (frustum[i].A >= 0) { p.X = max.X; n.X = min.X; }
            if (frustum[i].B >= 0) { p.Y = max.Y; n.Y = min.Y; }
            if (frustum[i].C >= 0) { p.Z = max.Z; n.Z = min.Z; }

            // is the positive vertex outside?
            if (frustum[i].GetDistance(p) < 0)
               return IntersectionResult.OUTSIDE;
            // is the negative vertex outside?	
            else if (frustum[i].GetDistance(n) < 0)
               ret = IntersectionResult.PARTIAL;
         }

         return ret;
      }

      public static IntersectionResult polygonInFrustum(Vector4[] frustum, Vector3[] points)
      {
         int f, p;

         for (f = 0; f < 6; f++)
         {
            for (p = 0; p < points.Length; p++)
            {
               if (frustum[p].X * points[p].X + frustum[p].Y * points[p].Y + frustum[p].Z * points[p].Z + frustum[p].W > 0)
                  break;
            }
            if (p == points.Length)
            {
               return IntersectionResult.OUTSIDE;
            }
            else
            {
               return IntersectionResult.PARTIAL;
            }
         }
         return IntersectionResult.INSIDE;
      }
   }
      #endregion
}
