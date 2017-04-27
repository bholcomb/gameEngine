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
	public class NetworkConnectedEvent : Event
	{
		static EventName theName;

		String myPeer;

		public NetworkConnectedEvent(): base() { myName=theName; }
		public NetworkConnectedEvent(String peer) : this(peer, TimeSource.defaultClock.currentTime(), 0.0) { }
		public NetworkConnectedEvent(String peer, double timeStamp) : this(peer, timeStamp, 0.0) { }
		public NetworkConnectedEvent(String peer, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myPeer=peer;
		}

		static NetworkConnectedEvent()
		{
			theName = new EventName("network.connected");
			
		}


		public String peer
		{
			get { return myPeer;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=System.Text.Encoding.Unicode.GetByteCount(myPeer) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myPeer);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myPeer);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myPeer=reader.ReadString();
		}

	#endregion
	

	}
	
}

	