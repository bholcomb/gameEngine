/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;

namespace BehaviorTree
{
   public abstract class Behavior
   {
      protected BehaviorTree myTree;
      protected Status myStatus = Status.Suspended;

      public Behavior(BehaviorTree tree)
      {
         myTree = tree;
      }

      public virtual Status tick(double dt)
      {
         if (myStatus == Status.Invalid)
            onInitialize();

         myStatus = onUpdate(dt);

         if (myStatus != Status.Running)
            onShutdown();

         return myStatus;
      }

      public abstract Status onUpdate(double dt);
      public virtual void onInitialize() { }
      public virtual void onShutdown() { }

      public virtual void onTerminate() { }

      public virtual void reset()
      {
         myStatus = Status.Invalid;
      }
   };
}