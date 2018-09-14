using System;
using System.Collections.Generic;

namespace Util
{
   public class Clock
   {
      double myFrameTime;
      double myCurrentTime;
      double myTimeScale;

      bool myIsPaused;
      double myPauseTime;

      List<Timer> myTimers = new List<Timer>();
      List<Timer> myReleaseTimers = new List<Timer>();

      public Clock()
      {
         myTimeScale = 1.0;
         myIsPaused = false;
         myPauseTime = 0.0;
         myCurrentTime = 0.0;
         myFrameTime = 0.0;
      }

      internal Clock(double setTime)
      {
         myTimeScale = 1.0;
         myIsPaused = false;
         myPauseTime = 0.0;
         myCurrentTime = setTime;
         myFrameTime = 0.0;
      }

      public void reset()
      {
         myTimeScale = 1.0;
         myIsPaused = false;
         myPauseTime = 0.0;
         myCurrentTime = 0.0;
         myFrameTime = 0.0;
      }

      public double currentTime()
      {
         return myCurrentTime;
      }

      public void setTime(double newTime)
      {
         myCurrentTime = newTime;
      }

      public double timeThisFrame()
      {
         return myFrameTime;
      }

      public double timeScale
      {
         get { return myTimeScale; }
         set { myTimeScale = value; }
      }

      public void setPause(bool setting)
      {
         myIsPaused = setting;
         if (myIsPaused == true)
         {
            myPauseTime = myCurrentTime;
         }
         else
         {
            myCurrentTime = myPauseTime;
         }
      }

      public bool isPaused()
      {
         return myIsPaused;
      }

      #region timers

      public Timer newTimer()
      {
         Timer t = new Timer(0.0, false, this);
         myTimers.Add(t);
         return t;
      }
      
      public Timer newTimer(double countdownTime)
      {
         Timer t = new Timer(countdownTime, false, this);
         myTimers.Add(t);
         return t;
      }

      public Timer newTimer(double countdownTime, bool loop)
      {
         Timer t = new Timer(countdownTime, loop, this);
         myTimers.Add(t);
         return t;
      }

      public void releaseTimer(Timer t)
      {
         myReleaseTimers.Add(t);
      }
      
      #endregion

      public void notify()
      {
         if (myIsPaused == true)
         {
            return;
         }

         myFrameTime = TimeSource.timeThisFrame() * myTimeScale;
         myCurrentTime += myFrameTime;
         foreach (Timer t in myTimers)
         {
            if (t.notify() == false)
            {
               releaseTimer(t);
            }
         }

         //remove any old timers
         foreach (Timer t in myReleaseTimers)
         {
            myTimers.Remove(t);
         }

         myReleaseTimers.Clear();
      }
   }
}

