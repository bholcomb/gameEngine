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
	public class DynamicChangeEvent : Event
	{
		static EventName theName;

		UInt64 myEntity;
		bool myDynamic;

		public DynamicChangeEvent(): base() { myName=theName; }
		public DynamicChangeEvent(UInt64 entity, bool dynamic) : this(entity, dynamic, TimeSource.defaultClock.currentTime(), 0.0) { }
		public DynamicChangeEvent(UInt64 entity, bool dynamic, double timeStamp) : this(entity, dynamic, timeStamp, 0.0) { }
		public DynamicChangeEvent(UInt64 entity, bool dynamic, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
			myDynamic=dynamic;
		}

		static DynamicChangeEvent()
		{
			theName = new EventName("entity.attribute.dynamic");
			Entity.registerDispatcher(theName.myName, dispatchAttributeChange);
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public bool dynamic
		{
			get { return myDynamic;}
		}
	

	#region "dispatch attribute changes"
	public static void dispatchAttributeChange(Entity e, object att)
	{
		DynamicChangeEvent evt=new DynamicChangeEvent(e.id, (bool)att);
		Kernel.eventManager.queueEvent(evt);
	}

	#endregion
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=sizeof(bool);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
			writer.Write(myDynamic);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
			myDynamic=reader.ReadBoolean();
		}

	#endregion
	

	}
	
}

	