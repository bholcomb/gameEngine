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

         win.beginGroup(win.menuBarRect.position - win.position, Group.Layout.Horizontal);
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
         win.endGroup();
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
         Vector2 labelSize = style.font.size(label) + style.menuButton.padding;

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

         Vector2 labelSize = style.font.size(label);
         win.beginGroup(Group.Layout.Horizontal, new float[] { 0.0f, style.font.fontSize });

         float width = win.currentGroup().myElements[0].size;
         bool pressed = selectable(label, ref selected, new Vector2(width, 0), SelectableFlags.MenuItem);

         Rect iconRect = Rect.fromPosSize(new Vector2(win.currentGroup().myElements[1].position + style.selectable.padding.X, win.cursorPosition.Y + style.selectable.padding.Y) + win.position, new Vector2(style.font.fontSize, style.font.fontSize));
         win.canvas.addIcon(selected ? Icons.CHECKBOX_CHECKED : Icons.CHECKBOX_UNCHECKED, iconRect);

         win.addItem(new Vector2(style.font.fontSize, style.font.fontSize));

         win.endGroup();
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