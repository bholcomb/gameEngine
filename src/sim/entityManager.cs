/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using OpenTK;
using Util;
using Engine;

namespace Sim
{
   public class EntityManager
   {
      EntityDatabase myDb;
      EntityUpdateView myUpdateableEntites;

      ConcurrentQueue<Event> myEventQueue = new ConcurrentQueue<Event>();

      MultiMap<String, Entity> myMessageInterestMap = new MultiMap<string, Entity>();

      List<Int32> myPasses = new List<Int32>(5);

      EntityFactory myEntityFactory;
      ParameterDatabase myParameterDatabase;

      public EntityManager()
      {
         myDb = new EntityDatabase(this);
         myUpdateableEntites = (EntityUpdateView)myDb.getSharedView("Update");
      }

      public bool init(Initializer init)
      {
         myParameterDatabase = new ParameterDatabase();
         myParameterDatabase.init(init);

         myEntityFactory = new EntityFactory(this);
         myEntityFactory.init(init);


         addUpdatePass(Passes.General);
         addUpdatePass(Passes.Physics);
         addUpdatePass(Passes.Animation);
         addUpdatePass(Passes.Render);
         addUpdatePass(Passes.AttributeUpdate);

         return true;
      }

      public void addUpdatePass(int passNumber)
      {
         myPasses.Add(passNumber);
         myPasses.Sort((a, b) => a.CompareTo(b));
      }

      public void removeUpdatePass(int passNumber)
      {
         myPasses.Remove(passNumber);
      }

      public void tick(double dt)
      {
         //multithreaded message dispatch

         Event evt;
         while (myEventQueue.TryDequeue(out evt))
         {
            if (myMessageInterestMap.ContainsKey(evt.name) == true)
            {
               List<Entity> interestedEntities = myMessageInterestMap[evt.name];
               Parallel.ForEach(interestedEntities, ent=>
                  {
                     Event levt=evt;
                     ent.onMessage(levt);
                  }
               );
            }
         }

         //for each pass
         foreach (Int32 p in myPasses)
         {
            int updatesRemaining = 0;

            //for each bucket group (helps with dependencies) have to be done sequentially
            for (int b = 0; b < myUpdateableEntites.buckets.Count; b++)
            {
               //for each entity in group
               Parallel.ForEach(myUpdateableEntites.buckets[b], e =>
                  {
                     System.Threading.Interlocked.Increment(ref updatesRemaining);
                     int lp = p;

                     e.onUpdate(lp, dt);
                     System.Threading.Interlocked.Decrement(ref updatesRemaining);
                  }
               );
            }

            //wait for running tasks to finish
            while (updatesRemaining > 0)
            {
               System.Threading.Thread.Sleep(0);
            }
         }
      }

      public EventManager.EventResult eventListener(Event e)
      {
         myEventQueue.Enqueue(e);
         return EventManager.EventResult.HANDLED;
      }

      public EntityFactory factory
      {
         get { return myEntityFactory; }
      }

      public ParameterDatabase paramterDatabase
      {
         get { return myParameterDatabase; }
      }

      public EntityDatabase db
      {
         get { return myDb; }
      }

      public Entity createEntity(String templateName)
      {
         Entity e = myEntityFactory.create(templateName);
         if (e != null)
         {
            myDb.addEntity(e);
         }

         return e;
      }

      public Entity createReflectedEntity(String templateName, ulong id)
      {
         Entity e = myEntityFactory.createReflected(templateName, id);
         if (e != null)
         {
            myDb.addEntity(e);
         }

         return e;
      }

      public void destroyEntity(Entity e)
      {
         if (e != null)
         {
            e.shutdown();
            myDb.removeEntity(e);
         }
      }

      public MultiMap<String, Entity> messageInterestMap
      {
         get { return myMessageInterestMap; }
      }

      public void addInterest(String messageName, Entity e)
      {
         if (myMessageInterestMap.ContainsKey(messageName) == false)
         {
            myMessageInterestMap.Add(messageName, e);
            Application.eventManager.addListener(eventListener, messageName);
         }
         else
         {
            //already added the event listener
            if (myMessageInterestMap[messageName].Contains(e) == false)
            {
               myMessageInterestMap.Add(messageName, e);
            }
         }
      }

      public void removeInterest(String messageName, Entity e)
      {
         myMessageInterestMap.Remove(messageName, e);
         if (myMessageInterestMap.ContainsKey(messageName) == false)
         {
            Application.eventManager.removeListener(eventListener, messageName);
         }
      }
   }
}