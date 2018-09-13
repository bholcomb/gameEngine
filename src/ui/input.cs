using System;

using Util;

using OpenTK;
using OpenTK.Input;

namespace GUI
{
	//pressed is down this frame
	//released is going up this frame
	//clicked is a down and up motion
	//double click is if there has been 2 clicks within the double click period
	public enum MouseAction { PRESSED, RELEASED, CLICKED, DOUBLE_CLICKED };

	public class MouseButtonState
	{
		public bool pressed;
		public bool released;
		public bool doubleClicked;
	
		public bool down;
		public double pressedTime;
		public Vector2 pressedPos = new Vector2();
		public bool downOwned;
		public double downDuration;
		public double downDurationPrev;
		public float dragMaxDistanceSqr;
	}

   public class MouseState
   {
      public const int MAX_BUTTONS = 10;

      public float doubleClickTime = 0.3f;
      public float doubleClickDistance = 6.0f;
      public float dragThreshold = 6.0f;
      public float buttonRepeatDelay = 0.25f;
      public float buttonRepeatRate = 0.02f;

      //mouse state
      public Vector2 pos;
		public Vector2 prev;
		public Vector2 delta;
		public Vector2 scrollDelta;
		public bool grab;
		public bool grabbed;
		public bool ungrab;

      public MouseButtonState[] buttons = new MouseButtonState[MAX_BUTTONS];
		public float wheel;
		public float wheelPrev;
		public float wheelDelta;

		public bool drawCursor;

      public MouseState()
      {
         pos = new Vector2();
         prev = new Vector2();
         delta = new Vector2();
         for (int i = 0; i < MAX_BUTTONS; i++)
         {
				buttons[i] = new MouseButtonState();
         }
      }

      public bool isButtonClicked(MouseButton button, bool repeat = false)
      {
			MouseButtonState mb = buttons[(int)button];

			if (mb.pressed) //this is a pressed event
         {
            return true;
         }

			//look for repeat "pressed" events
			double t = buttons[(int)button].downDuration;
			if (repeat && t > buttonRepeatDelay)
         {
            double delay = buttonRepeatDelay;
            double rate = buttonRepeatRate;
            double halfRate = rate * 0.5;
            if (((t - delay % rate) > halfRate) != (((t - delay - UI.dt) % rate) > halfRate))
               return true;
         }

         return false;
      }

		internal void newFrame()
      {
         delta = pos - prev;
         prev = pos;

         wheelDelta = wheel - wheelPrev;
         wheelPrev = wheel;

         for (int i = 0; i < MAX_BUTTONS; i++)
         {
				MouseButtonState mb = buttons[i];
				mb.pressed = mb.down && mb.downDuration < 0;
				mb.released = !mb.down && mb.downDuration >= 0;
				mb.downDurationPrev = mb.downDuration;
				mb.downDuration = mb.down ? (mb.downDuration < 0 ? 0.0 : mb.downDuration + UI.dt) : -1.0;
				mb.doubleClicked = false;

            if (mb.pressed)
            {
               if (UI.time - mb.pressedTime < doubleClickTime)
               {
                  if ((pos - mb.pressedPos).LengthSquared < doubleClickDistance * doubleClickDistance)
                  {
							mb.doubleClicked = true;
							mb.pressedTime = -1.0f;
                  }
               }
               else
               {
						mb.pressedTime = UI.time;
               }

					mb.pressedPos = pos;
					mb.dragMaxDistanceSqr = 0;
            }
            else if (mb.down)
            {
					mb.dragMaxDistanceSqr = Math.Max(mb.dragMaxDistanceSqr, (pos - mb.pressedPos).LengthSquared);
            }
         }
      }

      public bool buttonIsDown(MouseButton button)
      {
         return buttons[(int)button].down;
      }

      public bool buttonAction(MouseAction action, MouseButton button)
      {
         switch (action)
         {
            case MouseAction.CLICKED: return isButtonClicked(button);
            case MouseAction.PRESSED: return buttons[(int)button].pressed;
            case MouseAction.RELEASED: return buttons[(int)button].released;
            case MouseAction.DOUBLE_CLICKED: return buttons[(int)button].doubleClicked;
         }

         return false;
      }

      public bool buttonActionInRect(MouseAction action, MouseButton button, Rect r)
      {
         switch (action)
         {
            case MouseAction.CLICKED:
               {
                  return r.containsPoint(buttons[(int)button].pressedPos) && isButtonClicked(button);
               }
            case MouseAction.PRESSED:
               {
                  return r.containsPoint(pos) && buttons[(int)button].down;
               }
            case MouseAction.RELEASED:
               {
                  return r.containsPoint(pos) && buttons[(int)button].released;
               }
            case MouseAction.DOUBLE_CLICKED:
               {
                  return r.containsPoint(buttons[(int)button].pressedPos) && buttons[(int)button].doubleClicked;
               }
         }

         return false;
      }

   }

   public class KeyboardState
   {
      public Util.KeyModifiers modifiers { get; set; }
      public bool[] keysDown = new bool[256];
      public double[] keysDownDuration = new double[256];
      public double[] keysDownDurationPrev = new double[256];
		public string text;

      public float keyRepeatDelay = 0.25f;
      public float keyRepeatRate = 0.02f;

      public KeyboardState()
      {

      }

		internal void newFrame()
      {
         keysDownDuration.CopyTo(keysDownDurationPrev, 0);
         for (int i = 0; i < 256; i++)
         {
            if (keysDown[i] == true)
            {
               keysDownDuration[i] = keysDownDuration[i] < 0 ? 0 : keysDownDuration[i] + UI.dt;
            }
            else
            {
               keysDownDuration[i] = -1.0f;
            }
         }
      }

      public bool keyPressed(Key key)
      {
         return keysDownDuration[(int)key] >= 0.0;
      }

      public bool keyReleased(Key key)
      {
         //key is not long pressed, but it was, so it has been released
         return (keysDown[(int)key] == false) && (keysDownDurationPrev[(int)key] > 0.0);
      }

      public bool keyJustPressed(Key key)
      {
         return (keysDown[(int)key] == true) && (keysDownDuration[(int)key] == 0.0);
      }
   }
}