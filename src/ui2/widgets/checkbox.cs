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
      #region widget
      public static bool checkbox(String label, ref bool selected)
      {
         Window win = currentWindow;
         if (win.skipItems == true)
         {
            return false;
         }

         Vector2 labelSize = style.font.size(label);
         win.beginGroup(Group.Layout.Horizontal, new float[] { 0.0f, style.font.fontSize });
         float width = labelSize.X + style.font.fontSize * 1.2f;
         
         bool pressed = selectable(label, ref selected, new Vector2(width, 0), SelectableFlags.HasToggle);

         Rect iconRect = Rect.fromPosSize(new Vector2(win.currentGroup().myElements[1].position, win.cursorPosition.Y + style.checkbox.padding.Y) + win.position, new Vector2(style.font.fontSize, style.font.fontSize));
         win.canvas.addIcon(selected ? Icons.CHECKBOX_CHECKED : Icons.CHECKBOX_UNCHECKED, iconRect);

         return pressed;
      }
      #endregion

      #region behavior
      #endregion

      #region drawing

      #endregion
   }
}