using System;
using System.Collections.Generic;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using GpuNoise;

using Util;
using Lua;

namespace WorldEditor
{
   public class Generator
   {
      ModuleTree myElevationGenerator = new ModuleTree(WorldParameters.theWorldSize, WorldParameters.theWorldSize);
      ModuleTree myHeatGenerator = new ModuleTree(WorldParameters.theWorldSize, WorldParameters.theWorldSize);
      ModuleTree myMoistureGenerator = new ModuleTree(WorldParameters.theWorldSize, WorldParameters.theWorldSize);

      public Texture myElevationMap;
      public Texture myHeatMap;
      public Texture myMoistureMap;
      public Texture myBiomeMap;

      ShaderProgram myApplyElevationShader;
      ShaderProgram myApplyHeatShader;
      ShaderProgram myGenerateBiomeShader;
      ShaderProgram myApplyMoistureShader;

      public ModuleTree elevation { get { return myElevationGenerator; } }
      public ModuleTree heat { get {return myHeatGenerator; } }
      public ModuleTree moisture { get { return myMoistureGenerator; } }

      World myWorld;

      public Generator(World world)
      {
         myWorld = world;

         myBiomeMap = new Texture(WorldParameters.theWorldSize, WorldParameters.theWorldSize, PixelInternalFormat.Rgba32f);
         myBiomeMap.setName("Biome Map");

         //setup shaders
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "World Editor.shaders.applyElevation-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myApplyElevationShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "World Editor.shaders.applyHeat-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myApplyHeatShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "World Editor.shaders.applyMoisture-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myApplyMoistureShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "World Editor.shaders.applyBiome-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myGenerateBiomeShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         //setup generators
         load("../data/terrain/worldDefinitions/terrain.lua");
      }

      void restoreDefault()
      {

         myElevationGenerator = defaultElevationGenerator();
         myHeatGenerator = defaultHeatGenerator();
         myMoistureGenerator = defaultMoistureGenerator();

         myElevationMap = myElevationGenerator.output.output;
         myHeatMap = myHeatGenerator.output.output;
         myMoistureMap = myMoistureGenerator.output.output;
      }

      ModuleTree defaultElevationGenerator()
      {
         ModuleTree generator = new ModuleTree(WorldParameters.theWorldSize, WorldParameters.theWorldSize);
         Fractal2d f = generator.createModule(Module.Type.Fractal2d, "Elevation Fractal") as Fractal2d;
         f.output.setName("fractal");
         f.seed = WorldParameters.seed;
         f.octaves = WorldParameters.theTerrainOctaves;
         f.frequency = WorldParameters.theTerrainFrequency;

         AutoCorrect ac = generator.createModule(Module.Type.AutoCorrect, "Elevation AutoCorrect") as AutoCorrect;
         ac.output.setName("Elevation AutoCorrect");
         ac.source = f;

         generator.output = ac;

         return generator;
      }

      ModuleTree defaultHeatGenerator()
      {
         ModuleTree generator = new ModuleTree(WorldParameters.theWorldSize, WorldParameters.theWorldSize);
         Fractal2d f = generator.createModule(Module.Type.Fractal2d, "Heat Fractal") as Fractal2d;
         f.output.setName("Heat Fractal");
         f.seed = WorldParameters.seed;
         f.octaves = WorldParameters.theHeatOctaves;
         f.frequency = WorldParameters.theHeatFrequency;

         AutoCorrect ac = generator.createModule(Module.Type.AutoCorrect, "Heat AutoCorrect") as AutoCorrect;
         ac.output.setName("Heat AutoCorrect");
         ac.source = f;

         Gradient sg = generator.createModule(Module.Type.Gradient, "South Gradient") as Gradient;
         sg.output.setName("Heat South gradient");
         sg.x0 = 0; sg.x1 = 1; sg.y0 = 0; sg.y1 = 1;

         Gradient ng = generator.createModule(Module.Type.Gradient, "North Gradient") as Gradient;
         ng.output.setName("Heat North gradient");
         ng.x0 = 0; ng.x1 = 1; ng.y0 = 1; ng.y1 = 0;

         //          ScaleDomain s = generator.addModule(Module.Type.ScaleDomain, "scaleDomain") as ScaleDomain;
         //          s.x = 1.0f;
         //          s.y = 0.1f;
         //          s.input = g;

         Combiner comb = generator.createModule(Module.Type.Combiner, "Heat Combiner") as Combiner;
         comb.output.setName("Heat Combiner");
         comb.inputs[0] = ac;
         comb.inputs[1] = sg;
         comb.inputs[2] = ng;
         comb.action = Combiner.CombinerType.Multiply;

         generator.output = comb;

         return generator;
      }

