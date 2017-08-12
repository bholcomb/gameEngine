using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Util;

namespace Audio
{
   public static class Audio
   {
      public static bool checkError(string str)
      {
         ALError err = AL.GetError();
         if (err != ALError.NoError)
         {
            Warn.print(str + ": " + AL.GetErrorString(err));
            //there was some kind of error
            return false;
         }

         //everything was ok
         return true;
      }
   };


   public class AudioManager
   {
      public static AudioManager instance()
      {
         if (theAudioManager == null)
         {
            theAudioManager = new AudioManager();
            theAudioManager.init();
         }

         return theAudioManager;
      }

      public static void shutdown()
      {
         AudioManager me = AudioManager.instance();
         
         //stop any playing sounds
         foreach(AbstractAudio snd in me.myPlayingSounds)
         {
            snd.stop();
         }

         me.clearVoices();
         me.myCaptureDevice.Dispose();
         me.myContext.Dispose();
      }

      public void tick(double dt)
      {
         myCurrentTime += dt;

         //update the listener
         Vector3 pos = myListener.position;
         Vector3 vel = myListener.velocity;

         AL.Listener(ALListener3f.Position, pos.X, pos.Y, pos.Z);
         AL.Listener(ALListener3f.Velocity, vel.X, vel.Y, vel.Z);
         AL.Listener(ALListenerf.Gain, myMasterGain);


         float[] listenerOri = new float[6] { 0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f };
         listenerOri[0] = myListener.forward.X;
         listenerOri[1] = myListener.forward.Y;
         listenerOri[2] = myListener.forward.Z;
         listenerOri[3] = myListener.up.X;
         listenerOri[4] = myListener.up.Y;
         listenerOri[5] = myListener.up.Z;
         AL.Listener(ALListenerfv.Orientation, ref listenerOri);

         //internal pseudo lock to prevent clobbering of std::vectors while iterating them
         myIsTicking = true;

         //remove the playing sound from the playing list if it asked to be stopped the previous tick
         foreach(AbstractAudio snd in myToRemovePlayingSounds)
         {
            myPlayingSounds.Remove(snd);
         }
         myToRemovePlayingSounds.Clear();

         foreach(AbstractAudio snd in myToAddPlayingSounds)
         {
            myPlayingSounds.Add(snd);
         }
         myToAddPlayingSounds.Clear();

         if(myIsEnabled)
         {
            foreach(AbstractAudio snd in myPlayingSounds)
            {
               snd.update(dt);
            }
         }

         myIsTicking = false;
      }

      public bool enabled 
      {
         get { return myIsEnabled; }
         set {
            myIsEnabled = value;
            if (myIsEnabled == true)
               resumeAll();
            else
               stopAll();
         }
      }

      public void startUpdating(AbstractAudio snd)
      {
         myToAddPlayingSounds.Add(snd);
      }

      public void stopUpdating(AbstractAudio snd)
      {
         myToRemovePlayingSounds.Add(snd);
      }

      public Listener listener { get { return myListener; } }

      public float soundVolume { get { return myMasterGain; } set { myMasterGain = value; } }
      public float musicVolume { get { return myMusicVolume; } set { myMusicVolume = value; } }

      public void resumeAll()
      {
         foreach(AbstractAudio snd in myPlayingSounds)
         {
            snd.play();
         }
      }

      public void pauseAll()
      {
         foreach (AbstractAudio snd in myPlayingSounds)
         {
            snd.pause();
         }
      }

      public void stopAll()
      {
         foreach (AbstractAudio snd in myPlayingSounds)
         {
            snd.stop();
         }
      }

