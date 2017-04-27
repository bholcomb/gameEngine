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
	public class StopSoundEvent : Event
	{
		static EventName theName;

		Sound mySound;

		public StopSoundEvent(): base() { myName=theName; }
		public StopSoundEvent(Sound sound) : this(sound, TimeSource.defaultClock.currentTime(), 0.0) { }
		public StopSoundEvent(Sound sound, double timeStamp) : this(sound, timeStamp, 0.0) { }
		public StopSoundEvent(Sound sound, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			mySound=sound;
		}

		static StopSoundEvent()
		{
			theName = new EventName("audio.sound.stop");
			
		}


		public Sound sound
		{
			get { return mySound;}
		}
	







	}
	
}

	