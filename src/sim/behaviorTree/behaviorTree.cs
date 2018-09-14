/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;

namespace BehaviorTree
{
   public enum Status { Invalid, Success, Failure, Running, Suspended };

   public class BehaviorTree
   {
      Behavior myRoot;
      Dictionary<String, Object> myData = new Dictionary<string, Object>();
      public BehaviorTree()
      {

      }

      public Dictionary<String, Object> data { get { return myData; } }

      public Status tick(double dt)
      {
         return myRoot.tick(dt);
      }
   }
}