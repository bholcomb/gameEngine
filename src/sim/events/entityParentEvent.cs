/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

using Engine;


using Util;
using Events;


namespace Sim
{
	public class ParentChangeEvent : Event
	{
		static EventName theName;

		UInt64 myEntity;
		UInt64 myParent;

		public ParentChangeEvent(): base() { myName=theName; }
		public ParentChangeEvent(UInt64 entity, UInt64 parent) : this(entity, parent, TimeSource.defaultClock.currentTime(), 0.0) { }
		public ParentChangeEvent(UInt64 entity, UInt64 parent, double timeStamp) : this(entity, parent, timeStamp, 0.0) { }
		public ParentChangeEvent(UInt64 entity, UInt64 parent, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
			myParent=parent;
		}

		static ParentChangeEvent()
		{
			theName = new EventName("entity.attribute.parent");
			Entity.registerDispatcher(theName.myName, dispatchAttributeChange);
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public UInt64 parent
		{
			get { return myParent;}
		}
	

	#region "dispatch attribute changes"
	public static void dispatchAttributeChange(Entity e, object att)
	{
		ParentChangeEvent evt=new ParentChangeEvent(e.id, (UInt64)att);
		Kernel.eventManager.queueEvent(evt);
	}

	#endregion
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=sizeof(UInt64);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
			writer.Write(myParent);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
			myParent=reader.ReadUInt64();
		}

	#endregion
	

	}
	
}

	