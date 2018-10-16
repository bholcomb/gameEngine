using System;
using System.Collections.Generic;

using OpenTK;

using Util;

namespace Terrain
{
   public class TerrainPager
   {
      public const int loadedSize = 10;
      public const int elevation = 2;

      List<UInt64> myRequestedChunks = new List<UInt64>();

      Vector3i interestChunk { get; set; }

      World myWorld;
      TerrainSource myTerrainSource;
      TerrainCache myDatabase;
      Dictionary<UInt64, Chunk> myChunks;

      public Vector3 interestPoint { get; set; }

      public TerrainPager(World w)
      {
         myWorld = w;
         myTerrainSource = w.terrainSource;
         myDatabase = w.database;
         myChunks = w.chunks;
         interestChunk = new Vector3i(-10000000, -10000000, -1000000);
         interestPoint = new Vector3();
      }

      public void setInterest(Vector3 loc)
      {
         interestPoint = loc;

         Vector3i location = ChunkKey.createIdFromWorldLocation(loc);
         if (location == interestChunk)
            return;

         interestChunk = location;

         buildRequestedChunkList();
         requestMissingChunks();
         purgeExcessChunks();
      }

      public void buildRequestedChunkList()
      {
         myRequestedChunks.Clear();

         //determine all the chunks that should be in memory
         for (int x = -loadedSize; x <= loadedSize; x++)
         {
            for (int z = -loadedSize; z <= loadedSize; z++)
            {
               for(int y = interestChunk.Y; y >=0; y--)
               { 
                  Vector3i temp = interestChunk;
                  temp.X += x;
                  temp.Y = y;
                  temp.Z += z;

                  UInt64 key = ChunkKey.createKey(temp);
                  myRequestedChunks.Add(key);
               }
            }
         }

         UInt64 interestKey = ChunkKey.createKey(interestChunk);
         myRequestedChunks.Sort((x, y) => (x-interestKey).CompareTo(y-interestKey));
      }

      public void requestMissingChunks()
      {
         foreach (UInt64 key in myRequestedChunks)
         {
            if (myChunks.ContainsKey(key) == false)
               myTerrainSource.requestChunk(key);
         }
      }

      public void purgeExcessChunks()
      {
         List<UInt64> toRemove = new List<UInt64>();
         foreach (Chunk c in myChunks.Values)
         {
            if((interestPoint - c.myLocation).Length > 2000)
            {
               toRemove.Add(c.key);
            }
         }

         foreach (UInt64 key in toRemove)
         {
            myWorld.removeChunk(key);
         }
      }

      public void tick()
      {
         //get any newly awaiting chunks
         Chunk chunk = myTerrainSource.nextChunk();
         while (chunk != null)
         {
            if (myChunks.ContainsKey(chunk.key) == false)
            {
               myWorld.addChunk(chunk);
            }

            chunk = myTerrainSource.nextChunk();
         }
      }
   }
}