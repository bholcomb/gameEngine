using System;
using System.Collections.Generic;

using Util;
using Engine;

namespace Sim
{
   public abstract class EntityDatabaseViewCreator
   {
      protected String myName;

      public EntityDatabaseViewCreator()
      {
      }

      public abstract EntityDatabaseView create(EntityDatabase db);

      public String name
      {
         get { return myName; }
      }
   } 

   public class EntityDatabaseView
   {
      protected List<Entity> myEntities=new List<Entity>();
      protected EntityDatabase myDatabase;
      protected Predicate<Entity> myCriteria;

      public event EntityAddedCb onEntityAdded;
      public event EntityRemovedCb onEntityRemoved;

      public EntityDatabaseView(EntityDatabase db) : this(db, null) { }
      public EntityDatabaseView(EntityDatabase db, Predicate<Entity> criteria)
      {
         myDatabase = db;
         myDatabase.onEntityAdded += new EntityAddedCb(checkEntityAdd);
         myDatabase.onEntityRemoved += new EntityRemovedCb(checkEntityRemoved);
         myCriteria = criteria;
      }

      public List<Entity> entities
      {
         get { return myEntities; }
      }

      //called when an new entity is added to the main entity database
      protected virtual void checkEntityAdd(Entity e)
      {
         if (myCriteria == null)
         {
            return;
         }

         if (myCriteria(e) == true)
         {
            myEntities.Add(e);
            if (onEntityAdded != null)
            {
               //signal that an entity was added
               onEntityAdded(e);
            }
         }
      }

      //called when an new entity is removed to the main entity database
      protected virtual void checkEntityRemoved(Entity e)
      {
         if (myCriteria == null)
         {
            return;
         }

         if (myCriteria(e) == true)
         {
            myEntities.Remove(e);
            if (onEntityRemoved != null)
            {
               onEntityRemoved(e);
            }
         }
      }
   }
}