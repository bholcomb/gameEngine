using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Util
{
   public static class TimeSource
   {
      static double myPrevTime = 0.0;
      static double myCurrentTime = 0.0;
      static double myFrameTime = 0.0;
      static double myPrevFrameTime = 0.0;
      static double myTimeRunning = 0.0;

      static int myFrameNumber = 0;
      static double myFps = 0.0;
      static double myMinFps = 0.0;
      static double myMaxFps = 0.0;
      static Stopwatch myStopwatch;

      static List<Clock> myClocks = new List<Clock>();

      static Clock myDefaultClock;

      static TimeSource()
      {
         reset();
         myStopwatch = new Stopwatch();
         myStopwatch.Start();
         
         //create a default clock
         myDefaultClock = new Clock(currentTime());
         myClocks.Add(myDefaultClock);
      }

      public static void frameStep()
      {
         myFrameNumber++;
         myCurrentTime = currentTime();
         updateTimeThisFrame();
         foreach (Clock c in myClocks)
         {
            c.notify();
         }
      }

      public static Clock defaultClock
      {
         get { return myDefaultClock; }
      }

      public static double now()
      {
         return myStopwatch.Elapsed.TotalSeconds;
      }

      public static void reset()
      {
         myFrameTime = 0.0;
         myPrevFrameTime = 0.0;
         myCurrentTime = 0.0;
         myPrevTime = 0.0;
         myFps = 0.0;
         myMinFps = 1000000.0;
         myMaxFps = -1000000.0;
         myFrameNumber = 0;
         myTimeRunning = 0.0;
      }

      public static Clock newClock()
      {
         Clock c = new Clock();
         myClocks.Add(c);
         return c;
      }

      public static void releaseClock(Clock c)
      {
         if (myClocks.Remove(c) == false)
         {
            //warn << "Cannot find clock to release" << endl;
         }
      }

      public static double timeThisFrame()
      {
         return myFrameTime;
      }

      public static double clockTime()
      {
         return myCurrentTime;
      }

      public static int frameNumber()
      {
         return myFrameNumber;
      }

      public static double timeRunning()
      {
         return myTimeRunning;
      }

      public static double fps()
      {
         return myFps;
      }

      public static double avgFps()
      {
         return (double)myFrameNumber / myTimeRunning;
      }

      public static double maxFps()
      {
         return myMaxFps;
      }

      public static double minFps()
      {
         return myMinFps;
      }

      public static double frameMs()
      {
         return myFrameTime * 1000.0;
      }

      public static double avgFrameMs()
      {
         return (myTimeRunning / (double)myFrameNumber) * 1000.0;
      }

      private static void updateTimeThisFrame()
      {
         //update time;
         myPrevFrameTime = myFrameTime;
         myFrameTime = myCurrentTime - myPrevTime;
         myPrevTime = myCurrentTime;

         //check for very long waits (as in debugging)
#if DEBUG
         if (myFrameTime > 0.33)
         {
            //debug << "Excessive lag in TimeSource frame update using acceptable time" << endl;
            myFrameTime = 0.16;
         }
#endif

         myTimeRunning += myFrameTime;

         //update FPS
         myFps = 1.0 / (myFrameTime);
         if (myFps > myMaxFps) myMaxFps = myFps;
         if (myFps < myMinFps) myMinFps = myFps;
      }

      public static double currentTime()
      {
         return (double)myStopwatch.ElapsedTicks / Stopwatch.Frequency;
      }
   }
}

