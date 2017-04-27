using System;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Audio
{
   public class Listener
   {
      Vector3 myPosition;
      Vector3 myVelocity;

      public Listener()
      {
      }

      public Vector3 position
      {
         get { return myPosition; }
         set { myPosition = value; }
      }

      public Vector3 velocity
      {
         get { return myVelocity; }
         set { myVelocity = value; }
      }

   }
}