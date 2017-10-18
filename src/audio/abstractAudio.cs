using System;

namespace Audio
{
   public abstract class AbstractAudio: IDisposable
   {
      public enum State {STOPPED, PLAYING, PAUSED};
      public enum Priority {BACKGROUND_FX, SECONDARY_FX, PRIMARY_FX, DIALOG, MUSIC};
      public const int MaxQueuedBuffers=3;
      public const int NoMoreBuffers=-1;

      protected float myVolume;

      protected AudioManager myAudioManager;

      public AbstractAudio()
      {
         myVolume = 1.0f;
         pitch = 1.0f;
         priority = Priority.BACKGROUND_FX;
         transient = false;

         myAudioManager = AudioManager.instance();
      }

      public abstract bool play();
      public abstract bool pause();
      public abstract bool stop();
      public abstract void update(double dt);
      public abstract void Dispose();

      public bool playing { get; set; }
      public bool paused { get; set; }
      public float volume { get { return myVolume; } set { myVolume = value; } }
      public float pitch { get; set; }
      public Priority priority { get; set; }
      public State state { get; set; }
      public bool transient { get; set; }

   }
}