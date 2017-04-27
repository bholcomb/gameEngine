using System;

using Util;

namespace Engine
{
   public class NetworkTask : Task
   {
      public NetworkTask() :
         base("Network Task")
      {
         frequency = 15;
      }

      protected override void onUpdate(double dt)
      {
      }
   }

   public static class NetworkManager
   {
      static NetworkTask myTask;

      static NetworkManager()
      {
         myTask = new NetworkTask();
      }

      static public bool init(Initializer initializer)
      {
         
         return true;
      }
   }
}
