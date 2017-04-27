using System;
using System.Collections.Generic;
using System.Linq;

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
      public static bool beginWindow(String name)
      {
         bool closed = false;
         return beginWindow(name, ref closed, 0);
      }

      public static bool beginWindow(String name, ref bool closed, Window.Flags flags)
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
			
         bool ret = win.end();
         popWindow(win);
         return ret;
      }

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
         ImGui.setNextWindowPositionValue = pos;
         ImGui.setNextWindowPositionCondition = cond;
      }

      public static void setNextWindowSize(Vector2 size, SetCondition cond = SetCondition.Always)
      {
         ImGui.setNextWindowSizeValue = size;
         ImGui.setNextWindowSizeCondition = cond;
      }

      public static void setNextWindowFocus()
      {
         ImGui.setNextWindowFocused = true;
      }

      public static void setWindowLayout(Window.Layout layout)
      {
         Window win = currentWindow;
         win.setLayout(layout);
      }

      public static void setWindowLayout(String winName, Window.Layout layout)
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

      public static void beginGroup()
      {
         Window win = currentWindow;
         win.beginGroup();
      }

      public static void endGroup()
      {
         Window win = currentWindow;
         win.endGroup();
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