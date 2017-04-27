using System;
using System.Collections.Generic;

using Util;

namespace Audio
{
   public class SourceDescriptor : ResourceDescriptor
   {
      public SourceDescriptor(string name) : this(name, false) { }
      public SourceDescriptor(string name, bool stream)
         : base(name)
      {
         streaming = stream;
      }

      public override IResource create() { return null; }

      public bool streaming { get; set; }
      public bool is3d { get; set; }
   }

   public abstract class Source : IResource
   {
      public enum SourceState {FAILED, UNLOADED, LOADING, LOADED, STREAMING, FINISHED};

      protected String myFilename;
      protected bool myIsStreaming;
      protected bool myIs3d;
      protected bool myIsLoaded;
      protected int mySampleRate;
      protected int myNumChannels;
      protected SourceState myState;

      protected List<AudioBuffer> myBuffers=new List<AudioBuffer>();

      public Source(SourceDescriptor desc)
      {
         myFilename = desc.name;
         myIsStreaming = desc.streaming;
         myIs3d = desc.is3d;
      }

      public void Dispose()
      {
      }
      
      public abstract bool load();

      public virtual bool unLoad()
      {
         myBuffers.Clear();
         myState = SourceState.UNLOADED;
         return true;
      }

      public bool isLoaded()
      {
         return myIsLoaded;
      }


      public String filename()
      {
         return myFilename;
      }

      public bool isStreaming()
      {
         return myIsStreaming;
      }

      public SourceState state()
      {
         return myState;
      }

      public int channels()
      {
         return myNumChannels;
      }

      public List<AudioBuffer> buffers()
      {
         return myBuffers;
      }

      //streaming functions
      public virtual void reset()
      {
      }

      public virtual void finishedBuffer(int bufferId)
      {
      }

      public virtual int nextBuffer(ref int nextBufferIndex)
      {
         int bufferId = -1;

         //are we done
         if ((nextBufferIndex == myBuffers.Count))
         {
            return -1;
         }

         bufferId = myBuffers[nextBufferIndex++].id;
         return bufferId;
      }
   }
}