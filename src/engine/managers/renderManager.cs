using System;

using Audio;
using Util;
using Engine;
using Events;
using Graphics;

namespace Engine
{
   public class RenderTask : Task
   {
      public RenderTask() :
         base("Render Task")
      {
         frequency = 30;
         Kernel.taskManager.attach(this);

		   Renderer.present = present;
      }

      protected override void onUpdate(double dt)
      {
         Renderer.render();
      }

		void present()
		{
			WindowManager.swapBuffers();
		}
   }

   public static class RenderManager
   {
      static RenderTask myTask;

      static RenderManager()
      {
         myTask = new RenderTask();
      }

      static public bool init(Initializer initializer)
      {
         Renderer.init(initializer.findData<InitTable>("renderer"));
         return true;
      }
   }
}
