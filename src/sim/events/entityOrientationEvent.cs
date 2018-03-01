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
	public class OrientationChangeEvent : Event
	{
		static EventName theName;

		UInt64 myEntity;
		Quaternion myOrientation;

		public OrientationChangeEvent(): base() { myName=theName; }
		public OrientationChangeEvent(UInt64 entity, Quaternion orientation) : this(entity, orientation, TimeSource.defaultClock.currentTime(), 0.0) { }
		public OrientationChangeEvent(UInt64 entity, Quaternion orientation, double timeStamp) : this(entity, orientation, timeStamp, 0.0) { }
		public OrientationChangeEvent(UInt64 entity, Quaternion orientation, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
			myOrientation=orientation;
		}

		static OrientationChangeEvent()
		{
			theName = new EventName("entity.attribute.orientation");
			Entity.registerDispatcher(theName.myName, dispatchAttributeChange);
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public Quaternion orientation
		{
			get { return myOrientation;}
		}
	

	#region "dispatch attribute changes"
	public static void dispatchAttributeChange(Entity e, object att)
	{
		OrientationChangeEvent evt=new OrientationChangeEvent(e.id, (Quaternion)att);
		Kernel.eventManager.queueEvent(evt);
	}

	#endregion
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=sizeof(float)*4;

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
		writer.Write(myOrientation.X);
		writer.Write(myOrientation.Y);
		writer.Write(myOrientation.Z);
		writer.Write(myOrientation.W);
	
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
		myOrientation.X=reader.ReadSingle();
		myOrientation.Y=reader.ReadSingle();
		myOrientation.Z=reader.ReadSingle();
		myOrientation.W=reader.ReadSingle();
	
		}

	#endregion
	

	}
	
}

	