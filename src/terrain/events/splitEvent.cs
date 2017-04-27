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
	public class SplitEvent : Event
	{
		static EventName theName;

		NodeLocation myBlock=new NodeLocation();

		public SplitEvent(): base() { myName=theName; }
		public SplitEvent(NodeLocation block) : this(block, TimeSource.defaultClock.currentTime(), 0.0) { }
		public SplitEvent(NodeLocation block, double timeStamp) : this(block, timeStamp, 0.0) { }
		public SplitEvent(NodeLocation block, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myBlock=block;
		}

		static SplitEvent()
		{
			theName = new EventName("terrain.edit.split");
			
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

	