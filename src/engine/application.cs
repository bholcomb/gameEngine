/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using Util;

namespace Engine
{
   public delegate void postInit();
   public delegate void preShutdown();
   public delegate void preFrame();
   public delegate void postFrame();

   public static class Application
   {
      static Initializer myInitializer;
      static TaskManager myTaskManager;
      static EventManager myEventManager;
      
      static bool myShouldQuit = false;

      static ulong myUniqueId;
      static uint myProcId;
      static uint myAppId;

      static Clock myClock;
      static float myMinTime = -1.0f;
      static float myMaxTime = -1.0f;

      public static event postInit onPostInit;
      public static event preShutdown onPreShutdown;
      public static event preFrame onPreFrame;
      public static event postFrame onPostFrame;

      static Application()
      {
         string executingAssemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
         string subfolder;
         if (Environment.Is64BitProcess)
            subfolder = "x64";
         else
            subfolder = "x32";

         string dllPath = Path.Combine(executingAssemblyFolder, subfolder);
         String currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
         dllPath = dllPath.Replace("/", "\\");
         if (currentPath.Contains(dllPath) == false)
         {
            String newPath = currentPath + Path.PathSeparator + dllPath;
            Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Process);
         }

         Thread.CurrentThread.Name = "Main Thread";
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
         myClock = TimeSource.newClock();
      }

      public static Initializer initializer { get { return myInitializer; } }
      public static ulong uniqueId { get { return myUniqueId; } }
      public static uint procId { get { return myProcId; } }
      public static Clock clock { get { return myClock; } }

      public static TaskManager taskManager { get { return myTaskManager; } }
      public static EventManager eventManager { get { return myEventManager; } }

      public static bool init(Initializer init)
      {
         myInitializer = init;

         if (!myTaskManager.init(myInitializer))
         {
            return false;
         }
         if (!myEventManager.init(myInitializer))
         {
            return false;
         }

         Printer.VerboseLevel vl = (Printer.VerboseLevel)myInitializer.findDataOr<int>("application.verboseLevel", 4);
         Printer.verboseLevel = vl;

         myAppId = myInitializer.findDataOr<uint>("application.appId", myProcId);
         myMinTime = myInitializer.findDataOr<float>("application.minTick", -1.0f);
         myMaxTime = myInitializer.findDataOr<float>("application.maxTick", -1.0f);

         if (onPostInit != null)
         {
            onPostInit();
         }

         return true;
      }

      public static void run()
      {
         while (myShouldQuit == false)
         {
            if (onPreFrame != null)
            {
               onPreFrame();
            }

            TimeSource.frameStep();
            myEventManager.tick(myMinTime, myMaxTime);
            myTaskManager.tick(myClock.timeThisFrame());

            if (onPostFrame != null)
            {
               onPostFrame();
            }
         }

         if (onPreShutdown != null)
         {
            onPreShutdown();
         }

         //shutdown tasks
         myEventManager.shutdown();
         myTaskManager.shutdown();
      }

      public static void quit()
      {
         myShouldQuit = true;
      }
   }
}
