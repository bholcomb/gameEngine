/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;

using OpenTK;
using Util;
using Engine;

namespace Sim
{
   public class TypeViewCreator : EntityDatabaseViewCreator
   {
      public TypeViewCreator()
      {
         myName = "Type";
      }

      public override EntityDatabaseView create(EntityDatabase db)
      {
         TypeView sv = new TypeView(db);
         return sv;
      }
   }

   public class TypeView : EntityDatabaseView
   {
      List<String> myAcceptableTypes = new List<string>();

      public TypeView(EntityDatabase db)
         : base(db)
      {
         myCriteria = shouldAdd;
      }

      public void addType(String type)
      {
         if(myAcceptableTypes.Contains(type)==false)
            myAcceptableTypes.Add(type);

         //search for all entities that match the type that already exist in database
         foreach(Entity e in myDatabase.entities.Values)
         {
            if(myAcceptableTypes.Contains(e.type)==true)
            {
               if (myEntities.Contains(e) == false)
               {
                  myEntities.Add(e);
               }
            }
         }
      }

      public void removeType(String type)
      {
         myAcceptableTypes.Remove(type);
      }

      //the predicate function that should be called to determine if an entity should be added or not
      bool shouldAdd(Entity e)
      {
         foreach (String type in myAcceptableTypes)
         {
            if (e.type == type)
               return true;
         }
         return false;
      }
   }
}