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

		public static bool hasEmbeddedResource(string resourceName)
      {
         Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
         foreach (Assembly ass in asses)
         {
            AssemblyName[] refs = ass.GetReferencedAssemblies();
            string[] resources = ass.GetManifestResourceNames();
            foreach (string s in resources)
            {
               if (s == resourceName)
               {
                  return true;
               }
            }
         }

         System.Console.WriteLine("Cannot find embedded resource {0}", resourceName);
         return false;
      }

      public static string getString(string resourceName)
      {
         Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
         foreach (Assembly ass in asses)
         {
            string[] resources = ass.GetManifestResourceNames();
            foreach (string s in resources)
            {
               if (s == resourceName)
               {
                  Stream stream = ass.GetManifestResourceStream(resourceName);
                  if (stream == null)
                  {
                     System.Console.WriteLine("Cannot find embedded resource {0}", resourceName);
                     return "";
                  }
                  StreamReader streamReader = new StreamReader(stream);
                  return streamReader.ReadToEnd();
               }
            }
         }

         Warn.print("Failed to find embedded string resource {0}", resourceName);
         return "";
      }

      public static Texture getEmbeddedTexture(string resourceName)
      {
         Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
         foreach (Assembly ass in asses)
         {
            string[] resources = ass.GetManifestResourceNames();
            foreach (string s in resources)
            {
               if (s == resourceName)
               {
                  Stream stream = ass.GetManifestResourceStream(resourceName);
                  if (stream == null)
                  {
                     System.Console.WriteLine("Cannot find embedded resource {0}", resourceName);
                     return null;
                  }

                  Texture t = new Texture();
                  t.loadFromStream(stream);
                  return t;
               }
            }
         }

         Warn.print("Failed to find embedded texture {0}", resourceName);
         return null;
      }
   }
}