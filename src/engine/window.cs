/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

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

namespace Engine
{
   //derived from a game window, but don't call run.  
   public class Window : GameWindow
   {
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
         this.Location = new System.Drawing.Point(0,0);
         init();
      }

      public void init()
      {
         //register for events
         Closing += new EventHandler<CancelEventArgs>(handleWindowClose);
         Resize += new EventHandler<EventArgs>(handleWindowResize);
         MouseMove += new EventHandler<MouseMoveEventArgs>(handleMouseMove);
         MouseDown += new EventHandler<MouseButtonEventArgs>(handleMouseButtonDown);
         MouseUp += new EventHandler<MouseButtonEventArgs>(handleMouseButtonUp);
         MouseWheel += new EventHandler<MouseWheelEventArgs>(handleMouseWheel);
         KeyDown += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardDown);
         KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);
      }

      void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(Keyboard.GetState());
         KeyUpEvent ev = new KeyUpEvent(e.Key, modifiers);
         Application.eventManager.queueEvent(ev);
      }

      void handleKeyboardDown(object sender, KeyboardKeyEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(Keyboard.GetState());
         KeyDownEvent ev = new KeyDownEvent(e.Key, modifiers);
         Application.eventManager.queueEvent(ev);
      }

      void handleMouseWheel(object sender, MouseWheelEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(Keyboard.GetState());
         MouseWheelEvent ev = new MouseWheelEvent(e.X, Height-e.Y, e.Value, e.Delta, modifiers);
         Application.eventManager.queueEvent(ev);
      }

      void handleMouseButtonUp(object sender, MouseButtonEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(Keyboard.GetState());
         MouseButtonUpEvent ev = new MouseButtonUpEvent(e.Button, e.X, Height-e.Y, modifiers);
         Application.eventManager.queueEvent(ev);
      }

      void handleMouseButtonDown(object sender, MouseButtonEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(Keyboard.GetState());
         MouseButtonDownEvent ev = new MouseButtonDownEvent(e.Button, e.X, Height - e.Y, modifiers);
         Application.eventManager.queueEvent(ev);
      }

      void handleMouseMove(object sender, MouseMoveEventArgs e)
      {
         Util.KeyModifiers modifiers = new Util.KeyModifiers();
         modifiers.getModifiers(Keyboard.GetState());
         MouseMoveEvent ev = new MouseMoveEvent(e.X, Height - e.Y, e.XDelta, -e.YDelta, modifiers);
         Application.eventManager.queueEvent(ev);
      }

      protected void handleWindowClose(object callingWin, CancelEventArgs args)
      {
         Window win = (Window)callingWin;
         if (win == this)
         {
            WindowCloseEvent wce = new WindowCloseEvent(win);
            Application.eventManager.queueEvent(wce);
            WindowManager.removeWindow(win);
         }
      }

      void handleWindowResize(object callingWin, EventArgs e)
      {
         Window win = (Window)callingWin;
         WindowResizeEvent wre = new WindowResizeEvent(win, this.Width, this.Height);
         Application.eventManager.queueEvent(wre);
      }
      

   }
}