/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

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
      Vector3 myForward;
      Vector3 myUp;

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

      public Vector3 forward
      {
         get { return myForward; }
         set { myForward = value; }
      }

      public Vector3 up
      {
         get { return myUp; }
         set { myUp = value; }
      }

      public Quaternion orientation
      {
         set
         {
            Vector3 fwd = new Vector3(1, 0, 0);
            Vector3 _up = new Vector3(0, 1, 0);

            forward = value * fwd;
            up = value * up;
         }
      }
   }
}