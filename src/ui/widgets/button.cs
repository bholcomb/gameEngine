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
   public enum ButtonFlags
   {
      Repeat = 1 << 0,
      PressedOnClick = 1 << 1,
      PressedOnRelease = 1 << 2,
      FlattenChildren = 1 << 3, 
      DontClosePopups = 1 << 4,
      Disabled = 1 << 5,
      AlignTextBaseLine = 1 << 6,
      NoKeyModifiers = 1 << 7
   };

   public static partial class UI
   {
      #region widgets
      public static bool button(String s, Vector2 size)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;

         win.addItem(style.button.padding);

         UInt32 id = win.getChildId(s);
         Vector2 pos = win.cursorScreenPosition;
         Rect r = Rect.fromPosSize(pos, size);

         bool hovered;
         bool held;
         bool pressed = buttonBehavior(r, id, out hovered, out held);

         drawButtonBackground(r, win, hovered, held);
         drawButtonText(s, r, win, hovered, held);

         //update the window cursor
         win.addItem(size);

         return pressed;
      }

      public static bool button(Texture t, Vector2 size)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;


         win.addItem(style.button.padding);

         UInt32 id = win.getChildId(t.ToString());
         Vector2 pos = win.cursorScreenPosition;
         Rect r = Rect.fromPosSize(pos, size);

         bool hovered;
         bool held;
         bool pressed = buttonBehavior(r, id, out hovered, out held);

         drawButtonBackground(r, win, hovered, held);

         Rect ir = r;
         ir.shrink(style.button.imagePadding);
         win.canvas.addImage(t, ir, Canvas.uv_zero, Canvas.uv_one, Canvas.col_white);

         //update the window cursor         
         win.addItem(size);

         return pressed;
      }

      public static bool button(ArrayTexture t, int idx, Vector2 size)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;

         win.addItem(style.button.padding);

         string name = t.ToString() + "-" + idx.ToString();
         UInt32 id = win.getChildId(name);
         Vector2 pos = win.cursorScreenPosition;
         Rect r = Rect.fromPosSize(pos, size);

         bool hovered;
         bool held;
         bool pressed = buttonBehavior(r, id, out hovered, out held);

         drawButtonBackground(r, win, hovered, held);

         //draw the thing (need to convert to screen space)
         Vector2 min = new Vector2(r.left, displaySize.Y - r.top);
         Vector2 max = new Vector2(r.right, displaySize.Y - r.bottom);
         RenderTexture2dCommand cmd = new RenderTexture2dCommand(min, max, t, idx, pressed ? .25f : 1.0f);
         win.canvas.addCustomRenderCommand(cmd);

         //update the window cursor
         win.addItem(size);

         return pressed;
      }
      
      #endregion

      #region behavior
      static bool buttonBehavior(Rect r, UInt32 id, out bool hovered, out bool held, ButtonFlags flags = 0)
      {
         Window win = currentWindow;

         if (flags.HasFlag(ButtonFlags.Disabled) == true || win != hoveredWindow)
         {
            hovered = false;
            held = false;
            return false;
         }

         bool pressed = false;

         //move mouse coords into window space by substracting window position
         hovered = r.containsPoint(mouse.pos.X, mouse.pos.Y);
         if (hovered)
         {
            hoveredId = id;
            if (!flags.HasFlag(ButtonFlags.NoKeyModifiers) || (!keyboard.keyPressed(Key.ControlLeft) && !keyboard.keyPressed(Key.ShiftLeft) && !keyboard.keyPressed(Key.LAlt)))
            {
               if (mouse.buttonAction(MouseAction.CLICKED, MouseButton.Left) == true)
               {
                  if (flags.HasFlag(ButtonFlags.PressedOnClick))
                  {
                     pressed = true;
                     setActiveId(0);
                  }
                  else
                  {
                     setActiveId(id);
                  }
                  focusWindow(win);
               }
               else if (mouse.buttonAction(MouseAction.RELEASED, MouseButton.Left) && flags.HasFlag(ButtonFlags.PressedOnRelease))
               {
                  pressed = true;
                  setActiveId(0);
               }
               else if (flags.HasFlag(ButtonFlags.Repeat) && activeId == id && mouse.isButtonClicked(MouseButton.Left, true)) //mouse was clicked
               {
                  pressed = true;
               }
            }
         }

         held = false;
         if (activeId == id)
         {
            if (mouse.buttons[(int)MouseButton.Left].down)
            {
               held = true;
            }
            else
            {
               if (hovered)
               {
                  pressed = true;
               }
               setActiveId(0);
            }
         }

         return pressed;
      }

      #endregion

      #region draw functions
      static void drawButtonText(String s, Rect r, Window win, bool hovered, bool held)
      {
         Color4 textColor = style.button.textNormal;
         if (hovered) textColor = style.button.textHover;
         if (held) textColor = style.button.textActive;

         //draw text
         win.canvas.pushClipRect(r);

         win.canvas.addText(r, textColor, s, style.button.textAlignment);

         win.canvas.popClipRect();
      }
      
      static void drawButtonBackground(Rect r, Window win, bool hovered, bool held)
      {
         StyleItem background = style.button.normal;
         if (hovered) background = style.button.hover;
         if (held) background = style.button.active;

         switch(background.type)
         {
            case StyleItem.Type.COLOR:
               win.canvas.addRectFilled(r, background.color, style.button.rounding);
               win.canvas.addRect(r, style.button.borderColor, style.button.rounding);
               break;
            case StyleItem.Type.IMAGE:
               win.canvas.addImage(background.image, r);
               break;
            case StyleItem.Type.SPRITE:
               win.canvas.addImage(background.sprite, r);
               break;
            case StyleItem.Type.NINEPATCH:
               win.canvas.addImage(background.patch, r);
               break;
         }
      }
      #endregion

   }
}