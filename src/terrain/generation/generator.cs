using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;

using Util;
using GpuNoise;
using Graphics;
using Lua;

namespace Terrain
{
   public class TerrainGenerator
   {
      Initializer myInitializer;
      World myWorld;
      public HashSet<UInt64> myRequestedLocationsSet = new HashSet<UInt64>();
      Queue<UInt64> myRequestedLocations = new Queue<UInt64>();
      public ConcurrentQueue<Chunk> myGeneratedChunks = new ConcurrentQueue<Chunk>();
      public ConcurrentBag<UInt64> myForcedRebuild = new ConcurrentBag<UInt64>();
      
      LuaState myTerrainConfig;

      //ModuleTree myTerrainModules;
      Dictionary<UInt64, Region> myRegionGenerators = new Dictionary<UInt64, Region>();

      Thread myWorkerThread;
      bool myShouldQuit = false;

      public TerrainGenerator(Initializer initData, World world)
      {
         myInitializer = initData;
         myWorld = world;
         init(myInitializer);

         myWorkerThread = new Thread(tick);
         myWorkerThread.Start();
      }

      public void init(Initializer initData)
      {
         myRegionGenerators.Clear();
         string terrainFilename = initData.findDataOr("terrain.terrainDefinition", "../data/terrain/worldDefinitions/terrain.lua");

         myTerrainConfig = new LuaState();
         myTerrainConfig.doFile(terrainFilename);

         Vector3 pos = new Vector3(512 * 102.4f, 0, 512 *102.4f);
         UInt64 id = regionId(pos);
         Region region = new Region(this, id);
         region.init(myTerrainConfig["terrain"]);
         region.world = myWorld;
         myRegionGenerators.Add(id, region);
      }

      public virtual void requestChunk(UInt64 key)
      {
         lock (myRequestedLocations)
         {
            if (myRequestedLocationsSet.Contains(key) == false)
            {
               myRequestedLocationsSet.Add(key);
               myRequestedLocations.Enqueue(key);
            }
         }
      }

      public void reset()
      {
         myShouldQuit = true;
         myWorkerThread.Join(100);
         
         //clear out any pending requests
         myRequestedLocationsSet.Clear();
         myRequestedLocations.Clear();
         while (myGeneratedChunks.IsEmpty == false)
         {
            Chunk tc;
            myGeneratedChunks.TryDequeue(out tc);
         }
         while (myForcedRebuild.IsEmpty == false)
         {
            UInt64 id;
            myForcedRebuild.TryTake(out id);
         }

         //reload the heightmap and biomes
         init(myInitializer);

         myShouldQuit = false;
         myWorkerThread = new Thread(tick);
         myWorkerThread.Start();
      }

      public virtual void shutdown()
      {
         myShouldQuit = true;
      }

      public void forceRebuild(UInt64 key)
      {
         myForcedRebuild.Add(key);
      }

      public Chunk nextChunk()
      {
         Chunk next = null;
         if (myGeneratedChunks.TryDequeue(out next) == true)
         {
            myRequestedLocationsSet.Remove(next.myChunkKey.myKey);
         }

         return next;
      }

      public void tick()
      {
         while (myShouldQuit == false)
         {
            UInt64 key;
            while (myRequestedLocations.Count > 0)
            {
               lock (myRequestedLocations)
               {
                  key = myRequestedLocations.Dequeue();
               }

               Chunk c = buildChunk(key);
               myGeneratedChunks.Enqueue(c);
            }

            //forcing a rebuild of a known chunk
            while (myForcedRebuild.Count > 0)
            {
               UInt64 kkey;
               myForcedRebuild.TryTake(out kkey);
               ChunkKey ck = new ChunkKey(kkey);
               Chunk c = buildChunk(kkey);
               c.changeNumber++; //since this is a new one
               myGeneratedChunks.Enqueue(c);
            }

            Thread.Sleep(10);
         }
      }

      public UInt64 regionId(Vector3 pos)
      {
         Vector3 ret = new Vector3();
         ret.X = (float)Math.Floor(pos.X / WorldParameters.theWorldSize);
         ret.Y = 0;// (float)Math.Floor(pos.Y / WorldParameters.theWorldSize);
         ret.Z = (float)Math.Floor(pos.Z / WorldParameters.theWorldSize);

         return ChunkKey.createKeyFromWorldLocation(ret);
      }

      public Chunk buildChunk(UInt64 key)
      {
         Vector3 pos = ChunkKey.createWorldLocationFromKey(key);
         UInt64 id = regionId(pos);
         Region region;
         if (myRegionGenerators.TryGetValue(id, out region) == false)
         {
            region = new Region(this, id);
            region.init(myTerrainConfig["terrain"]);
            region.world = myWorld;
            myRegionGenerators.Add(id, region);
         }

         Chunk chunk = region.buildChunk(key);
         return chunk;
      }
   }
}
