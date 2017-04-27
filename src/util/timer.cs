using System;

namespace Util
{
	public class Timer
	{
		public delegate void TimerFunction();
		
		Clock myClock;
		double myCountdownTime;
		double myTimeRemaining;
		bool myLoop;

		public Timer (double countdownTime, bool loop, Clock c)
		{
         myClock = c;
			myCountdownTime=countdownTime;
			myLoop=loop;

         myTimeRemaining = countdownTime;
		}

      public event TimerFunction onTimer;
		
		public bool notify()
		{
			if(myTimeRemaining<=0 &&  myLoop==false)
			{
				return false;
			}
			
			myTimeRemaining=myTimeRemaining-myClock.timeThisFrame();
			if(myTimeRemaining<=0)
			{
            if (onTimer != null)
            {
               onTimer();
            }

            if (myLoop == true)
            {
               myTimeRemaining = myCountdownTime;
            }
			}
			
			return true;
		}

      public double countdownTime
      {
         get { return myCountdownTime; }
         set { myCountdownTime = value; myTimeRemaining = myCountdownTime; }
      }

      public bool loop
      {
         get { return myLoop; }
         set { myLoop = value; }
      }
	}
}

