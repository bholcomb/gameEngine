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


namespace Terrain
{
	public class TerrainRequestEvent : Event
	{
		static EventName theName;

		UInt64 myChunkId;

		public TerrainRequestEvent(): base() { myName=theName; }
		public TerrainRequestEvent(UInt64 chunkId) : this(chunkId, TimeSource.defaultClock.currentTime(), 0.0) { }
		public TerrainRequestEvent(UInt64 chunkId, double timeStamp) : this(chunkId, timeStamp, 0.0) { }
		public TerrainRequestEvent(UInt64 chunkId, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myChunkId=chunkId;
		}

		static TerrainRequestEvent()
		{
			theName = new EventName("terrain.chunk.request");
			
		}


		public UInt64 chunkId
		{
			get { return myChunkId;}
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

			writer.Write(myChunkId);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myChunkId=reader.ReadUInt64();
		}

	#endregion
	

	}
	
}

	