using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;
using Events;

namespace UI
{
   public class GuiEventHandler
   {
      GameWindow myWindow;

      public GuiEventHandler(GameWindow win)
      {
         myWindow = win;
         myWindow.RenderFrame +=new EventHandler<FrameEventArgs>(handleRenderFrame);
         myWindow.FocusedChanged += new EventHandler<EventArgs>(handleFocusChanged);
         myWindow.Mouse.Move += new EventHandler<MouseMoveEventArgs>(handleMouseMove);
         myWindow.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(handleMouseButtonDown);
         myWindow.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(handleMouseButtonUp);
         myWindow.Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(handleMouseWheel);
         myWindow.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardDown);
         myWindow.Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);
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
         ImGui.displaySize = new Vector2(myWindow.Width, myWindow.Height);
         ImGui.dt = e.Time;
         ImGui.time += e.Time;
      }

      public void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(myWindow.Keyboard);
         ImGui.keyboard.modifiers = modifiers;
         ImGui.keyboard.keysDown[(int)e.Key] = false;
      }

      public void handleKeyboardDown(object sender, KeyboardKeyEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(myWindow.Keyboard);
         ImGui.keyboard.modifiers = modifiers;
         ImGui.keyboard.keysDown[(int)e.Key] = true;
      }

      public void handleMouseWheel(object sender, MouseWheelEventArgs e)
      {
         ImGui.mouse.wheel = e.ValuePrecise;
         ImGui.mouse.wheelDelta = e.DeltaPrecise;
      }

      public void handleMouseButtonDown(object sender, MouseButtonEventArgs e)
      {
         ImGui.mouse.buttonDown[(int)e.Button] = true;
      }

      public void handleMouseButtonUp(object sender, MouseButtonEventArgs e)
      {
         ImGui.mouse.buttonDown[(int)e.Button] = false;
      }

      public void handleMouseMove(object sender, MouseMoveEventArgs e)
      {
         //OpenTK is reporting mouse coords with origin in top left
         //this is what we want since the UI window space has the coords in the 
         //top left, it's only when we go to render, that we adjust for screen space
         //and use the bottom left like openGL wants
         ImGui.mouse.pos = new Vector2(e.X, e.Y);
      }
      #endregion
   }
}
