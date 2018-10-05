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
      public static bool beginCombo(String name, String currentName, Vector2 size)
      {
        Window win = currentWindow;

         if (win.skipItems == true)
         {
            return false;
         }

         UInt32 id = win.getChildId(name);
         
         bool pressed = false;
         bool opened = isPopupOpen(id);

         Vector2 pos = win.cursorScreenPosition;
         Vector2 popupPos;

         //assumes horizontal layout
         popupPos = new Vector2(pos.X, pos.Y + win.menuBarRect.size.Y);

         bool shouldOpen = false;

         Rect r = Rect.fromPosSize(win.cursorScreenPosition, size);
         win.canvas.addRectFilled(r, style.combo.normal.color);
         win.canvas.addRect(r, style.combo.labelNormal);
         pressed = selectable(currentName, ref shouldOpen, size, SelectableFlags.Menu | SelectableFlags.DontClosePopups);

         if (!opened && shouldOpen)
         {
            openPopup(name);
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
               opened = beginPopup(name, Window.Flags.Inputs | Window.Flags.Background | Window.Flags.Borders | Window.Flags.ComboBox);
            }
         }

         return opened;
      }

      public static void endCombo()
      {
         endPopup();
      }

      public static void combo(String name, ref int currentItem, List<String> items)
      {
         Vector2 size = new Vector2(75, 20);
         if (beginCombo(name, items[currentItem], size) == true)
         {
            for (int i = 0; i < items.Count; i++)
            {
               String s = items[i];
               if (selectable(s, size, SelectableFlags.MenuItem) == true)
               {
                  currentItem = i;
               }
            }

            endCombo();
         }
      }

      public static void combo<T>(String name, ref T currentEnum) 
      {
         List<String> names = new List<string>();
         names.AddRange(Enum.GetNames(typeof(T)));

         List<T> values = new List<T>();
         foreach (T item in Enum.GetValues(typeof(T)))
         {
            values.Add(item);
         }

         int currentItem = 0;

         foreach(T item in values)
         {
            if (currentEnum.Equals(item) == true)
               break;

            currentItem++;
         }

         combo(name, ref currentItem, names);

         currentEnum = values[currentItem];
      }
   }
}