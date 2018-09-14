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
	public class JoinEvent : Event
	{
		public static EventId theId;
		public static String theName;

		NodeLocation myBlock=new NodeLocation();

		public JoinEvent(): base() { myName = theName; myId = theId; }
      public JoinEvent(NodeLocation block) : this(block, TimeSource.defaultClock.currentTime(), 0.0) { }
		public JoinEvent(NodeLocation block, double timeStamp) : this(block, timeStamp, 0.0) { }
		public JoinEvent(NodeLocation block, double timeStamp, double delay)
         : base(timeStamp, delay)
		{
			myName = theName;
			myId = theId;
			myBlock=block;
		}
   

		static JoinEvent()
		{
			theName = "terrain.edit.join";
			theId = new EventId("terrain.edit.join");
		}


		public NodeLocation block
		{
			get { return myBlock;}
		}
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt32)*3;

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

		writer.Write(myBlock.nx);
		writer.Write(myBlock.ny);
		writer.Write(myBlock.nz);
	
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

		myBlock.nx=reader.ReadUInt32();
		myBlock.ny=reader.ReadUInt32();
		myBlock.nz=reader.ReadUInt32();
	
		}

	#endregion
	

	}
	
}

	