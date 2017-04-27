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
	public class AddBlocksEvent : Event
	{
		static EventName theName;

		UInt32 myMaterialId;
		List<NodeLocation> myBlocks=new List<NodeLocation>();

		public AddBlocksEvent(): base() { myName=theName; }
		public AddBlocksEvent(UInt32 materialId, List<NodeLocation> blocks) : this(materialId, blocks, TimeSource.defaultClock.currentTime(), 0.0) { }
		public AddBlocksEvent(UInt32 materialId, List<NodeLocation> blocks, double timeStamp) : this(materialId, blocks, timeStamp, 0.0) { }
		public AddBlocksEvent(UInt32 materialId, List<NodeLocation> blocks, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myMaterialId=materialId;
			myBlocks=blocks;
		}

		static AddBlocksEvent()
		{
			theName = new EventName("terrain.edit.addBlocks");
			
		}


		public UInt32 materialId
		{
			get { return myMaterialId;}
		}
	
		public List<NodeLocation> blocks
		{
			get { return myBlocks;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt32);
			size+=4; //for the count of the items in the list
			size+=myBlocks.Count * sizeof(UInt32)*3;


			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myMaterialId);
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

			myMaterialId=reader.ReadUInt32();
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

	