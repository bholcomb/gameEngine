/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.Collections.Generic;

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Audio
{
   public class CaptureInitializer
   {
      public string deviceName;
      public int frequency;
      public AudioBuffer.AudioFormat format;
      public int captureSampleringBufferSize;

      public CaptureInitializer()
      {
         deviceName = "";
         frequency = 22050;
         format = AudioBuffer.AudioFormat.MONO16;
         captureSampleringBufferSize = 4410; //200ms of data
      }
   };

   public class Capture : IDisposable
   {
      AudioCapture myDevice;
      string myDeviceName;
      int myFrequency;
      AudioBuffer.AudioFormat myFormat;
      int myBufferSize;

      public Capture(CaptureInitializer init)
      {
         myFrequency = init.frequency;
         myFormat = init.format;
         myBufferSize = init.captureSampleringBufferSize;
         myDeviceName = init.deviceName;

         ALFormat alFormat = 0;
         switch(myFormat)
         {
            case AudioBuffer.AudioFormat.MONO8: alFormat = ALFormat.Mono8; break;
            case AudioBuffer.AudioFormat.MONO16: alFormat = ALFormat.Mono16; break;
            case AudioBuffer.AudioFormat.STEREO8: alFormat = ALFormat.Stereo8; break;
            case AudioBuffer.AudioFormat.STEREO16: alFormat = ALFormat.Stereo16; break;
         }

         myDevice = new AudioCapture(myDeviceName == "" ? AudioCapture.DefaultDevice : myDeviceName, myFrequency, alFormat, myBufferSize);
      }

      public void Dispose()
      {
         stop();
         myDevice.Dispose();
      }

      public int frequency { get { return myFrequency; } }
      public int bufferSize { get { return myBufferSize; } }
      public AudioBuffer.AudioFormat format {get {return myFormat;} }
      public string deviceName { get { return myDeviceName; } }

      public void start()
      {
         myDevice.Start();
      }

      public void stop()
      {
         myDevice.Stop();
      }

      public AudioBuffer nextBuffer()
      {
         int samples = myDevice.AvailableSamples;
         if(samples > 0 )
         {
            AudioBuffer buffer = new AudioBuffer(myFormat, myFrequency);
            buffer.size = buffer.calculateBufferSize(myFormat, samples);
            myDevice.ReadSamples(buffer.data, samples);
            return buffer;
         }

         return null;
      }

      public static List<string> availableCaptureDevices()
      {
         List<string> devices = new List<string>(AudioCapture.AvailableDevices);
         return devices;
      }
   }
}