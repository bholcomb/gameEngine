using System;
using System.Collections.Generic;

using OpenTK;
using Util;
using Events;
using Engine;

namespace Sim
{
   public class SpatialViewCreator : EntityDatabaseViewCreator
   {
      public SpatialViewCreator()
      {
         myName = "Spatial";
      }

      public override EntityDatabaseView create(EntityDatabase db)
      {
         SpatialView sv = new SpatialView(db);
         return sv;
      }
   } 

   public class SpatialView : EntityDatabaseView, IDisposable
   {
      static Octree<Entity> theOctree;

      Dictionary<UInt64, OctreeElement<Entity>> theOctreeEntityMap = new Dictionary<ulong, OctreeElement<Entity>>();
      Dictionary<OctreeElement<Entity>, UInt64> myReverseOctreeEntityMap = new Dictionary<OctreeElement<Entity>, UInt64>();

      public SpatialView(EntityDatabase db)
         : base(db)
      {
         myCriteria = shouldAdd;
         Kernel.eventManager.addListener(handleAttributeUpdate, "entity.attribute.position");
      }

      static SpatialView()
      {
         //hard coded values for size to match the terrain
         theOctree = new Octree<Entity>(20000, 16, 1000000);
         Application.instance().onPostFrame += new postFrame(SpatialView_onPostFrame);
      }

      static void SpatialView_onPostFrame()
      {
         theOctree.cleanUnusedNodes();
      }

      public Octree<Entity> octree()
      {
         return theOctree;
      }

      public void Dispose()
      {
         Kernel.eventManager.removeListener(handleAttributeUpdate, "entity.attribute.position");
      }

      public void update(UInt64 id)
      {
         OctreeElement<Entity> el;
         if (theOctreeEntityMap.TryGetValue(id, out el))
         {
            if (convertPosition(ref el, el.myObject.attribute<Vector3>("position").value()) == true)
            {
               theOctree.update(el);
            }
         }
      }


      //the predicate function that should be called to determine if an entity should be added or not
      bool shouldAdd(Entity e)
      {
         if (e.hasAttribute("position")==true)
         {
            return true;
         }
         return false;
      }

      protected override void checkEntityAdd(Entity e)
      {
         if (myCriteria(e) == true)
         {
            Attribute<Vector3> pos = e.attribute<Vector3>("position");
            if (e != null)
            {
               addEntity(e, pos.value());               
            }
         }
      }

      protected override void checkEntityRemoved(Entity e)
      {
         if (myCriteria(e) == true)
         {
            OctreeElement<Entity> el;
            if(theOctreeEntityMap.TryGetValue(e.id, out el))
            {
               theOctree.remove(el);
               theOctreeEntityMap.Remove(e.id);
               myReverseOctreeEntityMap.Remove(el);
            }
         }
      }

      public EventManager.EventResult handleAttributeUpdate(Event e)
      {
         PositionChangeEvent em = e as PositionChangeEvent;
         if (em != null)
         {
            //is this an entity we are tracking
            OctreeElement<Entity> el;
            if (theOctreeEntityMap.TryGetValue(em.entity, out el))
            {
               if (convertPosition(ref el, em.position) == true)
               {
                  theOctree.update(el);
               }
               return EventManager.EventResult.HANDLED;
            }
         }

         return EventManager.EventResult.IGNORED;
      }
           
      public void addEntity(Entity e, Vector3 pos)
      {
         OctreeElement<Entity> el = new OctreeElement<Entity>();
         el.myObject = e;
         el.mySize = 2; //need to figure out the best way to get this info, default of 2 is fine for now
         convertPosition(ref el, pos);
         theOctree.insert(el);
         theOctreeEntityMap.Add(e.id, el);
         myReverseOctreeEntityMap.Add(el, e.id);
      }

      //returns whether or not the value changed
      public bool convertPosition(ref OctreeElement<Entity> element, Vector3 pos)
      {
         uint X, Y, Z;
         X = (uint)pos.X;
         Y = (uint)pos.Y;
         Z = (uint)pos.Z;
         
         bool needsUpdate=false;
         if(X!=element.myX || Y!=element.myY || Z!=element.myZ)
         {
            needsUpdate=true;
         }

         //for the moment, just a straight passthrough
         element.myX = X;
         element.myY = Y;
         element.myZ = Z;

         return needsUpdate;
      }

#region "spatial queries"
      public Entity nearestNeighbor(Entity e)
      {
         OctreeElement<Entity> el;
         if (theOctreeEntityMap.TryGetValue(e.id, out el))
         {
            OctreeElement<Entity> nel=theOctree.nearestNeighbor(el);
            if(nel!=null)
            {
               return nel.myObject;
            }
         }
         return null;
      }

      public List<Entity> neighborsWithin(Entity e, double distance)
      {
         OctreeElement<Entity> el;
         if (theOctreeEntityMap.TryGetValue(e.id, out el))
         {
            List<OctreeElement<Entity>> nel=theOctree.neighborsWithin(el, (uint)distance);
            if(nel!=null)
            {
               List<Entity> ret=new List<Entity>(nel.Count);
               foreach(OctreeElement<Entity> oe in nel)
               {
                  ret.Add(oe.myObject);
               }

               return ret;
            }
         }
         return null;
      }

      public List<Entity> intersection(Ray r, float t0, float t1)
      {
         List<OctreeElement<Entity>> octreeHits = theOctree.rayIntersection(r, t0, t1);

         if (octreeHits != null)
         {
            List<Entity> ret = new List<Entity>(octreeHits.Count);
            foreach (OctreeElement<Entity> oe in octreeHits)
            {
               ret.Add(oe.myObject);
            }

            return ret;
         }

         return null;
      }

#endregion
   }
}