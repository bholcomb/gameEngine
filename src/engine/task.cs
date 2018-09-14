/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

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
      int myPriority = 0;
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
            if (initialize != null)
            {
               initialize();
            }

            onInitialize();

            myIsInitialized = true;
         }

         if (myIsPaused == false)
         {
            if (timeToRun(dt))
            {
               if (preTick != null)
               {
                  preTick(myCycleTime);
               }

               if (update != null)
               {
                  update(myCycleTime);
               }

               onUpdate(myCycleTime);

               if (postTick != null)
               {
                  postTick(myCycleTime);
               }
            }
         }
      }

      public bool timeToRun(double dt)
      {
         myRunTimer -= dt;
         if (myRunTimer <= 0.0)
         {
            myRunTimer += myCycleTime;
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

      public int priority
      {
         get { return myPriority; }
         set { myPriority = value; }
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

      public delegate bool InitDelegate();
      public delegate void UpdateDelegate(double dt);

      public event InitDelegate initialize;
      public event UpdateDelegate preTick;
      public event UpdateDelegate update;
      public event UpdateDelegate postTick;

      public virtual bool onInitialize()
      {
         return true;
      }

      public virtual void onUpdate(double dt)
      {

      }
   }
}

