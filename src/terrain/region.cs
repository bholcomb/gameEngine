using System;
using System.Collections.Generic;

using OpenTK;
using Util;
using Noise;

namespace Terrain
{
   /*
   public class Region
   {
      TerrainGenerator myGenerator;
      ModuleTree myTree;
      UInt64 myId;
      Vector3 myOrigin;
      float[] myElevation;
      float[] myMoisture;
      float[] myTemperature;
      float[] myVegetationProb;
      float[] myFeatureProb;
      float[] myWater;
      Vector4[] myBiome;
      Module myCaveGenerator;
      
      //Region consists of 
      //elevation     \
      //moisture       |-> determine biome  
      //temperature   /
      //vegetation probability map
      //feature probability map
      //cave generator

      //biome is used to determine vegitation, feature, color lookup tables

      public Region(TerrainGenerator generator, ModuleTree tree, UInt64 id)
      {
         myOrigin = ChunkKey.createWorldLocationFromKey(id);
         myGenerator = generator;
         myTree = tree;
         myId = id;
         init();
      }

      public World world { get; set; }

      public void init()
      {
         //allocate the space
         myElevation = new float[WorldParameters.theRegionSize * WorldParameters.theRegionSize];
         myMoisture = new float[WorldParameters.theRegionSize * WorldParameters.theRegionSize];
         myTemperature = new float[WorldParameters.theRegionSize * WorldParameters.theRegionSize];
         myVegetationProb = new float[WorldParameters.theRegionSize * WorldParameters.theRegionSize];
         myFeatureProb = new float[WorldParameters.theRegionSize * WorldParameters.theRegionSize];
         myWater = new float[WorldParameters.theRegionSize * WorldParameters.theRegionSize];
         myBiome = new Vector4[WorldParameters.theRegionSize * WorldParameters.theRegionSize];

         //find all the modules
         Module elevation = myTree.findModule("elevation");
         Module moisture = myTree.findModule("moisture");
         Module temperature = myTree.findModule("temperature");
         Module vegitation = myTree.findModule("vegetation");
         Module feature = myTree.findModule("feature");
         Module water = myTree.findModule("water");
         myCaveGenerator = myTree.findModule("caves");

         if (elevation == null) elevation = new Constant(myTree, 0.5);
         if (moisture == null) moisture = new Constant(myTree, 0.5);
         if (temperature == null) temperature = new Constant(myTree, 0.5);
         if (vegitation == null) vegitation = new Constant(myTree, 0);
         if (feature == null) feature = new Constant(myTree, 0.0);
         if (water == null) water = new Constant(myTree, 0.4);

         //generate the data arrays
         int regionSize = WorldParameters.theRegionSize;
         for (int y = 0; y < WorldParameters.theRegionSize ; y++)
         {
            for (int x = 0; x < WorldParameters.theRegionSize ; x++)
            {
               int offset = y * WorldParameters.theRegionSize + x;

               double xx, yy;
               xx = ((double)x / (double)WorldParameters.theRegionSize) + (myOrigin.X/(double)WorldParameters.theRegionSize);
               yy = ((double)y / (double)WorldParameters.theRegionSize) + (myOrigin.Z / (double)WorldParameters.theRegionSize);

               myElevation[offset] = (float)elevation.get(xx, yy) * WorldParameters.theMaxElevation;
               myMoisture[offset] = (float)MathExt.clamp(moisture.get(xx, yy), 0.0, 1.0);
               myTemperature[offset] = (float)MathExt.clamp(temperature.get(xx, yy), 0.0, 1.0);
               myVegetationProb[offset] = (float)vegitation.get(xx, yy);
               myFeatureProb[offset] = (float)feature.get(xx, yy);
               myWater[offset] = (float)water.get(xx, yy) * WorldParameters.theMaxElevation;
               myBiome[offset] = myGenerator.calcBiomes(myElevation[offset], myTemperature[offset], myMoisture[offset]);
            }
         }
      }

      public Chunk buildChunk(UInt64 key)
      {
         Vector3 position = ChunkKey.createWorldLocationFromKey(key);

         UInt32 air = 0;
         UInt32 water = Hash.hash("water");

         //how big a step are we taking
         int count = 128; 
         float stepSize = WorldParameters.theChunkSize / count; //with a count of 32 and the default cube of 102.4m this is a stepsize of 3.2m
         bool hasSolid = false;
         bool hasAir = false;

         //generate point cloud
         UInt32[, ,] pc = new UInt32[count, count, count];
         for (int x = 0; x < count; x++)
         {
            for (int z = 0; z < count; z++)
            {
               double nx = (position.X-myOrigin.X )+ (x * stepSize);
               double nz = (position.Z-myOrigin.Z )+ (z * stepSize);
               int offset=(int)nz * WorldParameters.theRegionSize + (int)nx;
               double elevation = myElevation[offset] * (double)WorldParameters.theMaxElevation;

               for (int y = 0; y < count; y++)
               {
                  double ny = position.Y + (y * stepSize);

                  if (ny <  myElevation[offset]) //underground
                  {
                     double xx, yy, zz;
                     xx = ((double)nx / (double)WorldParameters.theRegionSize) + (myOrigin.X / (double)WorldParameters.theRegionSize);
                     yy = ((double)ny / (double)WorldParameters.theRegionSize) + (myOrigin.Y / (double)WorldParameters.theRegionSize);
                     zz = ((double)nz / (double)WorldParameters.theRegionSize) + (myOrigin.Z / (double)WorldParameters.theRegionSize);

                     if(myCaveGenerator.get(xx, yy, zz) > 0)
                     {
                        pc[x,y,z]=air;
                        hasAir=true;
                     }
                  else
                     {
                        Vector4 biomes=myBiome[offset];
                        //just take the first one for now
                        int b=(int)biomes[0];
                        Biome biome=myGenerator.findBiome(b);
                        pc[x, y, z] = biome.mySoilProbabilities[0].id;// just the first soil value for now
                        hasSolid = true;
                     }
                  }
                  else if (ny ==  myElevation[offset])
                  {
                     Vector4 biomes = myBiome[offset];
                     //just take the first one for now
                     int b = (int)biomes[0];
                     Biome biome = myGenerator.findBiome(b);
                     pc[x, y, z] = biome.myTopProbabilities[0].id;// just the first soil value for now
                     hasSolid = true;
                  }
                  else
                  {
                     if (ny > myWater[offset])
                     {
                        pc[x, y, z] = air;
                        hasAir = true;
                     }
                     else
                     {
                        pc[x, y, z] = water;
                        hasSolid = true;
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

         return chunk;
      }
   }
    */
}