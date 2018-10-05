using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using Util;

namespace GUI
{
   public static partial class UI
   {
      public static void separator()
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         Vector2 size = new Vector2(win.size.X, 6);
         
         Vector2 a = win.cursorScreenPosition + new Vector2(1, 2);
         Vector2 b = win.cursorScreenPosition + new Vector2(win.size.X - 1, 2);

         win.canvas.addLine(a, b, style.window.borderColor, 1);

         win.addItem(size);
      }

      public static void spacer(float x)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         Vector2 space;
         if(win.currentLayout.myDirection == Layout.Direction.Horizontal)
         {
            space = new Vector2(x, 0);
         }
         else
         {
            space = new Vector2(0, x);
         }

         win.addItem(space);
      }

      public static float percent(float p, Layout.Direction dir)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return 0.0f;

         float size = win.size[(int)dir];

         return (p / 100.0f) * size;
      }
   }
}