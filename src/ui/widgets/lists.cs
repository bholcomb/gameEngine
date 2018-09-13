using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using Util;

namespace GUI
{
   [Flags]
   public enum SelectableFlags
   {
      DontClosePopups = 1 << 0,
      SpanAllColumns = 1 << 1,
      Menu = 1 << 2,
      MenuItem = 1 << 3,
      Disabled = 1 << 4,
      DrawFillAvailWidth = 1 << 5,
      HasToggle = 1 << 6
   }


   public static partial class UI
   {
      public static bool selectable(String label, ref bool selected, Vector2 sizeArg, SelectableFlags flags = 0)
      {
         Window win = currentWindow;
         if (win.skipItems == true)
         {
            return false;
         }

         win.addItem(style.selectable.padding);

         UInt32 id = win.getChildId(label);
         Vector2 labelSize = style.font.size(label);
         Vector2 size = new Vector2(sizeArg.X != 0 ? sizeArg.X : labelSize.X, sizeArg.Y != 0 ? sizeArg.Y : labelSize.Y);
         Rect r = Rect.fromPosSize(win.cursorScreenPosition, size);

         bool hovered;
         bool held;
         ButtonFlags buttonFlags = 0;
         if (flags.HasFlag(SelectableFlags.Menu)) buttonFlags |= ButtonFlags.PressedOnClick;
         if (flags.HasFlag(SelectableFlags.MenuItem)) buttonFlags |= ButtonFlags.PressedOnClick | ButtonFlags.PressedOnRelease;
         if (flags.HasFlag(SelectableFlags.Disabled)) buttonFlags |= ButtonFlags.Disabled;

         bool pressed = buttonBehavior(r, id, out hovered, out held, buttonFlags);

         if (hovered)
         {
            Color4 col = style.selectable.textHoverActive;
            win.canvas.addRectFilled(r, col);
         }

         win.canvas.addText(r, style.selectable.textNormal, label, style.selectable.textAlignment);

         //close popups
         if (pressed && flags.HasFlag(SelectableFlags.DontClosePopups) == false && win.flags.HasFlag(Window.Flags.Popup))
         {
            closeCurrentPopup();
         }

         if (pressed)
         {
            selected = !selected;
         }

         
         win.addItem(size);

         return pressed;
      }

      public static bool selectable(String label, Vector2 sizeArg, SelectableFlags flags = 0)
      {
         bool temp = false;
         return selectable(label, ref temp, sizeArg, flags);
      }
   }
}