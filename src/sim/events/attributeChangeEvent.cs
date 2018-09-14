/*********************************************************************************

Copyright (c) 2018 Apexica LLC

*********************************************************************************/


using System;
using System.IO;
using System.Runtime.InteropServices;

using Util;
using Engine;


namespace Sim
{
	public class AttributeChangedEvent<T> : Event
	{
		public static EventId theId;
		public static String theName;

		UInt64 myEntity;
		Int32 myAttributeId;
		T myValue;

		public AttributeChangedEvent(): base() { myName = theName; myId = theId; }
		public AttributeChangedEvent(UInt64 entity, Int32 attributeId, T value) : this(entity, attributeId, value, TimeSource.defaultClock.currentTime(), 0.0) { }
		public AttributeChangedEvent(UInt64 entity, Int32 attributeId, T value, double timeStamp) : this(entity, attributeId, value, timeStamp, 0.0) { }
		public AttributeChangedEvent(UInt64 entity, Int32 attributeId, T value, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myEntity = entity;
			myAttributeId = attributeId;
			myValue = value;
		}

		static AttributeChangedEvent()
		{
			theName = "entity.attribute." + typeof(T);
			theId = new EventId(theName);
		}


		public UInt64 entity
		{
			get { return myEntity;}
		}
	
		public Int32 attributeId
		{
			get { return myAttributeId;}
		}
	
		public T value
		{
			get { return myValue;}
		}
	



	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=sizeof(UInt64);
			size+=sizeof(Int32);
			size+=Marshal.SizeOf(myValue);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myEntity);
			writer.Write(myAttributeId);
         //TODO: FIX ME
         //writer.Write(myValue);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

         myEntity = reader.ReadUInt64();
         myAttributeId = reader.ReadInt32();
         //TODO: FIX ME
         //myValue = reader.Read();
		}

	#endregion
	}
	
}

	