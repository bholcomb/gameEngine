using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using OpenTK;
using Util;
using Events;
using Engine;

namespace Sim
{
   public class EntityManager : Engine.Task
   {
      EntityDatabase myDb;
      EntityUpdateView myUpdateableEntites;

      ConcurrentQueue<Event> myEventQueue = new ConcurrentQueue<Event>();

      struct Pass
      {
         public int passNumber;
         public string passName;
      }
      List<Pass> myPasses = new List<Pass>(5);

      EntityFactory myEntityFactory;
      ParameterDatabase myParameterDatabase;

      public EntityManager()
         : base("Entity Update")
      {
         myDb = new EntityDatabase(this);
         myUpdateableEntites = (EntityUpdateView)myDb.getSharedView("Update");

         addUpdatePass("general", 1);
         addUpdatePass("physics", 2);
         addUpdatePass("animation", 3);
         addUpdatePass("render", 4);
         addUpdatePass("updateAttributes", int.MaxValue);

         Kernel.taskManager.attach(this);

         frequency = 30;
      }

      public bool init(Initializer init)
      {
         myParameterDatabase = new ParameterDatabase();
         myParameterDatabase.init(init);

         myEntityFactory = new EntityFactory(this);
         myEntityFactory.init(init);

         return true;
      }

      public void addUpdatePass(string name, int passNumber)
      {
         Pass p;
         p.passName = name;
         p.passNumber = passNumber;
         myPasses.Add(p);
         myPasses.Sort((a, b) => a.passNumber.CompareTo(b.passNumber));
      }

      public int updatePassId(string name)
      {
         foreach (Pass p in myPasses)
         {
            if (p.passName == name)
               return p.passNumber;
         }

         throw new Exception("Unknown pass name " + name);
      }

      public void removeUpdatePass(int passNumber)
      {
         foreach (Pass p in myPasses)
         {
            if (p.passNumber == passNumber)
            {
               myPasses.Remove(p);
               return;
            }
         }
      }

      public void removeUpdatePass(string name)
      {
         foreach (Pass p in myPasses)
         {
            if (p.passName == name)
            {
               myPasses.Remove(p);
               return;
            }
         }
      }

      protected override void onUpdate(double dt)
      {
         //multithreaded message dispatch

         Event evt;
         while (myEventQueue.TryDequeue(out evt))
         {
            if (myDb.messageInterestMap.ContainsKey(evt.name) == true)
            {
               List<Entity> interestedEntities = myDb.messageInterestMap[evt.name];
               Parallel.ForEach(interestedEntities, ent=>
                  {
                     Event levt=evt;
                     ent.onMessage(levt);
                  }
               );
            }
         }

         //for each pass
         foreach (Pass p in myPasses)
         {
            int updatesRemaining = 0;

            //for each bucket group (helps with dependencies) have to be done sequentially
            for (int b = 0; b < myUpdateableEntites.buckets.Count; b++)
            {
               //for each entity in group
               Parallel.ForEach(myUpdateableEntites.buckets[b], e =>
                  {
                     System.Threading.Interlocked.Increment(ref updatesRemaining);
                     int lp = p.passNumber;

                     String s = "Entity update: " + p.passName;
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

      public void addEventInterest(string name)
      {
         Kernel.eventManager.addListener(eventListener, name);
      }

      public void removeEventInterest(string name)
      {
         Kernel.eventManager.removeListener(eventListener, name);
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
         myDb.removeEntity(e);
      }
   }
}