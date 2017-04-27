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
	public class StateChangeEvent : Event
	{
		static EventName theName;

		UInt64 myEntity;
		String myState;

		public StateChangeEvent(): base() { myName=theName; }
		public StateChangeEvent(UInt64 entity, String state) : this(entity, state, TimeSource.defaultClock.currentTime(), 0.0) { }
		public StateChangeEvent(UInt64 entity, String state, double timeStamp) : this(entity, state, timeStamp, 0.0) { }
		public StateChangeEvent(UInt64 entity, String state, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity=entity;
			myState=state;
		}

		static StateChangeEvent()
		{
			theName = new EventName("entity.attribute.state");
			Entity.registerDispatcher(theName.myName, dispatchAttributeChange);
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public String state
		{
			get { return myState;}
		}
	

	#region "dispatch attribute changes"
	public static void dispatchAttributeChange(Entity e, object att)
	{
		StateChangeEvent evt=new StateChangeEvent(e.id, (String)att);
		Kernel.eventManager.queueEvent(evt);
	}

	#endregion
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=System.Text.Encoding.Unicode.GetByteCount(myState) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myState);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
			writer.Write(myState);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
			myState=reader.ReadString();
		}

	#endregion
	

	}
	
}

	