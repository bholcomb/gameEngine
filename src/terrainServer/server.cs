using System;

using Engine;
using Terrain;
using Util;

namespace TerrainServer
{
   public static class TerrainServer
   {
      static bool shouldQuit = false;
      public static void Main(String[] args)
      {
         Initializer init=new Initializer(args);
         Kernel.init(init);
         initializeTasks(init);

         try
         {
            while (shouldQuit == false)
            {
               Kernel.tick();
               if (Console.KeyAvailable == true)
               {
                  ConsoleKeyInfo ki= Console.ReadKey(true);
                  if (ki.Key == ConsoleKey.Escape)
                  {
                     Console.WriteLine("Bye!");
                     shouldQuit = true;
                  }
               }
            }
         }
         finally
         {
            Kernel.shutdown();
            TerrainGenerationTask tgt = Kernel.taskManager.findTask("Terrain Generation") as TerrainGenerationTask;
            if (tgt!=null)
            {
               tgt.shutdown();
            }

            TerrainNetworkTask tnt = Kernel.taskManager.findTask("Terrain Server") as TerrainNetworkTask;
            if (tnt != null)
            {
               tnt.shutdown();
            }
         }
      }

      public static void initializeTasks(Initializer init)
      {
         Kernel.taskManager.attach(new TerrainGenerationTask(init));
         Kernel.taskManager.attach(new TerrainNetworkTask(init));
      }
   }
}
