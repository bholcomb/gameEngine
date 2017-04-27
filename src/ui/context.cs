
using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;

namespace UI
{
   public class Context
   {
      Dictionary<UInt32, Dictionary<String, Object>> myMap = new Dictionary<UInt32, Dictionary<String, Object>>();
      
      public Context()
      {
         
      }

      public T getValue<T>(UInt32 id, String name)
      {
         Dictionary<String, object> objMap;
         if (myMap.TryGetValue(id, out objMap) == false)
         {
            throw new Exception(String.Format("Unknown UI Id: {0}", id));
         }

         Object val;
         if (objMap.TryGetValue(name, out val) == false)
         {
            throw new Exception(String.Format("Unknown field: {0} for UI Id: {1}", name, id));
         }

         return (T)val;
      }

      public void setValue(UInt32 id, String name, Object value)
      {
         Dictionary<String, object> objMap;
         if (myMap.TryGetValue(id, out objMap) == false)
         {
            objMap = new Dictionary<string, object>();
            myMap[id] = objMap;
         }

         objMap[name] = value;
      }
   }
}