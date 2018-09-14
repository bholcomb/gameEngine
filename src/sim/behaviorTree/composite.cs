/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;

namespace BehaviorTree
{
   public abstract class Composite : Behavior
   {
      protected List<Behavior> myChildren = new List<Behavior>();
      public Composite(BehaviorTree tree): base(tree)
      {

      }

      public List<Behavior> children { get { return myChildren; } }
   }

   public class Sequence : Composite
   {
      int myCurrentChild;
      public Sequence(BehaviorTree tree): base(tree)
      {
      }

      public override void onInitialize()
      {
         myCurrentChild = 0;
      }

      public override Status onUpdate(double dt)
      {
         while(myCurrentChild < myChildren.Count)
         {
            Status s = myChildren[myCurrentChild].tick(dt);
            
            if(s != Status.Success)
            {
               return s;
            }

            myCurrentChild++;
            if(myCurrentChild == myChildren.Count)
               return Status.Success;
         }

         return Status.Invalid;
      }
   }

   public class Selector : Composite
   {
      int myCurrentChild;
      public Selector(BehaviorTree tree) : base(tree)
      {
      }

      public override void onInitialize()
      {
         myCurrentChild = 0;
      }

      public override Status onUpdate(double dt)
      {
         while (myCurrentChild < myChildren.Count)
         {
            Status s = myChildren[myCurrentChild].tick(dt);
            if(s == Status.Success)
            {
               return s;
            }

            myCurrentChild++;
            if(myCurrentChild == myChildren.Count)
            {
               return Status.Failure;
            }
         }

         return Status.Invalid;
      }
   }

   public class Parallel : Composite
   { 
      public enum Policy
      {
         RequireOne,
         RequireAll,
      };


      int myCurrentChild;
      public Parallel(BehaviorTree tree) : base(tree)
      {
      }
      
      public Policy forSuccess { get; set; }
      public Policy forFailure { get; set; }


      public override void onInitialize()
      {
         myCurrentChild = 0;
      }

      public override Status onUpdate(double dt)
      {
         int successsCount = 0;
         int failCount = 0;

         foreach(Behavior child in children)
         {
            Status s = child.tick(dt);
            if (s == Status.Success)
            {
               successsCount++;
               if (forSuccess == Policy.RequireOne)
               {
                  return Status.Success;
               }
            }

            if (s == Status.Failure)
            {
               failCount++;
               if (forFailure == Policy.RequireOne)
               {
                  return Status.Failure;
               }
            }
         }

         if(forFailure == Policy.RequireAll && failCount == children.Count)
         {
            return Status.Failure;
         }

         if(forSuccess == Policy.RequireAll && successsCount == children.Count)
         {
            return Status.Success;
         }

         return Status.Running;
      }
      public override void onTerminate()
      {
         foreach (Behavior child in children)
         {
            child.onTerminate();
         }
      }  
   }
}