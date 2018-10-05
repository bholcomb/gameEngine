using System;
using System.Collections.Generic;

using Graphics;
using Util;

namespace GUI
{
   public class Icons
   {
      public const int CHECKBOX_UNCHECKED = 0;
      public const int CHECKBOX_CHECKED = 1;
      public const int NONE = 2;
      public const int X = 3;
      public const int UNDERSCORE = 4;
      public const int CIRCLE_SOLID = 5;
      public const int CIRCLE_OUTLINE = 6;
      public const int RECT_SOLID = 7;
      public const int RECT_OUTLINE = 8;
      public const int TRIANGLE_UP = 9;
      public const int TRIANGLE_DOWN = 10;
      public const int TRIANGLE_LEFT = 11;
      public const int TRIANGLE_RIGHT = 12;
      public const int PLUS = 13;
      public const int MINUS = 14;
      public const int MAX = 15;
   };

   public static class IconFactory
   {
      static Dictionary<int, Texture> myIconMap;
      static IconFactory()
      {
         myIconMap = new Dictionary<int, Texture>();
         loadDefaults();
      }

      public static void loadDefaults()
      {
         myIconMap.Add(Icons.CHECKBOX_UNCHECKED, new Texture("../data/ui/checkbox_unchecked.png"));
         myIconMap.Add(Icons.CHECKBOX_CHECKED, new Texture("../data/ui/checkbox_checked.png"));
         myIconMap.Add(Icons.TRIANGLE_UP, new Texture("../data/ui/triangle_up.png"));
         myIconMap.Add(Icons.TRIANGLE_DOWN, new Texture("../data/ui/triangle_down.png"));
         myIconMap.Add(Icons.TRIANGLE_LEFT, new Texture("../data/ui/triangle_left.png"));
         myIconMap.Add(Icons.TRIANGLE_RIGHT, new Texture("../data/ui/triangle_right.png"));
         myIconMap.Add(Icons.PLUS, new Texture("../data/ui/plus.png"));
         myIconMap.Add(Icons.MINUS, new Texture("../data/ui/minus.png"));

      }

      public static Texture findIcon(int icon)
      {
         Texture t = null;
         if(myIconMap.TryGetValue(icon, out t) == false)
         {
            Warn.print("Failed to find icon id {0}", icon);
         }

         return t;
      }

   }
}