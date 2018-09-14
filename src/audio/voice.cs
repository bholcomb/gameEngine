/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Util;

namespace Audio
{
   public class Voice : IDisposable
   {
      protected int myId;
      protected int myNumQueuedBuffers;

      public Voice()
      {
         myId = -1;
         myNumQueuedBuffers = 0;
      }

      public bool init()
      {
         myId = AL.GenSource();
         bool ret = Audio.checkError("Generating OpenAL source");
         if (ret == false)
         {
            myId = -1;
         }

         return ret;
      }

      public void Dispose()
      {
         if(myId > 0)
         {
            AL.DeleteSource(myId);
         }
      }


      public int id {get { return myId; } }

      public void reset()
      {
         stop();

         Vector3 z = Vector3.Zero;
         AL.Source(myId, ALSource3f.Position, ref z);
         AL.Source(myId, ALSource3f.Velocity, ref z);
         AL.Source(myId, ALSource3f.Direction, ref z);
         AL.Source(myId, ALSourcef.Gain, 1.0f);
         AL.Source(myId, ALSourcef.Pitch, 1.0f);
         AL.Source(myId, ALSourceb.Looping, false);
         AL.Source(myId, ALSourceb.SourceRelative, false);
         AL.Source(myId, ALSourcef.ReferenceDistance, 1.0f);
         AL.Source(myId, ALSourcef.MaxDistance, 10.0f);

         removeAllBuffers();
      }

      public void addBuffer(AudioBuffer buffer)
      {
         int[] id = new int[] { buffer.id };
         AL.SourceQueueBuffers(myId, 1, id);
         myNumQueuedBuffers++;

         if (!isPlaying())
         {
            //probably ran out of buffers and stopped playing, restart the source again
            start();
         }
      }

      public int queuedBuffers()
      {
         int numBuffers;
         AL.GetSource(myId, ALGetSourcei.BuffersQueued, out numBuffers);
         return numBuffers;
      }

      public int finishedBuffer()
      {
         int numBuffers = 0;
         AL.GetSource(myId, ALGetSourcei.BuffersProcessed, out numBuffers);
         if(numBuffers > 0)
         {
            int[] buffer = new int[1];
            AL.SourceUnqueueBuffers(myId, 1, buffer);
            myNumQueuedBuffers--;
            return buffer[0];
         }

         return 0;
      }

      public void removePlayedBuffers()
      {
         int numBuffers = 0;
         AL.GetSource(myId, ALGetSourcei.BuffersProcessed, out numBuffers);
         while (numBuffers-- > 0)
         {
            int[] buffer = new int[1];
            AL.SourceUnqueueBuffers(myId, 1, buffer);
            myNumQueuedBuffers--;
         }
      }

      public void removeAllBuffers()
      {
         int numBuffers = 0;
         AL.GetSource(myId, ALGetSourcei.BuffersQueued, out numBuffers);
         while(numBuffers-- > 0)
         {
            int[] buffers = new int[1];
            AL.SourceUnqueueBuffers(myId, 1, buffers);
            myNumQueuedBuffers--;
         }
      }

      public void start()
      {
         AL.SourcePlay(myId);
      }

      public void pause()
      {
         AL.SourcePause(myId);
      }

      public void stop()
      {
         AL.SourceStop(myId);
      }

      public bool isPlaying()
      {
         ALSourceState state;
         state = AL.GetSourceState(myId);
         return state == ALSourceState.Playing;
      }

      public void setPosition(Vector3 pos)
      {
         AL.Source(myId, ALSource3f.Position, ref pos);
      }
     
      public void setVelocity(Vector3 vel)
      {
         AL.Source(myId, ALSource3f.Velocity, ref vel);
      }

      public void setVolume(float vol)
      {
         AL.Source(myId, ALSourcef.Gain, vol);
      }

      public void setPitch(float pitch)
      {
         AL.Source(myId, ALSourcef.Pitch, pitch);
      }

      public void setLooping(bool loop)
      {
         AL.Source(myId, ALSourceb.Looping, loop);
      }

      public void setRelativeLocation(bool rel)
      {
         AL.Source(myId, ALSourceb.SourceRelative, rel);
      }

      public void setReferenceDistance(float dist)
      {
         AL.Source(myId, ALSourcef.ReferenceDistance, dist);
      }

      public void setMaxFalloffDistance(float dist)
      {
         AL.Source(myId, ALSourcef.MaxDistance, dist);
      }
   }
}
