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
      public static void icon(int iconId)
      {
         icon(iconId, Color4.White);
      }

      public static void icon(int iconId, Color4 col)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         win.addItem(style.selectable.padding);

         Vector2 pos = win.cursorScreenPosition;
         Vector2 size = new Vector2(style.font.fontSize);
         Rect r = Rect.fromPosSize(pos, size);

         win.canvas.addIcon(iconId, r);

         win.addItem(r.size);
      }
   }
}