using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using OpenTK;
using OpenTK.Graphics;

using Util;

namespace Engine
{
   public static class Kernel 
   {
      static TaskManager myTaskManager;
      static EventManager myEventManager;
      static ulong myUniqueId;
      static uint myProcId;
      static uint myAppId;

      static float myMinTime = -1.0f;
      static float myMaxTime = -1.0f;

      static Kernel()
      {
         System.Threading.Thread.CurrentThread.Name = "Main Thread";
         myTaskManager = new TaskManager();
         myEventManager = new EventManager();

         NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

         foreach (var x in nics[0].GetIPProperties().UnicastAddresses)
         {
            if (x.Address.AddressFamily == AddressFamily.InterNetwork)
            {
               myUniqueId = (ulong)((int)x.Address.Address) << 32;
               break;
            }
         }

         myProcId = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;

         myUniqueId = myUniqueId | (ulong)myProcId;
      }

      public static ulong uniqueId
      {
         get { return myUniqueId; }
      }

      public static uint procId
      {
         get { return myProcId; }
      }

      public static bool init(Initializer init)
      {
         if (!myTaskManager.init(init))
         {
            return false;
         }
         if (!myEventManager.init(init))
         {
            return false;
         }

         Printer.VerboseLevel vl = (Printer.VerboseLevel)init.findDataOr<int>("core.verboseLevel", 4);
         Printer.verboseLevel = vl;

         myAppId = init.findDataOr<uint>("network.appId", myProcId);

         myMinTime = init.findDataOr<float>("core.minTick", -1.0f);
         myMaxTime = init.findDataOr<float>("core.maxTick", -1.0f);
            
         return true;
      }

      public static void shutdown()
      {
         myEventManager.shutdown();
         myTaskManager.shutdown();
      }

      public static TaskManager taskManager
      {
         get { return myTaskManager; }
      }

      public static EventManager eventManager
      {
         get { return myEventManager; }
      }

      public static void tick()
      {
         TimeSource.frameStep();
         myEventManager.tick(myMinTime, myMaxTime);
         myTaskManager.tick(TimeSource.timeThisFrame());
      }
   }
}

