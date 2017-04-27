using System;
using System.Collections.Generic;

using OpenTK;

using Graphics;
using Util;

namespace Terrain
{
   public class Material
   {
      [Flags]
      public enum Property {
         UNKNOWN = 0
        ,AIR= 1 << 0
        ,SOLID= 1 << 1
        ,TRANSPARENT= 1 << 2
        ,WATER = 1 << 3
        ,CLIP = 1 << 4
        ,DEATH =  1 << 5
        ,ACTIVE = 1 << 6
        ,ANIMATED = 1 << 7

        ,ALL=0xffff
        };

      public Property myProperty;
      int myTopTexture;
      int mySideTexture;
      int myBottomTexture;
      String myName;
      UInt32 myId;
      UInt32 myPackedTextures;

      public Material(UInt32 id, String name, int top, int side, int bottom, float s, Property prop)
      {
         myId = id;
         myName = name;
         myProperty = prop;
         myTopTexture = top;
         mySideTexture = side;
         myBottomTexture = bottom;
         scale = s;

         //just 1 texture
         if (top !=-1 && side==-1 && bottom ==-1)
         {
            myPackedTextures |= 0x40000000; //# of textures
            myPackedTextures |= (UInt32)(top & 0x3fffffff);
         }
         //two textures
         else if (top != -1 && side!=-1 && bottom==-1)
         {
            myPackedTextures |= 0x80000000; //# of textures
            myPackedTextures |= (UInt32)(top & 0x7fff) << 15;
            myPackedTextures |= (UInt32)(side & 0x7fff);
         }
         //three textures
         else 
         {
            myPackedTextures |= 0xC0000000; //# of textures
            myPackedTextures |= (UInt32)(top & 0x3ff) << 20;
            myPackedTextures |= (UInt32)(side & 0x3ff) << 10;
            myPackedTextures |= (UInt32)(bottom & 0x3ff);
         }
      }

      public float scale { get; set; }
      public string name { get { return myName; } }
      public Property property { get { return myProperty; } }
      public int top { get { return myTopTexture; } }
      public int side { get { return mySideTexture; } }
      public int bottom { get { return myBottomTexture; } }
      public UInt32 id { get { return myId; } }
      public UInt32 packedTextures { get { return myPackedTextures; } }
   };
}