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
	public class EntityStateResponseMessage : Event
	{
		static EventName theName;

		UInt64 myEntity;
		String myType;
		Vector3 myPosition;
		Quaternion myOrientation;
		bool myDynamic;
		String myState;
		UInt64 myParent;

		public EntityStateResponseMessage(): base() { myName=theName; }
		public EntityStateResponseMessage(UInt64 entity, String type, Vector3 position, Quaternion orientation, bool dynamic, String state, UInt64 parent) : this(entity, type, position, orientation, dynamic, state, parent, TimeSource.defaultClock.currentTime(), 0.0) { }
		public EntityStateResponseMessage(UInt64 entity, String type, Vector3 position, Quaternion orientation, bool dynamic, String state, UInt64 parent, double timeStamp) : this(entity, type, position, orientation, dynamic, state, parent, timeStamp, 0.0) { }
		public EntityStateResponseMessage(UInt64 entity, String type, Vector3 position, Quaternion orientation, bool dynamic, String state, UInt64 parent, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
			myType=type;
			myPosition=position;
			myOrientation=orientation;
			myDynamic=dynamic;
			myState=state;
			myParent=parent;
		}

		static EntityStateResponseMessage()
		{
			theName = new EventName("entity.state.response");
			
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public String type
		{
			get { return myType;}
		}
	
		public Vector3 position
		{
			get { return myPosition;}
		}
	
		public Quaternion orientation
		{
			get { return myOrientation;}
		}
	
		public bool dynamic
		{
			get { return myDynamic;}
		}
	
		public String state
		{
			get { return myState;}
		}
	
		public UInt64 parent
		{
			get { return myParent;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=System.Text.Encoding.Unicode.GetByteCount(myType) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myType);
			size+=sizeof(float)*3;
			size+=sizeof(float)*4;
			size+=sizeof(bool);
			size+=System.Text.Encoding.Unicode.GetByteCount(myState) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myState);
			size+=sizeof(UInt64);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
			writer.Write(myType);
		writer.Write(myPosition.X);
		writer.Write(myPosition.Y);
		writer.Write(myPosition.Z);
	
		writer.Write(myOrientation.X);
		writer.Write(myOrientation.Y);
		writer.Write(myOrientation.Z);
		writer.Write(myOrientation.W);
	
			writer.Write(myDynamic);
			writer.Write(myState);
			writer.Write(myParent);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
			myType=reader.ReadString();
		myPosition.X=reader.ReadSingle();
		myPosition.Y=reader.ReadSingle();
		myPosition.Z=reader.ReadSingle();
	
		myOrientation.X=reader.ReadSingle();
		myOrientation.Y=reader.ReadSingle();
		myOrientation.Z=reader.ReadSingle();
		myOrientation.W=reader.ReadSingle();
	
			myDynamic=reader.ReadBoolean();
			myState=reader.ReadString();
			myParent=reader.ReadUInt64();
		}

	#endregion
	

	}
	
}

	