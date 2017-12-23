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

      public static bool beginMenuBar()
      {
         Window win = currentWindow;
         if(win.skipItems== true)
         {
            return false;
         }

         if(win.flags.HasFlag(Window.Flags.MenuBar) == false)
         {
            return false;
         }

         win.pushMenuDrawSettings(win.menuBarRect.position - win.position, Window.Layout.Horizontal);
         idStack.push("menuBar");
         
         return true;
      }

      public static void endMenuBar()
      {
         Window win = currentWindow;
         if (win.skipItems == true)
         {
            return;
         }

         idStack.pop();
         win.popMenuDrawSettings();
      }

      public static bool beginMenu(String label, bool enabled = true)
      {
         Window win = currentWindow;
         if (win.skipItems == true)
         {
            return false;
         }

         //add the menu bar flag if we don't already have it
         if(win.flags.HasFlag(Window.Flags.MenuBar) == false)
         {
            win.flags |= Window.Flags.MenuBar;
         }

         UInt32 id = win.getChildId(label);
         Vector2 labelSize = style.textSize(label) + style.framePadding2x;

         bool pressed = false;
         bool opened = isPopupOpen(id);

         Vector2 pos = win.cursorScreenPosition;
         Vector2 popupPos;
         
         //assumes horizontal layout
         popupPos = new Vector2(pos.X , pos.Y + win.menuBarRect.size.Y);

         bool shouldOpen = false;
         pressed = selectable(label, ref shouldOpen, new Vector2(labelSize.X, 0.0f), SelectableFlags.Menu | SelectableFlags.DontClosePopups);

         if(!opened && shouldOpen)
         {
            openPopup(label);
            return false;
         }

         if (opened)
         {
            if (pressed)
            {
               closeCurrentPopup();
               return false;
            }
            else
            {
               setNextWindowPosition(popupPos);
               opened = beginPopup(label, Window.Flags.Background | Window.Flags.Borders | Window.Flags.ChildMenu);
            }
         }

         return opened;
      }

      public static void endMenu()
      {
         endPopup();
      }

      public static bool menuItem(String label, ref bool selected, bool enabled = true)
      {
         Window win = currentWindow;
         if (win.skipItems == true)
         {
            return false;
         }

         Vector2 labelSize = style.textSize(label);
         float width = win.menuColums.declareColumns(labelSize.X, 0, style.currentFontSize * 1.2f);

         bool pressed = selectable(label, ref selected, new Vector2(width, 0), SelectableFlags.MenuItem | SelectableFlags.HasToggle);

         Rect iconRect = Rect.fromPosSize(new Vector2(win.menuColums.positions[2], win.cursorPosition.Y + style.framePadding.Y) + win.position, new Vector2(style.currentFontSize, style.currentFontSize));
         win.canvas.addIcon( selected ? Canvas.Icons.CHECKBOX_CHECKED : Canvas.Icons.CHECKBOX_UNCHECKED, iconRect);

         win.addItem(new Vector2(width, style.currentFontSize + style.framePadding.Y));

         return pressed;
      }

      public static bool menuItem(String label, bool enabled = true)
      {
         Window win = currentWindow;
         if (win.skipItems == true)
         {
            return false;
         }

         Vector2 labelSize = style.textSize(label);
         float width = win.menuColums.declareColumns(labelSize.X, 0, 0);

         bool selected = false;
         bool pressed = selectable(label, ref selected, new Vector2(0, 0), SelectableFlags.MenuItem);

         return pressed;
      }
   }
}