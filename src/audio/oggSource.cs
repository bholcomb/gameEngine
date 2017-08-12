using System;

using Util;
using NVorbis;

namespace Audio
{
   public class OggSourceDescriptor : SourceDescriptor
   {
      public OggSourceDescriptor(string name) : this(name, false) { }
      public OggSourceDescriptor(string name, bool streaming)
         : base(name, streaming)
      {
      }

      public override IResource create()
      {
         OggSource w = new OggSource(this);
         if (w.load() == false)
         {
            return null;
         }
         return w;
      }
   }

   public class OggSource : Source
   {
      VorbisReader myReader;

      public OggSource(SourceDescriptor desc)
         : base(desc)
      {
         myIs3d = false;
      }

      public new void Dispose()
      {
         if (myReader != null)
         {
            myReader.Dispose();
            myReader = null;
         }
      }

      public override bool load()
      {
         float[] readSampleBuffer=new float[AudioBuffer.MAX_BUFFER_SIZE];
         short[] convBuffer =new short[AudioBuffer.MAX_BUFFER_SIZE];

         myState = SourceState.LOADING;
         myReader = new VorbisReader(myFilename);

         myNumChannels=myReader.Channels;
         mySampleRate = myReader.SampleRate;
         long sampleCount = myReader.TotalSamples * myNumChannels;

         int numBuffers = (int)Math.Ceiling((double)sampleCount / (double)AudioBuffer.MAX_BUFFER_SIZE);

         for (int i = 0; i < numBuffers; i++)
         {
            AudioBuffer buffer = new AudioBuffer(myNumChannels == 1 ? AudioBuffer.AudioFormat.MONO16 : AudioBuffer.AudioFormat.STEREO16, mySampleRate);
            int samplesRead = myReader.ReadSamples(readSampleBuffer, 0, AudioBuffer.MAX_BUFFER_SIZE);
            buffer.size = samplesRead;
            castBuffer(readSampleBuffer, buffer.data, samplesRead);

            //put it in the audio system
            buffer.buffer();
            myBuffers.Add(buffer);
         }

         myState = Source.SourceState.LOADED;
         Debug.print("Loaded audio file: {0}", myFilename);
         return true;
      }

      public override bool unLoad()
      {
         myBuffers.Clear();

         return true;
      }

      public override AudioBuffer nextBuffer(ref int nextBufferIndex)
      {
         //are we done?
         if (nextBufferIndex == myBuffers.Count)
         {
            return null;
         }

         AudioBuffer buffer = myBuffers[nextBufferIndex++];
         return buffer;
      }

      public override void finishedBuffer(int bufferId)
      {
         //noop
      }

      public override void reset()
      {
         //noop
      }

      void castBuffer(float[] inBuffer, short[] outBuffer, int length)
      {
         for (int i = 0; i < length; i++)
         {
            var temp = (int)(32767f * inBuffer[i]);
            if (temp > short.MaxValue) temp = short.MaxValue;
            else if (temp < short.MinValue) temp = short.MinValue;
            outBuffer[i] = (short)temp;
         }
      }
   }
}