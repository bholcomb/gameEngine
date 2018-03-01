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
	public class TerrainResetEvent : Event
	{
		static EventName theName;

		bool myReload;

		public TerrainResetEvent(): base() { myName=theName; }
		public TerrainResetEvent(bool reload) : this(reload, TimeSource.defaultClock.currentTime(), 0.0) { }
		public TerrainResetEvent(bool reload, double timeStamp) : this(reload, timeStamp, 0.0) { }
		public TerrainResetEvent(bool reload, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myReload=reload;
		}

		static TerrainResetEvent()
		{
			theName = new EventName("terrain.reset");
			
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

	