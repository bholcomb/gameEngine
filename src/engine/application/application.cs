using System;
using System.Collections.Generic;
using System.IO;

using Util;

namespace Engine
{
   public delegate void postInit();
   public delegate void preShutdown();
   public delegate void preFrame();
   public delegate void postFrame();

   public class Application
   {
      static Application theApplication = null;
      Initializer myInitializer;

      bool myShouldQuit = false;

      public event postInit onPostInit;
      public event preShutdown onPreShutdown;
      public event preFrame onPreFrame;
      public event postFrame onPostFrame;

      public static Application instance()
      {
         if (theApplication == null)
         {
            throw new Exception("Application not created");
         }

         return theApplication;
      }

      public Application(Initializer init)
      {
         myInitializer = init;
         theApplication = this;

         string executingAssemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
         string subfolder;
         if (Environment.Is64BitProcess)
             subfolder = "x64";
         else
             subfolder = "x86";

         string dllPath = Path.Combine(executingAssemblyFolder, subfolder);
         String currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
         dllPath = dllPath.Replace("/", "\\");
         if (currentPath.Contains(dllPath) == false)
         {
             String newPath = currentPath + Path.PathSeparator + dllPath;
             Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Process);
         }
      }

      public Initializer initializer { get { return myInitializer; } }

      public bool init()
      {
         if (Kernel.init(myInitializer) == false)
         {
            Error.print("Error Initializing kernel");
            return false;
         }

         if (onPostInit != null)
         {
            onPostInit();
         }

         return true;
      }

      public void run()
      {
         while (myShouldQuit == false)
         {
            if (onPreFrame != null)
            {
               onPreFrame();
            }

            Kernel.tick();

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
         Kernel.shutdown();
      }

      public void quit()
      {
         myShouldQuit = true;
      }
   }
}
