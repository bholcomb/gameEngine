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
   public class Window
   {
      [Flags]
      public enum Flags
      {
         None                 = 0,

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

      public string name { get; set; }
      public UInt32 id { get; set; }
      public Flags flags { get; set; }
      public int lastFrameActive { get; set; }
      public bool active { get; set; }
      public bool skipItems { get; set; }
      public UInt32 moveId { get; set; }

      Stack<Layout> myLayoutStack = new Stack<Layout>();

      Vector2 myPosition = Vector2.Zero;
      public Vector2 size = UI.displaySize;
      bool myIsCollapsed = false;

      internal Window parent = null;
      internal Window child = null;
      internal Window sibling = null;

      public Canvas canvas = new Canvas();

      SetCondition mySetPositionAllowFlags;
      SetCondition mySetSizeAllowFlags;

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
            Layout l = myLayoutStack.Peek();
            return l.myPos + l.myCursorPos;
         }
         set {
            Layout l = myLayoutStack.Peek();
            l.myCursorPos = value - l.myPos;
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

         //setup the layout for a window
         List<float> layoutSizes = new List<float>();
         if (flags.HasFlag(Flags.TitleBar))
            layoutSizes.Add(titleBarHeight);

         if (flags.HasFlag(Flags.MenuBar))
            layoutSizes.Add(menuBarHeight);

         layoutSizes.Add(0);

         beginLayout(Layout.Direction.Vertical, layoutSizes);

         //is this the first time we've begun this window?
         if (lastFrameActive != UI.frame)
         {
            lastFrameActive = UI.frame;
            active = true;
         }

         if (UI.currentWindow != this)
         {
            UI.pushWindow(this);
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

         drawWindowForeground();
         return true;
      }

      public bool end()
      {
         canvas.popClipRect();

         if (flags.HasFlag(Flags.AutoResize))
         {
            //new size for next frame
            size = myLayoutStack.Peek().mySize + UI.style.window.padding;
         }

         myLayoutStack.Clear();
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

         float alpha = UI.style.window.backgroundColor.A;

         if (flags.HasFlag(Flags.Root))
         {
            canvas.pushClipRectFullscreen();
            UI.styleStacks.floats.Push(UI.style.window.rounding);
            UI.style.window.rounding = 0.0f;
         }
         else
         {
            Rect clip = rect; //add 1 since we want to include that last pixel
            clip.width += 1;
            clip.height += 1;
            canvas.pushClipRect(clip);
         }

         if (!myIsCollapsed)
         {
            //window background
            if(flags.HasFlag(Flags.Background) == true)
            {
               StyleItem background = UI.style.window.background;
               switch (background.type)
               {
                  case StyleItem.Type.COLOR:
                     canvas.addRectFilled(rect, background.color, UI.style.window.rounding);
                     break;
                  case StyleItem.Type.IMAGE:
                     canvas.addImage(background.image, rect);
                     break;
                  case StyleItem.Type.SPRITE:
                     canvas.addImage(background.sprite, rect);
                     break;
                  case StyleItem.Type.NINEPATCH:
                     canvas.addImage(background.patch, rect);
                     break;
               }
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
               addItem(titleBarRect.size);
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

               StyleItem border = UI.style.window.border;
               switch (border.type)
               {
                  case StyleItem.Type.COLOR:
                     canvas.addRectFilled(rect, border.color, UI.style.window.rounding, Canvas.Corners.ALL);
                     break;
                  case StyleItem.Type.IMAGE:
                     canvas.addImage(border.image, rect);
                     break;
                  case StyleItem.Type.SPRITE:
                     canvas.addImage(border.sprite, rect);
                     break;
                  case StyleItem.Type.NINEPATCH:
                     canvas.addImage(border.patch, rect);
                     break;
               }
               
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

      public void setLayout(Layout.Direction layout)
      {
         myLayoutStack.Peek().myDirection = layout;
      }

      public Layout currentLayout
      {
         get {
            if (myLayoutStack.Count == 0)
            {
               return null;
            }

            return myLayoutStack.Peek();
         }
      }

      public void addItem(Vector2 itemSize)
      {
         Layout l = myLayoutStack.Peek();
         l.addItem(itemSize);
      }

      public void nextLine()
      {
         Layout l = myLayoutStack.Peek();
         l.nextLine();
      }

      public void beginLayout(Layout.Direction layout, List<float> spacing = null)
      {
         Vector2 pos = Vector2.Zero;
         if (myLayoutStack.Count > 0)
         {
            pos = cursorPosition;
         }

         Layout l = new Layout(this, layout, pos, spacing);
         myLayoutStack.Push(l);
      }

      public void beginLayout(Vector2 position, Layout.Direction layout, List<float> spacing = null)
      {
         Layout l = new Layout(this, layout, position, spacing);
         myLayoutStack.Push(l);
      }

      public void endLayout()
      {
         Layout l = myLayoutStack.Pop();

         addItem(l.mySize);
      }
   }
}