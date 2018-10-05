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
   public static partial class UI
   {
      public enum SliderFlags
      {
         Vertical = 1 << 0
      };

      #region widgets
      public static bool slider(String s, ref float val, float min, float max, String displayFormat = "")
      {
         Window win = currentWindow;
         if (win.skipItems)
            return false;

         UInt32 id = win.getChildId(s);

         win.addItem(style.slider.padding);

         float width = win.size.X * 0.65f;
         Vector2 labelSize = style.font.size(s);

         //move cursor down for the size of the text accounting for the padding
         Vector2 pos = win.cursorScreenPosition;
         Rect sliderRect = Rect.fromPosSize(pos, new Vector2(width, labelSize.Y) + style.slider.padding);
         Rect totalRect = Rect.fromPosSize(pos, sliderRect.size + new Vector2(labelSize.X, 0));

         win.addItem(totalRect.size);

         bool hovered = false;
         bool isHorizontal = true;
        
         bool valChanged = sliderBehavior(sliderRect, id, ref val, min, max, ref hovered);

         string valString = "";
         if (displayFormat != "")
         {
            valString = String.Format(displayFormat, val);
         }
         else
         {
            valString = String.Format("{0:0.00}", val);
         }

         float grab_t = (MathExt.clamp<float>(val, min, max) - min) / (max - min);
         if (!isHorizontal)
         {
            grab_t = 1.0f - grab_t;
         }

         //drawing
         drawSliderBackground(sliderRect, win, hovered);
         drawSliderGrabber(sliderRect, win, isHorizontal, grab_t, activeId == id);
         drawSliderValueText(sliderRect, win, valString);
         drawSliderLabelText(totalRect, win, s);

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

         win.addItem(style.slider.padding);

         float width = win.size.X * 0.65f;
         Vector2 labelSize = style.font.size(s);

         //move cursor down for the size of the text accounting for the padding
         Vector2 pos = win.cursorScreenPosition;
         Rect sliderRect = Rect.fromPosSize(pos, new Vector2(width, labelSize.Y) + style.slider.padding);
         Rect totalRect = Rect.fromPosSize(pos, sliderRect.size + new Vector2(labelSize.X, 0));

         win.addItem(totalRect.size);

         bool isHorizontal = true;
         bool hovered = false;

         int min = Enum.GetValues(typeof(T)).GetLowerBound(0);
         int max = Enum.GetValues(typeof(T)).GetUpperBound(0);
         float val = (float)Convert.ToInt32(enumVal);

         bool valChanged = sliderBehavior(sliderRect, id, ref val, min, max, ref hovered);
         enumVal = (T)(Object)((int)val);
         string valString = Enum.GetName(typeof(T), enumVal);


         float grab_t = (MathExt.clamp<float>(val, min, max) - min) / (max - min);
         if (!isHorizontal)
         {
            grab_t = 1.0f - grab_t;
         }

         //drawing
         drawSliderBackground(sliderRect, win, hovered);
         drawSliderGrabber(sliderRect, win, isHorizontal, grab_t, activeId == id);
         drawSliderValueText(sliderRect, win, valString);
         drawSliderLabelText(totalRect, win, s);

         return valChanged;
      }
      #endregion

      #region behavior
      static bool sliderBehavior(Rect r, UInt32 id, ref float val, float min, float max, ref bool hovered, SliderFlags flags = 0)
      {
         Window win = currentWindow;
         if (win != hoveredWindow)
         {
            return false;
         }

         hovered = r.containsPoint(mouse.pos);
         if (hovered)
         {
            hoveredId = id;
         }

         if (hovered && mouse.buttonAction(MouseAction.CLICKED, MouseButton.Left))
         {
            setActiveId(id);
         }

         bool isHorizontal = flags.HasFlag(SliderFlags.Vertical) == false;
         float grabPadding = 2.0f;
         float sliderSize = isHorizontal ? r.width - grabPadding * 2.0f : r.height - grabPadding * 2.0f;
         float grabSize = Math.Min(style.slider.cursorSize.X, sliderSize);
         float siderUsableSize = sliderSize - grabSize;
         float sliderMinPos = (isHorizontal ? r.SW.X : r.SW.Y) + grabPadding + grabSize * 0.5f;
         float sliderMaxPos = (isHorizontal ? r.NE.X : r.NE.Y) - grabPadding - grabSize * 0.5f;

         bool valueChanged = false;
         if (activeId == id)
         {
            if (mouse.buttons[(int)MouseButton.Left].down == true)
            {
               float mouseAbsPos = isHorizontal ? (mouse.pos.X) : (mouse.pos.Y);
               float normalizedPos = MathExt.clamp<float>((mouseAbsPos - sliderMinPos) / siderUsableSize, 0.0f, 1.0f);
               if (!isHorizontal)
               {
                  normalizedPos = 1.0f - normalizedPos; //reverse it
               }

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

         return valueChanged;
      }

      #endregion

      #region drawing
      static void drawSliderBackground(Rect r, Window win, bool hovered)
      {
         win.canvas.addRect(r, style.slider.barNormal, style.slider.rounding);
         if (hovered)
         {
            win.canvas.addRect(r, Color4.Red);
         }

      }

      static void drawSliderGrabber(Rect r, Window win, bool isHorizontal, float grab_t, bool active)
      {
         float grabPadding = 2.0f;
         float sliderSize = isHorizontal ? r.width - grabPadding * 2.0f : r.height - grabPadding * 2.0f;
         float grabSize = Math.Min(style.slider.cursorSize.X, sliderSize);
         float siderUsableSize = sliderSize - grabSize;
         float sliderMinPos = (isHorizontal ? r.SW.X : r.SW.Y) + grabPadding + grabSize * 0.5f;
         float sliderMaxPos = (isHorizontal ? r.NE.X : r.NE.Y) - grabPadding - grabSize * 0.5f;

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

         Color4 col = style.slider.cursorNormal.color;
         if (active) col = style.slider.cursorActive.color;
         win.canvas.addRectFilled(grabRect, col, style.slider.rounding);
      }

      static void drawSliderValueText(Rect r, Window win, string valueText)
      {
         win.canvas.addText(r, style.text.color, valueText, Alignment.Middle);
      }

      static void drawSliderLabelText(Rect r, Window win, String labelText)
      {
         if (labelText != "")
         {
            Rect textRect = new Rect(r.SE + style.slider.padding, r.NW + style.slider.padding);
            win.canvas.addText(textRect, style.text.color, labelText, Alignment.VCenter);
         }
      }

      #endregion
   }
}