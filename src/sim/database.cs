/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Util;
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

      EntityManager myEntityManager;

      public EntityDatabase(EntityManager em)
      {
         myEntityManager = em;

         addViewCreator(new SpatialViewCreator());
         addViewCreator(new RenderableViewCreator());
         addViewCreator(new EntityUpdateViewCreator());
         addViewCreator(new TypeViewCreator());
      }

      public EntityManager entityManager { get { return myEntityManager; } }

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
         onEntityAdded(e);

         EntityAddedEvent em = new EntityAddedEvent(e.id, e.state.attribute<String>(Attributes.Type).value());
         Application.eventManager.queueEvent(em);
         return e;
      }

      public void removeEntity(Entity e)
      {
         onEntityRemoved(e);
         EntityRemovedEvent em = new EntityRemovedEvent(e.id);
         Application.eventManager.queueEvent(em);
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

      public EntityDatabaseView createView(String viewName)
      {
         EntityDatabaseViewCreator c;
         if (myViewCreators.TryGetValue(viewName, out c))
         {
            EntityDatabaseView view = c.create(this);
            myViews[viewName] = view;
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

      public event EntityAddedCb onEntityAdded;
      public event EntityRemovedCb onEntityRemoved;

   }
}