using System;

using OpenTK;
using Util;

namespace Terrain
{
   public struct ChunkKey
   {
      [Flags]
      public enum Neighbor
      {
         NONE = 0,
         LEFT = 1,
         RIGHT = 2,
         BOTTOM = 4,
         TOP = 8,
         FRONT = 16,
         BACK = 32,
         LBF = LEFT | BOTTOM | FRONT,
         LB = LEFT | BOTTOM,
         LBB = LEFT | BOTTOM | BACK,
         BF = BOTTOM | FRONT,
         BB = BOTTOM | BACK,
         RBF = RIGHT | BOTTOM | FRONT,
         RB = RIGHT | BOTTOM,
         RBB = RIGHT | BOTTOM | BACK,
         LF = LEFT | FRONT,
         BL = LEFT | BACK,
         RF = RIGHT | FRONT,
         BR = RIGHT | BACK,
         LTF = LEFT | TOP | FRONT,
         LT = LEFT | TOP,
         LTB = LEFT | TOP | BACK,
         TF = TOP | FRONT,
         TB = TOP | BACK,
         RTF = RIGHT | TOP | FRONT,
         RT = RIGHT | TOP,
         RTB = RIGHT | TOP | BACK,
         ALL = LEFT | RIGHT | BOTTOM | TOP | FRONT | BACK
      };

      static int theUsableBits = 21;
      static int theBias = (1 << theUsableBits) / 2; // 1048576 when using 21 bits to allow for negative locations to be mapped to positive integer space

      public UInt64 myKey;
      public Vector3i myId;
      public Vector3 myLocation;

      public ChunkKey(Vector3 worldLocation)
      {
         myLocation = clampToChunk(worldLocation);
         myId = createIdFromWorldLocation(myLocation);
         myKey = createKey(myId);
      }

      public ChunkKey(Vector3i id)
      {
         myLocation = createWorldLocationFromId(id);
         myId = id;
         myKey = createKey(myId);
      }

      public ChunkKey(UInt64 key)
      {
         myKey = key;
         myId = createIdFromKey(myKey);
         myLocation = createWorldLocationFromId(myId);
      }

      public Vector3i biasedId
      {
         get
         {
            Vector3i id = new Vector3i(myId);
            id.X += theBias;
            id.Y += theBias;
            id.Z += theBias;
            return id;
         }
      }

      public ChunkKey neighbor(Neighbor which)
      {
         Vector3i id = myId;
         if (which.HasFlag(Neighbor.LEFT)) id.X -= 1;
         if (which.HasFlag(Neighbor.RIGHT)) id.X += 1;
         if (which.HasFlag(Neighbor.BOTTOM)) id.Y -= 1;
         if (which.HasFlag(Neighbor.TOP)) id.Y += 1;
         if (which.HasFlag(Neighbor.FRONT)) id.Z -= 1;
         if (which.HasFlag(Neighbor.BACK)) id.Z += 1;

         return new ChunkKey(id);
      }

      #region static conversion functions
		static int roundToNearestInt(float val)
		{
			int ret = (int)(val + 0.5);

			if (val < 0)
			{
				if (val < ret)
					ret -= 1;
			}
			else
			{
				if (val < ret)
					ret += 1;
			}

			return ret;
		}

      public static Vector3 clampToChunk(Vector3 loc)
      {
         Vector3 ret = new Vector3();

			ret.X = roundToNearestInt(loc.X / WorldParameters.theChunkSize);
			ret.Y = roundToNearestInt(loc.Y / WorldParameters.theChunkSize);
			ret.Z = roundToNearestInt(loc.Z / WorldParameters.theChunkSize);

			return ret * WorldParameters.theChunkSize;
      }

      public static Vector3 clampToRegion(Vector3 loc)
      {
         Vector3 ret = new Vector3();
			ret.X = roundToNearestInt(loc.X / WorldParameters.theRegionSize);
			ret.Y = roundToNearestInt(loc.Y / WorldParameters.theRegionSize);
			ret.Z = roundToNearestInt(loc.Z / WorldParameters.theRegionSize);
			return ret * WorldParameters.theRegionSize;
      }

      public static Vector3i createIdFromWorldLocation(Vector3 loc)
      {
         Vector3i ret = new Vector3i();
			ret.X = roundToNearestInt(loc.X / WorldParameters.theChunkSize);
			ret.Y = roundToNearestInt(loc.Y / WorldParameters.theChunkSize);
			ret.Z = roundToNearestInt(loc.Z / WorldParameters.theChunkSize);

			return ret;
      }

      public static UInt64 createKey(Vector3i chunkId)
      {
         UInt32 x = (UInt32)(chunkId.X + theBias);
         UInt32 y = (UInt32)(chunkId.Y + theBias);
         UInt32 z = (UInt32)(chunkId.Z + theBias);

         UInt64 ret = 0;
         ret |= (UInt64)(x & 0x1fffff) << (theUsableBits * 2);
         ret |= (UInt64)(y & 0x1fffff) << theUsableBits;
         ret |= (UInt64)(z & 0x1fffff);

         return ret;
      }

      public static UInt64 createKeyFromWorldLocation(Vector3 worldLocation)
      {
         Vector3i id = createIdFromWorldLocation(worldLocation);
         return createKey(id);
      }

      public static Vector3i createIdFromKey(UInt64 key)
      {
         Vector3i ret = new Vector3i();

         ret.X = (int)((key >> (theUsableBits * 2)) & 0x1fffff) - theBias;
         ret.Y = (int)((key >> theUsableBits) & 0x1fffff) - theBias;
         ret.Z = (int)(key & 0x1fffff) - theBias;

         return ret;
      }

      public static Vector3 createWorldLocationFromId(Vector3i id)
      {
         Vector3 ret = id;
         return ret * WorldParameters.theChunkSize;
      }

      public static Vector3 createWorldLocationFromKey(UInt64 key)
      {
         Vector3i id = createIdFromKey(key);
         return createWorldLocationFromId(id);
      }
      #endregion
   }
}