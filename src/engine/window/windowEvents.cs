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

using Util;
using Events;

namespace Engine
{
   public class WindowCloseEvent : Event
   {
      static EventName theName;
      Window myWin;

      public WindowCloseEvent() :this(null) { }
      public WindowCloseEvent(Window win) : this(win, TimeSource.defaultClock.currentTime(), 0.0) { }
      public WindowCloseEvent(Window win, double timeStamp) : this(win, timeStamp, 0.0) { }
      public WindowCloseEvent(Window win, double timeStamp, double delay)
         : base(timeStamp, delay)
      {
         myWin = win;
         myName = theName;
      }

      static WindowCloseEvent()
      {
         theName = new EventName("window.close");
      }

      public Window window
      {
         get { return myWin; }
      }
   }

   public class WindowResizeEvent : Event
   {
      static EventName theName;
      Window myWin;
      int myNewX;
      int myNewY;

      public WindowResizeEvent() :this(null, 0, 0) { }
      public WindowResizeEvent(Window win, int x, int y) : this(win, x, y, TimeSource.defaultClock.currentTime(), 0.0) { }
      public WindowResizeEvent(Window win, int x, int y, double timeStamp) : this(win, x, y, timeStamp, 0.0) { }
      public WindowResizeEvent(Window win, int x, int y, double timeStamp, double delay)
         : base(timeStamp, delay)
      {
         myWin = win;
         myNewX = x;
         myNewY = y;
         myName = theName;
      }

      static WindowResizeEvent()
      {
         theName = new EventName("window.resize");
      }

      public Window window
      {
         get { return myWin; }
      }

      public int x
      {
         get { return myNewX; }
      }

      public int y
      {
         get { return myNewY; }
      }
   }

}