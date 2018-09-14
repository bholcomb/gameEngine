/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

using Util;

namespace Engine
{
   public abstract class GameSystem
   {
      string myName;

      public GameSystem(String name)
      {
         myName = name;
      }

      public string name { get { return myName; } }


      public abstract bool init(Initializer init);

      public abstract bool shutdown();
   }
}