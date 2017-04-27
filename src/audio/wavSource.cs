using System;
using System.IO;

using Util;

namespace Audio
{
   public class WavSourceDescriptor : SourceDescriptor
   {
      public WavSourceDescriptor(string name) : this(name, false) { }
      public WavSourceDescriptor(string name, bool streaming)
         : base(name, streaming)
      {
      }

      public override IResource create()
      {
         WavSource w = new WavSource(this);
         if(w.load()==false)
         {
            return null;
         }
         return w;
      }
   }

   public class WavSource : Source
   {
      public WavSource(SourceDescriptor desc)
         : base(desc)
      {
      }

      public override bool load()
      {
         if (File.Exists(myFilename) == false)
         {
            Warn.print("Cannot find file {0}", myFilename);
            myState = SourceState.FAILED;
            return false;
         }

         using (Stream waveFileStream = File.Open(myFilename, System.IO.FileMode.Open))
         {
            BinaryReader reader = new BinaryReader(waveFileStream);

            int chunkID = reader.ReadInt32();
            if (chunkID != 0x46464952)
            {
               Warn.print("{0} is not a RIFF formated file", myFilename);
               myState = SourceState.FAILED;
               return false;
            }
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            if (riffType != 0x45564157 ) //"WAVE" in bytes
            {
               Warn.print("{0} is not a WAV", myFilename);
               myState = SourceState.FAILED;
               return false;
            }
            int fmtID = reader.ReadInt32();
            if (fmtID != 0x20746d66 ) //"fmt " in bytes
            {
               Warn.print("Cannot find valid fmt chunk in {0}", myFilename);
               myState = SourceState.FAILED;
               return false;
            }

            int fmtSize = reader.ReadInt32();
            if (fmtSize != 16)
            {
               Warn.print("{0} is not in 16 bit format", myFilename);
               myState = SourceState.FAILED;
               return false;
            }
            int fmtCode = reader.ReadInt16();
            if (fmtCode != 1) //PCM data
            {
               Warn.print("{0} is not in PCM format", myFilename);
               myState = SourceState.FAILED;
               return false;
            }
            myNumChannels = reader.ReadInt16();
            if (myIs3d == true && myNumChannels != 1)
            {
               Error.print("Unable to load stereo files for 3D capability");
               myState = SourceState.FAILED;
               return false;
            }

            mySampleRate = reader.ReadInt32();
            int fmtAvgBPS = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();

            if (fmtSize == 18)
            {
               // Read any extra values
               int fmtExtraSize = reader.ReadInt16();
               reader.ReadBytes(fmtExtraSize);
            }

            int dataID = reader.ReadInt32();
            if (dataID != 0x61746164) //"data" in bytes
            {
               Warn.print("Cannot find valid data chunk in file {0}", myFilename);
               myState = SourceState.FAILED;
               return false;
            }
            int dataSize = reader.ReadInt32();

            if (bitDepth != 16)
            {
               Warn.print("WAV files must be 16-bit PCM format");
               myState = SourceState.FAILED;
               return false;
            }

            //read the data
            byte[] data;
            data = reader.ReadBytes(dataSize);

            //convert to shorts
            short[] audioData = new short[dataSize / 2];
            for (int i = 0; i < dataSize / 2; i++)
            {
               audioData[i] = BitConverter.ToInt16(data, i * 2);
            }

            //fill audio buffers
            int numSamples = dataSize / 2;
            int numBuffers = (int)Math.Ceiling((double)numSamples / (double)AudioBuffer.MAX_BUFFER_SIZE);
            int sampleOffset=0;
            int samplesRemaining = numSamples;
            for (int i = 0; i < numBuffers; i++)
            {
               AudioBuffer buffer = new AudioBuffer(myNumChannels == 1 ? AudioBuffer.AudioFormat.MONO16 : AudioBuffer.AudioFormat.STEREO16, mySampleRate);
               int samplesToCopy = Math.Min(AudioBuffer.MAX_BUFFER_SIZE, samplesRemaining);
               buffer.size = samplesToCopy;
               Array.Copy(audioData, sampleOffset, buffer.data, 0, samplesToCopy);
               sampleOffset += samplesToCopy;
               samplesRemaining -= samplesToCopy;

               //put it in the audio system
               buffer.buffer();
               myBuffers.Add(buffer);
            }
         }

         myState = Source.SourceState.LOADED;
         Debug.print("Loaded audio file: {0}", myFilename);
         return true;
      }
   }
}