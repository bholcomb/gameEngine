/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;

namespace BehaviorTree
{
   public abstract class Decorator : Behavior
   {
      public Decorator(BehaviorTree tree): base(tree)
      {

      }

      public Behavior child { get; set; }
   }

   public class Inverter : Decorator
   {
      public Inverter(BehaviorTree tree) : base(tree)
      {

      }

      public override Status onUpdate(double dt)
      {
         Status s = child.tick(dt);

         if (s == Status.Success) return Status.Failure;
         if (s == Status.Failure) return Status.Success;

         return s;
      }
   }

   public class AlwaysSucceed : Decorator
   {
      public AlwaysSucceed(BehaviorTree tree) : base(tree)
      {

      }

      public override Status onUpdate(double dt)
      {
         Status s = child.tick(dt);

         if (s == Status.Success || s == Status.Failure) return Status.Success;

         return s;
      }
   }

   public class AlwaysFail : Decorator
   {
      public AlwaysFail(BehaviorTree tree) : base(tree)
      {

      }

      public override Status onUpdate(double dt)
      {
         Status s = child.tick(dt);

         if (s == Status.Success || s == Status.Failure) return Status.Failure;

         return s;
      }
   }

   public class Repeater : Decorator
   {
      public Repeater(BehaviorTree tree) : base(tree)
      {

      }

      public override Status onUpdate(double dt)
      {
         while (true)
         {
            Status s = child.tick(dt);
            if (s == Status.Running) return Status.Running;
            if (s == Status.Failure) return Status.Failure;

            child.reset();
         }
      }
   }

   public class RepeaterUntilSucceed : Decorator
   {
      public RepeaterUntilSucceed(BehaviorTree tree) : base(tree)
      {

      }

      public override Status onUpdate(double dt)
      {
         while (true)
         {
            Status s = child.tick(dt);
            if (s == Status.Running) return Status.Running;
            if (s == Status.Success) return Status.Success;

            child.reset();
         }
      }
   }

   public class RepeaterUntilFail : Decorator
   {
      public RepeaterUntilFail(BehaviorTree tree) : base(tree)
      {

      }

      public override Status onUpdate(double dt)
      {
         while(true)
         {
            Status s = child.tick(dt);
            if (s == Status.Running) return Status.Running;
            if (s == Status.Failure) return Status.Success;

            child.reset();
         }
      }
   }

   public class RepeaterCount : Decorator
   {
      int myCurrentCount;
      public RepeaterCount(BehaviorTree tree) : base(tree)
      {

      }

      public int count { get; set; }

      public override void onInitialize()
      {
         myCurrentCount = 0;
      }

      public override Status onUpdate(double dt)
      {
         while (true)
         {
            Status s = child.tick(dt);
            if (s == Status.Running) return Status.Running;
            if (s == Status.Failure) return Status.Failure;

            myCurrentCount++;
            if (myCurrentCount == count)
            {
               return Status.Success;
            }

            child.reset();
         }
      }
   }

   /*

 Decorator
    Inverter
    Succeeder
    Repeater
    RepeaterCount
    RepeatUntilFail
  */
}