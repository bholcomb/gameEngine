using System;
using System.Collections.Generic;

using OpenTK;

using Util;

namespace Audio
{
   public class SoundScape : AbstractAudio
   {
      public SoundScape(string filename)
      {

      }


      public override bool play()
      {
         throw new NotImplementedException();
      }

      public override bool pause()
      {
         throw new NotImplementedException();
      }

      public override bool stop()
      {
         throw new NotImplementedException();
      }

      public override void update(double dt)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public float volume { get; set; }


      protected void loadBackground()
      {

      }

      protected void loadPeriodic()
      {

      }

      struct Varying
      {
         public float currentVal;
         public float duration;
         public float rate;

         public float minVal;
         public float maxVal;
         public float minTime;
         public float maxTime;

         public void reset()
         {

         }

         public void update(double dt)
         {

         }
      }

      struct Background
      {
         SoundScape soundscape;
         Sound sound;
         Varying volume;
         Varying pitch;
         
         public Background(SoundScape ss)
         {
            soundscape = ss;
            sound = null;
            volume = new Varying();
            pitch = new Varying();
         }

         public void update(double dt)
         {

         }

      }
   }
}