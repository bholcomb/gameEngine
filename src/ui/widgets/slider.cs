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
   public static partial class ImGui
   {
      public enum SliderFlags
      {
         Vertical = 1 << 0
      };

      static bool sliderBehavior(Rect r, UInt32 id, ref float val, float min, float max, SliderFlags flags = 0)
      {
         Window win = currentWindow;

         bool hovered = r.containsPoint(mouse.pos.X - win.position.X, mouse.pos.Y - win.position.Y);
         if (hovered)
         {
            hoveredId = id;
            win.canvas.addRect(r, Color4.Red);
         }

         if (hovered && mouse.buttonClicked[(int)MouseButton.Left])
         {
            setActiveId(id);
         }

         bool isHorizontal = flags.HasFlag(SliderFlags.Vertical) == false;
         float grabPadding = 2.0f;
         float sliderSize = isHorizontal ? r.width - grabPadding * 2.0f : r.height - grabPadding * 2.0f;
         float grabSize = Math.Min(style.grabMinSize, sliderSize);
         float siderUsableSize = sliderSize - grabSize;
         float sliderMinPos = (isHorizontal ? r.SW.X : r.SW.Y) + grabPadding + grabSize * 0.5f;
         float sliderMaxPos = (isHorizontal ? r.NE.X : r.NE.Y) - grabPadding - grabSize * 0.5f;

         bool valueChanged = false;
         if (activeId == id)
         {
            if (mouse.buttonDown[(int)MouseButton.Left] == true)
            {
               float mouseAbsPos = isHorizontal ? (mouse.pos.X - win.position.X) : (mouse.pos.Y - win.position.Y);
               float normalizedPos = MathExt.clamp<float>((mouseAbsPos - sliderMinPos) / siderUsableSize, 0.0f, 1.0f);
               if (!isHorizontal)
                  normalizedPos = 1.0f - normalizedPos; //reverse it

               float newValue = MathExt.lerp(min, max, normalizedPos);
               if (val != newValue)
               {
                  val = newValue;
                  valueChanged = true;
               }

            }
            else
            {
               setActiveId(0);
            }
         }

         float grab_t = (MathExt.clamp<float>(val, min, max) - min) / (max - min);

         //drawing
         win.canvas.addRect(r, style.colors[(int)ElementColor.FrameBg], style.frameRounding);

         if (!isHorizontal)
            grab_t = 1.0f - grab_t;

         float grabPosition = MathExt.lerp(sliderMinPos, sliderMaxPos, grab_t);
         Rect grabRect;
         if (isHorizontal)
         {
            grabRect = new Rect(new Vector2(grabPosition - grabSize * 0.5f, r.SW.Y + grabPadding),
                                new Vector2(grabPosition + grabSize * 0.5f, r.NE.Y - grabPadding));
         }
         else
         {
            grabRect = new Rect(new Vector2(r.SW.X + grabPadding, grabPosition + grabSize * 0.5f),
                                new Vector2(r.NE.X - grabPadding, grabPosition + grabSize * 0.5f));
         }

         win.canvas.addRectFilled(grabRect, activeId == id ? style.colors[(int)ElementColor.SliderGrabActive] : style.colors[(int)ElementColor.SliderGrabActive], style.grabRounding);

         return valueChanged;
      }

      public static bool slider(String s, ref float val, float min, float max, String displayFormat = "")
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;

         UInt32 id = win.getChildId(s);

         float width = win.size.X * 0.65f;
         Vector2 labelSize = style.textSize(s);

         //move cursor down for the size of the text accounting for the padding
         Vector2 pos = win.cursorPosition + new Vector2(0, -(labelSize.Y + style.framePadding2x.Y));
         Rect sliderRect = new Rect(pos, pos + new Vector2(width, labelSize.Y + style.framePadding2x.Y));
         Rect totalRect = new Rect(sliderRect.SW, sliderRect.NE + new Vector2(style.itemInnerSpacing.X + labelSize.X, 0));

         win.addItem(totalRect.size);

         bool valChanged = sliderBehavior(sliderRect, id, ref val, min, max, 0);

         string valString = "";
         if (displayFormat != "")
         {
            valString = String.Format(displayFormat, val);
         }
         else
         {
            valString = String.Format("{0:0.00}", val);
         }

         win.canvas.addText(sliderRect, style.colors[(int)ElementColor.Text], valString, Alignment.Middle);

         if (s != "")
         {
            Rect textRect = new Rect(sliderRect.SE + style.framePadding, totalRect.NW - style.framePadding);
            win.canvas.addText(textRect, style.colors[(int)ElementColor.Text], s, Alignment.VCenter);
         }

         return valChanged;
      }

      public static bool slider(String s, ref int val, int min, int max, String displayFormat = "")
      {
         float tval = (float)val;

         bool changed = slider(s, ref tval, (float)min, (float)max, displayFormat == "" ? "{0:0}" : displayFormat);
         val = (int)tval;

         return changed;
      }

      //to be used for enumerations
      public static bool slider<T>(String s, ref T enumVal) 
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;

         UInt32 id = win.getChildId(s);

         float width = win.size.X * 0.65f;
         Vector2 labelSize = style.textSize(s);

         //move cursor down for the size of the text accounting for the padding
         Vector2 pos = win.cursorPosition + new Vector2(0, -(labelSize.Y + style.framePadding2x.Y));
         Rect sliderRect = new Rect(pos, pos + new Vector2(width, labelSize.Y + style.framePadding2x.Y));
         Rect totalRect = new Rect(sliderRect.SW, sliderRect.NE + new Vector2(style.itemInnerSpacing.X + labelSize.X, 0));

         win.addItem(totalRect.size);

         int min = Enum.GetValues(typeof(T)).GetLowerBound(0);
         int max = Enum.GetValues(typeof(T)).GetUpperBound(0);
         float val = (float)Convert.ToInt32(enumVal);

         bool valChanged = sliderBehavior(sliderRect, id, ref val, min, max, 0);

         enumVal = (T)(Object)((int)val);

         string valString = Enum.GetName(typeof(T), enumVal);
         win.canvas.addText(sliderRect, style.colors[(int)ElementColor.Text], valString, Alignment.Middle);

         if (s != "")
         {
            Rect textRect = new Rect(sliderRect.SE + style.framePadding, totalRect.NW - style.framePadding);
            win.canvas.addText(textRect, style.colors[(int)ElementColor.Text], s, Alignment.VCenter);
         }

         return valChanged;
      }
   }
}