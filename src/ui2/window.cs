using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using GUI;
using Util;

namespace GUI
{
   public class Window
   {
      [Flags]
      public enum Flags
      {
         //decorations
         Background           = 1 << 0,
         TitleBar             = 1 << 1,
         MenuBar              = 1 << 2,
         Borders              = 1 << 3,
         HorizontalScrollbar  = 1 << 4,
         VerticalScrollbar    = 1 << 5,
         ScrollBars           = HorizontalScrollbar | VerticalScrollbar,
         

         //behavior
         Movable              = 1 << 10,
         Collapsable          = 1 << 11,
         Resizable            = 1 << 12,
         Inputs               = 1 << 13,
         ScrollableWithMouse  = 1 << 14,
         FocusOnAppearing     = 1 << 15,
         BringToFrontOnFocus  = 1 << 16,
         AutoResize            = 1 << 17,

         //window types
         Root                 = 1 << 20,
         ChildWindow          = 1 << 21,
         ComboBox             = 1 << 22,
         Tooltip              = 1 << 23,
         Popup                = 1 << 24,
         Modal                = 1 << 25,
         ChildMenu            = 1 << 26,

         //common use cases
         DefaultWindow = Background | TitleBar | MenuBar | Borders | Movable | Collapsable | Resizable | Inputs | BringToFrontOnFocus | ChildWindow,
      };

      public enum Layout
      {
         Vertical,
         Horizontal
      }
      class Group
      {
         public Vector2 myPos;
         public Vector2 mySize;
         public Vector2 myCursorPos;
         public Layout myLayout;
      }

      public string name { get; set; }
      public UInt32 id { get; set; }
      public Flags flags { get; set; }
      public int lastFrameActive { get; set; }
      public bool active { get; set; }
      public bool skipItems { get; set; }
      public UInt32 moveId { get; set; }

      Stack<Group> myGroupStack = new Stack<Group>();

      Vector2 myPosition = Vector2.Zero;
      public Vector2 size = UI.displaySize;
      bool myIsCollapsed = false;

      internal Window parent = null;
      internal Window child = null;
      internal Window sibling = null;

      public Canvas canvas = new Canvas();

      SetCondition mySetPositionAllowFlags;
      SetCondition mySetSizeAllowFlags;

      //menu window only stuff
      public class MenuColumns
      {
         Window myWindow;
         float[] widths = new float[3];

         public float[] positions = new float[3];

         public MenuColumns(Window win)
         {
            myWindow = win;
         }

         public float declareColumns(float c1, float c2, float c3)
         {
            widths[0] = Math.Max(c1, widths[0]);
            widths[1] = Math.Max(c2, widths[1]);
            widths[2] = Math.Max(c3, widths[2]);

            return update();
         }

         float update()
         {
            float totalWidth = 0;
            positions[0] = 0;
            for (int i = 0; i < 3; i++)
            {
               totalWidth += widths[i];

               if (i > 0)
               {
                  if (widths[i] > 0)
                  {
                     totalWidth += UI.style.window.menuPadding.X;
                  }

                  positions[i] = totalWidth - widths[i];
               }
            }
            
            return totalWidth;
         }
      };

      public MenuColumns menuColums;

      public Window(String winName, Flags createFlags = Flags.DefaultWindow)
      {
         parent = null;
         child = null;
         sibling = null;
         flags = createFlags;

         //setup the ID of this window
         name = winName;
         id = UI.idStack.getId(winName);
         moveId = getChildId("MOVE");

         mySetPositionAllowFlags = mySetSizeAllowFlags = SetCondition.Always | SetCondition.Appearing | SetCondition.FirstUseEver | SetCondition.Once;

         menuColums = new MenuColumns(this);

         Group g = new Group();
         g.myLayout = Layout.Vertical;
         myGroupStack.Push(g);
      }

      public UInt32 getChildId(String name)
      {
         UI.idStack.push(id);
         UInt32 childId = UI.idStack.getId(name);
         UI.idStack.pop();
         UI.keepAliveId(childId);
         return childId;
      }

      public Vector2 position
      {
         get { return myPosition; }
         set { myPosition = value; }
      }

      public Vector2 cursorPosition
      {
         //this is in window space coordinates.  The conversion to screen space happens during rendering
         get {
            Group g = myGroupStack.Peek();
            return g.myPos + g.myCursorPos;
         }
         set {
            Group g = myGroupStack.Peek();
            g.myCursorPos = value - g.myPos;
         }
      }

      public Vector2 cursorScreenPosition
      {
         get { return position + cursorPosition; }
         set { cursorPosition = value - position; }
      }

