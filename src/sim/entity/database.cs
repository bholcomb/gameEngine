
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Util;
using Events;
using Engine;

namespace Sim
{
   public delegate void EntityAddedCb(Entity e);
   public delegate void EntityRemovedCb(Entity e);

   public class EntityDatabase
   {
      Dictionary<UInt64, Entity> myEntityMap = new Dictionary<UInt64, Entity>();
      Dictionary<String, EntityDatabaseView> myViews = new Dictionary<string, EntityDatabaseView>();
      Dictionary<String, EntityDatabaseViewCreator> myViewCreators = new Dictionary<string, EntityDatabaseViewCreator>();

      MultiMap<String, Entity> myMessageInterestMap = new MultiMap<string, Entity>();
      EntityManager myEntityManager;

      public EntityDatabase(EntityManager em)
      {
         myEntityManager = em;

         addViewCreator(new SpatialViewCreator());
         addViewCreator(new RenderableViewCreator());
         addViewCreator(new EntityUpdateViewCreator());
         addViewCreator(new TypeViewCreator());
      }

      public Dictionary<UInt64, Entity> entities
      {
         get { return myEntityMap; }
      }

      public int count
      {
         get { return myEntityMap.Count; }
      }

      public Entity addEntity(Entity e)
      {
         myEntityMap[e.id] = e;
         entityAdded(e);

         EntityAddedEvent em = new EntityAddedEvent(e.id, e.attribute<String>("type").value());
         Kernel.eventManager.queueEvent(em);
         return e;
      }

      public void removeEntity(Entity e)
      {
         entityRemoved(e);
         EntityRemovedEvent em = new EntityRemovedEvent(e.id);
         Kernel.eventManager.queueEvent(em);
      }

      public Entity findEntity(UInt64 id)
      {
         Entity e;
         if (myEntityMap.TryGetValue(id, out e))
         {
            return e;
         }

         return null;
      }

      public EntityDatabaseView getView(String viewName)
      {
         EntityDatabaseViewCreator c;
         if (myViewCreators.TryGetValue(viewName, out c))
         {
            EntityDatabaseView view = c.create(this);
            return view;
         }

         throw new Exception("Unknown view name: " + viewName);
      }

      public EntityDatabaseView getSharedView(String viewName)
      {
         EntityDatabaseView v;
         if (myViews.TryGetValue(viewName, out v))
         {
            return v;
         }

         EntityDatabaseViewCreator c;
         if (myViewCreators.TryGetValue(viewName, out c))
         {
            myViews[viewName] = c.create(this);
            return myViews[viewName];
         }

         throw new Exception("Unknown view name: " + viewName);
      }

      public void addViewCreator(EntityDatabaseViewCreator creator)
      {
         myViewCreators[creator.name] = creator;
      }

      public event EntityAddedCb entityAdded;
      public event EntityRemovedCb entityRemoved;

      public MultiMap<String, Entity> messageInterestMap
      {
         get { return myMessageInterestMap; }
      }

      public void addInterest(String messageName, Entity e)
      {
         if (myMessageInterestMap.ContainsKey(messageName) == false)
         {
            myMessageInterestMap.Add(messageName, e);
            myEntityManager.addEventInterest(messageName);
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
            myEntityManager.removeEventInterest(messageName);
         }
      }

      public EventManager.EventResult messageHandler(Event e)
      {
         EventManager.EventResult ret = EventManager.EventResult.IGNORED;

         foreach (Entity ent in myMessageInterestMap[e.name])
         {
            EventManager.EventResult res;
            res=ent.onMessage(e);
            if (res == EventManager.EventResult.EATEN)
            {
               return res;
            }
           
            if (res == EventManager.EventResult.HANDLED)
            {
               ret = res;
            }
         }

         return ret;
      }
   }
}