using System;
using System.Collections.Generic;

using Graphics;
using Util;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GUI
{
   [Flags]
   public enum SetCondition
   {
      Always = 1 << 0,
      Once = 1 << 1,
      FirstUseEver = 1 << 2,
      Appearing = 1 << 3
   };

   public static partial class UI
   {
      public static readonly Vector2 theInvalidVec2 = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
      public static readonly Vector2 theZeroVec2 = new Vector2(0, 0);
      public static bool initialized = false;
      public static IdStack idStack;

      //frame timing
      public static double dt;
      public static double time;
      public static int frame;

      //input device state
      public static MouseState mouse;
      public static KeyboardState keyboard;

      //Output
      public static bool wantCaptureMouse { get; set; }
      public static bool wantCaptureKeyboard { get; set; }
      public static bool wantTextInput { get; set; }

      //current style
      public static Style style;
      public static StyleStacks styleStacks;

      //windows
      public static Window myRootWindow;
      public static Window myCurrentWindow;
      public static Window myHoveredWindow;
      public static Window myFocusedWindow;
      static Stack<Window> myWindowStack;
      internal static List<Window> windows;

      //popups
      public static Stack<Popup> myOpenedPopupStack = new Stack<Popup>();
      public static List<Popup> myCurrentPopupStack = new List<Popup>();

      //active elements
      public static UInt32 hoveredId { get; set; }
      public static UInt32 hoveredIdPrevFrame { get; set; }
      public static UInt32 activeId { get; set; }
      public static UInt32 activeIdPrevFrame { get; set; }
      public static bool activeIdIsAlive { get; set; }

      //settings
      public static Vector2 displaySize { get; set; }

      public static Context ctx;

      static UI()
      {
         ctx = new Context();

         idStack = new IdStack();
         mouse = new MouseState();
         keyboard = new KeyboardState();
         style = new Style();
         styleStacks = new StyleStacks();
         setDefaultStyle();

         myWindowStack = new Stack<Window>();
         windows = new List<Window>();

         setNextWindowPositionValue = theInvalidVec2;
         setNextWindowSizeValue = theInvalidVec2;
      }

      public static void beginFrame()
      {
         frame++;

         //update input states
         mouse.newFrame();
         keyboard.newFrame();

         //reset state
         hoveredIdPrevFrame = hoveredId;
         hoveredId = 0;
         if (activeIdIsAlive == false && activeIdPrevFrame == activeId && activeId != 0)
         {
            setActiveId(0);
         }
         activeIdPrevFrame = activeId;
         activeIdIsAlive = false;

         // Find the window we are hovering. Child windows can extend beyond the limit of their parent so we need to derive HoveredRootWindow from HoveredWindow
         myHoveredWindow = findHoveredWindow(mouse.pos, false);

         //reset all windows
         foreach (Window w in windows)
            w.active = false;

         //reset the setNextWindow* functions
         setNextWindowPositionCondition = 0;
         setNextWindowSizeCondition = 0;
         setNextWindowPositionValue = theInvalidVec2;
         setNextWindowSizeValue = theInvalidVec2;

         //begin the root window
         setNextWindowSize(displaySize, SetCondition.Always);
         setNextWindowPosition(Vector2.Zero, SetCondition.Always);
         bool closed = false;
         beginWindow("root", ref closed, Window.Flags.Root | Window.Flags.Inputs);
         myRootWindow = findWindow("root");
      }

      public static void endFrame()
      {
         endWindow();

         //check for focused window
         if (myHoveredWindow != null &&
            activeId == 0 &&
            (mouse.isButtonClicked(MouseButton.Left) || 
             mouse.isButtonClicked(MouseButton.Right) || 
             mouse.isButtonClicked(MouseButton.Middle)) == true)
         {
            focusWindow(myHoveredWindow);
         }

         //check for window move
         if (myHoveredWindow != null &&
            activeId == 0 &&
            hoveredId == 0 &&
            myHoveredWindow.mouseOverTitle() &&
            mouse.isButtonClicked(MouseButton.Middle) == true)
         {
            setActiveId(myHoveredWindow.moveId);
         }
      }

      public static void setActiveId(UInt32 id)
      {
         activeId = id;
      }

      public static void keepAliveId(UInt32 id)
      {
         if (activeId == id)
         {
            activeIdIsAlive = true;
         }
      }

      //the window currently "open" (from a begin statement) used for building the GUI
      public static Window currentWindow { get { return myCurrentWindow; } }

      //the window the mouse is currently
      public static Window hoveredWindow { get { return myHoveredWindow; } }
      public static Window focusedWindow { get { return myFocusedWindow; } }

      public static void focusWindow(Window win)
      {
         myFocusedWindow = win;

         if (win == null)
            return;

         if (win.flags.HasFlag(Window.Flags.BringToFrontOnFocus))
            return;

         win.makeLastSibling();
      }

      public static Window findWindow(String name)
      {
         foreach (Window win in windows)
         {
            if (win.name == name)
               return win;
         }

         return null;
      }

      static Window findHoveredWindow(Vector2 pos, bool excludeChildren, Window w)
      {
         Window temp = null;
         if (w.sibling != null)
         {
            temp = findHoveredWindow(pos, excludeChildren, w.sibling);
            if (temp != null)
               return temp;
         }

         if (w.child != null)
         {
            temp = findHoveredWindow(pos, excludeChildren, w.child);
            if (temp != null)
               return temp;
         }


         if (w.active == false)
         {
            return null;
         }

         if (w.flags.HasFlag(Window.Flags.Inputs) == false)
         {
            return null;
         }

         if (excludeChildren == true && (w.flags.HasFlag(Window.Flags.ChildWindow) == true))
         {
            return null;
         }

         Rect bb = Rect.fromPosSize(w.position, w.size);
         if (bb.containsPoint(pos))
         {
            return w;
         }

         return null;
      }

      public static Window findHoveredWindow(Vector2 pos, bool excludeChildren)
      {
         if (myRootWindow != null)
         {
            return findHoveredWindow(pos, excludeChildren, myRootWindow);
         }


         return null;
      }

      public static Window createWindow(String name, Window.Flags flags)
      {
         Window win = new Window(name, flags);

         Window currWin = currentWindow;
         if (currentWindow != null)
         {
            currentWindow.addChild(win);
            win.parent = currentWindow;
         }

         windows.Add(win);

         return win;
      }

      internal static void pushWindow(Window win)
      {
         //push onto the stack
         if (myWindowStack.Count == 0 || myWindowStack.Peek() != win)
         {
            myWindowStack.Push(win);
         }

         myCurrentWindow = win;
      }

      internal static void popWindow(Window win)
      {
         myWindowStack.Pop();
         myCurrentWindow = myWindowStack.Count > 0 ? myWindowStack.Peek() : null;
      }

      public static List<RenderCommand> getRenderCommands()
      {
         List<RenderCommand> ret = new List<RenderCommand>();

         //clear the depth buffer
         ret.Add(new ClearCommand(ClearBufferMask.DepthBufferBit));

         if (myRootWindow != null)
         {
            myRootWindow.getRenderCommands(ref ret);
         }

         return ret;
      }

      public static bool isPopupOpen(UInt32 id)
      {
         foreach (Popup pop in myOpenedPopupStack)
         {
            if (pop.myPopUpId == id)
               return true;
         }

         return false;
      }
   }
}