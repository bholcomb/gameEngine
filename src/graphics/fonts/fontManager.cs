using System;
using System.Collections.Generic;

namespace Graphics
{
   public static class FontManager
   {
      static Dictionary<String, Font> theFonts = new Dictionary<String, Font>();

      static FontManager()
      {
         
      }

      public static Font findFont(String name, int size = 16)
      {
         Font f;
         string fname = name + "-" + size.ToString();
         if (theFonts.TryGetValue(fname, out f))
         {
            return f;
         }

         if(name != "DEFAULT")
         {
            return null;
         }

         f = findFont("DEFAULT");
         return f;
      }

      public static void init()
      {
         loadDefaults();
      }

      public static void addFont(Font f)
      {
         string fname = f.name + "-" + ((int)f.fontSize).ToString();
         theFonts.Add(fname, f);
      }

      private static void loadDefaults()
      {
         addFont(new TextureFont("DEFAULT", "../data/fonts/proggy12.png", 16, 16, 32));
         addFont(new TextureFont("PROGGY", "../data/fonts/proggy12.png", 16, 16, 32));
         addFont(new TextureFont("CONSOLA", "../data/fonts/consola.png", 16, 16, 32));
         addFont(new TextureFont("COURIER", "../data/fonts/font.png", 16, 16, 32));
         addFont(new TTFFont("SANS", "../data/fonts/freeSans.ttf", 16));
         addFont(new SDFont("FREESANS", "../data/fonts/freeSans", 16));
      }
   }
}
