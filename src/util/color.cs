using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;

namespace Util
{
   public static class ColorExt
   {
      public static UInt32 toUInt(this Color4 c)
      {
         UInt32 i = 0;
         MathExt.clamp(c.R, 0.0f, 1.0f);
         MathExt.clamp(c.G, 0.0f, 1.0f);
         MathExt.clamp(c.B, 0.0f, 1.0f);
         MathExt.clamp(c.A, 0.0f, 1.0f);

         i  = (UInt32)(c.R * 255.0f + 0.5f);
         i |= (UInt32)(c.G * 255.0f + 0.5f) << 8;
         i |= (UInt32)(c.B * 255.0f + 0.5f) << 16;
         i |= (UInt32)(c.A * 255.0f + 0.5f) << 24;

         return i;
      }

      public static void fromUInt(this Color4 c, UInt32 i)
      {
         float s = 1.0f / 255.0f;
         c.R = (i & 0xff) * s;
         c.G = ((i >> 8) & 0xff) * s;
         c.B = ((i >> 16) & 0xff) * s;
         c.A = (i >> 24) * s;
      }

      public static Color4 mix(Color4 lhs, Color4 rhs, float amount)
      {
         Color4 ret = new Color4();
         ret.R = lhs.R + (rhs.R - lhs.R) * amount;
         ret.G = lhs.G + (rhs.G - lhs.G) * amount;
         ret.B = lhs.B + (rhs.B - lhs.B) * amount;
         ret.A = lhs.A + (rhs.A - lhs.A) * amount;

         return ret;
      }
   }
}

