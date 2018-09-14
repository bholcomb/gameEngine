using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Util
{
   public static class Embedded
   {
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
         Stream s = getStream(resourceName);
         if(s != null)
         {
            StreamReader streamReader = new StreamReader(s);
            return streamReader.ReadToEnd();
         }

         Warn.print("Failed to find embedded string resource {0}", resourceName);
         return "";
      }

      public static Stream getStream(string resourceName)
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

                  return stream;
               }
            }
         }

         Warn.print("Failed to find embedded stream {0}", resourceName);
         return null;
      }
   }
}