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
         Initializer init = new Initializer(filename);

         InitTable soundscape = init.findData<InitTable>("Soundscape");
         volume = init.findDataOr<float>("volume", 1.0f);

         InitTable backgrounds = soundscape.findDataOr<InitTable>("Backgrounds", null);
         InitTable periodics = soundscape.findDataOr<InitTable>("Periodics", null);

         //stupid lua tables go from 1-count
         for (int i = 1; i <= backgrounds.count(); i++)
         {
            InitTable bg = backgrounds.findData<InitTable>(i);
            addBackground(bg);
         }

         for (int i = 1; i <= periodics.count(); i++)
         {
            InitTable per = periodics.findData<InitTable>(i);
            addPeriodic(per);
         }
      }


      public override bool play()
      {
         foreach (Background bg in myBackgrounds)
         {
            bg.sound.play();
         }

         //We're already playing this sound
         if (state == AbstractAudio.State.PLAYING)
         {
            return true;
         }

         //so the audio file gets updated by audio manager
         myAudioManager.startUpdating(this);

         state = AbstractAudio.State.PLAYING;
         update(0.0);

         return true;
      }

      public override bool pause()
      {
         myAudioManager.stopUpdating(this);

         foreach (Background bg in myBackgrounds)
         {
            bg.sound.pause();
         }

         foreach (Periodic pd in myPeriodics)
         {
            pd.sound.pause();
         }

         state = State.PAUSED;

         return true;
      }

      public override bool stop()
      {
         myAudioManager.stopUpdating(this);

         foreach(Background bg in myBackgrounds)
         {
            bg.sound.stop();
         }

         foreach(Periodic pd in myPeriodics)
         {
            pd.sound.stop();
         }

         state = State.STOPPED;

         return true;
      }

      public override void update(double dt)
      {
         foreach(Background bg in myBackgrounds)
         {
            bg.update(dt);
         }

         foreach(Periodic pd in myPeriodics)
         {
            pd.update(dt);
         }
      }

      public override void Dispose()
      {
         foreach(Background bg in myBackgrounds)
         {
            bg.sound.Dispose();
         }
         myBackgrounds.Clear();

         foreach(Periodic pd in myPeriodics)
         {
            pd.sound.Dispose();
         }
         myPeriodics.Clear();
      }

      public new float volume {
         get { return myVolume; }
         set
         {
            myVolume = value;
            foreach (Background bg in myBackgrounds)
            {
               bg.sound.volume = myVolume * bg.volume.currentVal;
            }
         }
      }


      protected void addBackground(InitTable config)
      {
         SoundDescriptor desc = new SoundDescriptor();
         desc.filename = config.findData<string>("sound");
         desc.is3d = false;
         desc.isLooping = true;
         desc.priority = Priority.BACKGROUND_FX;
         Sound snd = new Sound(desc);

         Background bg = new Background(this);
         bg.sound = snd;

         Random rand = new Random();
         bg.pitch.minVal = config.findDataOr<float>("pitch.minVal", 0.8f);
         bg.pitch.maxVal = config.findDataOr<float>("pitch.maxVal", 1.2f);
         bg.pitch.currentVal = rand.randomInRange(bg.pitch.minVal, bg.pitch.maxTime);
         bg.pitch.minTime = config.findDataOr<float>("pitch.minTime", 1.0f);
         bg.pitch.maxTime = config.findDataOr<float>("pitch.maxTime", 5.0f);
         bg.pitch.reset();

         bg.volume.minVal = config.findDataOr<float>("volume.minVal", 0.8f);
         bg.volume.maxVal = config.findDataOr<float>("volume.maxVal", 1.2f);
         bg.volume.currentVal = rand.randomInRange(bg.volume.minVal, bg.volume.maxTime);
         bg.volume.minTime = config.findDataOr<float>("volume.minTime", 1.0f);
         bg.volume.maxTime = config.findDataOr<float>("volume.maxTime", 5.0f);
         bg.volume.reset();

         myBackgrounds.Add(bg);
      }

      protected void addPeriodic(InitTable config)
      {
         SoundDescriptor desc = new SoundDescriptor();
         desc.filename = config.findData<string>("sound");
         desc.is3d = true;
         desc.isLooping = false;
         desc.priority = Priority.BACKGROUND_FX;

         Sound snd = new Sound(desc);
         snd.relativePosition = true;

         Periodic pr = new Periodic(this);
         pr.sound = snd;

         Random rand = new Random();
         pr.minPitch = config.findDataOr<float>("pitch.min", 0.8f);
         pr.maxPitch = config.findDataOr<float>("pitch.max", 1.2f);
         pr.minDelay = config.findDataOr<float>("delay.min", 1.0f);
         pr.maxDelay = config.findDataOr<float>("delay.min", 5.0f);
         pr.maxRange = config.findDataOr<Vector3>("maxRange", new Vector3(20, 20, 20));

         pr.nextTime = rand.randomInRange(pr.minDelay, pr.maxDelay);

         myPeriodics.Add(pr);
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
            Random rand = new Random();
            float stopVal = rand.randomInRange(minVal, maxVal);
            duration = rand.randomInRange(minTime, maxTime);
            rate = (stopVal - currentVal) / duration;
         }

         public void update(double dt)
         {
            duration -= (float)dt;
            currentVal += rate * (float)dt;
            
            if(duration <= 0.0f)
            {
               reset();
            }
         }
      }

      struct Background
      {
         public SoundScape soundscape;
         public Sound sound;
         public Varying volume;
         public Varying pitch;
         
         public Background(SoundScape ss)
         {
            soundscape = ss;
            sound = null;
            volume = new Varying();
            pitch = new Varying();
         }

         public void update(double dt)
         {
            pitch.update(dt);
            volume.update(dt);

            sound.pitch = pitch.currentVal;
            sound.volume = soundscape.volume * volume.currentVal;
            sound.play();
         }

      }

      struct Periodic
      {
         public SoundScape soundscape;
         public Sound sound;
         public Vector3 maxRange;
         public float nextTime;
         public float minDelay;
         public float maxDelay;
         public float minPitch;
         public float maxPitch;

         public Periodic(SoundScape ss)
         {
            soundscape = ss;
            sound = null;
            maxRange = Vector3.Zero;
            nextTime = 0.0f;
            minDelay = 0.0f;
            maxDelay = 0.0f;
            minPitch = 0.0f;
            maxPitch = 0.0f;
         }

         public void update(double dt)
         {
            nextTime -= (float)dt;
            if(nextTime <= 0.0f)
            {
               Random rand = new Random();
               nextTime = rand.randomInRange(minDelay, maxDelay);

               Vector3 pos = new Vector3(
                  rand.randomInRange(0.0f, maxRange[0]),
                  rand.randomInRange(0.0f, maxRange[1]), 
                  rand.randomInRange(0.0f, maxRange[2])
               );

               sound.position = pos;
               sound.pitch = rand.randomInRange(minPitch, maxPitch);
               sound.volume = soundscape.volume;
               sound.play();
            }
         }
      }

      List<Background> myBackgrounds = new List<Background>();
      List<Periodic> myPeriodics = new List<Periodic>();
   }
}