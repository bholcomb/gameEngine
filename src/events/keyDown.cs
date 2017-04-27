/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

using OpenTK.Input;


using Util;
using Events;


namespace Events
{
	public class KeyDownEvent : Event
	{
		static EventName theName;

		Key myKey;
		Util.KeyModifiers myModifiers;

		public KeyDownEvent(): base() { myName=theName; }
		public KeyDownEvent(Key key, Util.KeyModifiers modifiers) : this(key, modifiers, TimeSource.defaultClock.currentTime(), 0.0) { }
		public KeyDownEvent(Key key, Util.KeyModifiers modifiers, double timeStamp) : this(key, modifiers, timeStamp, 0.0) { }
		public KeyDownEvent(Key key, Util.KeyModifiers modifiers, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myKey=key;
			myModifiers=modifiers;
		}

		static KeyDownEvent()
		{
			theName = new EventName("input.keyboard.key.down");
			
		}


		public Key key
		{
			get { return myKey;}
		}
	
		public Util.KeyModifiers modifiers
		{
			get { return myModifiers;}
		}
	



	public Char unicode()
    {
        return myModifiers.unicodeFromKey(myKey);  
    }
	



	}
	
}

	