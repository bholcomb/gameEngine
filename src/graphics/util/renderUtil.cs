using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public static class Util
   {
		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

      public static Texture getEmbeddedTexture(string resourceName)
      {
         Stream stream = Embedded.getStream(resourceName);
         if(stream != null)
         {
            Texture t = new Texture();
            t.loadFromStream(stream);
            return t;
         }

         Warn.print("Failed to find embedded texture {0}", resourceName);
         return null;
      }
   }
}