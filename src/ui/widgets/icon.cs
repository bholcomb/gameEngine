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
      public static void icon(Canvas.Icons iconId, Color4 col)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         Vector2 iconSize = new Vector2(style.currentFontSize, style.currentFontSize);

         win.canvas.addIcon(iconId, win.cursorPosition + new Vector2(0, -iconSize.Y), win.cursorPosition + iconSize);
         win.addItem(iconSize);
      }
   }
}