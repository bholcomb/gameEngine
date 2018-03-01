/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;



using Util;
using Events;


namespace Engine
{
	public class PrintEvent : Event
	{
		static EventName theName;

		String myText;

		public PrintEvent(): base() { myName=theName; }
		public PrintEvent(String text) : this(text, TimeSource.defaultClock.currentTime(), 0.0) { }
		public PrintEvent(String text, double timeStamp) : this(text, timeStamp, 0.0) { }
		public PrintEvent(String text, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myText=text;
		}

		static PrintEvent()
		{
			theName = new EventName("system.print");
			
		}


		public String text
		{
			get { return myText;}
		}
	





	#region "Serialize/Deserialize"

		protected override int messageSize()
		{
			int size = base.messageSize();

			size+=System.Text.Encoding.Unicode.GetByteCount(myText) < 128 ? 1 : 2;
			size+=System.Text.Encoding.Unicode.GetByteCount(myText);

			return size;
		}

		protected override void serialize(ref BinaryWriter writer)
		{
			base.serialize(ref writer);

			writer.Write(myText);
		}

		protected override void deserialize(ref BinaryReader reader)
		{
			base.deserialize(ref reader);

			myText=reader.ReadString();
		}

	#endregion
	

	}
	
}

	