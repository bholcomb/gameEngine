using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using Util;

namespace UI2
{
   public static partial class UI
   {
      public static void icon(Canvas.Icons iconId, Color4 col)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         Vector2 iconSize = new Vector2(style.currentFontSize, style.currentFontSize);

         win.canvas.addIcon(iconId, win.cursorScreenPosition, win.cursorScreenPosition + iconSize);
         win.addItem(iconSize);
      }
   }
}