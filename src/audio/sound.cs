using System;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Util;

namespace Audio
{
   public class Sound : AbstractAudio, IDisposable
   {
      Source mySource;
      int myNextBufferIndex;
      int myNumQueuedBuffers;

      public Sound(Source src)
         : base()
      {
         mySource = src;
         position = new Vector3();
         velocity = new Vector3();
         coneOrientation = new Vector3();
         myNextBufferIndex = 0;
         myNumQueuedBuffers = 0;
         is3d = false;
      }

      public void Dispose()
      {
         if (playing == true || paused == true)
         {
            stop();
         }
      }

      public bool is3d { get; set; }
      public Vector3 position { get; set; }
      public Vector3 velocity { get; set; }
      public Vector3 coneOrientation { get; set; }
      public float insideConeAngle { get; set; }
      public float outsideConeAngle { get; set; }
      public float coneOutsideVoluem { get; set; }
      public float minDistance { get; set; }
      public float maxDistance { get; set; }

      public override bool play()
      {
         if (playing == true)
         {
            Warn.print("Already playing sound");
            return false;
         }

         if (mySource.state() == Source.SourceState.FAILED)
         {
            Warn.print("cannot play a sound with a failed source");
            return false;
         }

         if (mySource.state() == Source.SourceState.UNLOADED || mySource.state() == Source.SourceState.LOADING)
         {
            Warn.print("cannot play a sound with a loading source");
            return false;
         }

         voice = AudioSystem.play(this);
         if (voice <= 0)
         {
            //Warn.print("Cannot play sound without a voice");
            return false;
         }

         playing = true;
         paused = false;

         update();

         AL.SourcePlay(voice);
         AudioSystem.checkError("Sound.play-AL.SourcePlay");
         return true;
      }

      public override bool pause()
      {
         //may want to make these messages and do it asynchronously
         if (mySource.state() == Source.SourceState.FAILED)
         {
            Warn.print("cannot pause a sound with a failed source");
            return false;
         }

         playing = false;
         paused = true;

         AL.SourcePause(voice);
         AudioSystem.checkError("Sound.pause-AL.SourcePause");

         return true;
      }

      public override bool stop()
      {
         AL.SourceStop(voice);
         AudioSystem.checkError("Sound.stop-AL.SourceStop");

         popBuffers();

         myNumQueuedBuffers = 0;
         myNextBufferIndex = 0;
         playing = false;
         paused = false;

         return true;
      }

      public override void update()
      {
         if (voice <= 0)
         {
            Error.print("Updating sound without a voice");
            stop();
            return;
         }

         if (!playing)
         {
            stop();
            return;
         }

         if (paused)
         {
            return;
         }

         //update variables;
         AL.Source(voice, ALSourcef.Pitch, pitch);
         AL.Source(voice, ALSourcef.Gain, volume);

         //set position if necessary
         if (is3d==true)
         {
            if (mySource.channels() != 1)
            {
               Warn.print("Cannot localize a stereo sound in OpenAL");
            }
            AL.Source(voice, ALSource3f.Position, position.X, position.Y, position.Z);
            AL.Source(voice, ALSource3f.Velocity, velocity.X, velocity.Y, velocity.Z);
         }
         else
         {
            Vector3 pos = AudioSystem.listener.position;
            Vector3 vel = AudioSystem.listener.velocity;
            AL.Source(voice, ALSource3f.Position, pos.X, pos.Y, pos.Z);
            AL.Source(voice, ALSource3f.Velocity, vel.X, vel.Y, vel.Z);
         }
         AL.Source(voice, ALSourceb.Looping, false);

         AudioSystem.checkError("Sound.update-updated variables");

         //pop off any buffers that are done
         popBuffers();

         //add any new buffers if there are any
         pushBuffers();
      }

      void popBuffers()
      {
         int processed;
         AL.GetSource(voice, ALGetSourcei.BuffersProcessed, out processed);
         AudioSystem.checkError("Sound.stop-buffers processed");

         //pop off the played buffers and queue next buffers
         while (processed-- != 0)
         {
            int buffer;
            buffer = AL.SourceUnqueueBuffer(voice);
            AudioSystem.checkError("Sound.stop-buffers unqueue");
            mySource.finishedBuffer(buffer);
            myNumQueuedBuffers--;
         }
      }

      void pushBuffers()
      {
         bool addedBuffers = false;
         while (myNumQueuedBuffers < MaxQueuedBuffers)
         {
            int buffer;
            buffer = mySource.nextBuffer(ref myNextBufferIndex);
            if (buffer == NoMoreBuffers && looping)
            {
               mySource.reset();
               myNextBufferIndex = 0;
               buffer = mySource.nextBuffer(ref myNextBufferIndex);
            }

            //actually buffer the data in OpenAL Context
            if (buffer != NoMoreBuffers)
            {
               //Debug.print("Queued buffer: {0}", buffer);
               AL.SourceQueueBuffer(voice, buffer);
               AudioSystem.checkError("Sound::update-buffers queued");
               myNumQueuedBuffers++;
               addedBuffers = true;
            }
            else
            {
               //no more buffers to play, we must be done
               if (myNumQueuedBuffers == 0)
                  playing = false;
               break;
            }
         }

         if (addedBuffers == true)
         {
            ALSourceState state = AL.GetSourceState(voice);
            if (state == ALSourceState.Stopped)
            {
               AL.SourcePlay(voice);
            }
         }
      }
   }
}