      ModuleTree defaultMoistureGenerator()
      {
         ModuleTree generated = new ModuleTree(WorldParameters.theWorldSize, WorldParameters.theWorldSize);
         Fractal2d f = generated.createModule(Module.Type.Fractal2d, "Moisture Fractal") as Fractal2d;
         f.seed = WorldParameters.seed;
         f.octaves = WorldParameters.theMoistureOctaves;
         f.frequency = WorldParameters.theMoistureFrequency;

         AutoCorrect ac = generated.createModule(Module.Type.AutoCorrect, "Moisture AutoCorrect") as AutoCorrect;
         ac.output.setName("Moisture AutoCorrect");
         ac.source = f;

         generated.output = ac;

         return generated;
      }

      void updateElevation()
      {
         Renderer.device.pushDebugMarker("Update Elevation");
         //update the elevation height map
         myElevationGenerator.update();
         Renderer.device.popDebugMarker();
      }

      void updateTemperature()
      {
         Renderer.device.pushDebugMarker("Update Heat");
         myHeatGenerator.update();
         Renderer.device.popDebugMarker();
      }

      void updateMoisture()
      {
         Renderer.device.pushDebugMarker("Update Moisture");

         myMoistureGenerator.update();
         Renderer.device.popDebugMarker();
      }

      void updateBiomes()
      {
         Renderer.device.pushDebugMarker("Update Biomes");

         ComputeCommand cmd;

         String m2 = "Apply Elevation";
         GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m2.Length, m2);
         cmd = new ComputeCommand(myApplyElevationShader, myBiomeMap.width / 32, myBiomeMap.height / 32);
         cmd.addImage(myElevationMap, TextureAccess.ReadOnly, 0);
         cmd.addImage(myBiomeMap, TextureAccess.WriteOnly, 1);
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
         GL.PopDebugGroup();

         String m3 = "Apply Heat";
         GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m3.Length, m3);
         cmd = new ComputeCommand(myApplyHeatShader, myBiomeMap.width / 32, myBiomeMap.height / 32);
         cmd.addImage(myHeatGenerator.output.output, TextureAccess.ReadOnly, 0);
         cmd.addImage(myBiomeMap, TextureAccess.ReadWrite, 1);
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
         GL.PopDebugGroup();

         String m4 = "Apply Moisture";
         GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m4.Length, m4);
         cmd = new ComputeCommand(myApplyMoistureShader, myBiomeMap.width / 32, myBiomeMap.height / 32);
         cmd.addImage(myMoistureGenerator.output.output, TextureAccess.ReadOnly, 0);
         cmd.addImage(myBiomeMap, TextureAccess.ReadWrite, 1);
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
         GL.PopDebugGroup();

         String m5 = "Apply Biome";
         GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m5.Length, m5);
         cmd = new ComputeCommand(myGenerateBiomeShader, myBiomeMap.width / 32, myBiomeMap.height / 32);
         cmd.addImage(myBiomeMap, TextureAccess.ReadWrite, 0);
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
         GL.PopDebugGroup();

         Renderer.device.popDebugMarker();
      }

      public void update()
      {
         updateElevation();
         updateTemperature();
         updateMoisture();
         updateBiomes();
      }

      public void load(String s)
      {
         LuaState state = new LuaState();
         state.doFile(s);

         LuaObject config = state.findObject("terrain");
         LuaObject worldConfig = config["world"];
         myWorld.mySeed = worldConfig["seed"];
         myWorld.myWidth = worldConfig.get<int>("size[1]");
         myWorld.myHeight = worldConfig.get<int>("size[2]");

         LuaObject genConfig = config["generator"];
         myElevationGenerator = ModuleFactory.create(genConfig["elevation"]);
         myHeatGenerator = ModuleFactory.create(genConfig["heat"]);
         myMoistureGenerator = ModuleFactory.create(genConfig["moisture"]);

         myElevationMap = myElevationGenerator.output.output;
         myHeatMap = myHeatGenerator.output.output;
         myMoistureMap = myMoistureGenerator.output.output;
      }

      public void save(String s)
      {
         LuaState state = new LuaState();
         state.doFile("../data/scripts/serialize.lua");

         LuaObject terrain = state.createTable();
         state.global["terrain"] = terrain;
         LuaObject world = state.createTable();
         terrain["world"] = world;
         world.set(myWorld.mySeed, "seed");
         LuaObject size = state.createTable();
         size.set(myWorld.myWidth, 1);
         size.set(myWorld.myHeight, 2);
         world.set(size, "size");

         LuaObject generator = state.createTable();
         terrain["generator"] = generator;

         LuaObject e = state.createTable();
         LuaObject h = state.createTable();
         LuaObject m = state.createTable();

         ModuleFactory.serialize(elevation, e);
         ModuleFactory.serialize(moisture, m);
         ModuleFactory.serialize(heat, h);

         generator["elevation"] = e;
         generator["heat"] = h;
         generator["moisture"] = m;


         LuaObject func = state.findObject("printTable");
         string ret = func.call(terrain, 0, "terrain");

         File.WriteAllText(s, ret);
      }
   }

}
