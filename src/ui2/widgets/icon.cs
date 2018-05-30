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
      public static void icon(Icons iconId, Color4 col)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         Vector2 iconSize = new Vector2(style.font.fontSize, style.font.fontSize);

         win.canvas.addIcon(iconId, win.cursorScreenPosition, win.cursorScreenPosition + iconSize);
         win.addItem(iconSize);
      }
   }
}