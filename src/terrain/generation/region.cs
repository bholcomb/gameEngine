using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Util;
using GpuNoise;
using Graphics;
using Lua;

namespace Terrain
{
   
   public class Region
   {
      TerrainGenerator myGenerator;

      UInt64 myId;
      Vector3 myOrigin;

      //Region consists of several generators
      //elevation     \
      //moisture       |-> determine biome  
      //temperature   /
      //vegetation probability map
      //feature probability map
      //cave generator

      //biome is used to determine vegitation, feature, color lookup tables


      ModuleTree myElevationGenerator;
      ModuleTree myHeatGenerator;
      ModuleTree myMoistureGenerator;
      float waterLevel = 0.4f;

      ShaderProgram myApplyElevationShader;
      ShaderProgram myApplyHeatShader;
      ShaderProgram myGenerateBiomeShader;
      ShaderProgram myApplyMoistureShader;

      Sampler2D mySampler;
      Texture myBiome;


      public Region(TerrainGenerator generator, UInt64 id)
      {
         myOrigin = ChunkKey.createWorldLocationFromKey(id);
         myGenerator = generator;
         myId = id;
         Info.print("Creating region {0}", id);
      }

      public World world { get; set; }

      public void init(LuaObject config)
      {
         myBiome = new Texture((int)WorldParameters.theRegionSize, (int)WorldParameters.theRegionSize, PixelInternalFormat.Rgba32f);
         myBiome.setName("Biome Map");
         mySampler = new Sampler2D(myBiome);

         LuaObject genConfig = config["generator"];
         myElevationGenerator = ModuleFactory.create(genConfig["elevation"]);
         myHeatGenerator = ModuleFactory.create(genConfig["heat"]);
         myMoistureGenerator = ModuleFactory.create(genConfig["moisture"]);

         //setup shaders
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "Terrain.shaders.applyElevation-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myApplyElevationShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "Terrain.shaders.applyHeat-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myApplyHeatShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "Terrain.shaders.applyMoisture-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myApplyMoistureShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "Terrain.shaders.applyBiome-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myGenerateBiomeShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         update();
      }

      public Chunk buildChunk(UInt64 key)
      {
         Vector3 position = ChunkKey.createWorldLocationFromKey(key);

         UInt32 air = 0;
         UInt32[] mats = new UInt32[12]
         {
            Hash.hash("snow"), //Ice 0
            Hash.hash("gravel"), //Tundra 1
            Hash.hash("sand"), //Desert 2
            Hash.hash("grass"), //Grassland 3
            Hash.hash("dirt"), //Savanna 4
            Hash.hash("log_spruce"), //TemperateSeasonalForest 5
            Hash.hash("log_oak"), //TropicalSeasonalForest 6
            Hash.hash("marble"), //Taiga 7
            Hash.hash("finished_oak"), //TemperateRainforest 8
            Hash.hash("finished_birch"), //TropicalRainforest 9
            Hash.hash("sandstone"), //ShallowOcean 10
            Hash.hash("bedrock") //DeepOcean 11
         };

         UInt32 water = Hash.hash("water");

         //how big a chunk are we taking
         int count = WorldParameters.theNodeCount; 
         float stepSize = WorldParameters.theChunkSize / count; //with a count of 32 and the default cube of 102.4m this is a stepsize of 3.2m
         bool hasSolid = false;
         bool hasAir = false;

         float px = position.X / WorldParameters.theWorldSize;
         float pz = position.Z / WorldParameters.theWorldSize;

         float elevation = mySampler.get(px, pz, 0) * WorldParameters.theMaxElevation;
         float biomeFloat = mySampler.get(px, pz, 3);
         UInt32 biome = BitConverter.ToUInt32(BitConverter.GetBytes(biomeFloat), 0);

         biome = biome & 0xf;


         //generate point cloud
         UInt32[, ,] pc = new UInt32[count, count, count];
         for (int x = 0; x < count; x++)
         {
            for (int z = 0; z < count; z++)
            {
               for (int y = 0; y < count; y++)
               {
                  double ny = position.Y + (y * stepSize);

                  if (ny <  elevation) //underground
                  {
                     hasSolid = true;
                     pc[x, y, z] = mats[biome];
                  }
                  else
                  {
                     if (ny < (waterLevel * WorldParameters.theMaxElevation))
                     {
                        pc[x, y, z] = water;
                        hasSolid = true;
                     }
                     else
                     {
                        pc[x, y, z] = air;
                        hasAir = true;
                     }
                  }
               }
            }
         }

         Chunk chunk = new Chunk(position);
         chunk.world = world;
         if (hasSolid == false && hasAir == true)
         {
            chunk.myRoot.materialId = air;
         }
         else
         {
            chunk.fromPointCloud(pc);
         }

         Info.print("Built chunk {0}", chunk.chunkKey.myKey);
         return chunk;
      }

      public void update()
      {
         bool elvChanged = myElevationGenerator.update();
         bool heatChanged = myHeatGenerator.update();
         bool moistChanged = myMoistureGenerator.update();
         if (elvChanged || heatChanged || moistChanged)
         {
            Info.print("Updating Biome Map");
            Renderer.device.pushDebugMarker("Update Biomes");

            ComputeCommand cmd;

            String m2 = "Apply Elevation";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m2.Length, m2);
            cmd = new ComputeCommand(myApplyElevationShader, myBiome.width / 32, myBiome.height / 32);
            cmd.addImage(myElevationGenerator.output.output, TextureAccess.ReadOnly, 0);
            cmd.addImage(myBiome, TextureAccess.WriteOnly, 1);
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            GL.PopDebugGroup();

            String m3 = "Apply Heat";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m3.Length, m3);
            cmd = new ComputeCommand(myApplyHeatShader, myBiome.width / 32, myBiome.height / 32);
            cmd.addImage(myHeatGenerator.output.output, TextureAccess.ReadOnly, 0);
            cmd.addImage(myBiome, TextureAccess.ReadWrite, 1);
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            GL.PopDebugGroup();

            String m4 = "Apply Moisture";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m4.Length, m4);
            cmd = new ComputeCommand(myApplyMoistureShader, myBiome.width / 32, myBiome.height / 32);
            cmd.addImage(myMoistureGenerator.output.output, TextureAccess.ReadOnly, 0);
            cmd.addImage(myBiome, TextureAccess.ReadWrite, 1);
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            GL.PopDebugGroup();

            String m5 = "Apply Biome";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m5.Length, m5);
            cmd = new ComputeCommand(myGenerateBiomeShader, myBiome.width / 32, myBiome.height / 32);
            cmd.addImage(myBiome, TextureAccess.ReadWrite, 0);
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            GL.PopDebugGroup();

            Renderer.device.popDebugMarker();

            mySampler.updateData();
         }
      }
   }
}
