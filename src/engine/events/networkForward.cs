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


namespace Engine
{
	public class NetworkForwardEvent : Event
	{
		static EventName theName;

		String myMessage;

		public NetworkForwardEvent(): base() { myName=theName; }
		public NetworkForwardEvent(String message) : this(message, TimeSource.defaultClock.currentTime(), 0.0) { }
		public NetworkForwardEvent(String message, double timeStamp) : this(message, timeStamp, 0.0) { }
		public NetworkForwardEvent(String message, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myMessage=message;
		}

		static NetworkForwardEvent()
		{
			theName = new EventName("network.forward");
			
		}


		public String message
		{
			get { return myMessage;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=System.Text.Encoding.Unicode.GetByteCount(myMessage) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myMessage);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myMessage);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myMessage=reader.ReadString();
		}

	#endregion
	

	}
	
}

	