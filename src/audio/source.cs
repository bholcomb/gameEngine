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

      public override IResource create(ResourceManager mgr) { return null; }

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

      protected Source()
      {

      }

      public Source(SourceDescriptor desc)
      {
         myFilename = desc.name;
         myIsStreaming = desc.streaming;
         myIs3d = desc.is3d;
      }

      public virtual void Dispose()
      {
      }
      
      public abstract bool load();
      public abstract bool unLoad();

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
      public abstract void reset();
      public abstract void finishedBuffer(int bufferId);
      public abstract AudioBuffer nextBuffer(ref int nextBufferIndex);
   }
}
