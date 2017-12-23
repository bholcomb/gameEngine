using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

using Graphics;
using Util;
using Events;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace UI
{
   public class IdStack
   {
      Stack<UInt32> mySeeds = new Stack<uint>();

      public IdStack()
      {
         mySeeds.Push(Hash.theInitValue);
      }

      public void push(UInt32 id)
      {
         mySeeds.Push(id);
      }

      public void push(String name)
      {
         UInt32 id = getId(name);
         push(id);
      }

      public void pop()
      {
         mySeeds.Pop();
      }

      public UInt32 getId(String str)
      {
         UInt32 seed = mySeeds.Peek();
         return Hash.hash(str, seed);
      }
   }

   [Flags]
   public enum SetCondition
   {
      Always = 1 << 0,
      Once = 1 << 1,
      FirstUseEver = 1 << 2,
      Appearing = 1 << 3
   }

   public class PopupRef
   {
      public UInt32 myPopUpId;
      public Window myWin;
      public Window myParentWin;
      public UInt32 myParentMenuSet;
      public Vector2 myMousePositionOnOpen;

      public PopupRef()
      {

      }

      public PopupRef(UInt32 id, Window parentwin, UInt32 parentMenuSet, Vector2 mousePos)
      {
         myPopUpId = id;
         myWin = null;
         myParentWin = parentwin;
         myParentMenuSet = parentMenuSet;
         myMousePositionOnOpen = mousePos;
      }
   }

   public static partial class ImGui
   {
      #region state
      public static readonly Vector2 theInvalidVec2 = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
      public static readonly Vector2 theZeroVec2 = new Vector2(0, 0);
      public static bool initialzied = false;
      public static IdStack idStack = new IdStack();

      //size accessors
      public static int width { get { return (int)displaySize.X; } }
      public static int height { get { return (int)displaySize.Y; } }

      //frame timing
      public static double dt;
      public static double time;
      public static int frame;

      //input device state
      public static MouseState mouse { get; set; }
      public static KeyboardState keyboard { get; set; }

      //Output
      public static bool wantCaptureMouse { get; set; }
      public static bool wantCaptureKeyboard { get; set; }
      public static bool wantTextInput { get; set; }

      //current style
      public static Style style { get; set; }

      //windows
      public static Window myRootWindow;
      public static Window myCurrentWindow;
      public static Window myHoveredWindow;
      public static Window myFocusedWindow;
      internal static List<Window> windows { get; set; }
      static List<Window> myWindowStack = new List<Window>();

      public static Vector2 setNextWindowPositionValue { get; set; }
      public static Vector2 setNextWindowSizeValue { get; set; }
      public static bool setNextWindowFocused { get; set; }
      public static SetCondition setNextWindowPositionCondition { get; set; }
      public static SetCondition setNextWindowSizeCondition { get; set; }
      public static SetCondition setNextWindowFocusCondition { get; set; }

      //popups
      public static Stack<PopupRef> myOpenedPopupStack = new Stack<PopupRef>();
      public static List<PopupRef> myCurrentPopupStack = new List<PopupRef>();

      //active elements
      public static UInt32 hoveredId { get; set; }
      public static UInt32 hoveredIdPrevFrame { get; set; }
      public static UInt32 activeId { get; set; }
      public static UInt32 activeIdPrevFrame { get; set; }
      public static bool activeIdIsAlive { get; set; }

      //misc
      public static bool captureMouseNextFrame { get; set; }
      public static bool captureKeyboardNextFrame { get; set; }
      #endregion

      #region settings
      public static Vector2 displaySize { get; set; }
      public static float mouseDoubleClickTime { get; set; }
      public static float mouseDoubleClickDistance { get; set; }
      public static float mouseDragThreshold { get; set; }
      public static float keyRepeatDelay { get; set; }
      public static float keyRepeatRate { get; set; }
      #endregion

      static ImGui()
      {
         mouse = new MouseState();
         keyboard = new KeyboardState();
         style = new Style();

         //default settings
         mouseDoubleClickTime = 0.3f;
         mouseDoubleClickDistance = 6.0f;
         mouseDragThreshold = 6.0f;
         keyRepeatDelay = 0.25f;
         keyRepeatRate = 0.02f;

         windows = new List<Window>();

         setNextWindowPositionValue = theInvalidVec2;
         setNextWindowSizeValue = theInvalidVec2;
      }

      public static void beginFrame()
      {
         frame++;

         style.darkStyle();

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
         ImGui.beginWindow("root", ref closed, Window.Flags.Root | Window.Flags.MenuBar);
         myRootWindow = findWindow("root");
         myRootWindow.myBackgroundAlpha = 0.0f;
      }

      public static void endFrame()
      {
         ImGui.endWindow();

         //check for focus and move
         if (myHoveredWindow !=null && 
            activeId == 0 && 
            hoveredId == 0 &&  
            myHoveredWindow.mouseOverTitle() &&  
            mouse.buttonClicked[(int)MouseButton.Middle] == true)
         {
            ImGui.setActiveId(myHoveredWindow.moveId);
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

         if (win.flags.HasFlag(Window.Flags.BringToFrontOnFocus) )
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
         if (w.siblingWindow != null)
         {
            temp = findHoveredWindow(pos, excludeChildren, w.siblingWindow);
            if (temp != null)
               return temp;
         }

         if (w.childWindow != null)
         {
            temp = findHoveredWindow(pos, excludeChildren, w.childWindow);
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

         if (excludeChildren == true && (w.flags.HasFlag(Window.Flags.ChildWindow) == true) )
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
         Window win = new Window(name);
         win.flags = flags;
         windows.Add(win);

         Window currWin = currentWindow;
         if (currentWindow != null)
         {
            currentWindow.addChild(win);
            win.parentWindow = currentWindow;
         }

         return win;
      }

      internal static void pushWindow(Window win)
      {
         //push onto the stack
         if (myWindowStack.Count == 0 || myWindowStack.Last() != win)
         {
            myWindowStack.Add(win);
         }

         myCurrentWindow = win;
      }

      internal static void popWindow(Window win)
      {
         myWindowStack.Remove(myWindowStack.Last());
         myCurrentWindow = myWindowStack.Count > 0 ? myWindowStack.Last() : null;
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
         foreach(PopupRef pop in myOpenedPopupStack)
         {
            if (pop.myPopUpId == id)
               return true;
         }

         return false;
      }
   }
}
