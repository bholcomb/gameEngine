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

         win.addItem(style.checkbox.padding);

         Vector2 labelSize = style.font.size(label);
         win.beginLayout(Layout.Direction.Horizontal);
         
         bool pressed = selectable(label, ref selected, new Vector2(labelSize.X, 0), SelectableFlags.HasToggle);

         icon(selected ? Icons.CHECKBOX_CHECKED : Icons.CHECKBOX_UNCHECKED);

         win.endLayout();

         return pressed;
      }
      #endregion

      #region behavior
      #endregion

      #region drawing

      #endregion
   }
}