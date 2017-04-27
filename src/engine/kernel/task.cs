using System;

namespace Engine
{
   public class Task
   {
      String myName;
      bool myShouldKill = false;
      bool myIsActive = false;
      bool myIsPaused = false;
      bool myIsInitialized = false;
      bool myIsAttached = false;
      double myFrequency;
      double myCycleTime;
      double myRunTimer;
      Task myNext = null;

      public Task(String name)
      {
         myName = name;
         frequency = 30;
      }

      public void tick(double dt)
      {
         if (myIsInitialized == false)
         {
            onInitialize();
            myIsInitialized = true;
         }

         if (myIsPaused == false)
         {
            if (timeToRun(dt))
            {
               onUpdate(myCycleTime);
            }
         }
      }

      public bool timeToRun(double dt)
      {
         myRunTimer -= dt;
         if (myRunTimer <= 0.0)
         {
            myRunTimer = myCycleTime;
            return true;
         }

         return false;
      }

      public String name()
      {
         return myName;
      }

      public bool isDead()
      {
         return myShouldKill;
      }

      public bool active
      {
         get { return myIsActive; }
         set { myIsActive = value; }
      }

      public bool attached
      {
         get { return myIsAttached; }
         set { myIsAttached = value; }
      }

      public bool paused
      {
         get { return myIsPaused; }
         set { myIsPaused = value; }
      }

      public double frequency
      {
         get { return myFrequency; }
         set
         {
            myFrequency = value;
            myCycleTime = 1.0 / myFrequency;
            myRunTimer = myCycleTime;
         }
      }

      public Task next
      {
         get { return myNext; }
         set { myNext = value; }
      }

      public bool isInitialized()
      {
         return myIsInitialized;
      }

      public void kill()
      {
         myShouldKill = true;
      }

      public void togglePaused()
      {
         myIsPaused = !myIsPaused;
      }

      protected virtual void onInitialize()
      {
         
      }

      protected virtual void onUpdate(double dt)
      {
      }
   }
}

