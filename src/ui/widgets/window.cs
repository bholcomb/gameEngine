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
      public static bool beginWindow(String name, Window.Flags flags = Window.Flags.DefaultWindow)
      {
         bool closed = false;
         return beginWindow(name, ref closed, flags);
      }

      public static bool beginWindow(String name, ref bool closed, Window.Flags flags = Window.Flags.DefaultWindow)
      {
         Window win = findWindow(name);

         if (win == null)
         {
            win = createWindow(name, flags);
         }

         bool ret = win.begin(ref closed);
         return ret;
      }

      public static bool endWindow()
      {
         Window win = currentWindow;
         if(win == null)
         {
            return false;
         }
			
         bool ret = win.end();
         popWindow(win);
         return ret;
      }

      public static Vector2 setNextWindowPositionValue { get; set; }
      public static Vector2 setNextWindowSizeValue { get; set; }
      public static bool setNextWindowFocused { get; set; }
      public static SetCondition setNextWindowPositionCondition { get; set; }
      public static SetCondition setNextWindowSizeCondition { get; set; }
      public static SetCondition setNextWindowFocusCondition { get; set; }

      public static void setWindowPosition(Vector2 pos, SetCondition cond = SetCondition.Always)
      {
         Window win = currentWindow;
         win.setPosition(pos, cond);
      }

      public static void setWindowSize(Vector2 sz, SetCondition cond = SetCondition.Always)
      {
         Window win = currentWindow;
         win.setSize(sz, cond);
      }

      public static void setWindowFocus()
      {
         Window win = currentWindow;
         focusWindow(win);
      }

      public static void setWindowPosition(String winName, Vector2 pos, SetCondition cond = SetCondition.Always)
      {
         Window win = findWindow(winName);
         if(win != null)
            win.setPosition(pos, cond);
      }

      public static void setWindowSize(String winName, Vector2 sz, SetCondition cond = SetCondition.Always)
      {
         Window win = findWindow(winName);
         if(win != null)
            win.setSize(sz, cond);
      }

      public static void setNextWindowPosition(Vector2 pos, SetCondition cond = SetCondition.Always)
      {
         setNextWindowPositionValue = pos;
         setNextWindowPositionCondition = cond;
      }

      public static void setNextWindowSize(Vector2 size, SetCondition cond = SetCondition.Always)
      {
         setNextWindowSizeValue = size;
         setNextWindowSizeCondition = cond;
      }

      public static void setNextWindowFocus()
      {
         setNextWindowFocused = true;
      }

      public static void setWindowLayout(Layout.Direction layout)
      {
         Window win = currentWindow;
         win.setLayout(layout);
      }

      public static void setWindowLayout(String winName, Layout.Direction layout)
      {
         Window win = findWindow(winName);
         if (win != null)
            win.setLayout(layout);
      }

      public static void setWindowFocus(String winName)
      {
         Window win = findWindow(winName);
         if (win != null)
            focusWindow(win);
      }

      public static void beginLayout(Layout.Direction layout)
      {
         Window win = currentWindow;
         win.beginLayout(layout);
      }

      public static void endLayout()
      {
         Window win = currentWindow;
         win.endLayout();
      }

      public static Vector2 cursorPosition()
      {
         Window win = currentWindow;
         return win.cursorPosition;
      }

      public static void setCursorPosition(Vector2 newPos)
      {
         Window win = currentWindow;
         win.cursorPosition = newPos;
      }

      public static void nextLine()
      {
         Window win = currentWindow;
         win.nextLine();
      }
   }
}