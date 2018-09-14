/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;

using Util;

namespace Engine
{
   public class EventManager
   {
      EventListenerTreeNode myRootEventListener = new EventListenerTreeNode("*", 0);
      ConcurrentQueue<Event> myEventQueue = new ConcurrentQueue<Event>();

      public int processedMessages { get; set; }

      struct EventListenerInfo
      {
         public string eventName;
         public EventListener func;
      }

      bool myIsTicking = false;
      ConcurrentQueue<EventListenerInfo> myPendingAdds = new ConcurrentQueue<EventListenerInfo>();
      ConcurrentQueue<EventListenerInfo> myPendingRemoves = new ConcurrentQueue<EventListenerInfo>();

      ConcurrentQueue<Event> myPendingEvents = new ConcurrentQueue<Event>();

      Random myRand = new Random();

      public enum EventResult { IGNORED, HANDLED, EATEN };

      public EventManager()
      {
         registerEvents();
      }

      public ConcurrentQueue<Event> eventQueue
      {
         get { return myEventQueue; }
      }

      public bool init(Initializer init)
      {
         return true;
      }

      public void shutdown()
      {

      }

      public bool tick()
      {
         return tick(-1.0, -1.0);
      }

      public bool tick(double maxSeconds)
      {
         return tick(-1.0, maxSeconds);
      }

      public bool tick(double minSeconds, double maxSeconds)
      {
         processedMessages = 0;
         double startTime = TimeSource.clockTime();
         double minTime = minSeconds == -1.0 ? startTime : startTime + minSeconds;
         double maxTime = maxSeconds == -1.0 ? startTime + 1000000.0 : startTime + maxSeconds;

         myIsTicking = true;

         //determine number of messages to process for this frame
         int count = myEventQueue.Count;

         int runningTasks=0;

         //keep looping if we have time
         while (TimeSource.currentTime() < maxTime && processedMessages < count)
         {
            Event e;

            //no message available, sleep until min time is met
            if (!myEventQueue.TryDequeue(out e))
            {
               break;
            }
            processedMessages++;

            //is it time to process this event (i.e enough delay has passed)
            if (TimeSource.currentTime() - e.timeStamp < e.delay)
            {
               //still has time left, put it back on the queue
               myEventQueue.Enqueue(e);
               continue;
            }

            EventResult res = dispatchEvent(e);
         }

         //wait for running tasks to finish
         while (runningTasks > 0)
         {
            System.Threading.Thread.Sleep(0);
         }

         myIsTicking = false;

         //handle any changes that may have occurred during event processing 
         //can't do this during event processing since it may invalidate 
         //iterators
         EventListenerInfo eli;
         while(myPendingAdds.TryDequeue(out eli))
         {
            registerEvent(eli.eventName);
            myRootEventListener.addEventListener(eli.func, eli.eventName);
         }

         while (myPendingRemoves.TryDequeue(out eli))
         {
            myRootEventListener.removeEventListener(eli.func, eli.eventName);
         }

         //sleep for the rest of the time
         double now = TimeSource.clockTime();
         if (now < minTime)
         {
            double sleepTime = minTime - now;
            Thread.Sleep((int)(sleepTime*1000.0f));
         }

         return true;
      }

      public void addListener(EventListener func, String type)
      {
         //if adding during an event dispatch, queue for later
         if (myIsTicking == true)
         {
            EventListenerInfo i = new EventListenerInfo();
            i.func = func;
            i.eventName = type;
            myPendingAdds.Enqueue(i);
         }
         //otherwise just add the event handler here
         else
         {
            registerEvent(type);
            myRootEventListener.addEventListener(func, type);
         }
      }

      public void removeListener(EventListener func, String type)
      {
         //don't remove event handlers during tick events
         if (myIsTicking == true)
         {
            EventListenerInfo i = new EventListenerInfo();
            i.func = func;
            i.eventName = type;
            myPendingRemoves.Enqueue(i);
         }
         //otherwise it is safe to remove them here
         else
         {
            myRootEventListener.removeEventListener(func, type);
         }
      }

      public bool queueEvent(Event e)
      {
         myEventQueue.Enqueue(e);
         return true;
      }

      public void registerEvent(String eventName)
      {
         String[] tokens = eventName.Split('.');
         EventListenerTreeNode node = myRootEventListener;

         for (int i = 0; i < tokens.Length; i++)
         {
            if (tokens[i] == "*")
            {
               continue;
            }

            String name = tokens[0];
            for (int j = 1; j <= i; j++)
            {
               name = name + "." + tokens[j];
            }

            //is this the last token in the event name, don't add a wildcard
            if (i == tokens.Length - 1)
            {
               node = node.addChild(name);
            }
            else
            {
               node = node.addChild(name + ".*");
            }
         }
      }

      protected EventManager.EventResult dispatchEvent(Event e)
      { 
         EventManager.EventResult res;
         res= myRootEventListener.dispatchEvent(e);
         return res;
      }

      protected void registerEvents()
      {
         //poll the assemblies for classes derived from event and call the static constructors to register any attribute change events
         Assembly assembly = Assembly.GetExecutingAssembly();
         foreach (Type type in assembly.GetExportedTypes())
         {
            if (type.IsSubclassOf(typeof(Event)))
            {
               Type[] types = new Type[0];
               ConstructorInfo ci = type.GetConstructor(types);
               if (ci != null)
               {
                  ci.Invoke(null);
               }
            }
         }
      }
   }
}