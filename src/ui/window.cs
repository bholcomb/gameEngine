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
         NoTitleBar = 1 << 0,
         NoResize = 1 << 1,
         NoMove = 1 << 2,
         NoScrollBar = 1 << 3,
         NoScrollWithMouse = 1 << 4,
         NoCollapse = 1 << 5,
         AlwaysAutoResize = 1 << 6,
         ShowBorders = 1 << 7,
         NoBackground = 1 << 8,

         NoInputs = 1 << 9,
         MenuBar = 1 << 10,
         HorizontalScrollbar = 1 << 11,
         NoFocusOnAppearing = 1 << 12,
         NoBringToFrontOnFocus = 1 << 13,

         //internal
         ChildWindow = 1 << 20,
         ChildWindowAutoFitX = 1 << 21,
         ChildWindowAutoFitY = 1 << 22,
         ComboBox = 1 << 23,
         Tooltip = 1 << 24,
         Popup = 1 << 25,
         Modal = 1 << 26,
         ChildMenu = 1 << 27,

         Root = NoTitleBar | NoBackground | NoResize | NoMove | NoMove | NoCollapse | NoInputs | MenuBar,
      };

      public enum Layout
      {
         Vertical,
         Horizontal
      }

      struct Group
      {
         public Vector2 myBackupCursorPosition;
         public Vector2 myBackupCursorSize;
         public Layout myBackupLayout;
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

      //layout stuff
      Vector2 myCursorPos; //this is a screen space value
      Vector2 myCursorSize; //this is the largest bounds of all the drawn stuff
      Layout myLayoutType;

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

      public Window(String winName)
      {
         parentWindow = null;
         childWindow = null;
         siblingWindow = null;

         flags |= Flags.ShowBorders;

         //setup the ID of this window
         name = winName;
         id = ImGui.idStack.getId(winName);
         moveId = getChildId("MOVE");

         mySetPositionAllowFlags = mySetSizeAllowFlags = SetCondition.Always | SetCondition.Appearing | SetCondition.FirstUseEver | SetCondition.Once;

         menuColums = new MenuColumns(this);
      }

      public UInt32 getChildId(String name)
      {
         ImGui.idStack.push(id);
         UInt32 childId = ImGui.idStack.getId(name);
         ImGui.idStack.pop();
         ImGui.keepAliveId(childId);
         return childId;
      }

      public Vector2 cursorPosition
      {
         //this is in window space coordinates.  The conversion to screen space happens during rendering
         get { return myCursorPos; }
         set { myCursorPos = value; }
      }

      public Vector2 cursorScreenPosition
      {
         get { return myCursorPos + position; }
         set { myCursorPos = value - position; }
      }

      public void addItem(Vector2 itemSize)
      {
         myCursorSize.X = Math.Max(myCursorSize.X, myCursorPos.X + itemSize.X);
         myCursorSize.Y = Math.Max(myCursorSize.Y, (size.Y - myCursorPos.Y) + itemSize.Y);

         switch (myLayoutType)
         {
            case Layout.Horizontal:
               {
                  myCursorPos.X += itemSize.X;
                  break;
               }
            case Layout.Vertical:
               {
                  myCursorPos.Y -= itemSize.Y;
                  break;
               }
         }
      }

      public void nextLine()
      {
         myCursorPos.X = ImGui.style.framePadding.X;
         myCursorPos.Y = myCursorSize.Y;
      }

      public bool begin(ref bool closed)
      {
			if (closed == true)
				return false;

         //is this the first time we've begun this window?
         if (lastFrameActive != ImGui.frame)
         {
            if (flags.HasFlag(Flags.AlwaysAutoResize))
            {
               //from the previous frame
               size = myCursorSize + ImGui.style.displayWindowPadding;
            }

            lastFrameActive = ImGui.frame;
            active = true;
            myCursorPos = new Vector2(0, size.Y);
            myCursorSize = new Vector2(0, 0);
            myLayoutType = Layout.Vertical;
         }

         if (ImGui.currentWindow != this)
         {
            ImGui.pushWindow(this);
         }

         //process the setNextWindow* functions
         if (ImGui.setNextWindowPositionCondition != 0)
         {
            bool shouldSet = (mySetPositionAllowFlags & ImGui.setNextWindowPositionCondition) != 0;
            if (shouldSet && (ImGui.setNextWindowPositionValue != ImGui.theInvalidVec2))
            {
               setPosition(ImGui.setNextWindowPositionValue, ImGui.setNextWindowPositionCondition);
               ImGui.setNextWindowPositionCondition = 0;
               ImGui.setNextWindowPositionValue = ImGui.theInvalidVec2;
            }
         }

         if (ImGui.setNextWindowSizeCondition != 0)
         {
            bool shouldSet = (mySetSizeAllowFlags & ImGui.setNextWindowSizeCondition) != 0;
            if (shouldSet && (ImGui.setNextWindowSizeValue != ImGui.theInvalidVec2))
            {
               setSize(ImGui.setNextWindowSizeValue, ImGui.setNextWindowSizeCondition);
               ImGui.setNextWindowSizeCondition = 0;
               ImGui.setNextWindowSizeValue = ImGui.theInvalidVec2;
            }
         }

         //check for collapsing window
         if (!flags.HasFlag(Flags.NoTitleBar) && !flags.HasFlag(Flags.NoCollapse))
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
               if ((flags & Flags.NoMove) == 0)
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

         drawWindow();

         skipItems = myIsCollapsed;

         return true;
      }

      public Rect rect { get { return Rect.fromPosSize(position, size); } }
      public float titleBarHeight { get { return flags.HasFlag(Flags.NoTitleBar) ? 0.0f : ImGui.style.currentFontSize + ImGui.style.framePadding2x.Y; } }
      public Rect titleBarRect { get { return Rect.fromPosSize(new Vector2(0, size.Y - titleBarHeight), new Vector2(size.X, titleBarHeight)); } }
      public float menuBarHeight { get { return flags.HasFlag(Flags.MenuBar) ? ImGui.style.currentFontSize + ImGui.style.framePadding2x.Y : 0.0f; } }
      public Rect menuBarRect { get { float y1 = titleBarHeight; return new Rect(0, size.Y - y1 - menuBarHeight, size.X, size.Y - y1); } }

      public bool end()
      {
         canvas.popClipRect();
         return true;
      }

      public bool mouseOverTitle()
      {
         return titleBarRect.containsPoint(ImGui.mouse.pos - position);
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

      public Vector2 position
      {
         get { return myPosition; }
         set { myPosition = value; }
      }

      void drawWindow()
      {
         canvas.reset();
         canvas.setPosition(position);
         float alpha = ImGui.style.alpha;

         if (flags.HasFlag(Flags.Root))
         {
            canvas.pushClipRectFullscreen();
            ImGui.style.pushStyleVar(StyleVar.WindowRounding, 0.0f);
         }
         else
         {
            canvas.pushClipRect(new Vector4(position.X, position.Y, size.X, size.Y));
         }

         if (myIsCollapsed)
         {
            //title bar
            Rect tbr = Rect.fromPosSize(new Vector2(0, 0), new Vector2(size.X, titleBarHeight));
            canvas.addRectFilled(tbr.SW, tbr.NE, ImGui.style.getColor(ElementColor.TitleBgCollapsed, alpha), ImGui.style.windowRounding, Canvas.Corners.ALL);
            canvas.addText(myCursorPos, ImGui.style.getColor(ElementColor.Text), name);
         }
         else
         {
            //window background
            if(!flags.HasFlag(Flags.NoBackground))
            {
               canvas.addRectFilled(new Vector2(0, 0), size, ImGui.style.getColor(ElementColor.WindowBg, myBackgroundAlpha), ImGui.style.windowRounding);
            }

            //title bar
            if (!flags.HasFlag(Flags.NoTitleBar))
            {
               Rect tbr = titleBarRect;
               canvas.addRectFilled(tbr.SW, tbr.NE, ImGui.style.getColor(ElementColor.TitleBg, alpha), ImGui.style.windowRounding, Canvas.Corners.TOP);
               ImGui.label(name);
            }

            //menu bar
            if (flags.HasFlag(Flags.MenuBar))
            {
               Rect mbr = menuBarRect;
               canvas.addRectFilled(mbr.SW, mbr.NE, ImGui.style.getColor(ElementColor.MenuBarBg, alpha));
            }
            //scroll bars

            //resize grip

            //borders
            if (flags.HasFlag(Flags.ShowBorders))
            {
               canvas.addRect(Vector2.One, size, ImGui.style.getColor(ElementColor.BorderShadow), ImGui.style.windowRounding, Canvas.Corners.ALL);
               canvas.addRect(Vector2.Zero, size - Vector2.One, ImGui.style.getColor(ElementColor.Border), ImGui.style.windowRounding, Canvas.Corners.ALL);

               if (flags.HasFlag(Flags.NoTitleBar) == false)
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
         myLayoutType = layout;
      }

      public void beginGroup()
      {
         Group g = new Group();
         g.myBackupCursorPosition = myCursorPos;
         g.myBackupCursorSize = myCursorSize;
         myCursorSize = Vector2.Zero;
         g.myBackupLayout = myLayoutType;
         myGroupStack.Push(g);
      }

      public void endGroup()
      {
         Group g = myGroupStack.Pop();
         Rect rect = Rect.fromPosSize(g.myBackupCursorPosition, myCursorSize);

         myCursorPos = g.myBackupCursorPosition;
         myCursorSize.X = Math.Max(g.myBackupCursorSize.X, myCursorSize.X);
         myCursorSize.Y = Math.Max(g.myBackupCursorSize.Y, myCursorSize.Y);
         myLayoutType = g.myBackupLayout;

         addItem(rect.size);
      }
   }
}