using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;
using Engine;

namespace GUI
{
   public class GuiEventHandler
   {
      GameWindow myWindow;

      public GuiEventHandler(GameWindow win)
      {
         myWindow = win;
         myWindow.RenderFrame +=new EventHandler<FrameEventArgs>(handleRenderFrame);
         myWindow.FocusedChanged += new EventHandler<EventArgs>(handleFocusChanged);
         myWindow.MouseMove += new EventHandler<MouseMoveEventArgs>(handleMouseMove);
         myWindow.MouseDown += new EventHandler<MouseButtonEventArgs>(handleMouseButtonDown);
         myWindow.MouseUp += new EventHandler<MouseButtonEventArgs>(handleMouseButtonUp);
         myWindow.MouseWheel += new EventHandler<MouseWheelEventArgs>(handleMouseWheel);
         myWindow.KeyDown += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardDown);
         myWindow.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);
      }

      #region OpenTK Gamewindow handler functions
      void handleFocusChanged(object sender, EventArgs e)
      {
         if (myWindow.Focused == false)
         {

         }
      }

      void handleRenderFrame(object sender, FrameEventArgs e)
      {
         UI.displaySize = new Vector2(myWindow.Width, myWindow.Height);
         UI.dt = e.Time;
         UI.time += e.Time;
      }

      public void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(Keyboard.GetState());
         UI.keyboard.modifiers = modifiers;
         UI.keyboard.keysDown[(int)e.Key] = false;
      }

      public void handleKeyboardDown(object sender, KeyboardKeyEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(Keyboard.GetState());
         UI.keyboard.modifiers = modifiers;
         UI.keyboard.keysDown[(int)e.Key] = true;
      }

      public void handleMouseWheel(object sender, MouseWheelEventArgs e)
      {
         UI.mouse.wheel = e.ValuePrecise;
         UI.mouse.wheelDelta = e.DeltaPrecise;
      }

      public void handleMouseButtonDown(object sender, MouseButtonEventArgs e)
      {
         UI.mouse.buttons[(int)e.Button].down = true;
      }

      public void handleMouseButtonUp(object sender, MouseButtonEventArgs e)
      {
         UI.mouse.buttons[(int)e.Button].down= false;
      }

      public void handleMouseMove(object sender, MouseMoveEventArgs e)
      {
         //OpenTK is reporting mouse coords with origin in top left
         //this is what we want since the UI window space has the coords in the 
         //top left, it's only when we go to render, that we adjust for screen space
         //and use the bottom left like openGL wants
         UI.mouse.pos = new Vector2(e.X, e.Y);
      }
      #endregion
   }
}
