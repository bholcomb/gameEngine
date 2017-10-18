using System;

using OpenTK;
using OpenTK.Graphics;

namespace Util
{
   public static class RandomExt
   {
      public static float randomInRange(this Random r, float min, float max)
      {
         double v = r.NextDouble();
         float scale = max - min;
         return min + ((float)v * scale);
      }
   }
   public static class Matrix4Ext
   {
      static public float getValue(this Matrix4 m, int i, int j)
      {
         Vector4 r = new Vector4();
         switch (i)
         {
            case 0:
               r = m.Row0;
               break;
            case 1:
               r = m.Row1;
               break;
            case 2:
               r = m.Row2;
               break;
            case 3:
               r = m.Row3;
               break;
         }

         float ret = 0.0f;

         switch (j)
         {
            case 0:
               ret = r.X;
               break;
            case 1:
               ret = r.Y;
               break;
            case 2:
               ret = r.Z;
               break;
            case 3:
               ret = r.W;
               break;
         }

         return ret;
      }

      static public Matrix4 fromHeadingPitchRoll(this Matrix4 mtx, Vector3 HeadingPitchRollInDegrees)
      {
         return mtx.fromHeadingPitchRoll(HeadingPitchRollInDegrees.X, HeadingPitchRollInDegrees.Y, HeadingPitchRollInDegrees.Z);
      }

      static public Matrix4 fromHeadingPitchRoll(this Matrix4 mtx, float headingDegrees, float pitchDegrees, float rollDegrees)
      {
         headingDegrees = MathHelper.DegreesToRadians(headingDegrees);
         pitchDegrees = MathHelper.DegreesToRadians(pitchDegrees);
         rollDegrees = MathHelper.DegreesToRadians(rollDegrees);

         float cosH = (float)Math.Cos((double)headingDegrees);
         float cosP = (float)Math.Cos((double)pitchDegrees);
         float cosR = (float)Math.Cos((double)rollDegrees);
         float sinH = (float)Math.Sin((double)headingDegrees);
         float sinP = (float)Math.Sin((double)pitchDegrees);
         float sinR = (float)Math.Sin((double)rollDegrees);

         mtx.M11 = cosR * cosH - sinR * sinP * sinH;
         mtx.M12 = sinR * cosH + cosR * sinP * sinH;
         mtx.M13 = -cosP * sinH;
         mtx.M14 = 0.0f;

         mtx.M21 = -sinR * cosP;
         mtx.M22 = cosR * cosP;
         mtx.M23 = sinP;
         mtx.M24 = 0.0f;

         mtx.M31 = cosR * sinH + sinR * sinP * cosH;
         mtx.M32 = sinR * sinH - cosR * sinP * cosH;
         mtx.M33 = cosP * cosH;
         mtx.M34 = 0.0f;

         mtx.M41 = 0.0f;
         mtx.M42 = 0.0f;
         mtx.M43 = 0.0f;
         mtx.M44 = 1.0f;

         return mtx;
      }

      static public void ToOpenGL(Matrix4[] source, ref float[] destination)
      {
         if (destination == null || destination.Length != source.Length * 16)
         {
            destination = new float[source.Length * 16];
         }
         for (int i = 0; i < source.Length; i++)
         {
            int j = i * 16;

            destination[j + 00] = source[i].Column0.X;
            destination[j + 01] = source[i].Column1.X;
            destination[j + 02] = source[i].Column2.X;
            destination[j + 03] = source[i].Column3.X;
            destination[j + 04] = source[i].Column0.Y;
            destination[j + 05] = source[i].Column1.Y;
            destination[j + 06] = source[i].Column2.Y;
            destination[j + 07] = source[i].Column3.Y;
            destination[j + 08] = source[i].Column0.Z;
            destination[j + 09] = source[i].Column1.Z;
            destination[j + 10] = source[i].Column2.Z;
            destination[j + 11] = source[i].Column3.Z;
            destination[j + 12] = source[i].Column0.W;
            destination[j + 13] = source[i].Column1.W;
            destination[j + 14] = source[i].Column2.W;
            destination[j + 15] = source[i].Column3.W;
         }
      }

