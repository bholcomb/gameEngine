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
	public class EntityAnimationStopEvent : Event
	{
		static EventName theName;

		UInt64 myEntity;

		public EntityAnimationStopEvent(): base() { myName=theName; }
		public EntityAnimationStopEvent(UInt64 entity) : this(entity, TimeSource.defaultClock.currentTime(), 0.0) { }
		public EntityAnimationStopEvent(UInt64 entity, double timeStamp) : this(entity, timeStamp, 0.0) { }
		public EntityAnimationStopEvent(UInt64 entity, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
		}

		static EntityAnimationStopEvent()
		{
			theName = new EventName("entity.animation.stop");
			
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
		}

	#endregion
	

	}
	
}

	