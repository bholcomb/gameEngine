/*********************************************************************************

Copyright (c) 2014 Bionic Dog Studios LLC

*********************************************************************************/

/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!!!!!!!!This is an auto-generated file.  Any changes will be destroyed!!!!!!!!!!!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


using System;
using System.IO;

using Audio;


using Util;
using Events;


namespace Engine
{
	public class PauseSoundEvent : Event
	{
		static EventName theName;

		Sound mySound;

		public PauseSoundEvent(): base() { myName=theName; }
		public PauseSoundEvent(Sound sound) : this(sound, TimeSource.defaultClock.currentTime(), 0.0) { }
		public PauseSoundEvent(Sound sound, double timeStamp) : this(sound, timeStamp, 0.0) { }
		public PauseSoundEvent(Sound sound, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			mySound=sound;
		}

		static PauseSoundEvent()
		{
			theName = new EventName("audio.sound.pause");
			
		}


		public Sound sound
		{
			get { return mySound;}
		}
	







	}
	
}

	