      static public void extractFrustumPlanes(this Matrix4 mtx, ref Plane[] planes)
      {
         /* new and busted
         //LEFT plane
         planes[0].A = mtx.M41 + mtx.M11;
         planes[0].B = mtx.M42 + mtx.M12;
         planes[0].C = mtx.M43 + mtx.M13;
         planes[0].D = mtx.M44 + mtx.M14;
         planes[0].Normalize();

         //RIGHT plane
         planes[1].A = mtx.M41 - mtx.M11;
         planes[1].B = mtx.M42 - mtx.M12;
         planes[1].C = mtx.M43 - mtx.M13;
         planes[1].D = mtx.M44 - mtx.M14;
         planes[1].Normalize();

         //BOTTOM plane
         planes[2].A = mtx.M41 + mtx.M21;
         planes[2].B = mtx.M42 + mtx.M22;
         planes[2].C = mtx.M43 + mtx.M23;
         planes[2].D = mtx.M44 + mtx.M24;
         planes[2].Normalize();

         //TOP plane
         planes[3].A = mtx.M41 - mtx.M21;
         planes[3].B = mtx.M42 - mtx.M22;
         planes[3].C = mtx.M43 - mtx.M23;
         planes[3].D = mtx.M44 - mtx.M24;
         planes[3].Normalize();

         //NEAR plane
         planes[4].A = mtx.M41 + mtx.M31;
         planes[4].B = mtx.M42 + mtx.M32;
         planes[4].C = mtx.M43 + mtx.M33;
         planes[4].D = mtx.M44 + mtx.M34;
         planes[4].Normalize();

         //FAR plane
         planes[5].A = mtx.M41 - mtx.M31;
         planes[5].B = mtx.M42 - mtx.M32;
         planes[5].C = mtx.M43 - mtx.M33;
         planes[5].D = mtx.M44 - mtx.M34;
         planes[5].Normalize();
         */

         // /*old and busted 
         // Extract the numbers for the RIGHT plane
         planes[0].A = mtx.M14 - mtx.M11;
         planes[0].B = mtx.M24 - mtx.M21;
         planes[0].C = mtx.M34 - mtx.M31;
         planes[0].D = mtx.M44 - mtx.M41;
         planes[0].Normalize();

         // Extract the numbers for the LEFT plane
         planes[1].A = mtx.M14 + mtx.M11;
         planes[1].B = mtx.M24 + mtx.M21;
         planes[1].C = mtx.M34 + mtx.M31;
         planes[1].D = mtx.M44 + mtx.M41;
         planes[1].Normalize();

         // Extract the BOTTOM plane
         planes[2].A = mtx.M14 + mtx.M12;
         planes[2].B = mtx.M24 + mtx.M22;
         planes[2].C = mtx.M34 + mtx.M32;
         planes[2].D = mtx.M44 + mtx.M42;
         planes[2].Normalize();

         // Extract the TOP plane
         planes[3].A = mtx.M14 - mtx.M12;
         planes[3].B = mtx.M24 - mtx.M22;
         planes[3].C = mtx.M34 - mtx.M32;
         planes[3].D = mtx.M44 - mtx.M42;
         planes[3].Normalize();

         // Extract the FAR plane
         planes[4].A = mtx.M14 - mtx.M13;
         planes[4].B = mtx.M24 - mtx.M23;
         planes[4].C = mtx.M34 - mtx.M33;
         planes[4].D = mtx.M44 - mtx.M43;
         planes[4].Normalize();

         // Extract the NEAR plane
         planes[5].A = mtx.M14 + mtx.M13;
         planes[5].B = mtx.M24 + mtx.M23;
         planes[5].C = mtx.M34 + mtx.M33;
         planes[5].D = mtx.M44 + mtx.M43;
         planes[5].Normalize();
         //*/
      }
   }

   public static class QuaternionExt      
   {
      static public void setMatrix(this Quaternion q, ref Matrix4 m)
      {
         m = q.toMatrix();
      }

      static public Matrix4 toMatrix(this Quaternion q)
      {
         Matrix4 m = new Matrix4();
         float x2 = q.X * q.X;
         float y2 = q.Y * q.Y;
         float z2 = q.Z * q.Z;

         float xy = q.X * q.Y;
         float xz = q.X * q.Z;
         float yz = q.Y * q.Z;
         float wx = q.W * q.X;
         float wy = q.W * q.Y;
         float wz = q.W * q.Z;

         // First row
         m.M11 = 1.0f - 2.0f * (y2 + z2);
         m.M12 = 2.0f * (xy + wz);
         m.M13 = 2.0f * (xz - wy);
         m.M14 = 0.0f;

         // Second row
         m.M21 = 2.0f * (xy - wz);
         m.M22 = 1.0f - 2.0f * (x2 + z2);
         m.M23 = 2.0f * (yz + wx);
         m.M24 = 0.0f;

         // Third row
         m.M31 = 2.0f * (xz + wy);
         m.M32 = 2.0f * (yz - wx);
         m.M33 = 1.0f - 2.0f * (x2 + y2);
         m.M34 = 0.0f;

         // Fourth row
         m.M41 = 0;
         m.M42 = 0;
         m.M43 = 0;
         m.M44 = 1.0f;

         return m;
      }

      static public void EulerToQuat(this Quaternion q, float roll, float pitch, float yaw)
      {
         float cr, cp, cy, sr, sp, sy, cpcy, spsy;

         // calculate trig identities
         cr = (float)Math.Cos(roll / 2.0);
         cp = (float)Math.Cos(pitch / 2.0);
         cy = (float)Math.Cos(yaw / 2.0);
         sr = (float)Math.Sin(roll / 2.0);
         sp = (float)Math.Sin(pitch / 2.0);
         sy = (float)Math.Sin(yaw / 2.0);

         cpcy = cp * cy;
         spsy = sp * sy;

         q.X = sr * cpcy - cr * spsy;
         q.Y = cr * sp * cy + sr * cp * sy;
         q.Z = cr * cp * sy - sr * sp * cy;
         q.W = cr * cpcy + sr * spsy;

         q.Normalize();
      }

