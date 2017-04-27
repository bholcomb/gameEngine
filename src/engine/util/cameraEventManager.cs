using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Util;
using Graphics;
using Engine;
using Events;

namespace Engine
{
   public class CameraEventHandler : Graphics.CameraEventHandler
   {
      public CameraEventHandler(Camera c)
         : base(c)
      {

      }

      public void install()
      {
         Kernel.eventManager.addListener(handleInput, "input.*");
      }

      public void uninstall()
      {
         Kernel.eventManager.removeListener(handleInput, "input.*");
      }

      public EventManager.EventResult handleInput(Event e)
      {
         switch(e.name)
         {
            case "input.keyboard.key.up":
               {
                  KeyUpEvent ke=e as KeyUpEvent;
                  handleKeyboardUp(ke.key);
                  break;
               }
            case "input.keyboard.key.down":
               {
                  KeyDownEvent ke=e as KeyDownEvent;
                  handleKeyboardDown(ke.key);
                  break;
               }
            case "input.mouse.wheel":
               {
                  MouseWheelEvent me=e as MouseWheelEvent;
                  handleMouseWheel(me.delta);
                  break;
               }
            case "input.mouse.button.up":
               {
                  MouseButtonUpEvent me=e as MouseButtonUpEvent;
                  handleMouseButtonUp(me.button);
                  break;
               }
            case "input.mouse.button.down":
               {
                  MouseButtonDownEvent me=e as MouseButtonDownEvent;
                  handleMouseButtonDown(me.button);
                  break;
               }
            case "input.mouse.move":
               {
                  MouseMoveEvent me=e as MouseMoveEvent;
                  handleMouseMove(me.xDelta, -me.yDelta);
                  break;
               }
         }

         return EventManager.EventResult.HANDLED;
      }
   }
}
