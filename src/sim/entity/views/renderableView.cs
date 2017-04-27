using System;
using System.Collections.Generic;

using OpenTK;
using Util;
using Engine;

namespace Sim
{
   public class RenderableViewCreator : EntityDatabaseViewCreator
   {
      public RenderableViewCreator()
      {
         myName = "Renderable";
      }

      public override EntityDatabaseView create(EntityDatabase db)
      {
         RenderableView rv = new RenderableView(db);
         return rv;
      }
   } 


   public class RenderableView : EntityDatabaseView
   {
      public RenderableView(EntityDatabase db)
         : base(db)
      {
         myCriteria = shouldAdd;
      }

      //the predicate function that should be called to determine if an entity should be added or not
      bool shouldAdd(Entity e)
      {
         if (e.hasAttribute("renderable"))
         {
            return true;
         }
         return false;
      }
   }
}