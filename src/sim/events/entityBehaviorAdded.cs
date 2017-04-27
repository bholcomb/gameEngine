/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;



using Util;
using Events;


namespace Sim
{
	public class EntityBehaviorAddedEvent : Event
	{
		static EventName theName;

		UInt64 myEntity;
		String myBehavior;

		public EntityBehaviorAddedEvent(): base() { myName=theName; }
		public EntityBehaviorAddedEvent(UInt64 entity, String behavior) : this(entity, behavior, TimeSource.defaultClock.currentTime(), 0.0) { }
		public EntityBehaviorAddedEvent(UInt64 entity, String behavior, double timeStamp) : this(entity, behavior, timeStamp, 0.0) { }
		public EntityBehaviorAddedEvent(UInt64 entity, String behavior, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
			myBehavior=behavior;
		}

		static EntityBehaviorAddedEvent()
		{
			theName = new EventName("entity.behavior.added");
			
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public String behavior
		{
			get { return myBehavior;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=System.Text.Encoding.Unicode.GetByteCount(myBehavior) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myBehavior);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
			writer.Write(myBehavior);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
			myBehavior=reader.ReadString();
		}

	#endregion
	

	}
	
}

	