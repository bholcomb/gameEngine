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

         win.beginLayout(win.menuBarRect.position - win.position, Layout.Direction.Horizontal);
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
         win.endLayout();
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
         Vector2 labelSize = new Vector2(style.font.size(label).X + style.menuButton.padding.X, win.menuBarHeight);

         bool pressed = false;
         bool opened = isPopupOpen(id);

         Vector2 pos = win.cursorScreenPosition;
         Vector2 popupPos;
         
         //assumes horizontal layout
         popupPos = new Vector2(pos.X , pos.Y + win.menuBarRect.size.Y);

         bool shouldOpen = false;
         pressed = selectable(label, ref shouldOpen, labelSize, SelectableFlags.Menu | SelectableFlags.DontClosePopups);

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
               setNextWindowSize(new Vector2(10, 10), SetCondition.FirstUseEver);
               opened = beginPopup(label, Window.Flags.Inputs | Window.Flags.Background | Window.Flags.Borders | Window.Flags.ChildMenu);
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

         Vector2 labelSize = style.font.size(label);
         win.beginLayout(Layout.Direction.Horizontal);
         bool pressed = selectable(label, ref selected, new Vector2(labelSize.X, 0), SelectableFlags.MenuItem);

         //spacer to push checkbox up on right side of menu box reguardless of size
         float s = win.size.X - (style.selectable.padding.X + labelSize.X + style.selectable.padding.X + style.font.fontSize + style.window.padding.X);
         if (s < 0.0f) s = 0.0f;
         Vector2 spacer = new Vector2(s, labelSize.Y);
         win.addItem(spacer);

         icon(selected ? Icons.CHECKBOX_CHECKED : Icons.CHECKBOX_UNCHECKED);

         win.endLayout();
         return pressed;
      }

      public static bool menuItem(String label, bool enabled = true)
      {
         Window win = currentWindow;
         if (win.skipItems == true)
         {
            return false;
         }

         Vector2 labelSize = style.font.size(label);
 
         bool selected = false;
         bool pressed = selectable(label, ref selected, new Vector2(0, 0), SelectableFlags.MenuItem);

         return pressed;
      }
   }
}