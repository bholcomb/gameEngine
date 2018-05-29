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

      public static bool checkbox(String label, ref bool selected)
      {
         Window win = currentWindow;
         if (win.skipItems == true)
         {
            return false;
         }

         Vector2 labelSize = style.textSize(label);
         float width = win.menuColums.declareColumns(labelSize.X, 0, style.currentFontSize * 1.2f);

         bool pressed = selectable(label, ref selected, new Vector2(width, 0), SelectableFlags.HasToggle);

         Rect iconRect = Rect.fromPosSize(new Vector2(win.menuColums.positions[2], win.cursorPosition.Y + style.framePadding.Y) + win.position, new Vector2(style.currentFontSize, style.currentFontSize));
         win.canvas.addIcon(selected ? Canvas.Icons.CHECKBOX_CHECKED : Canvas.Icons.CHECKBOX_UNCHECKED, iconRect);

         win.addItem(new Vector2(width, style.currentFontSize + style.framePadding.Y));

         return pressed;
      }
   }
}