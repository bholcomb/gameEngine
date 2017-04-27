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
	public class MouseMoveEvent : Event
	{
		static EventName theName;

		Int32 myX;
		Int32 myY;
		Int32 myXDelta;
		Int32 myYDelta;
		Util.KeyModifiers myModifiers;

		public MouseMoveEvent(): base() { myName=theName; }
		public MouseMoveEvent(Int32 x, Int32 y, Int32 xDelta, Int32 yDelta, Util.KeyModifiers modifiers) : this(x, y, xDelta, yDelta, modifiers, TimeSource.defaultClock.currentTime(), 0.0) { }
		public MouseMoveEvent(Int32 x, Int32 y, Int32 xDelta, Int32 yDelta, Util.KeyModifiers modifiers, double timeStamp) : this(x, y, xDelta, yDelta, modifiers, timeStamp, 0.0) { }
		public MouseMoveEvent(Int32 x, Int32 y, Int32 xDelta, Int32 yDelta, Util.KeyModifiers modifiers, double timeStamp, double delay)
		: base(timeStamp, delay)
		{
			myName = theName;
			myX=x;
			myY=y;
			myXDelta=xDelta;
			myYDelta=yDelta;
			myModifiers=modifiers;
		}

		static MouseMoveEvent()
		{
			theName = new EventName("input.mouse.move");
			
		}


		public Int32 x
		{
			get { return myX;}
		}
	
		public Int32 y
		{
			get { return myY;}
		}
	
		public Int32 xDelta
		{
			get { return myXDelta;}
		}
	
		public Int32 yDelta
		{
			get { return myYDelta;}
		}
	
		public Util.KeyModifiers modifiers
		{
			get { return myModifiers;}
		}
	







	}
	
}

	