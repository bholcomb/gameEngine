using System;
using System.Collections.Generic;
using System.Threading;

using Util;

namespace Audio
{
   public abstract class Stream : Source
   {
      protected UInt32 myNextBuffer = 0;
      protected Mutex myQueueMutex = new Mutex();
      protected Queue<AudioBuffer> myStreamingBuffers;
      protected Dictionary<int, AudioBuffer> myBufferMap;

      public Stream()
      {
         myState = SourceState.STREAMING;
      }

      public override void Dispose()
      {
         unLoad();
      }

      public override bool load()
      {
         return true;
      }

      public override bool unLoad()
      {
         reset();
         myState = SourceState.UNLOADED;

         return true;
      }

      public void addBuffer(AudioBuffer buffer)
      {
         myQueueMutex.WaitOne();
         myStreamingBuffers.Enqueue(buffer);
         myQueueMutex.ReleaseMutex();
      }

      //streaming functions
      public override void reset()
      {
         myQueueMutex.WaitOne();
         myStreamingBuffers.Clear();
         myQueueMutex.ReleaseMutex();
      }

      public override void finishedBuffer(int bufferId)
      {
         myBufferMap.Remove(bufferId);
      }

      public override AudioBuffer nextBuffer(ref int nextBufferIndex)
      {
         if(myStreamingBuffers.Count > 0)
         {
            myQueueMutex.WaitOne();
            AudioBuffer buff = myStreamingBuffers.Dequeue();
            myQueueMutex.ReleaseMutex();
            nextBufferIndex++;

            myBufferMap[buff.id] = buff;
            return buff;
         }

         return null;
      }
   }
}