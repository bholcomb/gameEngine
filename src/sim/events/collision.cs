/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

using OpenTK;


using Util;
using Events;


namespace Sim
{
	public class CollisionEvent : Event
	{
		static EventName theName;

		UInt64 myEntity1;
		UInt64 myEntity2;

		public CollisionEvent(): base() { myName=theName; }
		public CollisionEvent(UInt64 entity1, UInt64 entity2) : this(entity1, entity2, TimeSource.defaultClock.currentTime(), 0.0) { }
		public CollisionEvent(UInt64 entity1, UInt64 entity2, double timeStamp) : this(entity1, entity2, timeStamp, 0.0) { }
		public CollisionEvent(UInt64 entity1, UInt64 entity2, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity1=entity1;
			myEntity2=entity2;
		}

		static CollisionEvent()
		{
			theName = new EventName("entity.collision");
			
		}


		public UInt64 entity1
		{
			get { return myEntity1;}
		}
	
		public UInt64 entity2
		{
			get { return myEntity2;}
		}
	





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

			writer.Write(myEntity1);
			writer.Write(myEntity2);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity1=reader.ReadUInt64();
			myEntity2=reader.ReadUInt64();
		}

	#endregion
	

	}
	
}

	