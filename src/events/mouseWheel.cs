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
	public class MouseWheelEvent : Event
	{
		static EventName theName;

		Int32 myX;
		Int32 myY;
		Int32 myVal;
		Int32 myDelta;
		Util.KeyModifiers myModifiers;

		public MouseWheelEvent(): base() { myName=theName; }
		public MouseWheelEvent(Int32 x, Int32 y, Int32 val, Int32 delta, Util.KeyModifiers modifiers) : this(x, y, val, delta, modifiers, TimeSource.defaultClock.currentTime(), 0.0) { }
		public MouseWheelEvent(Int32 x, Int32 y, Int32 val, Int32 delta, Util.KeyModifiers modifiers, double timeStamp) : this(x, y, val, delta, modifiers, timeStamp, 0.0) { }
		public MouseWheelEvent(Int32 x, Int32 y, Int32 val, Int32 delta, Util.KeyModifiers modifiers, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myX=x;
			myY=y;
			myVal=val;
			myDelta=delta;
			myModifiers=modifiers;
		}

		static MouseWheelEvent()
		{
			theName = new EventName("input.mouse.wheel");
			
		}


		public Int32 x
		{
			get { return myX;}
		}
	
		public Int32 y
		{
			get { return myY;}
		}
	
		public Int32 val
		{
			get { return myVal;}
		}
	
		public Int32 delta
		{
			get { return myDelta;}
		}
	
		public Util.KeyModifiers modifiers
		{
			get { return myModifiers;}
		}
	







	}
	
}

	