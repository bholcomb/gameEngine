using System;
using System.Collections.Generic;
using Util;
using Events;
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
         : base(edb, e => e.hasAttribute("dynamic") ==true && e.attribute<bool>("dynamic") == true)
      {
         //need the default bucket
         myBuckets.Add(new List<Entity>());

         Kernel.eventManager.addListener(handleAttributeChange, "entity.attribute.parent");
         Kernel.eventManager.addListener(handleAttributeChange, "entity.attribute.dynamic");
      }

      public void Dispose()
      {
         Kernel.eventManager.removeListener(handleDependancy, "entity.attribute.parent");
         Kernel.eventManager.removeListener(handleDependancy, "entity.attribute.dynamic");
      }

      public List<List<Entity>> buckets
      {
         get { return myBuckets; }
      }

      protected void addEntity(Entity e)
      {
         if (e.hasAttribute("parent") == true && e.attribute<UInt64>("parent") != 0)
         {
            Entity parent = myDatabase.findEntity(e.attribute<UInt64>("parent").value());
            placeEntity(e, parent);
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
         ParentChangeEvent em = e as ParentChangeEvent;
         if (em != null)
         {
            Entity ent = myDatabase.findEntity(em.entity);
            Entity parent = myDatabase.findEntity(em.parent);

            placeEntity(ent, parent);
         }

         return EventManager.EventResult.HANDLED;
      }

      public EventManager.EventResult handleAttributeChange(Event e)
      {
         DynamicChangeEvent ac = e as DynamicChangeEvent;
         if (ac != null)
         {
            Entity ent = myDatabase.findEntity(ac.entity);
            if (ac.dynamic == true)
               myEntities.Add(ent);
            else
               myEntities.Remove(ent);

            return EventManager.EventResult.HANDLED;
         }

         return EventManager.EventResult.IGNORED;
      }
   }

}