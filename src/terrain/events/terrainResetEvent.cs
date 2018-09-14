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
	public class TerrainResetEvent : Event
	{
		public static EventId theId;
		public static String theName;

		bool myReload;

		public TerrainResetEvent(): base() { myName = theName; myId = theId; }
      public TerrainResetEvent(bool reload) : this(reload, TimeSource.defaultClock.currentTime(), 0.0) { }
		public TerrainResetEvent(bool reload, double timeStamp) : this(reload, timeStamp, 0.0) { }
		public TerrainResetEvent(bool reload, double timeStamp, double delay)
         : base(timeStamp, delay)
		{
			myName = theName;
			myId = theId;
			myReload=reload;
		}
   

		static TerrainResetEvent()
		{
			theName = "terrain.reset";
			theId = new EventId("terrain.reset");
		}


		public bool reload
		{
			get { return myReload;}
		}
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(bool);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myReload);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myReload=reader.ReadBoolean();
		}

	#endregion
	

	}
	
}

	