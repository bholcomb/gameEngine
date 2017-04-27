using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Platform.Windows;
using OpenTK.Graphics.OpenGL;

using Util;
using Events;

namespace Engine
{
   //derived from a game window, but don't call run.  
   public class Window : GameWindow
   {
      Color4 myClearColor = new Color4(0.2f, 0.2f, 0.2f, 1.0f);

      //GraphicsMode gm = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 32, 8, 0, ColorFormat.Empty, 2, false);

      public Window(int x, int y, string name)
         : base(x, y, new GraphicsMode(32, 24, 8, 8), name, GameWindowFlags.Default, DisplayDevice.Default, 4, 5,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
      {
         Visible = true;
         VSync = VSyncMode.Off;

         init();
      }

      public void clear()
      {
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      }

      public void setClearColor(Color4 c)
      {
         myClearColor = c;
         GL.ClearColor(myClearColor);
      }

      public void init()
      {
         //register for events
         Closing += new EventHandler<CancelEventArgs>(handleWindowClose);
         Resize += new EventHandler<EventArgs>(handleWindowResize);
         Mouse.Move += new EventHandler<MouseMoveEventArgs>(handleMouseMove);
         Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(handleMouseButtonDown);
         Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(handleMouseButtonUp);
         Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(handleMouseWheel);
         Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardDown);
         Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);
      }

      void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(this.Keyboard);
         KeyUpEvent ev = new KeyUpEvent(e.Key, modifiers);
         Kernel.eventManager.queueEvent(ev);
      }

      void handleKeyboardDown(object sender, KeyboardKeyEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(this.Keyboard);
         KeyDownEvent ev = new KeyDownEvent(e.Key, modifiers);
         Kernel.eventManager.queueEvent(ev);
      }

      void handleMouseWheel(object sender, MouseWheelEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(this.Keyboard);
         MouseWheelEvent ev = new MouseWheelEvent(e.X, Height-e.Y, e.Value, e.Delta, modifiers);
         Kernel.eventManager.queueEvent(ev);
      }

      void handleMouseButtonUp(object sender, MouseButtonEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(this.Keyboard);
         MouseButtonUpEvent ev = new MouseButtonUpEvent(e.Button, e.X, Height-e.Y, modifiers);
         Kernel.eventManager.queueEvent(ev);
      }

      void handleMouseButtonDown(object sender, MouseButtonEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(this.Keyboard);
         MouseButtonDownEvent ev = new MouseButtonDownEvent(e.Button, e.X, Height - e.Y, modifiers);
         Kernel.eventManager.queueEvent(ev);
      }

      void handleMouseMove(object sender, MouseMoveEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(this.Keyboard);
         MouseMoveEvent ev = new MouseMoveEvent(e.X, Height - e.Y, e.XDelta, -e.YDelta, modifiers);
         Kernel.eventManager.queueEvent(ev);
      }

      protected void handleWindowClose(object callingWin, CancelEventArgs args)
      {
         Window win = (Window)callingWin;
         if (win == this)
         {
            WindowCloseEvent wce = new WindowCloseEvent(win);
            Kernel.eventManager.queueEvent(wce);
            WindowManager.removeWindow(win);
         }
      }

      void handleWindowResize(object callingWin, EventArgs e)
      {
         Window win = (Window)callingWin;
         WindowResizeEvent wre = new WindowResizeEvent(win, this.Width, this.Height);
         Kernel.eventManager.queueEvent(wre);
      }

   }
}