      public Rect rect { get { return Rect.fromPosSize(position, size); } }
      public float titleBarHeight { get { return flags.HasFlag(Flags.TitleBar) ? UI.style.font.fontSize + UI.style.window.groupPadding.Y : 0.0f; } }
      public Rect titleBarRect { get { return Rect.fromPosSize(new Vector2(0, 0) + position, new Vector2(size.X, titleBarHeight)); } }
      public float menuBarHeight { get { return flags.HasFlag(Flags.MenuBar) ? UI.style.font.fontSize + UI.style.window.menuPadding.Y : 0.0f; } }
      public Rect menuBarRect { get { Vector2 pos = new Vector2(0, titleBarHeight) + position; return Rect.fromPosSize(pos, new Vector2(size.X, menuBarHeight)); } }

      public bool begin(ref bool closed)
      {
			if (closed == true)
				return false;

         //is this the first time we've begun this window?
         if (lastFrameActive != UI.frame)
         {
            if (flags.HasFlag(Flags.AutoResize))
            {
               //from the previous frame
               size = myGroupStack.Peek().mySize + UI.style.window.padding;
            }

            lastFrameActive = UI.frame;
            active = true;
            myGroupStack.Peek().myCursorPos = new Vector2(0, titleBarHeight + menuBarHeight);
            myGroupStack.Peek().mySize = new Vector2(0, 0);
            myGroupStack.Peek().myLayout = Layout.Vertical;
         }

         if (UI.currentWindow != this)
         {
            UI.pushWindow(this);
         }

         //process the setNextWindow* functions
         if (UI.setNextWindowPositionCondition != 0)
         {
            bool shouldSet = (mySetPositionAllowFlags.HasFlag(UI.setNextWindowPositionCondition));
            if (shouldSet && (UI.setNextWindowPositionValue != UI.theInvalidVec2))
            {
               setPosition(UI.setNextWindowPositionValue, UI.setNextWindowPositionCondition);
               UI.setNextWindowPositionCondition = 0;
               UI.setNextWindowPositionValue = UI.theInvalidVec2;
            }
         }

         if (UI.setNextWindowSizeCondition != 0)
         {
            bool shouldSet = (mySetSizeAllowFlags.HasFlag(UI.setNextWindowSizeCondition));
            if (shouldSet && (UI.setNextWindowSizeValue != UI.theInvalidVec2))
            {
               setSize(UI.setNextWindowSizeValue, UI.setNextWindowSizeCondition);
               UI.setNextWindowSizeCondition = 0;
               UI.setNextWindowSizeValue = UI.theInvalidVec2;
            }
         }

         //check for collapsing window
         if (flags.HasFlag(Flags.TitleBar) && flags.HasFlag(Flags.Collapsable))
         {
            Rect r = titleBarRect;
            if (UI.myHoveredWindow == this && r.containsPoint(UI.mouse.pos) && UI.mouse.buttons[(int)MouseButton.Left].doubleClicked)
            {
               myIsCollapsed = !myIsCollapsed;
            }
         }
         else
         {
            myIsCollapsed = false;
         }

         //check for moving window
         UI.keepAliveId(moveId);
         if (UI.activeId == moveId)
         {
            if (UI.mouse.buttonIsDown(MouseButton.Middle) == true)
            {
               if (flags.HasFlag(Flags.Movable) == true)
               {
                  myPosition += UI.mouse.delta;
               }
               UI.focusWindow(this);
            }
            else
            {
               UI.activeId = 0;
            }
         }

         drawWindowBackground();

         skipItems = myIsCollapsed;

         return true;
      }

      public bool end()
      {
         drawWindowForeground();
         canvas.popClipRect();
         return true;
      }

      public bool mouseOverTitle()
      {
         return titleBarRect.containsPoint(UI.mouse.pos);
      }

      public void getRenderCommands(ref List<RenderCommand> cmds)
      {
         if (active == true)
            canvas.generateCommands(ref cmds);

         if (sibling != null)
         {
            sibling.getRenderCommands(ref cmds);
         }

         if (child != null)
         {
            child.getRenderCommands(ref cmds);
         }
      }

      public void addChild(Window win)
      {
         //remove any previous relationships (if they existed)
         win.parent = this;
         win.sibling = null;

         //I ain't got no children, so add it
         if (child == null)
         {
            child = win;
            return;
         }

         //add this window to linked list of children
         Window temp = child;
         while (temp.sibling != null)
         {
            temp = temp.sibling;
         }

         temp.sibling = win;
      }

      public void remove()
      {
         if (parent == null)
         {
            return;
         }

         //I'm the parents first child
         if (parent.child == this)
         {
            parent.child = sibling;
         }
         else
         {
            Window temp = parent.child;
            while (temp.sibling != null)
            {
               if (temp.sibling == this)
               {
                  temp.sibling = sibling;
                  break;
               }

               temp = temp.sibling;
            }
         }
      }

      public void makeLastSibling()
      {
         if (parent != null && sibling != null)
         {
            Window p = parent;
            remove();
            p.addChild(this);
         }
      }