      public Voice getVoice(Sound snd)
      {
         Voice v = null;

         if(myInactiveVoiceList.Count > 0)
         {
            v = myInactiveVoiceList.Dequeue();
            myActiveVoiceList.Add(v);
            return v;
         }

         //want to try and take a voice from somebody else
         //priority system is based on priority then loudness (at the distance)
         //find one of a lower priority, stop it and get it's voice after it cleans up
         bool soundFound = false;
         foreach(AbstractAudio asnd in myPlayingSounds)
         {
            Sound playingSound = asnd as Sound;
            if (playingSound != null)
            {
               if (playingSound.priority < snd.priority)
               {
                  //found one of lower priority
                  playingSound.stop();
                  soundFound = true;
                  break;
               }
            }
         }

         //look for one of the same priority, but quieter that we can override
         if(soundFound == false)
         {
            //TODO
         }

         //try to get the voice again
         if (myInactiveVoiceList.Count > 0)
         {
            v = myInactiveVoiceList.Dequeue();
            myActiveVoiceList.Add(v);
         }

         //might not get a voice if you're not important enough
         return v;
      }

      public void returnVoice(Voice v)
      {
         v.reset();
      }

      public void playSoundOneTime(string filename, Vector3 pos, bool relative, Vector3 vel, float falloffDistance, AbstractAudio.Priority priority)
      {
         SoundDescriptor desc = new SoundDescriptor();
         desc.filename = filename;
         desc.is3d = true;
         desc.isLooping = false;
         desc.priority = priority;

         Sound snd = new Sound(desc);

         snd.position = pos;
         snd.velocity = vel;
         snd.relativePosition = relative;
         snd.transient = true;
      }

      Capture getCaptureDevice(CaptureInitializer init)
      {
         if(myCaptureDevice == null)
         {
            if(init != null)
            {
               myCaptureDevice = new Capture(init);
            }
         }
         else
         {
            if( init != null && 
               init.format != myCaptureDevice.format & 
               init.captureSampleringBufferSize != myCaptureDevice.bufferSize &&
               init.frequency != myCaptureDevice.frequency && 
               init.deviceName != myCaptureDevice.deviceName)
            {
               myCaptureDevice.Dispose();
               myCaptureDevice = new Capture(init);
            }
         }

         return myCaptureDevice;
      }

      protected AudioManager()
      {

      }

      protected bool init()
      {
         try
         {
            //try to get the DirectSound default (openAL-soft's target)
            string defaultDevice = AudioContext.DefaultDevice;
            myContext = new AudioContext(defaultDevice);
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

         createVoices(myMaxVoices);

         return true;
      }

      protected void clearVoices()
      {
         //stop any playing sounds
         foreach(AbstractAudio snd in myPlayingSounds)
         {
            snd.stop();
         }

         foreach(Voice v in myActiveVoiceList)
         {
            v.Dispose();
         }

         foreach(Voice v in myInactiveVoiceList)
         {
            v.Dispose();
         }

         //clean up the voices
         myActiveVoiceList.Clear();
         myInactiveVoiceList.Clear();
      }

      protected void createVoices(int maxVoices)
      {
         if (maxVoices < 0 || maxVoices > 256)
         {
            Warn.print("number of voices must be greater than 0 and less than 256");
            return;
         }

         clearVoices();

         for(int i=0; i< maxVoices; i++)
         {
            Voice v = new Voice();
            if(v.init() == true)
            {
               v.reset();
               myInactiveVoiceList.Enqueue(v);
            }
            else
            {
               v = null;
               Warn.print("Error creating voice");

            }
         }
      }

      static AudioManager theAudioManager;
      AudioContext myContext;
      Capture myCaptureDevice;

      int myMaxVoices = 32;
      bool myIsEnabled = false;
      bool myIsTicking = false;
      double myCurrentTime = 0.0;

      float myMasterGain = 1.0f;
      float myMusicVolume = 1.0f;

      Listener myListener = new Listener();

      List<AbstractAudio> myPlayingSounds = new List<AbstractAudio>();
      List<AbstractAudio> myToAddPlayingSounds = new List<AbstractAudio>();
      List<AbstractAudio> myToRemovePlayingSounds = new List<AbstractAudio>();


      List<Voice> myActiveVoiceList = new List<Voice>();
      Queue<Voice> myInactiveVoiceList = new Queue<Voice>();
   }
}