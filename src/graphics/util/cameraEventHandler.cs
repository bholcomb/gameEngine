using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class CameraEventHandler
   {
      Camera myCamera;
      bool myMouseLook = false;
      float myMouseLookHeading = 0;
      float myMouseLookPitch = 0;
      bool moveForward = false;
      bool moveBackward = false;
      bool moveLeft = false;
      bool moveRight = false;
      bool moveUp = false;
      bool moveDown = false;

      Quaternion myViewOri = Quaternion.Identity;

      float myStepSize;
      bool shiftDown = false;

      public CameraEventHandler(Camera c)
      {
         myCamera = c;
         defaultStepSize = 5.0f;
         defaultBigStepSize = 50.0f;
         defaultLocation = Vector3.Zero;
         defaultViewDir = new Vector3(0, 0, -1);
         mouseLookButton = MouseButton.Right;
      }

      public Vector3 defaultLocation { get; set; }
      public Vector3 defaultViewDir { get; set; }
      public float defaultStepSize { get; set; }
      public float defaultBigStepSize { get; set; }
      public MouseButton mouseLookButton { get; set; }
      public float heading { get { return myMouseLookHeading; } }

      #region internal handler functions
      public void handleKeyboardUp(Key key)
      {
         switch (key)
         {
            case Key.W:
               {
                  moveForward = false;
               }
               break;
            case Key.S:
               {
                  moveBackward = false;
               }
               break;
            case Key.A:
               {
                  moveLeft = false;
               }
               break;
            case Key.D:
               {
                  moveRight = false;
               }
               break;
            case Key.Q:
               {
                  moveUp = false;
               }
               break;
            case Key.E:
               {
                  moveDown = false;
               }
               break;
            case Key.ShiftLeft:
               {
                  myStepSize = defaultStepSize;
                  shiftDown = false;
               }
               break;
         }
      }

      public void handleKeyboardDown(Key key)
      {
         switch (key)
         {
            case Key.W:
               {
                  moveForward = true;
               }
               break;
            case Key.S:
               {
                  moveBackward = true;
               }
               break;
            case Key.A:
               {
                  moveLeft = true;
               }
               break;
            case Key.D:
               {
                  moveRight = true;
               }
               break;
            case Key.Q:
               {
                  moveUp = true;
               }
               break;
            case Key.E:
               {
                  moveDown = true;
               }
               break;
            case Key.ShiftLeft:
               {
                  myStepSize = defaultBigStepSize;
                  shiftDown = true;
               }
               break;
            case Key.Space:
               {
                  if (shiftDown == true)
                  {
                     myMouseLookHeading = 0;
                     myMouseLookPitch = 0;
                     Quaternion q = new Quaternion();
                     q = q.fromHeadingPitchRoll(0, 0, 0);
                     myViewOri = q;
                     defaultStepSize = 1.0f;
                     defaultBigStepSize = 25.0f;
                     myStepSize = defaultStepSize;
                     moveForward = false;
                     moveBackward = false;
                     moveLeft = false;
                     moveRight = false;
                     moveUp = false;
                     moveDown = false;
                     myCamera.position = Vector3.Zero;
                  }
               }
               break;
            case Key.Plus:
               {
                  if (shiftDown == true)
                     defaultBigStepSize *= 2.0f;
                  else
                     defaultStepSize *= 2.0f;

               }
               break;
            case Key.Minus:
               {
                  if (shiftDown == true)
                     defaultBigStepSize /= 2.0f;
                  else
                     defaultStepSize /= 2.0f;

                  if (defaultStepSize <= 0) defaultStepSize = 1.0f;
                  if (defaultBigStepSize <= 0) defaultStepSize = 1.0f;
               }
               break;
         }
      }

      public void handleMouseWheel(int delta)
      {

      }

      public void handleMouseButtonUp(MouseButton button)
      {
         if (button == mouseLookButton)
         {
            myMouseLook = false;
         }
      }

      public void handleMouseButtonDown(MouseButton button)
      {
         if (button == mouseLookButton)
         {
            myMouseLook = true;
         }
      }

      public void handleMouseMove(int xDelta, int yDelta)
      {
         if (myMouseLook == true)
         {
            Quaternion q = new Quaternion();
            myMouseLookHeading += xDelta;
            myMouseLookPitch += yDelta;
            myViewOri = q.fromHeadingPitchRoll(myMouseLookHeading, myMouseLookPitch, 0.0f);
         }
      }


      #endregion

      public void tick(float dt)
      {
         //update axis from orientation
         myCamera.setOrientation(myViewOri);

         if (shiftDown)
            myStepSize = defaultBigStepSize;
         else
            myStepSize = defaultStepSize;

         //move based on keys
         if (moveForward == true) myCamera.move(0.0f, 0.0f, myStepSize * dt);
         if (moveBackward == true) myCamera.move(0.0f, 0.0f, -myStepSize * dt);
         if (moveLeft == true) myCamera.move(-myStepSize * dt, 0.0f, 0.0f);
         if (moveRight == true) myCamera.move(myStepSize * dt, 0.0f, 0.0f);
         if (moveUp == true) myCamera.move(0.0f, myStepSize * dt, 0.0f);
         if (moveDown == true) myCamera.move(0.0f, -myStepSize * dt, 0.0f);
      }
   }

   public class GameWindowCameraEventHandler : CameraEventHandler
   {
      public GameWindowCameraEventHandler(Camera c, GameWindow win)
         : base(c)
      {       
         win.Mouse.Move += new EventHandler<MouseMoveEventArgs>(handleMouseMove);
         win.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(handleMouseButtonDown);
         win.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(handleMouseButtonUp);
         win.Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(handleMouseWheel);
         win.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardDown);
         win.Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);
      }

      #region OpenTK Gamewindow handler functions
      public void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
      {
         handleKeyboardUp(e.Key);
      }



      public void handleKeyboardDown(object sender, KeyboardKeyEventArgs e)
      {
         handleKeyboardDown(e.Key);
      }



      public void handleMouseWheel(object sender, MouseWheelEventArgs e)
      {
         handleMouseWheel(e.Delta);
      }



      public void handleMouseButtonUp(object sender, MouseButtonEventArgs e)
      {
         handleMouseButtonUp(e.Button);
      }



      public void handleMouseButtonDown(object sender, MouseButtonEventArgs e)
      {
         handleMouseButtonDown(e.Button);
      }



      public void handleMouseMove(object sender, MouseMoveEventArgs e)
      {
         handleMouseMove(e.XDelta, e.YDelta);
      }
      #endregion
   }
}
