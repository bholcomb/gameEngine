using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Util;

namespace Audio
{
   public static class AudioSystem
   {
      static int myMaxVoices = 32;

      static AudioContext myContext;
      static Listener myListener = new Listener();

      static List<AbstractAudio> mySounds = new List<AbstractAudio>();
      static List<AbstractAudio> myPlayingSounds = new List<AbstractAudio>();
      static List<AbstractAudio> myStoppingSounds = new List<AbstractAudio>();

      static List<Source> mySourceList = new List<Source>();

      static List<int> myActiveVoiceList = new List<int>();
      static Queue<int> myInactiveVoiceList = new Queue<int>();

      static float mySoundVolume = 1.0f;
      static float myMusicVolume = 1.0f;

      static AudioSystem()
      {
      }


      public static bool init(Initializer init)
      {
         String defaultDevice = init.findDataOrDefault("audio.defaultDevice", "DirectSound Default");

         try
         {
            IList<String> devices = AudioContext.AvailableDevices;
            foreach (String s in devices)
            {
               //try to get the DirectSound default (openAL-soft's target)
               if (s == defaultDevice)
               {
                  myContext = new AudioContext(s);
                  break;
               }
            }

            //try the default
            if (myContext == null)
            {
               myContext = new AudioContext();
            }
         }
         catch (AudioException ex)
         {
            Error.print("Exception trying to initialize OpenAL Context.  Verify OpenAL drivers are installed");
            Error.print("Exception: {0}", ex.Message);
            if (ex.InnerException != null)
            {
               Error.print("Inner Exception: {0}", ex.InnerException.Message);
            }

            return false;
         }

         //make the context current
         myContext.MakeCurrent();
         myContext.CheckErrors();

         //print some debug information about the system
         Debug.print("------------------AUDIO MANAGER----------------");
         Debug.print("Opened Audio device {0}", myContext.ToString());
         Debug.print("OpenAL Vendor: {0}", AL.Get(ALGetString.Vendor));
         Debug.print("OpenAL Version: {0}", AL.Get(ALGetString.Version));
         Debug.print("OpenAL Renderer: {0}", AL.Get(ALGetString.Renderer));
         Debug.print("OpenAL Extensions: {0}", AL.Get(ALGetString.Extensions));
         Debug.print("OpenAL Context Extensions: {0} ", Alc.GetString(Alc.GetContextsDevice(Alc.GetCurrentContext()), AlcGetString.Extensions));
         Debug.print("------------------AUDIO MANAGER----------------");

         //creating voices
         //try to create a new voice, this could fail on some hardware
         for (int i = 0; i < myMaxVoices; i++)
         {
            int v = AL.GenSource();
            checkError("creating voice");
            if (v > 0)
            {
               myInactiveVoiceList.Enqueue(v);
            }
         }

         return true;
      }

      public static void tick(double dt)
      {
         Vector3 pos = myListener.position;
         AL.Listener(ALListener3f.Position, pos.X, pos.Y, pos.Z);

         Vector3 vel = myListener.velocity;
         //AL.Listener(ALListener3f.Velocity, vel.X, vel.Y, vel.Z);

         float[] ListenerOri = new float[6] { 0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f };
         AL.Listener(ALListenerfv.Orientation, ref ListenerOri);

         //update any playing sounds
         List<AbstractAudio> toRemove = new List<AbstractAudio>();
         foreach (Sound s in myPlayingSounds)
         {
            s.update();
            checkError("updated sound");

            if (s.playing == false || s.paused == true)
            {
               toRemove.Add(s);
            }
         }

         //remove anything that is paused or finished playing
         foreach (AbstractAudio s in toRemove)
         {
            myPlayingSounds.Remove(s);
            returnVoice(s);
         }
      }

