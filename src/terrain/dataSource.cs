using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

using Util;

namespace Terrain
{
   public abstract class TerrainSource
   {
      protected ConcurrentQueue<Chunk> myAvaialbleChunks = new ConcurrentQueue<Chunk>();
      protected ConcurrentQueue<UInt64> myRequsetedChunks = new ConcurrentQueue<UInt64>();
      protected TerrainCache myChunkCache;
      protected HashSet<UInt64> myRequestedIds = new HashSet<UInt64>();

      protected Thread myWorkerThread;
      protected bool myShouldQuit = false;

      protected World myWorld;

      public TerrainSource(Initializer init, World world)
      {
         String path = init.findDataOr("terrain.cachePath", "../data/terrain/chunks/world.chunk");
         myChunkCache = new TerrainCache(path);
         myWorld = world;
      }

      public TerrainCache chunkCache { get { return myChunkCache; } }
      public int requestCount { get; set; }

      public abstract void tick();
      public abstract void forceRebuild(UInt64 id);
      public abstract void shutdown();
      public abstract void reset();
      public abstract void loadWorld(String worldName);
      
      public Chunk nextChunk()
      {
         Chunk tc=null;
         myAvaialbleChunks.TryDequeue(out tc);
         if (tc != null)
         {
            if (myRequestedIds.Contains(tc.key))
            {
               myRequestedIds.Remove(tc.chunkKey.myKey);
               requestCount--;
            }
         }
         return tc;
      }

      public void requestChunk(UInt64 id)
      {
         if (myRequestedIds.Contains(id) == true)
            return;

         //add it
         myRequestedIds.Add(id);
         requestCount++;

         myRequsetedChunks.Enqueue(id);
      }

      public bool checkDatabase(UInt64 id)
      {
         Chunk c = myChunkCache.findChunk(id);
         if (c != null)
         {
            c.world = myWorld;
            myAvaialbleChunks.Enqueue(c);
            return true;
         }

         return false;
      }

      public void startWorkerThread()
      {
         myWorkerThread = new Thread(tick);
         myWorkerThread.Name = "Terrain Source Worker thread";
         myWorkerThread.Start();
      }
   }

   public class LocalFileTerrainSource : TerrainSource
   {
      public LocalFileTerrainSource(Initializer init, World world)
         : base(init, world)
      {
         startWorkerThread();
      }

      public override void shutdown()
      {
         myChunkCache.shutdown();
         myShouldQuit = true;
         myWorkerThread.Join(100);
      }

      public override void reset()
      {
         //do not save the cache, since a reset should drop any changes
         //do not reset the cache, since that is loaded from a file
         myShouldQuit = true;
         myWorkerThread.Join(100);
         myChunkCache.reload();
         myShouldQuit = false;
         startWorkerThread();
      }

      public override void loadWorld(String worldName)
      {
         myChunkCache = new TerrainCache(worldName);
      }

      public override void tick()
      {
         while (myShouldQuit == false)
         {
            UInt64 id;
            while (myRequsetedChunks.TryDequeue(out id) == true)
            {
               Chunk tc = myChunkCache.findChunk(id);
               if (tc != null)
               {
                  tc.world = myWorld;
                  myAvaialbleChunks.Enqueue(tc);
               }
            }

            Thread.Sleep(10);
         }
      }

      public override void forceRebuild(ulong id)
      {
      }
   }

   public class LocalGeneratedTerrainSource : TerrainSource
   {
      TerrainGenerator myGenerator;

      public LocalGeneratedTerrainSource(Initializer init, World world)
         : base(init, world)
      {
         myGenerator = new TerrainGenerator(init, world);
         startWorkerThread();
      }

      public TerrainGenerator generator { get { return myGenerator; } }

      public override void loadWorld(string worldName)
      {
         myChunkCache = new TerrainCache(worldName);
      }

      public override void tick()
      {
         while(myShouldQuit==false)
         {
            UInt64 id;
            while (myRequsetedChunks.TryDequeue(out id) == true && myShouldQuit==false)
            {
               if(checkDatabase(id)==false)
                  myGenerator.generateChunk(id);
            }

            //clear out anything that the generator has created
            Chunk tc = myGenerator.nextChunk();
            while (tc != null && myShouldQuit==false)
            {
               myChunkCache.updateChunk(tc);
               tc.world = myWorld;
               myAvaialbleChunks.Enqueue(tc);
               tc = myGenerator.nextChunk();
            }

            Thread.Sleep(10);
         }
      }

      public override void forceRebuild(ulong id)
      {
         myGenerator.forceRebuild(id);
      }

      public override void shutdown()
      {
         myGenerator.shutdown();
         myChunkCache.shutdown();
         myShouldQuit = true;
         myWorkerThread.Join(100);
      }

      public override void reset()
      {
         myGenerator.reset();
         myChunkCache.reset();
      }
   }

   public class RemoteTerrainSource : TerrainSource
   {
      TerrainClient myClient;

      public RemoteTerrainSource(Initializer init, World world): base(init, world)
      {
         String addr=init.findDataOr("terrain.address", "127.0.0.1");
         int port=init.findDataOr("terrain.port", 2377);
         myClient = new TerrainClient(addr, port);
         startWorkerThread();
      }

      public override void loadWorld(string worldName)
      {
         throw new NotImplementedException();
      }

      public override void tick()
      {
         while(myShouldQuit==false)
         {
            UInt64 id;
            while (myRequsetedChunks.TryDequeue(out id) == true)
            {
               if (checkDatabase(id) == false)
                  myClient.requestChunk(id);
            }

            //clear out anything that the generator has created
            TerrainResponseEvent tc = myClient.nextResponse();
            while (tc != null)
            {
               Chunk chunk = myChunkCache.handleResponse(tc);
               chunk.world = myWorld;
               myAvaialbleChunks.Enqueue(chunk);
               tc = myClient.nextResponse();
            }

            Thread.Sleep(10);
         }
      }

      public override void forceRebuild(ulong id)
      {
         myClient.rebuildChunk(id);
      }

      public override void shutdown()
      {
         myClient.shutdown();
         myChunkCache.shutdown();
         myShouldQuit = true;
         myWorkerThread.Join(200);
      }

      public override void reset()
      {
         myClient.reset();
         myChunkCache.reset();
      }
   }
}