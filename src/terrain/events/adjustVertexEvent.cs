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
	public class AdjustVertexEvent : Event
	{
		static EventName theName;

		NodeLocation myBlock=new NodeLocation();
		Int32 myEdge;
		Int32 myVert;
		Int32 myAmount;

		public AdjustVertexEvent(): base() { myName=theName; }
		public AdjustVertexEvent(NodeLocation block, Int32 edge, Int32 vert, Int32 amount) : this(block, edge, vert, amount, TimeSource.defaultClock.currentTime(), 0.0) { }
		public AdjustVertexEvent(NodeLocation block, Int32 edge, Int32 vert, Int32 amount, double timeStamp) : this(block, edge, vert, amount, timeStamp, 0.0) { }
		public AdjustVertexEvent(NodeLocation block, Int32 edge, Int32 vert, Int32 amount, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myBlock=block;
			myEdge=edge;
			myVert=vert;
			myAmount=amount;
		}

		static AdjustVertexEvent()
		{
			theName = new EventName("terrain.edit.adjustVertex");
			
		}


		public NodeLocation block
		{
			get { return myBlock;}
		}
	
		public Int32 edge
		{
			get { return myEdge;}
		}
	
		public Int32 vert
		{
			get { return myVert;}
		}
	
		public Int32 amount
		{
			get { return myAmount;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt32)*3;
			size+=sizeof(Int32);
			size+=sizeof(Int32);
			size+=sizeof(Int32);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

		writer.Write(myBlock.nx);
		writer.Write(myBlock.ny);
		writer.Write(myBlock.nz);
	
			writer.Write(myEdge);
			writer.Write(myVert);
			writer.Write(myAmount);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

		myBlock.nx=reader.ReadUInt32();
		myBlock.ny=reader.ReadUInt32();
		myBlock.nz=reader.ReadUInt32();
	
			myEdge=reader.ReadInt32();
			myVert=reader.ReadInt32();
			myAmount=reader.ReadInt32();
		}

	#endregion
	

	}
	
}

	