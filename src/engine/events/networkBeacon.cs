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
	public class NetworkBeaconEvent : Event
	{
		static EventName theName;

		String myAddress;
		Int32 myPubPort;

		public NetworkBeaconEvent(): base() { myName=theName; }
		public NetworkBeaconEvent(String address, Int32 pubPort) : this(address, pubPort, TimeSource.defaultClock.currentTime(), 0.0) { }
		public NetworkBeaconEvent(String address, Int32 pubPort, double timeStamp) : this(address, pubPort, timeStamp, 0.0) { }
		public NetworkBeaconEvent(String address, Int32 pubPort, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myAddress=address;
			myPubPort=pubPort;
		}

		static NetworkBeaconEvent()
		{
			theName = new EventName("network.beacon");
			
		}


		public String address
		{
			get { return myAddress;}
		}
	
		public Int32 pubPort
		{
			get { return myPubPort;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=System.Text.Encoding.Unicode.GetByteCount(myAddress) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myAddress);
			size+=sizeof(Int32);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myAddress);
			writer.Write(myPubPort);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myAddress=reader.ReadString();
			myPubPort=reader.ReadInt32();
		}

	#endregion
	

	}
	
}

	