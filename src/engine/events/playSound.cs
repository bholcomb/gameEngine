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
	public class PlaySoundEvent : Event
	{
		static EventName theName;

		Sound mySound;

		public PlaySoundEvent(): base() { myName=theName; }
		public PlaySoundEvent(Sound sound) : this(sound, TimeSource.defaultClock.currentTime(), 0.0) { }
		public PlaySoundEvent(Sound sound, double timeStamp) : this(sound, timeStamp, 0.0) { }
		public PlaySoundEvent(Sound sound, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			mySound=sound;
		}

		static PlaySoundEvent()
		{
			theName = new EventName("audio.sound.play");
			
		}


		public Sound sound
		{
			get { return mySound;}
		}
	







	}
	
}

	