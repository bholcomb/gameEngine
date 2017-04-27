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
using Noise;
using Graphics;

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
      
      Thread myWorkerThread;
      bool myShouldQuit = false;

      //ModuleTree myTerrainModules;
      //Dictionary<UInt64, Region> myRegionGenerators = new Dictionary<UInt64, Region>();
      List<Biome> myBiomes = new List<Biome>();

      UInt32 dirt;
      UInt32 grass;
      UInt32 air = 0;

      Texture map;

      public TerrainGenerator(Initializer initData, World world)
      {
         myInitializer = initData;
         myWorld = world;
         init(myInitializer);
         
         myWorkerThread = new Thread(workerTask);
         myWorkerThread.Name = "Terrain Generation worker thread";
         myWorkerThread.Start();
      }

      public void init(Initializer initData)
      {
         //myRegionGenerators.Clear();
         string terrainFilename = initData.findDataOrDefault("terrain.terrainDefinition", "../data/terrain.json");
         string biomeFilename = initData.findDataOrDefault("terrain.biomeDefinition", "../data/biome.json");
         //myTerrainModules = ModuleFactory.loadDefinition(terrainFilename);
         JsonObject biomeData=JsonObject.loadFile(biomeFilename);
         foreach(String bdname in biomeData.keys)
         {
            Biome b = new Biome(biomeData[bdname]);
            myBiomes.Add(b);
         }
      }

      public virtual void generateChunk(UInt64 key)
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
         //join the worker thread
         myShouldQuit = true;
         myWorkerThread.Join(100);

         //clear out any pending requests
         myRequestedLocationsSet.Clear();
         myRequestedLocations.Clear();
         while(myGeneratedChunks.IsEmpty==false)
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

         //restart the worker thread
         myShouldQuit = false;
         myWorkerThread = new Thread(workerTask);
         myWorkerThread.Name = "Terrain Generation worker thread";
         myWorkerThread.Start();
      }

      public Biome findBiome(int index)
      {
         if (index >= myBiomes.Count)
         {
            return null;
         }

         return myBiomes[index];
      }

      public Vector4 calcBiomes(double elevation, double temperature, double moisture)
      {
         Vector4 ret = new Vector4(-1);

         for (int b = 0; b < myBiomes.Count; b++)
         {
            Biome biome = myBiomes[b];
            if (temperature >= biome.temperatureRange.min && temperature <= biome.temperatureRange.max &&
               moisture >= biome.moistureRange.min && moisture <= biome.moistureRange.max)
            {
               for (int i = 0; i < 4; i++)
               {
                  if (ret[i] == -1)
                  {
                     ret[i] = b;
                     break;
                  }
               }
            }
         }

         if (ret == new Vector4(-1))
            throw new Exception("Unknown moisture/temperature for biome selection");
         return ret;
      }

      public virtual void tick(double t)
      {

      }

      public void forceRebuild(UInt64 key)
      {
         myForcedRebuild.Add(key);
      }

      public Chunk nextChunk()
      {
         Chunk next=null;
         if (myGeneratedChunks.TryDequeue(out next)==true)
         {
            myRequestedLocationsSet.Remove(next.myChunkKey.myKey);
         }

         return next;
      }

      public void workerTask()
      {
         while (myShouldQuit == false)
         {
            UInt64 key;
            while (myRequestedLocations.Count > 0 && myShouldQuit==false)
            {
               lock (myRequestedLocations)
               {
                  key = myRequestedLocations.Dequeue();
               }

               Chunk c = buildChunk(key);
               myGeneratedChunks.Enqueue(c);
            }

            //forcing a rebuild of a known chunk
            while (myForcedRebuild.Count > 0 && myShouldQuit == false)
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

      public void shutdown()
      {
         myShouldQuit = true;
         myWorkerThread.Join(100);
      }

      public UInt64 regionId(Vector3 pos)
      {
         Vector3 ret = new Vector3();
         ret.X = (float)Math.Floor(pos.X / WorldParameters.theRegionSize);
         ret.Y = (float)Math.Floor(pos.Y / WorldParameters.theRegionSize);
         ret.Z = (float)Math.Floor(pos.Z / WorldParameters.theRegionSize);

         ret=ret * WorldParameters.theRegionSize;
         return ChunkKey.createKeyFromWorldLocation(ret);
      }

      public Chunk buildChunk(UInt64 key)
      {
         Vector3 pos = ChunkKey.createWorldLocationFromKey(key);
         UInt64 id = regionId(pos);
//          Region region;
//          if (myRegionGenerators.TryGetValue(id, out region) == false)
//          {
//             region = new Region(this, myTerrainModules, id);
//             region.world = myWorld;
//             myRegionGenerators.Add(id, region);
//          }
// 
//          Chunk chunk = region.buildChunk(key);
//         return chunk;
         return null;
      }
   }
}