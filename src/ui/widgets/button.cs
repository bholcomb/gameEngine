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

   public static partial class ImGui
   {
      static bool buttonBehavior(Rect r, UInt32 id, out bool hovered, out bool held, ButtonFlags flags=0 )
      {
         Window win = currentWindow;
         if(flags.HasFlag(ButtonFlags.Disabled) == true)
         {
            hovered = false;
            held = false;
            return false;
         }

         bool pressed = false;
         
         //move mouse coords into window space by substracting window position
         hovered = r.containsPoint(mouse.pos.X - win.position.X, mouse.pos.Y - win.position.Y);
         if (hovered)
         {
            hoveredId = id;
            if (!flags.HasFlag(ButtonFlags.NoKeyModifiers) || (!keyboard.keyPressed(Key.ControlLeft) && !keyboard.keyPressed(Key.ShiftLeft) && !keyboard.keyPressed(Key.LAlt)))
            {
               if(mouse.buttonClicked[(int)MouseButton.Left])
               {
                  if(flags.HasFlag(ButtonFlags.PressedOnClick))
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
               else if(mouse.buttonReleased[(int)MouseButton.Left] && flags.HasFlag(ButtonFlags.PressedOnRelease))
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
         if(activeId == id)
         {
            if(mouse.buttonDown[(int)MouseButton.Left])
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

      public static bool button(String s, Vector2 size)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;

         UInt32 id = win.getChildId(s);
         Vector2 labelSize = style.textSize(s);

         //move cursor down for the size of the text accounting for the padding
         Vector2 pos = win.cursorPosition + new Vector2(0, -size.Y);
         
         Rect r = new Rect(pos, pos + size);

         bool hovered;
         bool held;
         bool pressed = buttonBehavior(r, id, out hovered, out held);
         
         ElementColor useColor = (hovered && held) ? ElementColor.ButtonActive : (hovered ? ElementColor.ButtonHovered : ElementColor.Button);
         Color4 col = style.colors[(int)useColor];

         //draw the thing
         style.pushStyleVar(StyleVar.FrameRounding, 9.0f);
         win.canvas.addRectFilled(r, col, style.frameRounding);
         win.canvas.addRect(r, style.colors[(int)ElementColor.Border], style.frameRounding);
         Vector4 clip = new Vector4(r.left + win.position.X, r.bottom + win.position.Y, r.width, r.height);
         win.canvas.pushClipRect(clip);

         //center text in button
         win.canvas.addText(r, style.colors[(int)ElementColor.Text], s, Alignment.Middle);

         //cleanup
         win.canvas.popClipRect();
         style.popStyleVar(1);

         //update the window cursor
         win.addItem(size);

         return pressed;
      }

      public static bool button(Texture t, Vector2 size)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;

         UInt32 id = win.getChildId(t.ToString());
         Rect r = new Rect(win.cursorPosition, win.cursorPosition + size);

         bool hovered;
         bool held;
         bool pressed = buttonBehavior(r, id, out hovered, out held);

         //draw the thing
         win.canvas.addImage(t, r, Canvas.uv_zero, Canvas.uv_one, Canvas.col_white);
         win.canvas.addRect(r, style.colors[(int)ElementColor.Border], style.frameRounding);

         //update the window cursor
         win.addItem(size);

         return pressed;
      }

      public static bool button(ArrayTexture t, int idx, Vector2 size)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;

         UInt32 id = win.getChildId(t.ToString());
         Rect r = new Rect(win.cursorPosition, win.cursorPosition + size);

         bool hovered;
         bool held;
         bool pressed = buttonBehavior(r, id, out hovered, out held);

         //draw the thing
         //win.canvas.addCustomRenderCommand(new RenderTexture2DCommand(r.SW, r.NE, t, idx));
         win.canvas.addRect(r, style.colors[(int)ElementColor.Border], style.frameRounding);

         //update the window cursor
         win.addItem(size);

         return pressed;
      }
   }
}