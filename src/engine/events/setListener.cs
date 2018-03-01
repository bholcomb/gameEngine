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


namespace Engine
{
	public class SetListenerEvent : Event
	{
		static EventName theName;

		Vector3 myPosition;
		Vector3 myVelocity;

		public SetListenerEvent(): base() { myName=theName; }
		public SetListenerEvent(Vector3 position, Vector3 velocity) : this(position, velocity, TimeSource.defaultClock.currentTime(), 0.0) { }
		public SetListenerEvent(Vector3 position, Vector3 velocity, double timeStamp) : this(position, velocity, timeStamp, 0.0) { }
		public SetListenerEvent(Vector3 position, Vector3 velocity, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myPosition=position;
			myVelocity=velocity;
		}

		static SetListenerEvent()
		{
			theName = new EventName("audio.listener.set");
			
		}


		public Vector3 position
		{
			get { return myPosition;}
		}
	
		public Vector3 velocity
		{
			get { return myVelocity;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(float)*3;
			size+=sizeof(float)*3;

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

		writer.Write(myPosition.X);
		writer.Write(myPosition.Y);
		writer.Write(myPosition.Z);
	
		writer.Write(myVelocity.X);
		writer.Write(myVelocity.Y);
		writer.Write(myVelocity.Z);
	
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

		myPosition.X=reader.ReadSingle();
		myPosition.Y=reader.ReadSingle();
		myPosition.Z=reader.ReadSingle();
	
		myVelocity.X=reader.ReadSingle();
		myVelocity.Y=reader.ReadSingle();
		myVelocity.Z=reader.ReadSingle();
	
		}

	#endregion
	

	}
	
}

	