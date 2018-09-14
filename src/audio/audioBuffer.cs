/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Util;

namespace Audio
{
   public class AudioBuffer : IDisposable
   {
      //buffers need to be significantly big or else they don't work for some reason
      public static int MAX_BUFFER_SIZE = (44100 * 2);  // 1 second at high rates for stereo sound

      public enum AudioFormat { MONO8, MONO16, STEREO8, STEREO16 };

      short[] myData;
      int mySize;
      int myId;
      int myRate;
      AudioFormat myFormat;

      public AudioBuffer(AudioFormat format, int rate)
      {
         myFormat = format;
         myRate = rate;
         mySize = 0;
         myData = null;

         AL.GenBuffers(1, out myId);
      }

      public void Dispose()
      {
         AL.DeleteBuffer(myId);
      }

      public AudioFormat format
      {
         get { return myFormat; }
      }

      public int size
      {
         get { return mySize; }
         set
         {
            if (value == mySize) return;

            mySize = value;
            myData = new short[mySize];
            
         }
      }

      public int rate
      {
         get { return myRate; }
      }

      public int id
      {
         get { return myId; }
      }

      public short[] data
      {
         get { return myData; }
      }

      public void buffer()
      {
         if (myId <= 0)
         {
            Debug.print("Cannot buffer data without an id");
            return;
         }

         ALFormat format = myFormat == AudioFormat.MONO16 ? ALFormat.Mono16 : ALFormat.Stereo16;
         AL.BufferData(myId, format, myData, mySize * sizeof(short), myRate);
         ALError err = AL.GetError();
         if (err != ALError.NoError)
         {
            Debug.print("Audio Buffer-buffer error: {0}", AL.GetErrorString(err));
         }
      }

      public void clear()
      {
         Array.Clear(myData, 0, myData.Length);
      }

      public void setData(short[] buffer)
      {
         mySize = buffer.Length;
         myData = buffer;
      }

      public int numberOfSamples()
      {
         int numSamples = mySize;
         int multiplier = 1;
         
         switch(myFormat)
         {
            case AudioFormat.MONO8: multiplier = 1; break;
            case AudioFormat.MONO16: multiplier = 2; break;
            case AudioFormat.STEREO8: multiplier = 2; break;
            case AudioFormat.STEREO16: multiplier = 4; break;
         }

         return numSamples / multiplier;
      }

      public int calculateBufferSize(AudioFormat format, int numSamples)
      {
         int multiplier = 1;
         switch (format)
         {
            case AudioFormat.MONO8: multiplier = 1; break;
            case AudioFormat.MONO16: multiplier = 2; break;
            case AudioFormat.STEREO8: multiplier = 2; break;
            case AudioFormat.STEREO16: multiplier = 4; break;
         }

         return numSamples * multiplier;
      }
   }
}