/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;

using Util;
using Engine;

namespace Sim
{ 
   public class Entity
   {
      EntityManager myManager;
      State myState;
      List<Behavior> myBehaviors = new List<Behavior>();

      UInt64 myId;
      bool myReflected = false;
      string myType;

      MultiMap<int, Behavior> myBehaviorPassInterestMap = new MultiMap<int, Behavior>();
      MultiMap<string, Behavior> myMessageInterestMap = new MultiMap<string, Behavior>(); 

      public Entity(UInt64 id, EntityManager mgr)
      {
         myId = id;
         myType = type;
         myManager = mgr;
         myState = new State(this);
      }

      public string type {
         get { return myType; }
         set { myType = value; }
      }
 
      public UInt64 id { get { return myId; } }

      public bool reflected {  get { return myReflected; } }

      public EntityManager manager { get { return myManager; } }

      public EventManager.EventResult onMessage(Event e)
      {
         EventManager.EventResult ret = EventManager.EventResult.IGNORED;

         List<Behavior> interestedBehaviors;
         if(myMessageInterestMap.TryGetValue(e.name, out interestedBehaviors)==true)
         {
            foreach (Behavior b in interestedBehaviors)
            {
               ret = b.onEvent(e);
            }
         }

         return ret;
      }

      public void postInit()
      {
         foreach (Behavior b in myBehaviors)
         {
            b.postInit();
         }
      }

      public void onUpdate(int pass, double delta)
      {
         //in the last pass, switch over the values from the previous frame to 
         //be current
         if (pass == Int32.MaxValue)
         {
            myState.nextFrame();
         }

         //update the behaviors
         foreach (Behavior b in myBehaviorPassInterestMap[pass])
         {
            b.onUpdate(pass, delta);
         }
      }

      public void shutdown()
      {
         foreach(Behavior b in myBehaviors)
         {
            b.shutdown();
         }
      }

      public List<Behavior> behaviors
      {
         get { return myBehaviors; }
      }

      public Behavior findBehavior(int name)
      {
         foreach (Behavior b in myBehaviors)
         {
            if (b.name == name)
               return b;
         }

         return null;
      }

      public State state {  get { return myState; } }

      public void registerBehavior(Behavior behavior)
      {
         myBehaviors.Add(behavior);
         EntityBehaviorAddedEvent be = new EntityBehaviorAddedEvent(myId, behavior.name);
         Application.eventManager.queueEvent(be);
      }

      public void registerEventInterest(String eventType, Behavior behavior)
      {
         myMessageInterestMap.Add(eventType, behavior);
         myManager.addInterest(eventType, this);
      }

      public void removeEventInterest(String eventType, Behavior behavior)
      {
         myMessageInterestMap.Remove(eventType, behavior);
         myManager.removeInterest(eventType, this);
      }

      public void registerPassInterest(Int32 pass, Behavior behavior)
      {
         myBehaviorPassInterestMap.Add(pass, behavior);
      }

      public void removePassInterest(Int32 pass, Behavior behavior)
      {
         myBehaviorPassInterestMap.Remove(pass, behavior);
      }
   }
}