      void drawWindowBackground()
      {
         canvas.reset();
         canvas.setScreenResolution(UI.displaySize);
         //canvas.setScale(1.0f);

         float alpha = UI.style.window.background.A;

         if (flags.HasFlag(Flags.Root))
         {
            canvas.pushClipRectFullscreen();
            UI.styleStacks.floats.Push(UI.style.window.rounding);
            UI.style.window.rounding = 0.0f;
         }
         else
         {
            canvas.pushClipRect(rect);
         }

         if (!myIsCollapsed)
         {
            //window background
            if(flags.HasFlag(Flags.Background) == true)
            {
               canvas.addRectFilled(rect, UI.style.window.background, UI.style.window.rounding);
            }
         }
   
         //pop root special style
         if (flags.HasFlag(Flags.Root))
         {
            UI.style.window.rounding = UI.styleStacks.floats.Pop();
         }
      }

      void drawWindowForeground()
      {
         float alpha = UI.style.window.borderColor.A;

         if (flags.HasFlag(Flags.Root))
         {
            UI.styleStacks.floats.Push(UI.style.window.rounding);
            UI.style.window.rounding = 0.0f;
         }

         if (myIsCollapsed)
         {
            //title bar only
            canvas.addRectFilled(titleBarRect, UI.style.window.header.active.color, UI.style.window.rounding, Canvas.Corners.ALL);
            canvas.addText(titleBarRect, UI.style.window.header.labelNormal, name, Alignment.Middle);
         }
         else
         {
            //title bar
            if (flags.HasFlag(Flags.TitleBar) == true)
            {
               canvas.addRectFilled(titleBarRect, UI.style.window.header.active.color, UI.style.window.rounding, Canvas.Corners.TOP);
               canvas.addText(titleBarRect, UI.style.window.header.labelNormal, name, Alignment.Middle);
            }

            //menu bar
            if (flags.HasFlag(Flags.MenuBar) == true)
            {
               canvas.addRectFilled(menuBarRect, UI.style.window.menuBorderColor);
            }
            //scroll bars

            //resize grip

            //borders
            if (flags.HasFlag(Flags.Borders) == true)
            {

               canvas.addRect(Rect.fromPosSize(position, size), UI.style.window.borderColor, UI.style.window.rounding, Canvas.Corners.ALL);
  
               if (flags.HasFlag(Flags.TitleBar) == false)
               {
                  canvas.addLine(titleBarRect.NW + new Vector2(1, 0), titleBarRect.NE - new Vector2(1, 0), UI.style.window.borderColor);
               }
            }
         }

         //pop root special style
         if (flags.HasFlag(Flags.Root))
         {
            UI.style.window.rounding = UI.styleStacks.floats.Pop();
         }
      }
      public void setPosition(Vector2 pos, SetCondition cond)
      {
         if ((cond & mySetPositionAllowFlags) == 0)
            return;

         //turn off single use flags
         mySetPositionAllowFlags &= ~(SetCondition.Once | SetCondition.FirstUseEver | SetCondition.Appearing);

         position = pos;
      }

      public void setSize(Vector2 sz, SetCondition cond)
      {
         if ((cond & mySetSizeAllowFlags) == 0)
            return;

         //turn off single use flags
         mySetSizeAllowFlags &= ~(SetCondition.Once | SetCondition.FirstUseEver | SetCondition.Appearing);

         size = sz;
      }

      public void setLayout(Layout layout)
      {
         myGroupStack.Peek().myLayout = layout;
      }

      public void pushMenuDrawSettings(Vector2 newCursor, Layout layout)
      {
         Group g = new Group();
         g.myPos = newCursor;
         g.myLayout = layout;
         myGroupStack.Push(g);
      }

      public void popMenuDrawSettings()
      {
         Group g = myGroupStack.Pop();
      }

      public void addItem(Vector2 itemSize)
      {
         Group g = myGroupStack.Peek();
         g.mySize.X = Math.Max(g.mySize.X, g.myCursorPos.X + itemSize.X);
         g.mySize.Y = Math.Max(g.mySize.Y, g.myCursorPos.Y + itemSize.Y);

         switch (g.myLayout)
         {
            case Layout.Horizontal:
               {
                  g.myCursorPos.X += itemSize.X;
                  break;
               }
            case Layout.Vertical:
               {
                  g.myCursorPos.Y += itemSize.Y;
                  break;
               }
         }
      }

      public void nextLine()
      {
         Group g = myGroupStack.Peek();
         g.myCursorPos.X = UI.style.window.groupPadding.X;
         g.myCursorPos.Y = g.mySize.Y;
      }

      public void beginGroup()
      {
         Group g = new Group();
         g.myPos = myGroupStack.Peek().myCursorPos;
         g.mySize = Vector2.Zero;
         g.myCursorPos = Vector2.Zero;
         g.myLayout = myGroupStack.Peek().myLayout;

         myGroupStack.Push(g);
      }

      public void endGroup()
      {
         Group g = myGroupStack.Pop();

         addItem(g.mySize);
      }
   }
}