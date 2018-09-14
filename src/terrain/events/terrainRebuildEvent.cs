/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

using OpenTK;


using Util;
using Engine;


namespace Terrain
{
	public class TerrainRebuildEvent : Event
	{
		public static EventId theId;
		public static String theName;

		UInt64 myChunkId;

		public TerrainRebuildEvent(): base() { myName = theName; myId = theId; }
      public TerrainRebuildEvent(UInt64 chunkId) : this(chunkId, TimeSource.defaultClock.currentTime(), 0.0) { }
		public TerrainRebuildEvent(UInt64 chunkId, double timeStamp) : this(chunkId, timeStamp, 0.0) { }
		public TerrainRebuildEvent(UInt64 chunkId, double timeStamp, double delay)
         : base(timeStamp, delay)
		{
			myName = theName;
			myId = theId;
			myChunkId=chunkId;
		}
   

		static TerrainRebuildEvent()
		{
			theName = "terrain.chunk.rebuild";
			theId = new EventId("terrain.chunk.rebuild");
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

	