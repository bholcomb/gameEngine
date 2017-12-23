using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using UI;
using Util;

namespace UI
{
   public static partial class ImGui
   {
      public static void separator()
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         Vector2 size = new Vector2(win.size.X, 6);
         
         Vector2 a = win.cursorScreenPosition + new Vector2(1, 2);
         Vector2 b = win.cursorScreenPosition + new Vector2(win.size.X - 1, 2);

         win.canvas.addLine(a, b, style.colors[(int)ElementColor.Border], 1);

         win.addItem(size);
      }
   }
}