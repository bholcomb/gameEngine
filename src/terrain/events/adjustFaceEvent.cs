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
using System.Collections.Generic;


using Util;
using Engine;


namespace Terrain
{
	public class AdjustFaceEvent : Event
	{
		public static EventId theId;
		public static String theName;

		NodeLocation myBlock=new NodeLocation();
		Int32 myFace;
		Int32 myAmount;

		public AdjustFaceEvent(): base() { myName = theName; myId = theId; }
      public AdjustFaceEvent(NodeLocation block, Int32 face, Int32 amount) : this(block, face, amount, TimeSource.defaultClock.currentTime(), 0.0) { }
		public AdjustFaceEvent(NodeLocation block, Int32 face, Int32 amount, double timeStamp) : this(block, face, amount, timeStamp, 0.0) { }
		public AdjustFaceEvent(NodeLocation block, Int32 face, Int32 amount, double timeStamp, double delay)
         : base(timeStamp, delay)
		{
			myName = theName;
			myId = theId;
			myBlock=block;
			myFace=face;
			myAmount=amount;
		}
   

		static AdjustFaceEvent()
		{
			theName = "terrain.edit.adjustFace";
			theId = new EventId("terrain.edit.adjustFace");
		}


		public NodeLocation block
		{
			get { return myBlock;}
		}
	
		public Int32 face
		{
			get { return myFace;}
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

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

		writer.Write(myBlock.nx);
		writer.Write(myBlock.ny);
		writer.Write(myBlock.nz);
	
			writer.Write(myFace);
			writer.Write(myAmount);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

		myBlock.nx=reader.ReadUInt32();
		myBlock.ny=reader.ReadUInt32();
		myBlock.nz=reader.ReadUInt32();
	
			myFace=reader.ReadInt32();
			myAmount=reader.ReadInt32();
		}

	#endregion
	

	}
	
}

	