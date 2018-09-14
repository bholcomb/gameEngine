/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

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

      public static string enumToString(int att)
      {
         switch (att)
         {
            case (int)AlcContextAttributes.Frequency: return "ALC_FREQUENCY"; break;
            case (int)AlcContextAttributes.Refresh: return "ALC_REFRESH"; break;
            case (int)AlcContextAttributes.Sync: return "ALC_SYNC"; break;
            case (int)AlcContextAttributes.MonoSources: return "ALC_MONO_SOURCES"; break;
            case (int)AlcContextAttributes.StereoSources: return "ALC_STEREO_SOURCES"; break;
            case 0x1992: return "ALC_HRTF_SOFT"; break;
            case 0x1993: return "ALC_HRTF_STATUS_SOFT"; break;
            case (int)AlcContextAttributes.EfxMaxAuxiliarySends: return "ALC_MAX_AUXILIARY_SENDS"; break;
            default: { return att.ToString(); } break;
         }
      }
   };


   public class AudioManager
   {
      static AudioManager theAudioManager;
      AudioContext myContext;
      IntPtr myDevice;
      Capture myCaptureDevice;

      bool myIsInitialized = false;
      int myMaxVoices = 32;
      bool myIsEnabled = false;
      double myCurrentTime = 0.0;

      bool myEnvironmentalProcessingAvailable = false;
      bool myEnvironmentalProcessingEnabled = false;

      float myMasterGain = 1.0f;
      float myMusicVolume = 1.0f;

      double myMusicTransitionTime;
      double myCurrentMusicTransitionTime;
      Sound myCurrentMusic = null;
      Sound myNextMusic = null;

      Listener myListener = new Listener();

      List<AbstractAudio> myPlayingSounds = new List<AbstractAudio>();
      List<AbstractAudio> myToAddPlayingSounds = new List<AbstractAudio>();
      List<AbstractAudio> myToRemovePlayingSounds = new List<AbstractAudio>();

      List<Voice> myActiveVoiceList = new List<Voice>();
      Queue<Voice> myInactiveVoiceList = new Queue<Voice>();

      public static AudioManager instance()
      {
         if (theAudioManager == null)
         {
            theAudioManager = new AudioManager();
            theAudioManager.init();
         }

         return theAudioManager;
      }

      protected bool init()
      {
         if (myIsInitialized == true)
         {
            return true;
         }


         Info.print("------------------AUDIO MANAGER----------------");

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
         myDevice = Alc.GetContextsDevice(Alc.GetCurrentContext());

         //print out the attributs
         int attributeSize = 0;
         Alc.GetInteger(myDevice, AlcGetInteger.AttributesSize, 1, out attributeSize);
         int[] attBuffer = new int[attributeSize * 2 + 1];
         Alc.GetInteger(myDevice, AlcGetInteger.AllAttributes, attributeSize * 2 + 1, attBuffer);
         int idx = 0;
         while(attBuffer[idx] != 0)
         {
            Info.print(String.Format("Context attribute: {0}:{1}", Audio.enumToString(attBuffer[idx]), attBuffer[idx + 1]));
            idx += 2;
         }

         //print some debug information about the system
         string alExtensions = AL.Get(ALGetString.Extensions);
         string alcExtensions = Alc.GetString(myDevice, AlcGetString.Extensions);
         
         Info.print("Opened Audio device {0}", myContext.ToString());
         Info.print("OpenAL Vendor: {0}", AL.Get(ALGetString.Vendor));
         Info.print("OpenAL Version: {0}", AL.Get(ALGetString.Version));
         Info.print("OpenAL Renderer: {0}", AL.Get(ALGetString.Renderer));
         Info.print("OpenAL Extensions: {0}", AL.Get(ALGetString.Extensions));
         Info.print("OpenAL Context Extensions: {0} ", Alc.GetString(myDevice, AlcGetString.Extensions));



         string[] extensions = alcExtensions.Split(' ');
         for (int i = 0; i < extensions.Length; i++)
         {
            if (extensions[i] == "ALC_EXT_EFX")
            {
               myEnvironmentalProcessingAvailable = true;
            }
         }

         Info.print("Environmental Processing: " + (myEnvironmentalProcessingAvailable ? "available" : "unavailable"));

         createVoices(myMaxVoices);

         Info.print("------------------AUDIO MANAGER----------------");

         myIsInitialized = true;
         return true;
      }

      public void transitionMusic(SoundDescriptor music, double transitionTime)
      {
         //cleanup if we transition before we're done with a transition
         if(myNextMusic != null)
         {
            if (music.source.filename != myNextMusic.source.filename)
            {
               myNextMusic.stop();
               myNextMusic = null;
            }
            else
            {
               return; //don't transition to the same song
            }
         }

         //don't transition to the same song
         if(myCurrentMusic != null && myCurrentMusic.source.filename == music.source.filename)
         {
            return;
         }

         //transition to new song
         Sound snd = new Sound(music);
         myNextMusic = snd;
         myNextMusic.volume = 0.0f;
         myNextMusic.play();
         myMusicTransitionTime = transitionTime;
         myCurrentMusicTransitionTime = 0.0;
      }

      void updateMusicTransition(double dt)
      {
         if(myNextMusic != null)
         {
            myCurrentMusicTransitionTime += dt;

            //check for the end condition
            if(myCurrentMusicTransitionTime >= myMusicTransitionTime)
            {
               myNextMusic.volume = myMusicVolume;
               if (myCurrentMusic != null)
               {
                  myCurrentMusic.stop();
               }

               myCurrentMusic = myNextMusic;
               myNextMusic = null;
               return;
            }

            //adjust the volumes of the current music and next music
            float vol = myMusicVolume * (float)(myCurrentMusicTransitionTime / myMusicTransitionTime); ;
            myNextMusic.volume = vol;

            if(myCurrentMusic != null)
            {
               myCurrentMusic.volume = myMusicVolume - vol;
            }
         }
      }

      public void tick(double dt)
      {
         myCurrentTime += dt;

         if(myIsInitialized == false)
         {
            Warn.print("Audio manager not initialized, cannot tick");
            return;
         }

         myContext.MakeCurrent();

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

         //remove the playing sound from the playing list if it asked to be stopped the previous tick
         foreach (AbstractAudio snd in myToRemovePlayingSounds)
         {
            myPlayingSounds.Remove(snd);
         }
         myToRemovePlayingSounds.Clear();

         foreach (AbstractAudio snd in myToAddPlayingSounds)
         {
            myPlayingSounds.Add(snd);
         }
         myToAddPlayingSounds.Clear();

         if (myIsEnabled)
         {
            foreach (AbstractAudio snd in myPlayingSounds)
            {
               snd.update(dt);
            }
         }

         updateMusicTransition(dt);
      }

      public void shutdown()
      {
         if (myIsInitialized)
         {
            myContext.MakeCurrent();

            foreach(AbstractAudio snd in myToRemovePlayingSounds)
            {
               myPlayingSounds.Remove(snd);
            }
            myToRemovePlayingSounds.Clear();

            //stop any playing sounds
            foreach (AbstractAudio snd in myPlayingSounds)
            {
               snd.stop();
            }
            myPlayingSounds.Clear();

            clearVoices();
            myCaptureDevice.Dispose();
            myContext.Dispose();

            myDevice = IntPtr.Zero;
            myIsInitialized = false;
            myIsEnabled = false;
         }
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
         if (myIsEnabled)
         {
            myToAddPlayingSounds.Add(snd);
         }
      }

      public void stopUpdating(AbstractAudio snd)
      {
         myToRemovePlayingSounds.Add(snd);

         if(snd.transient == true)
         {
            snd.Dispose();
         }
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

      public void playSoundOnetime(SoundDescriptor sd)
      {
         Sound snd = new Sound(sd);
         snd.transient = true;
         snd.play();
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
   }
}
