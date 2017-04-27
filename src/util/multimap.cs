using System;
using System.Collections.Generic;

namespace Util
{

   public class MultiMap<K, V>
   {
      Dictionary<K, List<V>> myDictionary = new Dictionary<K, List<V>>();

      public void Add(K key, V value)
      {
         List<V> list;
         if (this.myDictionary.TryGetValue(key, out list))
         {
            list.Add(value);
         }
         else
         {
            //create a new list
            list = new List<V>();
            list.Add(value);
            this.myDictionary[key] = list;
         }
      }

      public void Remove(K key, V value)
      {
         List<V> list;
         if (this.myDictionary.TryGetValue(key, out list))
         {
            list.Remove(value);
            if (list.Count == 0)
            {
               myDictionary.Remove(key);
            }
         }
      }

      public bool ContainsKey(K key)
      {
         return myDictionary.ContainsKey(key);
      }

      public IEnumerable<K> Keys
      {
         get
         {
            return this.myDictionary.Keys;
         }
      }

      public bool TryGetValue(K key, out List<V> value)
      {
         return this.myDictionary.TryGetValue(key, out value);
      }

      public List<V> this[K key]
      {
         get
         {
            List<V> list;
            if (this.myDictionary.TryGetValue(key, out list))
            {
               return list;
            }
            else
            {
               return new List<V>();
            }
         }
      }
   }
}