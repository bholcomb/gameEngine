using System;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Util;

namespace Audio
{
   public class SoundDescriptor
   {
      public String filename = "";
      public Source source = null;
      public bool is3d = false;
      public bool isLooping = false;
      public bool isRelative = true;
      public Vector3 position = new Vector3();
      public Vector3 velocity = new Vector3();
      public float falloffDistance = 10.0f;
      public AbstractAudio.Priority priority = AbstractAudio.Priority.BACKGROUND_FX;
   };


   public class Sound : AbstractAudio
   {
      public bool is3d { get; set; }
      public bool isLooping { get; set; }
      public Vector3 position { get; set; }
      public Vector3 velocity { get; set; }
      public Vector3 coneOrientation { get; set; }
      public float insideConeAngle { get; set; }
      public float outsideConeAngle { get; set; }
      public float coneOutsideVoluem { get; set; }
      public float referenceDistance { get; set; }
      public float maxFalloffDistance { get; set; }
      public bool relativePosition { get; set; }

      Source mySource;
      int myNextBufferIndex;

      Voice myVoice;

      public Sound(SoundDescriptor desc)
         : base()
      {
         is3d = desc.is3d;
         isLooping = desc.isLooping;
         position = desc.position;
         velocity = desc.velocity;
         coneOrientation = new Vector3();
         referenceDistance = desc.falloffDistance;
         relativePosition = desc.isRelative;
         mySource = desc.source;
         priority = desc.priority;

         myVoice = null;
         myNextBufferIndex = 0;
      }

      public override void Dispose()
      {
         if (playing == true || paused == true)
         {
            stop();
         }

         if(myVoice != null)
         {
            myAudioManager.returnVoice(myVoice);
         }
      }

      public override bool play()
      {
         if (mySource == null)
         {
            Warn.print("Cannot play a sound without a valid source");
            stop();
            return false;
         }

         if (mySource.state() == Source.SourceState.FAILED)
         {
            Warn.print("cannot play a sound with a failed source");
            stop();
            return false;
         }

         if(myAudioManager.enabled == false)
         {
            //can't play a sound if the sound manager isn't enabled
            return false;
         }

         bool tooFar = false;
         if(is3d == true)
         {
            float dist = 0;
            if (relativePosition == true)
               dist = position.Length;
            else
               dist = (position - myAudioManager.listener.position).Length;

            if(dist  > maxFalloffDistance)
            {
               //wer're too far away to play
               tooFar = true;
            }
         }

         if(state == State.PLAYING)
         {
            if (tooFar)
               stop();
            else
               //its ok to call play on the playing sound
               return true;
         }

         //sound is too far away to hear, don't bother
         if (tooFar)
            return false;

         myVoice = myAudioManager.getVoice(this);
         if (myVoice == null)
         {
            Warn.print("Cannot play sound without a voice");
            return false;
         }

         //set reference and max falloff distances based on sound decibels
         myVoice.setReferenceDistance(referenceDistance);
         myVoice.setMaxFalloffDistance(maxFalloffDistance);

         myAudioManager.startUpdating(this);

         state = State.PLAYING;
         update(0.0);

         return true;
      }

      public override bool pause()
      {
         if (state == State.PAUSED)
         {
            return true;
         }

         //can't pause a sound that doesn't have a voice
         if (myVoice == null)
         {
            return false;
         }

         myAudioManager.stopUpdating(this);

         state = State.PAUSED;
         myVoice.pause();

         return true;
      }

      public override bool stop()
      {
         //can't stop a sound that isn't playing or paused
         if (state == State.STOPPED)
         {
            return false;
         }

         state = State.STOPPED;

         if (myVoice != null)
         {
            myAudioManager.returnVoice(myVoice);
            myVoice = null;
         }

         myAudioManager.stopUpdating(this);

         myNextBufferIndex = 0;

         return true;
      }

      public override void update(double dt)
      {
         //can't update a sound that doesn't have a voice
         if (myVoice == null)
         {
            return;
         }

         //can't update a sound that is paused or stopped
         if (state != State.PLAYING)
         {
            return;
         }

         if (mySource.state() == Source.SourceState.UNLOADED || mySource.state() == Source.SourceState.LOADING)
         {
            //gotta wait for the sound to finish loading
            return;
         }

         //update variables;
         myVoice.setPitch(pitch);
         myVoice.setVolume(volume);

         if (!is3d)
         {
            myVoice.setRelativeLocation(true);
            myVoice.setPosition(Vector3.Zero);
            myVoice.setVelocity(Vector3.Zero);
         }
         else
         {
            myVoice.setRelativeLocation(relativePosition);
            myVoice.setPosition(position);
            myVoice.setVelocity(velocity);
         }

         //we control looping in case the source has multiple buffers
         myVoice.setLooping(false);

         Audio.checkError("Sound.update-updated variables");

         //remove any finished buffers.  Tell the source in case it's streaming
         int finishedBuffer = myVoice.finishedBuffer();
         while (finishedBuffer != 0)
         {
            mySource.finishedBuffer(finishedBuffer);
            finishedBuffer = myVoice.finishedBuffer();
         }

         for (int i = myVoice.queuedBuffers(); i < MaxQueuedBuffers; i++)
         {
            AudioBuffer buffer;
            buffer = mySource.nextBuffer(ref myNextBufferIndex);

            if (buffer != null)
            {
               //debug << "Queued buffer: " << buffer << std::endl;
               myVoice.addBuffer(buffer);
            }
            else
            {
               //check for looping sounds
               if (isLooping)
               {
                  mySource.reset();
                  myNextBufferIndex = 0;
                  buffer = mySource.nextBuffer(ref myNextBufferIndex);
                  if (buffer != null)
                  {
                     myVoice.addBuffer(buffer);
                  }
               }
            }
         }

         //no more buffers to play so we must be done, 
         //unless we are streaming, in which case we'll wait for more data and an explicit stop
         if (myVoice.queuedBuffers() == 0 && myVoice.isPlaying() == false && mySource.state() != Source.SourceState.STREAMING)
         {
            stop();
         }
      }
   }
}
