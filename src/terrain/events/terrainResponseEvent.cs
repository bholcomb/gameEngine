/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

using OpenTK;
using System.Collections.Generic;


using Util;
using Events;


namespace Terrain
{
	public class TerrainResponseEvent : Event
	{
		static EventName theName;

		UInt64 myChunkId;
		List<byte> myData=new List<byte>();

		public TerrainResponseEvent(): base() { myName=theName; }
		public TerrainResponseEvent(UInt64 chunkId, List<byte> data) : this(chunkId, data, TimeSource.defaultClock.currentTime(), 0.0) { }
		public TerrainResponseEvent(UInt64 chunkId, List<byte> data, double timeStamp) : this(chunkId, data, timeStamp, 0.0) { }
		public TerrainResponseEvent(UInt64 chunkId, List<byte> data, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myChunkId=chunkId;
			myData=data;
		}

		static TerrainResponseEvent()
		{
			theName = new EventName("terrain.chunk.response");
			
		}


		public UInt64 chunkId
		{
			get { return myChunkId;}
		}
	
		public List<byte> data
		{
			get { return myData;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=4; //for the count of the items in the list
			size+=myData.Count * sizeof(byte);


			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myChunkId);
			writer.Write(myData.Count); //for the count of the items in the list
			for(int i=0; i<myData.Count; i++)
			{
							writer.Write(myData[i]);
			}

		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myChunkId=reader.ReadUInt64();
			int myData_count=reader.ReadInt32(); //for the count of the items in the list
			for(int i=0; i<myData_count; i++)
			{
				byte abyte=new byte();
							abyte=reader.ReadByte();
				myData.Add(abyte);
			}
		}

	#endregion
	

	}
	
}

	