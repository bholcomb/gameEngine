using System;
using System.Collections.Generic;

namespace Graphics
{
   public static class FontManager
   {
      static Dictionary<String, Font> theFonts = new Dictionary<String, Font>();

      static FontManager()
      {
         loadDefaults();
      }

      public static Font findFont(String name)
      {
         Font f;
         if (theFonts.TryGetValue(name, out f))
         {
            return f;
         }

         return theFonts["COURIER"];
      }

      public static void addFont(Font f)
      {
         theFonts.Add(f.name, f);
      }

      private static void loadDefaults()
      {
         addFont(new TextureFont("DEFAULT", "../data/fonts/proggy12.png", 15));
         addFont(new TextureFont("PROGGY", "../data/fonts/proggy12.png", 15));
         addFont(new TextureFont("CONSOLA", "../data/fonts/consola.png", 16));
         addFont(new TextureFont("COURIER", "../data/fonts/font.png", 15));
         //addFont(new TTFFont("SANS", "../data/fonts/freeSans.ttf", 15));
         addFont(new SDFont("FREESANS", "../data/fonts/freeSans", 15));
      }
   }
}