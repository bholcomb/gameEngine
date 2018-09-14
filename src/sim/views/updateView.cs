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
   public class EntityUpdateViewCreator : EntityDatabaseViewCreator
   {
      public EntityUpdateViewCreator()
      {
         myName = "Update";
      }

      public override EntityDatabaseView create(EntityDatabase db)
      {
         EntityUpdateView rv = new EntityUpdateView(db);
         return rv;
      }
   }
   public class EntityUpdateView : EntityDatabaseView, IDisposable
   {
      //entities in bucket 0 are updated before entities in bucket 1
      //allows for parent/child relationships 
      List<List<Entity>> myBuckets = new List<List<Entity>>();

      public EntityUpdateView(EntityDatabase edb)
         : base(edb, e => e.state.hasAttribute(Attributes.Dynamic) ==true && e.state.attribute<bool>(Attributes.Dynamic) == true)
      {
         //need the default bucket
         myBuckets.Add(new List<Entity>());

         Application.eventManager.addListener(handleAttributeChange, "entity.attribute.parent");
         Application.eventManager.addListener(handleAttributeChange, "entity.attribute.dynamic");
      }

      public void Dispose()
      {
         Application.eventManager.removeListener(handleDependancy, "entity.attribute.parent");
         Application.eventManager.removeListener(handleDependancy, "entity.attribute.dynamic");
      }

      public List<List<Entity>> buckets
      {
         get { return myBuckets; }
      }

      protected void addEntity(Entity e)
      {
         Attribute<UInt64> parent = e.state.attribute<UInt64>(Attributes.Parent);
         if (parent != null && parent.value() != 0)
         {
            Entity parentEntity = myDatabase.findEntity(parent.value());
            placeEntity(e, parentEntity);
         }
         else
         {
            myBuckets[0].Add(e);
         }
      }

      protected void removeEntity(Entity e)
      {
         for (int i = 0; i < myBuckets.Count; i++)
         {
            if (myBuckets[i].Contains(e) == true)
            {
               myBuckets[i].Remove(e);
               break;
            }
         }
      }

      protected override void checkEntityAdd(Entity e)
      {
         if (myCriteria(e) == true)
         {
            addEntity(e);
         }
      }

      protected override void checkEntityRemoved(Entity e)
      {
         if (myCriteria(e) == true)
         {
            removeEntity(e);
         }
      }

      public void placeEntity(Entity ent, Entity parent)
      {
         int parentBucket = -1;
         int entBucket = -1;

         for (int i = 0; i < myBuckets.Count; i++)
         {
            if (entBucket == -1 && myBuckets[i].Contains(ent) == true)
            {
               entBucket = i;
            }
            if (parentBucket == -1 && myBuckets[i].Contains(parent) == true)
            {
               parentBucket = i;
            }
         }

         if (entBucket <= parentBucket)
         {
            myBuckets[entBucket].Remove(ent);
            if(myBuckets.Count-1<parentBucket+1)
            {
               myBuckets.Add(new List<Entity>());
            }

            myBuckets[parentBucket + 1].Add(ent);
         }
      }

      public EventManager.EventResult handleDependancy(Event e)
      {
         AttributeChangedEvent<UInt64> em = e as AttributeChangedEvent<UInt64>;
         if (em != null && em.attributeId == Attributes.Parent)
         {
            Entity ent = myDatabase.findEntity(em.entity);
            Entity parent = myDatabase.findEntity(em.value);

            placeEntity(ent, parent);
         }

         return EventManager.EventResult.HANDLED;
      }

      public EventManager.EventResult handleAttributeChange(Event e)
      {
         AttributeChangedEvent<bool> ac = e as AttributeChangedEvent<bool>;
         if (ac != null && ac.attributeId == Attributes.Dynamic) 
         {
            Entity ent = myDatabase.findEntity(ac.entity);
            if (ac.value == true)
               myEntities.Add(ent);
            else
               myEntities.Remove(ent);

            return EventManager.EventResult.HANDLED;
         }

         return EventManager.EventResult.IGNORED;
      }
   }

}