      #region "Creation of sounds"
      public static Sound createSound(SourceDescriptor desc)
      {
         Source src = ResourceManager.getResource(desc) as Source;
         if (src == null)
         {
            Error.print("Cannot create sound with source {0}", desc.name);
            return null;
         }

         if (src.state() == Source.SourceState.FAILED)
         {
            Error.print("Cannot create sound from failed source {0}", src.filename());
            return null;
         }

         Sound snd = new Sound(src);
         mySounds.Add(snd);

         checkError("AudioManager.createSound");
         return snd;
      }

      public static void destroySound(Sound snd)
      {
         if (myPlayingSounds.Contains(snd) == true)
         {
            snd.stop();
            myPlayingSounds.Remove(snd);
         }

         mySounds.Remove(snd);

         checkError("AudioManager.destroySound");
         snd.Dispose();
      }
      #endregion

      #region "Playback"
      public static int play(AbstractAudio snd)
      {
         //check if we are close enough to even get the sound
         Sound s = snd as Sound;
         if (s != null)
         {
            //arbitrary sound limit for audio clipping
            if (s.is3d && (myListener.position - s.position).LengthFast > 200)
            {
               //sound is too far away for playing
               return 0;
            }

            //draw a sphere where the sound is originating from
            //DebugRenderer.addSphere(s.position, 1.0f, OpenTK.Graphics.Color4.Green, true, 1.0);
         }

         int voice = getVoice(snd.priority);

         if (voice <= 0)
         {
            Warn.print("Cannot get voice for sound");
            return voice;
         }

         //stop the sound if it is playing already
         AL.SourceStop(voice);
         checkError("AudioManager::play-AL.SourceStop");

         //flush the buffer on the source (if any)
         int numBuffers;
         AL.GetSource(voice, ALGetSourcei.BuffersQueued, out numBuffers);
         checkError("AudioManager::play-alGetSourcei(AL_BUFFERS_QUEUED)");
         while (numBuffers-- != 0)
         {
            int buffer;
            buffer = AL.SourceUnqueueBuffer(voice);
            checkError("AudioManager.play-AL.SourceUnquueBuffers)");
         }

         //add to playing sound list for updates
         myPlayingSounds.Add(snd);

         return voice;
      }

      public static int getVoice(Sound.Priority priority)
      {
         int voice = 0;

         //try to reuse a voice
         if (myInactiveVoiceList.Count > 0)
         {
            voice = myInactiveVoiceList.Dequeue();
            myActiveVoiceList.Add(voice);
            return voice;
         }

         //oops, shouldn't have gotten here, guess I need to take somebody else's voice
         Warn.print("Could not find a free voice-attempting to take one");

         //find one of a lower priority, stop it and get it's voice after it cleans up
         foreach (Sound s in myPlayingSounds)
         {
            if (s.priority < priority)
            {
               s.stop();
               //do that recursive thing
               voice = getVoice(priority);
            }
         }

         if (voice <= 0)
         {
            //oops, shouldn't have gotten here
            Warn.print("Cannot create voice-too many playing sounds");
         }
         //might not have gotten a voice if you're not important enough
         return voice;
      }

      public static void returnVoice(AbstractAudio snd)
      {
         int voice = snd.voice;
         if (voice >= 0)
         {
            myActiveVoiceList.Remove(voice);
            myInactiveVoiceList.Enqueue(voice);
            snd.voice = 0;
         }
      }

      //3d audio stuff
      public static Listener listener
      {
         get { return myListener; }
      }

      //playback
      public static float soundVolume
      {
         get { return mySoundVolume; }
         set { mySoundVolume = value; }
      }

      //volumes
      public static float musicVolume
      {
         get { return myMusicVolume; }
         set { myMusicVolume = value; }
      }

      public static void resumeAll()
      {
      }

      public static void pauseAll()
      {
      }

      public static void stopAll()
      {
      }
      #endregion

      public static void checkError(String tag)
      {
         ALError err = AL.GetError();
         if (err != ALError.NoError)
         {
            Debug.print("{0} : {1}", tag, AL.GetErrorString(err));
         }
      }
   }
}