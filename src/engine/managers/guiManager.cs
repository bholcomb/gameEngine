using System;
using System.Collections.Generic;

using Audio;
using Util;
using Engine;
using Events;
using Graphics;
using UI;

using OpenTK;
using OpenTK.Input;

namespace Engine
{
   public class GuiTask : Task
   {
      UI.GuiEventHandler myEventHandler;

      public GuiTask() :
         base("Gui Task")
      {
         frequency = 60;
         Kernel.taskManager.attach(this);

         Window win = WindowManager.windows()[0]; //just the first window now
         myEventHandler = new UI.GuiEventHandler(win);
      }

      protected override void onUpdate(double dt)
      {
         GuiManager.tick(dt);
      }
   }

   public static class GuiManager
   {
      static GuiTask myTask;

      static GuiManager()
      {
         myTask = new GuiTask();
      }

      public static bool init(Initializer initializer)
      {
         return true;
      }

      public static void tick(double dt)
      {
      }
   }
}
