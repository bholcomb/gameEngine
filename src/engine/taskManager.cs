/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;

using Util;

namespace Engine
{
   public delegate void TaskFunction(double dt);
   public delegate void TasksChanged();


   public class TaskManager
   {
      Mutex myAddProcessMutex = new Mutex();
      Mutex myRemoveProcessMutex = new Mutex();

      //public for gui, nobody else
      public List<Task> myTaskList = new List<Task>();
      List<Task> myToBeAddedList = new List<Task>();
      List<Task> myToBeRemovedList = new List<Task>();

      public event TasksChanged onTasksChanged;

      public TaskManager()
      {
      }

      public bool init(Initializer init)
      {
         return true;
      }

      public void shutdown()
      {
      }

      public Task findTask(string name)
      {
         Task t=myTaskList.Find(item=> item.name()==name);
         return t;
      }

      public void attach(Task process)
      {
         myAddProcessMutex.WaitOne();
         myToBeAddedList.Add(process);
         myAddProcessMutex.ReleaseMutex();

         process.attached = true;
         process.active = true;
      }

      public void detach(String name)
      {
         foreach (Task t in myTaskList)
         {
            if (t.name() == name)
            {
               detach(t);
            }
         }
      }

      public void detach(Task process)
      {
         myRemoveProcessMutex.WaitOne();
         myToBeRemovedList.Add(process);
         myRemoveProcessMutex.ReleaseMutex();

         process.attached = false;
         process.active = false;
      }

      public bool hasProccess()
      {
         return myTaskList.Capacity == 0;
      }

      public bool isProcessActive(String name)
      {
         return true;
      }

      public void tick(double deltatime)
      {
         //clean out any dead tasks
         foreach (Task t in myTaskList)
         {
            if (t.isDead() == true)
            {
               if (t.next != null)
               {
                  //execute the next chained task
                  attach(t.next);
                  t.next = null;
               }

               detach(t);
            }
         }

         //add any new tasks
         foreach (Task t in myToBeAddedList)
         {
            myTaskList.Add(t);
            if (onTasksChanged != null)
            {
               onTasksChanged();
            }
         }
         myToBeAddedList.Clear();

         //remove any old tasks
         foreach (Task t in myToBeRemovedList)
         {
            myTaskList.Remove(t);
            if (onTasksChanged != null)
            {
               onTasksChanged();
            }
         }
         myToBeRemovedList.Clear();

         //sort task list based on priority
         myTaskList.Sort((a, b) => a.priority.CompareTo(b.priority));
         
         //task tick loop
         foreach (Task t in myTaskList)
         {
            t.tick(deltatime);
         }

      }
   }
}

