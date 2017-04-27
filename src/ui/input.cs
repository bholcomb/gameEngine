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

   public class MouseState
   {
      public const int MAX_BUTTONS = 10;

      //mouse state
      public Vector2 pos { get; set; }
      public Vector2 posPrev { get; set; }
      public Vector2 posDelta { get; set; }
      public bool[] buttonDown = new bool[MAX_BUTTONS];
      public bool[] buttonClicked = new bool[MAX_BUTTONS];
      public double[] buttonClickedTime = new double[MAX_BUTTONS];
      public bool[] buttonDoubleClicked = new bool[MAX_BUTTONS];
      public bool[] buttonReleased = new bool[MAX_BUTTONS];
      public bool[] buttonDownOwned = new bool[MAX_BUTTONS];
      public double[] buttonDownDuration = new double[MAX_BUTTONS];
      public double[] buttonDownDurationPrev = new double[MAX_BUTTONS];
      public float[] buttonDragMaxDistanceSqr = new float[MAX_BUTTONS];
      public Vector2[] buttonClickedPos = new Vector2[MAX_BUTTONS];
      public float wheel { get; set; }
      public float wheelPrev { get; set; }
      public float wheelDelta { get; set; }
      public bool drawCursor { get; set; }

      public MouseState()
      {
         pos = new Vector2();
         posPrev = new Vector2();
         posDelta = new Vector2();
         for (int i = 0; i < MAX_BUTTONS; i++)
         {
            buttonClickedPos[i] = new Vector2();
         }
      }

      public bool isButtonClicked(MouseButton button, bool repeat)
      {
         float t = (float)buttonDownDuration[(int)button];
         if (t == 0.0)
         {
            return true;
         }

         if (repeat && t > ImGui.keyRepeatDelay)
         {
            float delay = ImGui.keyRepeatDelay;
            float rate = ImGui.keyRepeatRate;
            float halfRate = rate * 0.5f;
            if (((t - delay % rate) > halfRate) != (((t - delay - ImGui.dt) % rate) > halfRate))
               return true;
         }

         return false;
      }

      public void newFrame()
      {
         posDelta = pos - posPrev;
         posPrev = pos;

         wheelDelta = wheel - wheelPrev;
         wheelPrev = wheel;

         for (int i = 0; i < MAX_BUTTONS; i++)
         {
            buttonClicked[i] = buttonDown[i] && buttonDownDuration[i] < 0;
            buttonReleased[i] = !buttonDown[i] && buttonDownDuration[i] >= 0;
            buttonDownDurationPrev[i] = buttonDownDuration[i];
            buttonDownDuration[i] = buttonDown[i] ? (buttonDownDuration[i] < 0 ? 0 : buttonDownDuration[i] + ImGui.dt) : -1.0;
            buttonDoubleClicked[i] = false;

            if (buttonClicked[i])
            {
               if (ImGui.time - buttonClickedTime[i] < ImGui.mouseDoubleClickTime)
               {
                  if ((pos - buttonClickedPos[i]).LengthSquared < ImGui.mouseDoubleClickDistance * ImGui.mouseDoubleClickDistance)
                  {
                     buttonDoubleClicked[i] = true;
                     buttonClickedTime[i] = -1;
                  }
               }
               else
               {
                  buttonClickedTime[i] = ImGui.time;
               }

               buttonClickedPos[i] = pos;
               buttonDragMaxDistanceSqr[i] = 0;
            }
            else if (buttonDown[i])
            {
               buttonDragMaxDistanceSqr[i] = Math.Max(buttonDragMaxDistanceSqr[i], (pos - buttonClickedPos[i]).LengthSquared);
            }
         }
      }
   }

   public class KeyboardState
   {
      public Util.KeyModifiers modifiers { get; set; }
      public bool[] keysDown = new bool[256];
      public double[] keysDownDuration = new double[256];
      public double[] keysDownDurationPrev = new double[256];

      public KeyboardState()
      {

      }

      public void newFrame()
      {
         keysDownDuration.CopyTo(keysDownDurationPrev, 0);
         for (int i = 0; i < 256; i++)
         {
            if (keysDown[i] == true)
            {
               keysDownDuration[i] = keysDownDuration[i] < 0 ? 0 : keysDownDuration[i] + ImGui.dt;
            }
            else
            {
               keysDownDuration[i] = -1.0;
            }
         }
      }

      public bool keyPressed(Key key)
      {
         return keysDownDuration[(int)key] >= 0.0;
      }

      public bool keyReleased(Key key)
      {
         //key is no long pressed, but it was, so it has been released
         return (keysDown[(int)key] == false) && (keysDownDurationPrev[(int)key] > 0.0);
      }

      public bool keyJustPressed(Key key)
      {
         return (keysDown[(int)key] == true) && (keysDownDuration[(int)key] == 0.0);
      }
   }
}