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

using System.Collections.Generic;


using Util;
using Engine;


namespace Sim
{
	public class EntityStateRequestMessage : Event
	{
		public static EventId theId;
		public static String theName;

		UInt64 myEntity;
		List<Int32> myAttributes=new List<Int32>();

		public EntityStateRequestMessage(): base() { myName = theName; myId = theId; }
      public EntityStateRequestMessage(UInt64 entity, List<Int32> attributes) : this(entity, attributes, TimeSource.defaultClock.currentTime(), 0.0) { }
		public EntityStateRequestMessage(UInt64 entity, List<Int32> attributes, double timeStamp) : this(entity, attributes, timeStamp, 0.0) { }
		public EntityStateRequestMessage(UInt64 entity, List<Int32> attributes, double timeStamp, double delay)
         : base(timeStamp, delay)
		{
			myName = theName;
			myId = theId;
			myEntity=entity;
			myAttributes=attributes;
		}
   

		static EntityStateRequestMessage()
		{
			theName = "entity.attribute.request";
			theId = new EventId("entity.attribute.request");
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public List<Int32> attributes
		{
			get { return myAttributes;}
		}
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=4; //for the count of the items in the list
			size+=myAttributes.Count * sizeof(Int32);


			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
			writer.Write(myAttributes.Count); //for the count of the items in the list
			for(int i=0; i<myAttributes.Count; i++)
			{
							writer.Write(myAttributes[i]);
			}

		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myEntity=reader.ReadUInt64();
			int myAttributes_count=reader.ReadInt32(); //for the count of the items in the list
			for(int i=0; i<myAttributes_count; i++)
			{
				Int32 aInt32=new Int32();
							aInt32=reader.ReadInt32();
				myAttributes.Add(aInt32);
			}
		}

	#endregion
	

	}
	
}

	