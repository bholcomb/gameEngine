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
	public class EntityAddedEvent : Event
	{
		static EventName theName;

		UInt64 myEntity;
		String myType;

		public EntityAddedEvent(): base() { myName=theName; }
		public EntityAddedEvent(UInt64 entity, String type) : this(entity, type, TimeSource.defaultClock.currentTime(), 0.0) { }
		public EntityAddedEvent(UInt64 entity, String type, double timeStamp) : this(entity, type, timeStamp, 0.0) { }
		public EntityAddedEvent(UInt64 entity, String type, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
			myType=type;
		}

		static EntityAddedEvent()
		{
			theName = new EventName("entity.added");
			
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public String type
		{
			get { return myType;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=System.Text.Encoding.Unicode.GetByteCount(myType) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myType);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
			writer.Write(myType);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
			myType=reader.ReadString();
		}

	#endregion
	

	}
	
}

	