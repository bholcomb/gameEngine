using System;
using System.Collections.Generic;

using Util;
using Events;
using Engine;
using Lua;

namespace Sim
{
   public abstract class BehaviorCreator
   {
      protected String myName;

      public BehaviorCreator()
      {
      }

      public abstract Behavior create(Entity e, LuaObject initData);

      public String name
      {
         get { return myName; }
      }
   } 

   public class Behavior
   {
      protected Entity myEntity;
      protected String myName;
      protected double myUpdateFrequency;
      protected double myNextUpdate;

      public Behavior(Entity e, String name)
      {
         myEntity = e;
         myName = name;
         //default to 10hz
         myUpdateFrequency = 1.0 / 10.0;
         myNextUpdate = 0.0;

         myEntity.registerBehavior(this);
      }

      public virtual void postInit()
      {

      }

      public virtual void onUpdate(int pass, double delta)
      {

      }

      public bool shouldUpdate(double delta)
      {
         double currentTime=TimeSource.clockTime();

         if(currentTime >= myNextUpdate)
         {
            myNextUpdate = currentTime + myUpdateFrequency;
            return true;
         }

         return false;
      }

      public virtual void init(LuaObject initData)
      {
         float updateHertz = (float)initData["updateRate"];
         myUpdateFrequency = 1.0f / updateHertz;
      }

      public virtual EventManager.EventResult onEvent(Event e)
      {
         return EventManager.EventResult.IGNORED;
      }

      public String name
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