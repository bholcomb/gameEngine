/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;

namespace Sim
{
   public static partial class Passes
   {
      public static int General = 1;
      public static int Physics = 2;
      public static int Animation = 3;
      public static int Render = 4;

      public static int AttributeUpdate = Int32.MaxValue;

   }

   public static partial class Attributes
   {
      //base entity attributes
      public static int Type = 1;
      public static int Parent = 2;
      public static int Dynamic = 3;
      public static int Reflected = 4;

      public static int Position = 5;
      public static int Velocity = 6;
      public static int Acceleration = 7;
      public static int Orientation = 8;

      //sun/sky entity attributes
      public static int Period = 100;
      public static int Color = 101;
      public static int Direction = 102;
      public static int IsDaytime = 103;

      //Player/Mob Entity attributes
      public static int Health = 200;
      public static int State = 201;
      public static int Camera = 202;
      public static int DetectedEntites = 203;
      public static int IsAnimating = 204;
      public static int Airborne = 205;
      public static int InputHeading = 206;
      public static int InputPitch = 207;
      public static int InputMovement = 208;
      public static int InputJump = 209;
   };

   public static partial class Behaviors
   {
      public static int Animation = 1;
      public static int AudioListener = 2;
      public static int AudioSound = 3;
      public static int Camera = 4;
      public static int Collision = 5;
      public static int Damage = 6;
      public static int Embarkation = 7;
      public static int Health = 8;
      public static int Script = 9;
      public static int Sense = 10;
      public static int Weapon = 11;
      public static int Render = 12;
      public static int Physics = 13;
      public static int Reflect = 14;

      //sun/sky behaviors
      public static int Skybox = 100;
      public static int Sun = 101;

      //player behaviors
      public static int KinematicCharacter = 200;
   }
}