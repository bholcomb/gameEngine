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
	public class RemoveBlocksEvent : Event
	{
		static EventName theName;

		UInt64 myChunkId;
		List<NodeLocation> myBlocks=new List<NodeLocation>();

		public RemoveBlocksEvent(): base() { myName=theName; }
		public RemoveBlocksEvent(UInt64 chunkId, List<NodeLocation> blocks) : this(chunkId, blocks, TimeSource.defaultClock.currentTime(), 0.0) { }
		public RemoveBlocksEvent(UInt64 chunkId, List<NodeLocation> blocks, double timeStamp) : this(chunkId, blocks, timeStamp, 0.0) { }
		public RemoveBlocksEvent(UInt64 chunkId, List<NodeLocation> blocks, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myChunkId=chunkId;
			myBlocks=blocks;
		}

		static RemoveBlocksEvent()
		{
			theName = new EventName("terrain.edit.removeBlocks");
			
		}


		public UInt64 chunkId
		{
			get { return myChunkId;}
		}
	
		public List<NodeLocation> blocks
		{
			get { return myBlocks;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=4; //for the count of the items in the list
			size+=myBlocks.Count * sizeof(UInt32)*3;


			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myChunkId);
			writer.Write(myBlocks.Count); //for the count of the items in the list
			for(int i=0; i<myBlocks.Count; i++)
			{
						writer.Write(myBlocks[i].nx);
		writer.Write(myBlocks[i].ny);
		writer.Write(myBlocks[i].nz);
	
			}

		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myChunkId=reader.ReadUInt64();
			int myBlocks_count=reader.ReadInt32(); //for the count of the items in the list
			for(int i=0; i<myBlocks_count; i++)
			{
				NodeLocation aNodeLocation=new NodeLocation();
						aNodeLocation.nx=reader.ReadUInt32();
		aNodeLocation.ny=reader.ReadUInt32();
		aNodeLocation.nz=reader.ReadUInt32();
	
				myBlocks.Add(aNodeLocation);
			}
		}

	#endregion
	

	}
	
}

	