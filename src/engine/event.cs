/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Engine
{
   public class EventId
   {
      public string myName;
      public List<int> myIds = new List<int>();
      public static int theWildcard = 0;

      public EventId(string name)
      {
         myName = name;
         String[] tokens = myName.ToLower().Split('.');
         foreach (String s in tokens)
         {
            if (s == "*")
            {
               myIds.Add(theWildcard);
            }
            else
            {
               myIds.Add(s.GetHashCode());
            }
         }
      }

      public bool matches(EventId name)
      {
         int myCount = myIds.Count;
         int otherCount = name.myIds.Count;
         bool match = true;
         int place = 0;

         while (match)
         {
            if (myIds[place] != name.myIds[place] && myIds[place] != theWildcard)
            {
               match = false;
            }

            place += 1;
            if (place >= myCount || place >= otherCount)
            {
               break;
            }
         }

         return match;
      }

      public bool matches(EventId name, int depth)
      {
         int myCount = myIds.Count;
         int otherCount = name.myIds.Count;

         if (depth >= myCount || depth >= otherCount)
         {
            return false;
         }

         if (myIds[depth] != name.myIds[depth] && myIds[depth] != theWildcard)
         {
            return false;
         }

         return true;
      }
   }

   public class Event
   {
      protected String myName;
      protected EventId myId;
      protected Type myTypeInfo;
      protected double myTimeStamp;
      protected double myDelay;

      public Event() : this(0.0, 0.0) { }
      public Event(double timestamp) : this(timestamp, 0.0) { }
      public Event(double timestamp, double delay)
      {
         myTimeStamp = timestamp;
         myDelay = delay;
         myTypeInfo = this.GetType();
      }

      public double timeStamp
      {
         get { return myTimeStamp; }
         set { myTimeStamp = value; }
      }

      public double delay
      {
         get { return myDelay; }
         set { myDelay = value; }
      }

      public String name
      {
         get { return myName; }
      }

      public EventId id
      {
         get { return myId; }
      }

#region "Serialization/Deserialization"
      public virtual Byte[] encode()
      {
         MemoryStream ms = new MemoryStream(messageSize());
         BinaryWriter writer = new BinaryWriter(ms);
         serialize(ref writer);

         return ms.ToArray();
      }

      public static Event decode(ref Byte[] bytes)
      {
         Event e = createEvent(ref bytes);
         return e;
      }

      protected virtual int messageSize()
      {
         int temp = 0;
         temp += 4; //for the message size
         int nameSize = System.Text.Encoding.UTF8.GetByteCount(myTypeInfo.AssemblyQualifiedName);
         temp += nameSize < 128 ? 1 : 2; //size of name, implicitly included by string
         temp += nameSize;
         temp += 16; //mytimestamp and myDelay (2 * 8 bytes)

         return temp;
      }

      protected virtual void serialize(ref BinaryWriter writer)
      {
         writer.Write(messageSize());
         writer.Write(myTypeInfo.AssemblyQualifiedName);
         writer.Write(myTimeStamp);
         writer.Write(myDelay);
      }

      protected virtual void deserialize(ref BinaryReader reader)
      {
         int size = reader.ReadInt32();
         String classname = reader.ReadString();
         myTimeStamp = reader.ReadDouble();
         myDelay = reader.ReadDouble();
      }

      protected static Event createEvent(ref byte[] b)
      {
         //get name of class
         MemoryStream ms = new MemoryStream(b);
         BinaryReader reader = new BinaryReader(ms);
         int size = reader.ReadInt32();
         String className = reader.ReadString();

         //create an instance of object
         Type t = Type.GetType(className);
         Event e = (Event)Activator.CreateInstance(t);

         //reset the stream so that it deserializes from the beginning
         ms.Seek(0, 0);

         //fill it in
         e.deserialize(ref reader);

         return e;
      }
#endregion
   }
}