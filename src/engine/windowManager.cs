/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;

namespace Engine
{
   public static class WindowManager
   {
      static List<Window> myWindows = new List<Window>();
      static List<Window> myRemovingWindows = new List<Window>();
      static Window myFullscreenWindow = null;
      static Window myActiveWindow = null;
      static Window myActiveWindowBeforeFullscreen = null;

      public static bool init()
      {
         return true;
      }

      public static void shutdown()
      {
      }

      public static Window createWindow(int x, int y, string name)
      {
         Window win = new Window(x,y,name);

         addWindow(win);
         return win;
      }

      public static List<Window> windows()
      {
         return myWindows;
      }

      public static void swapBuffers()
      {
         foreach (Window win in myWindows)
         {
            //process any events
            win.ProcessEvents();

            if (win.Exists)
            {
               win.SwapBuffers();
            }
         }

         foreach (Window win in myRemovingWindows)
         {
            if (myWindows.Contains(win))
            {
               myWindows.Remove(win);
            }
         }

         myRemovingWindows.Clear();
      }

      public static Window currentFullscreenWindow()
      {
         return myFullscreenWindow;
      }

      public static bool setActiveWindow(Window win)
      {
         myActiveWindow = win;
         return true;
      }

      public static Window activeWindow()
      {
         return myActiveWindow;
      }

      public static bool beginFullscreen(Window win)
      {
         myActiveWindowBeforeFullscreen = myActiveWindow;
         setActiveWindow(win);

         //set the window fullscreen
         return true;
      }

      static bool endFullscreen(Window win)
      {
         return true;
      }

      public static bool addWindow(Window win)
      {
         if (myWindows.Contains(win))
         {
            return false;
         }

         myWindows.Add(win);
         setActiveWindow(win);
         return true;
      }

      public static bool removeWindow(Window win)
      {
         if (!myWindows.Contains(win))
         {
            return false;
         }

         myRemovingWindows.Add(win);
         return true;
      }
   }
}