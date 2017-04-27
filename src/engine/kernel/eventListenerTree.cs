using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using OpenTK;
using OpenTK.Graphics;

using Events;

namespace Engine
{
   public delegate EventManager.EventResult EventListener(Event e);

   public class EventListenerTreeNode
   {
      EventName myName;
      int myDepth = 0;
      List<EventListener> myEventListeners = new List<EventListener>();
      Dictionary<int, EventListenerTreeNode> myChildren = new Dictionary<int, EventListenerTreeNode>();

      public EventListenerTreeNode(String name, int depth)
      {
         myName = new EventName(name);
         myDepth = depth;
      }

      public int depth
      {
         get { return myDepth; }
      }

      public bool matches(EventName eventName)
      {
         return myName.matches(eventName);
      }

      public bool matches(EventName eventName, int depth)
      {
         return myName.matches(eventName, depth);
      }

      public EventManager.EventResult dispatchEvent(Event e)
      {
         EventManager.EventResult ret = EventManager.EventResult.IGNORED;
         foreach (EventListener el in myEventListeners)
         {
            EventManager.EventResult res = el(e);
            if (res == EventManager.EventResult.EATEN)
            {
               return res;
            }
            if (res == EventManager.EventResult.HANDLED)
            {
               ret = res;
            }
         }

         //give the children a chance to handle it
         if (myChildren.Count > 0)
         {
            int id = e.id.myIds[myDepth];
            if (myChildren.ContainsKey(id))
            {
               EventManager.EventResult res = myChildren[id].dispatchEvent(e);
               if (res == EventManager.EventResult.EATEN)
               {
                  return res;
               }
               if (res == EventManager.EventResult.HANDLED)
               {
                  ret = res;
               }
            }
         }

         return ret;
      }

      public EventListenerTreeNode addChild(String s)
      {
         EventListenerTreeNode node = null;
         EventName en = new EventName(s);

         int val = en.myIds[myDepth];

         if (myChildren.TryGetValue(val, out node))
         {
            node = myChildren[val];
         }
         else
         {
            node = new EventListenerTreeNode(s, myDepth + 1);
            myChildren[val] = node;
         }

         return node;
      }

      public bool addEventListener(EventListener func, String name)
      {
         if (name == myName.myName)
         {
            myEventListeners.Add(func);
            return true;
         }

         foreach (EventListenerTreeNode eln in myChildren.Values)
         {
            if (eln.addEventListener(func, name) == true)
            {
               return true;
            }
         }

         return false;
      }

      public bool removeEventListener(EventListener func, String name)
      {
         if (name == myName.myName)
         {
            myEventListeners.Remove(func);
            return true;
         }

         foreach (EventListenerTreeNode eln in myChildren.Values)
         {
            if (eln.removeEventListener(func, name) == true)
            {
               return true;
            }
         }

         return false;
      }
   }
}

