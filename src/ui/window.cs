using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using UI;
using Util;

namespace UI
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
      public Vector2 size = ImGui.displaySize;
      bool myIsCollapsed = false;

      internal Window parentWindow = null;
      internal Window childWindow = null;
      internal Window siblingWindow = null;

      public Canvas canvas = new Canvas();

      //render stuff
      internal float myBackgroundAlpha = ImGui.style.windowFillAlphaDefault;

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
                  if(widths[i] > 0)
                     totalWidth += ImGui.style.itemInnerSpacing.X;

                  positions[i] = totalWidth - widths[i];
               }
            }
            
            return totalWidth;
         }
      };

      public MenuColumns menuColums;

      public Window(String winName, Flags createFlags = Flags.DefaultWindow)
      {
         parentWindow = null;
         childWindow = null;
         siblingWindow = null;
         flags = createFlags;

         //setup the ID of this window
         name = winName;
         id = ImGui.idStack.getId(winName);
         moveId = getChildId("MOVE");

         mySetPositionAllowFlags = mySetSizeAllowFlags = SetCondition.Always | SetCondition.Appearing | SetCondition.FirstUseEver | SetCondition.Once;

         menuColums = new MenuColumns(this);

         Group g = new Group();
         g.myLayout = Layout.Vertical;
         myGroupStack.Push(g);
      }

      public UInt32 getChildId(String name)
      {
         ImGui.idStack.push(id);
         UInt32 childId = ImGui.idStack.getId(name);
         ImGui.idStack.pop();
         ImGui.keepAliveId(childId);
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
      public float titleBarHeight { get { return flags.HasFlag(Flags.TitleBar) ? ImGui.style.currentFontSize + ImGui.style.framePadding2x.Y : 0.0f; } }
      public Rect titleBarRect { get { return Rect.fromPosSize(new Vector2(0, 0) + position, new Vector2(size.X, titleBarHeight)); } }
      public float menuBarHeight { get { return flags.HasFlag(Flags.MenuBar) ? ImGui.style.currentFontSize + ImGui.style.framePadding2x.Y : 0.0f; } }
      public Rect menuBarRect { get { Vector2 pos = new Vector2(0, titleBarHeight) + position; return Rect.fromPosSize(pos, new Vector2(size.X, menuBarHeight)); } }

      public bool begin(ref bool closed)
      {
			if (closed == true)
				return false;

         //is this the first time we've begun this window?
         if (lastFrameActive != ImGui.frame)
         {
            if (flags.HasFlag(Flags.AutoResize))
            {
               //from the previous frame
               size = myGroupStack.Peek().mySize + ImGui.style.displayWindowPadding;
            }

            lastFrameActive = ImGui.frame;
            active = true;
            myGroupStack.Peek().myCursorPos = new Vector2(0, titleBarHeight + menuBarHeight);
            myGroupStack.Peek().mySize = new Vector2(0, 0);
            myGroupStack.Peek().myLayout = Layout.Vertical;
         }

         if (ImGui.currentWindow != this)
         {
            ImGui.pushWindow(this);
         }

         //process the setNextWindow* functions
         if (ImGui.setNextWindowPositionCondition != 0)
         {
            bool shouldSet = (mySetPositionAllowFlags.HasFlag(ImGui.setNextWindowPositionCondition));
            if (shouldSet && (ImGui.setNextWindowPositionValue != ImGui.theInvalidVec2))
            {
               setPosition(ImGui.setNextWindowPositionValue, ImGui.setNextWindowPositionCondition);
               ImGui.setNextWindowPositionCondition = 0;
               ImGui.setNextWindowPositionValue = ImGui.theInvalidVec2;
            }
         }

         if (ImGui.setNextWindowSizeCondition != 0)
         {
            bool shouldSet = (mySetSizeAllowFlags.HasFlag(ImGui.setNextWindowSizeCondition));
            if (shouldSet && (ImGui.setNextWindowSizeValue != ImGui.theInvalidVec2))
            {
               setSize(ImGui.setNextWindowSizeValue, ImGui.setNextWindowSizeCondition);
               ImGui.setNextWindowSizeCondition = 0;
               ImGui.setNextWindowSizeValue = ImGui.theInvalidVec2;
            }
         }

         //check for collapsing window
         if (flags.HasFlag(Flags.TitleBar) && flags.HasFlag(Flags.Collapsable))
         {
            Rect r = titleBarRect;
            if (ImGui.myHoveredWindow == this && r.containsPoint(ImGui.mouse.pos) && ImGui.mouse.buttonDoubleClicked[(int)MouseButton.Left])
            {
               myIsCollapsed = !myIsCollapsed;
            }
         }
         else
         {
            myIsCollapsed = false;
         }

         //check for moving window
         ImGui.keepAliveId(moveId);
         if (ImGui.activeId == moveId)
         {
            if (ImGui.mouse.buttonDown[(int)MouseButton.Middle] == true)
            {
               if (flags.HasFlag(Flags.Movable) == true)
               {
                  myPosition += ImGui.mouse.posDelta;
               }
               ImGui.focusWindow(this);
            }
            else
            {
               ImGui.activeId = 0;
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
         return titleBarRect.containsPoint(ImGui.mouse.pos);
      }

      public void getRenderCommands(ref List<RenderCommand> cmds)
      {
         if (active == true)
            canvas.generateCommands(ref cmds);

         if (siblingWindow != null)
         {
            siblingWindow.getRenderCommands(ref cmds);
         }

         if (childWindow != null)
         {
            childWindow.getRenderCommands(ref cmds);
         }
      }

      public void addChild(Window win)
      {
         //remove any previous relationships (if they existed)
         win.parentWindow = this;
         win.siblingWindow = null;

         //I ain't got no children, so add it
         if (childWindow == null)
         {
            childWindow = win;
            return;
         }

         //add this window to linked list of children
         Window temp = childWindow;
         while (temp.siblingWindow != null)
         {
            temp = temp.siblingWindow;
         }

         temp.siblingWindow = win;
      }

      public void remove()
      {
         if (parentWindow == null)
         {
            return;
         }

         //I'm the parents first child
         if (parentWindow.childWindow == this)
         {
            parentWindow.childWindow = siblingWindow;
         }
         else
         {
            Window temp = parentWindow.childWindow;
            while (temp.siblingWindow != null)
            {
               if (temp.siblingWindow == this)
               {
                  temp.siblingWindow = siblingWindow;
                  break;
               }

               temp = temp.siblingWindow;
            }
         }
      }

      public void makeLastSibling()
      {
         if (parentWindow != null && siblingWindow != null)
         {
            Window p = parentWindow;
            remove();
            p.addChild(this);
         }
      }

      void drawWindowBackground()
      {
         canvas.reset();
         canvas.setScreenResolution(ImGui.displaySize);
         canvas.setScale(1.0f);

         float alpha = ImGui.style.alpha;

         if (flags.HasFlag(Flags.Root))
         {
            canvas.pushClipRectFullscreen();
            ImGui.style.pushStyleVar(StyleVar.WindowRounding, 0.0f);
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
               canvas.addRectFilled(rect, ImGui.style.getColor(ElementColor.WindowBg, myBackgroundAlpha), ImGui.style.windowRounding);
            }
         }
   
         //pop root special style
         if (flags.HasFlag(Flags.Root))
         {
            ImGui.style.popStyleVar(1);
         }
      }

      void drawWindowForeground()
      {
         float alpha = ImGui.style.alpha;

         if (flags.HasFlag(Flags.Root))
         {
            ImGui.style.pushStyleVar(StyleVar.WindowRounding, 0.0f);
         }

         if (myIsCollapsed)
         {
            //title bar only
            canvas.addRectFilled(titleBarRect, ImGui.style.getColor(ElementColor.TitleBgCollapsed, alpha), ImGui.style.windowRounding, Canvas.Corners.ALL);
            canvas.addText(titleBarRect, ImGui.style.getColor(ElementColor.Text), name, Alignment.Middle);
         }
         else
         {
            //title bar
            if (flags.HasFlag(Flags.TitleBar) == true)
            {
               canvas.addRectFilled(titleBarRect, ImGui.style.getColor(ElementColor.TitleBg, alpha), ImGui.style.windowRounding, Canvas.Corners.TOP);
               canvas.addText(titleBarRect, ImGui.style.getColor(ElementColor.Text), name, Alignment.Middle);
            }

            //menu bar
            if (flags.HasFlag(Flags.MenuBar) == true)
            {
               canvas.addRectFilled(menuBarRect, ImGui.style.getColor(ElementColor.MenuBarBg, alpha));
            }
            //scroll bars

            //resize grip

            //borders
            if (flags.HasFlag(Flags.Borders) == true)
            {

               canvas.addRect(Rect.fromPosSize(position + Vector2.One, size), ImGui.style.getColor(ElementColor.BorderShadow), ImGui.style.windowRounding, Canvas.Corners.ALL);
               canvas.addRect(Rect.fromPosSize(position, size - Vector2.One), ImGui.style.getColor(ElementColor.Border), ImGui.style.windowRounding, Canvas.Corners.ALL);

               if (flags.HasFlag(Flags.TitleBar) == false)
               {
                  canvas.addLine(titleBarRect.NW + new Vector2(1, 0), titleBarRect.NE - new Vector2(1, 0), ImGui.style.getColor(ElementColor.Border));
               }
            }
         }

         //pop root special style
         if (flags.HasFlag(Flags.Root))
         {
            ImGui.style.popStyleVar(1);
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
         g.myCursorPos.X = ImGui.style.framePadding.X;
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