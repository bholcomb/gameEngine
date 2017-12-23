using System;

using Audio;
using Util;
using Engine;
using Events;

namespace Engine
{
   public class AudioTask : Task
   {
      public AudioTask() :
         base("Audio Task")
      {
         frequency = 20;
         Kernel.taskManager.attach(this);
      }

      protected override void onUpdate(double dt)
      {
         Audio.AudioManager.instance().tick(dt);  
      }
   }

   public static class AudioManager
   {
      static AudioTask myTask;

      static AudioManager()
      {
         myTask = new AudioTask();
      }

      static public bool init(Initializer initializer)
      {
         //return Audio.AudioManager.init();

         return true;
      }
   }
}
