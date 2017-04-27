using System;
using Util;
using Events;
using Engine;

namespace Sim
{
   public class AttributeChangedMessage : Event
   {
      static EventName theName;
      UInt64 myEntity;
      String myAttributeName;

      public AttributeChangedMessage() :this(0, "") { }
      public AttributeChangedMessage(UInt64 ent, String att) : this(ent, att, TimeSource.currentTime(), 0.0) { }
      public AttributeChangedMessage(UInt64 ent, String att, double timeStamp, double delay)
         : base(timeStamp, delay)
      {
         myEntity = ent;
         myAttributeName = att;

         myName = theName;
      }

      static AttributeChangedMessage()
      {
         theName = new EventName("entity.attribute.change");
      }

      public UInt64 entityId
      {
         get { return myEntity; }
      }

      public String attributeName
      {
         get { return myAttributeName; }
      }
   }

   public class EntityAddedMessage : Event
   {
      static EventName theName;

      UInt64 myEntity;

      public EntityAddedMessage() : this(0) { }
      public EntityAddedMessage(UInt64 ent) : this(ent, TimeSource.currentTime(), 0.0) {}
      public EntityAddedMessage(UInt64 ent, double timeStamp, double delay)
         : base(timeStamp, delay)
      {
         myEntity = ent;
         myName = theName;
      }

      static EntityAddedMessage()
      {
         theName=new EventName("entity.added");
      }

      public UInt64 entityId
      {
         get{return myEntity;}
      }
   }

   public class EntityRemovedMessage : Event
   {
      static EventName theName;
      UInt64 myEntity;

      public EntityRemovedMessage() : this(0) { }
      public EntityRemovedMessage(UInt64 ent) : this(ent, TimeSource.currentTime(), 0.0) { }
      public EntityRemovedMessage(UInt64 ent, double timeStamp, double delay)
         : base(timeStamp, delay)
      {
         myEntity = ent;
         myName = theName;
      }

      static EntityRemovedMessage()
      {
         theName=new EventName("entity.removed");
      }

      public UInt64 entityId
      {
         get{return myEntity;}
      }
   }

   public class EntityDependancyMessage : Event
   {
      static EventName theName;
      UInt64 myEntity;
      UInt64 myParent;

      public EntityDependancyMessage() : this(0, 0) { }
      public EntityDependancyMessage(UInt64 ent, UInt64 parent) : this(ent, parent, TimeSource.currentTime(), 0.0) { }
      public EntityDependancyMessage(UInt64 ent, UInt64 parent, double timeStamp, double delay)
         : base(timeStamp, delay)
      {
         myEntity = ent;
         myParent = parent;
         myName = theName;
      }

      static EntityDependancyMessage()
      {
         theName=new EventName("entity.dependency");
      }

      public UInt64 entityId
      {
         get { return myEntity; }
      }

      public UInt64 parent
      {
         get { return myParent; }
      }
   }



}