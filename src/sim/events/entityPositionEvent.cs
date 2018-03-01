/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

using Engine;
using OpenTK;


using Util;
using Events;


namespace Sim
{
	public class PositionChangeEvent : Event
	{
		static EventName theName;

		UInt64 myEntity;
		Vector3 myPosition;

		public PositionChangeEvent(): base() { myName=theName; }
		public PositionChangeEvent(UInt64 entity, Vector3 position) : this(entity, position, TimeSource.defaultClock.currentTime(), 0.0) { }
		public PositionChangeEvent(UInt64 entity, Vector3 position, double timeStamp) : this(entity, position, timeStamp, 0.0) { }
		public PositionChangeEvent(UInt64 entity, Vector3 position, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
			myPosition=position;
		}

		static PositionChangeEvent()
		{
			theName = new EventName("entity.attribute.position");
			Entity.registerDispatcher(theName.myName, dispatchAttributeChange);
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public Vector3 position
		{
			get { return myPosition;}
		}
	

	#region "dispatch attribute changes"
	public static void dispatchAttributeChange(Entity e, object att)
	{
		PositionChangeEvent evt=new PositionChangeEvent(e.id, (Vector3)att);
		Kernel.eventManager.queueEvent(evt);
	}

	#endregion
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=sizeof(float)*3;

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
		writer.Write(myPosition.X);
		writer.Write(myPosition.Y);
		writer.Write(myPosition.Z);
	
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
		myPosition.X=reader.ReadSingle();
		myPosition.Y=reader.ReadSingle();
		myPosition.Z=reader.ReadSingle();
	
		}

	#endregion
	

	}
	
}

	