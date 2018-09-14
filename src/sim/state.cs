/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;

namespace Sim
{
   public class State
   {
      Entity myEntity;
      Dictionary<int, BaseAttribute> myAttributes = new Dictionary<int, BaseAttribute>();
      public State(Entity e)
      {
         myEntity = e;
      }

      public Entity entity { get { return myEntity; } }

      public void nextFrame()
      {
         foreach (KeyValuePair<int, BaseAttribute> entry in myAttributes)
         {
            entry.Value.update();
         }
      }

      public bool hasAttribute(int name)
      {
         return myAttributes.ContainsKey(name);
      }

      public Attribute<T> attribute<T>(int name)
      {
         BaseAttribute a;
         if (myAttributes.TryGetValue(name, out a))
         {
#if DEBUG
            if (typeof(T) != a.GetType().GetGenericArguments()[0])
            {
               throw new Exception(string.Format("Requested attribute {0} with type {1}, expected {2}", 
                  name, typeof(T).Name, a.GetType().GetGenericArguments()[0].Name));
            }
#endif
            return (Attribute<T>)a;
         }

         return null;
      }

      public void registerAttribute(BaseAttribute att)
      {
         int name = att.name;

         //add or replace the attribute
         myAttributes[name] = att;
      }

   }
}
