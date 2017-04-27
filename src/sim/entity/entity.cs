using System;
using System.Collections.Generic;

using Util;
using Events;
using Engine;

namespace Sim
{ 
   public class Entity
   {
      public delegate void AttributeChangeDispatch(Entity e, object value);
      static Dictionary<string, AttributeChangeDispatch> theAttributeDispatchers=new Dictionary<string,AttributeChangeDispatch>();

      public static void registerDispatcher(string name, AttributeChangeDispatch del)
      {
         if(theAttributeDispatchers.ContainsKey(name)==false)
            theAttributeDispatchers.Add(name, del);
      }

      EntityDatabase myDb;
      Dictionary<String, BaseAttribute> myAttributes = new Dictionary<String, BaseAttribute>();
      List<Behavior> myBehaviors = new List<Behavior>();
      UInt64 myId;
      UInt64 myAppId;

      MultiMap<int, Behavior> myBehaviorPassInterestMap = new MultiMap<int, Behavior>();
      MultiMap<String, Behavior> myMessageInterestMap = new MultiMap<string, Behavior>(); 

      public Entity(UInt64 id, EntityDatabase db)
      {
         myId = id;
         myDb = db;
         myAppId = myId >> 32;
      }

      public UInt64 id
      {
         get { return myId; }
      }

      public UInt64 appId
      {
         get { return myAppId; }
      }

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

      public void onUpdate(int pass, double delta)
      {
         //in the last pass, switch over the values from the previous frame to 
         //be current
         if (pass == Int32.MaxValue)
         {
            foreach (KeyValuePair<String, BaseAttribute> entry in myAttributes)
            {
               entry.Value.update();
            }

            //and I'm spent
            return;
         }

         //update the behaviors
         foreach (Behavior b in myBehaviorPassInterestMap[pass])
         {
            b.onUpdate(pass, delta);
         }
      }

      public List<Behavior> behaviors
      {
         get { return myBehaviors; }
      }

      public Behavior behavior(String name)
      {
         foreach (Behavior b in myBehaviors)
         {
            if (b.name == name)
               return b;
         }

         return null;
      }

      public bool hasAttribute(String name)
      {
         name = name.ToLower();
         return myAttributes.ContainsKey(name);
      }

      public Attribute<T> attribute<T>(String name)
      {
         BaseAttribute a;
         name = name.ToLower();
         if (myAttributes.TryGetValue(name, out a))
         {
            return (Attribute<T>)a; 
         }

         return null;
      }

      public void registerBehavior(Behavior behavior)
      {
         myBehaviors.Add(behavior);
         EntityBehaviorAddedEvent be=new EntityBehaviorAddedEvent(myId, behavior.name);
         Kernel.eventManager.queueEvent(be);
      }

      public void registerAttribute(BaseAttribute att)
      {
         String name = att.name.ToLower();

         //add or replace the attribute
         myAttributes[name] = att;
      }

      public void postInit()
      {
         foreach (Behavior b in myBehaviors)
         {
            b.postInit();
         }
      }

      public void dispatchAttributeChangeEvent<T>(string name, T val)
      {
         AttributeChangeDispatch del;
         if (theAttributeDispatchers.TryGetValue(name, out del) == true)
         {
            del(this, val);
         }
      }
   
      public void registerEventInterest(String eventType, Behavior behavior)
      {
         myMessageInterestMap.Add(eventType, behavior);
         myDb.addInterest(eventType, this);
      }

      public void removeEventInterest(String eventType, Behavior behavior)
      {
         myMessageInterestMap.Remove(eventType, behavior);
         myDb.removeInterest(eventType, this);
      }

      public void registerPassInterest(String pass, Behavior behavior)
      {
         int passId = SimManager.entityManager.updatePassId(pass);
         myBehaviorPassInterestMap.Add(passId, behavior);
      }

      public void removePassInterest(String pass, Behavior behavior)
      {
         int passId = SimManager.entityManager.updatePassId(pass);
         myBehaviorPassInterestMap.Remove(passId, behavior);
      }
   }
}