      static public Quaternion fromMatrix(this Quaternion q, Matrix4 m)
      {
         float s = 0.0f;
         float[] qt = new float[4];
         float trace = m.M11 + m.M22 + m.M33;

         if (trace > 0.0f)
         {
            s = (float)Math.Sqrt((double)trace + 1.0);
            qt[3] = s * 0.5f;
            s = 0.5f / s;

            qt[0] = (m.M23 - m.M32) * s;
            qt[1] = (m.M31 - m.M13) * s;
            qt[2] = (m.M12 - m.M21) * s;
         }
         else
         {
            int[] nxt = new int[3];
            nxt[0] = 1; nxt[1] = 2; nxt[2] = 0;
            int i = 0, j = 0, k = 0;

            if (m.M22 > m.M11)
            {
               i = 1;
            }

            if (m.M33 > m.getValue(i, i))
            {
               i = 2;
            }

            j = nxt[i];
            k = nxt[j];
            s = (float)Math.Sqrt((double)(m.getValue(i, i) - (m.getValue(j, j) + m.getValue(k, k))) + 1.0);

            qt[i] = s * 0.5f;
            s = 0.5f / s;
            qt[3] = (m.getValue(j, k) - m.getValue(k, j)) * s;
            qt[j] = (m.getValue(i, j) + m.getValue(j, i)) * s;
            qt[k] = (m.getValue(i, k) + m.getValue(k, i)) * s;
         }

         q.X = qt[0]; q.Y = qt[1]; q.Z = qt[2]; q.W = qt[3];

         return q;

      }

      static public Quaternion fromHeadingPitchRoll(this Quaternion q, float headingDegrees, float pitchDegrees, float rollDegrees)
      {
         Matrix4 m = new Matrix4();
         m = m.fromHeadingPitchRoll(headingDegrees, pitchDegrees, rollDegrees);
         q = q.fromMatrix(m);

         return q;
      }
   }

   public static class MathExt
   {
      static public T clamp<T>(T value, T min, T max) where T : System.IComparable<T>
      {
         T result = value;
         if (value.CompareTo(max) > 0)
            result = max;
         if (value.CompareTo(min) < 0)
            result = min;
         return result;
      }

      public static float lerp(float a, float b, float t)
      {
         return a + (b - a) * t;
      }
   }

   public static class DoubleEquality
   {
      public struct Epsilon
      {
         public Epsilon(double value) { _value = value; }
         private double _value;
         internal bool IsEqual(double a, double b) { return (a == b) || (Math.Abs(a - b) < _value); }
         internal bool IsNotEqual(double a, double b) { return (a != b) && !(Math.Abs(a - b) < _value); }
      }

      static Epsilon e = new Epsilon(0.0000000001);

      public static bool EQ(this double a, double b) { return e.IsEqual(a, b); }
      public static bool LE(this double a, double b) { return e.IsEqual(a, b) || (a < b); }
      public static bool GE(this double a, double b) { return e.IsEqual(a, b) || (a > b); }

      public static bool NE(this double a, double b) { return e.IsNotEqual(a, b); }
      public static bool LT(this double a, double b) { return e.IsNotEqual(a, b) && (a < b); }
      public static bool GT(this double a, double b) { return e.IsNotEqual(a, b) && (a > b); }
   }

   public static class FloatEquality
   {
      public struct Epsilon
      {
         public Epsilon(float value) { _value = value; }
         private float _value;
         internal bool IsEqual(float a, float b) { return (a == b) || (Math.Abs(a - b) < _value); }
         internal bool IsNotEqual(float a, float b) { return (a != b) && !(Math.Abs(a - b) < _value); }
      }

      static Epsilon e = new Epsilon(0.000001f);

      public static bool EQ(this float a, float b) { return e.IsEqual(a, b); }
      public static bool LE(this float a, float b) { return e.IsEqual(a, b) || (a < b); }
      public static bool GE(this float a, float b) { return e.IsEqual(a, b) || (a > b); }

      public static bool NE(this float a, float b) { return e.IsNotEqual(a, b); }
      public static bool LT(this float a, float b) { return e.IsNotEqual(a, b) && (a < b); }
      public static bool GT(this float a, float b) { return e.IsNotEqual(a, b) && (a > b); }

      public static bool Within(this float a, float b, float tolerence) { return (Math.Abs(a - b) < tolerence); }
   }

   public static class Vector4Ext
   {
      public static Vector4 fromColor(Color4 orig)
      {
         return new Vector4(orig.R, orig.G, orig.B, orig.A);
      }
   }
}