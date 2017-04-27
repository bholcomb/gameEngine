using System;
using System.Collections.Generic;

namespace Util
{
   public static class Formatter
   {
      public static String bytesHumanReadable(int count)
      {
         float c = count;
         string[] sizes = { "B", "KB", "MB", "GB" };
         int order = 0;
         while (c >= 1024 && order + 1 < sizes.Length)
         {
            order++;
            c = c / 1024.0f;
         }

         return String.Format("{0:0.0} {1}", c, sizes[order]);
      }


      public static int stringListHashCode(List<String> strings)
      {
         if (strings == null)
         {
            return 0;
         }

         strings.Sort();
         String combined = "";
         foreach (String s in strings)
         {
            combined += s + "-";
         }

         return combined.GetHashCode();
      }
   }
}