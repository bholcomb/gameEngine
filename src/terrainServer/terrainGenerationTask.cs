using System;
using System.Collections.Generic;

using Engine;
using Terrain;
using Util;
using Events;

namespace TerrainServer
{
   public class TerrainGenerationTask : Task
   {
      TerrainCache myCache;
      TerrainGenerator myGenerator;
      World myWorld;

      public TerrainGenerationTask(Initializer init)
         : base("Terrain Generation")
      {
         myWorld = new World(init);
         myCache = myWorld.database;

         //use a null world for generation
         myGenerator = (myWorld.terrainSource as LocalGeneratedTerrainSource).generator;

         MaterialManager.init();

         Application.eventManager.addListener(handleTerrainRequest, "terrain.chunk.request");
         Application.eventManager.addListener(handleTerrainRebuild, "terrain.chunk.rebuild");
         Application.eventManager.addListener(handleTerrainReset, "terrain.reset");

         frequency = 10;
      }

      public void shutdown()
      {
         myWorld.shutdown();
      }

      protected override void onUpdate(double dt)
      {
         Chunk chunk = myWorld.terrainSource.nextChunk();
         while (chunk != null)
         {
            //force the update of the cache in this thread;
            myCache.updateChunk(chunk);

            //now tell whoever triggered this to be built that they can have it
            distributeChunk(chunk.key);
            chunk = myGenerator.nextChunk();
         }
      }

      public EventManager.EventResult handleTerrainRequest(Event e)
      {
         TerrainRequestEvent tr = e as TerrainRequestEvent;
         if (tr != null)
         {
            UInt64 id = tr.chunkId;
            if (myCache.containsChunk(id) == false)
            {
               myGenerator.requestChunk(id);
            }
            else
            {
               distributeChunk(id);
            }

            return EventManager.EventResult.HANDLED;
         }

         return EventManager.EventResult.IGNORED;
      }

      public EventManager.EventResult handleTerrainRebuild(Event e)
      {
         TerrainRebuildEvent te = e as TerrainRebuildEvent;
         if (te != null)
         {
            UInt64 id = te.chunkId;
            myGenerator.forceRebuild(id);
            return EventManager.EventResult.HANDLED;
         }

         return EventManager.EventResult.IGNORED;
      }

      public EventManager.EventResult handleTerrainReset(Event e)
      {
         myWorld.reset();
         return EventManager.EventResult.HANDLED;
      }

      public void distributeChunk(UInt64 id)
      {
         //create a response message
         List<byte> data = new List<byte>(myCache.compressedChunk(id));
         TerrainResponseEvent terrainResponse = new TerrainResponseEvent(id, data);
         Application.eventManager.queueEvent(terrainResponse);
      }
   }
}