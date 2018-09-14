/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;

using Util;
using Engine;
using Lua;

namespace Sim
{
   public abstract class BehaviorCreator
   {
      protected Int32 myName;

      public BehaviorCreator()
      {
      }

      public abstract Behavior create(Entity e, LuaObject initData);

      public Int32 name
      {
         get { return myName; }
      }
   } 

   public class Behavior
   {
      protected Entity myEntity;
      protected State myState;
      protected Int32 myName;
      protected double myUpdateFrequency;
      protected double myNextUpdate;

      public Behavior(Entity e, Int32 name)
      {
         myEntity = e;
         myState = e.state;
         myName = name;
         //default to 10hz
         myUpdateFrequency = 1.0 / 10.0;
         myNextUpdate = 0.0;

         myEntity.registerBehavior(this);
      }

      public virtual void init(LuaObject initData)
      {
         float updateHertz = (float)initData["updateRate"];
         myUpdateFrequency = 1.0f / updateHertz;
      }

      public virtual void postInit()
      {

      }

      public virtual void onUpdate(int pass, double delta)
      {

      }

      public virtual void shutdown()
      {

      }

      public bool shouldUpdate(double delta)
      {
         double currentTime = TimeSource.clockTime();

         if (currentTime >= myNextUpdate)
         {
            myNextUpdate = currentTime + myUpdateFrequency;
            return true;
         }

         return false;
      }

      public virtual EventManager.EventResult onEvent(Event e)
      {
         return EventManager.EventResult.IGNORED;
      }

      public Int32 name
      {
         get { return myName; }
      }

      public double updateRate
      {
         get { return 1.0 / myUpdateFrequency; }
         set { myUpdateFrequency = 1.0 / value; }
      }
   }
}