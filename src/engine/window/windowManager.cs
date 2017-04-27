/*********************************************************************************

Copyright (c) 2011 Robert C. Holcomb Jr.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in the
Software without restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


*********************************************************************************/

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

      public static void clearWindows()
      {
         foreach (Window win in myWindows)
         {
            win.MakeCurrent();
            win.clear();
         }
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