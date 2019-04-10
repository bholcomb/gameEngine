using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using GpuNoise;

using Util;

namespace WorldEditor
{
   public class Generator
   {
      ModuleTree myElevationGenerator = new ModuleTree(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize);
      ModuleTree myHeatGenerator = new ModuleTree(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize);
      ModuleTree myMoistureGenerator = new ModuleTree(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize);

      public CubemapTexture myElevationMap;
      public Texture myHeatMap;
      public Texture myMoistureMap;
      public CubemapTexture myBiomeMap;
      public Texture[] myElevationTex = new Texture[6];
      public Texture[] myBiomeTex = new Texture[6];
      public Fractal2d[] myElevationFractals = new Fractal2d[6];

      ShaderProgram myApplyElevationShader;
      ShaderProgram myApplyHeatShader;
      ShaderProgram myGenerateBiomeShader;
      ShaderProgram myApplyMoistureShader;

      public class ElevationInput
      {
         public int seed = WorldParameters.seed;
         public int octaves = WorldParameters.theTerrainOctaves;
         public float frequency = WorldParameters.theTerrainFrequency;
         public float offset = 0.0f;
         public float lacunarity = 2.0f;
         public float gain = 1.0f;
         public float H = 1.0f;
         public Fractal2d.Method function = Fractal2d.Method.multiFractal;
      }
      public ElevationInput elevation = new ElevationInput();

      public class HeatInput
      {
         public int seed = WorldParameters.seed;
         public int octaves = WorldParameters.theHeatOctaves;
         public float frequency = WorldParameters.theHeatFrequency;
         public float offset = 0.0f;
         public float lacunarity = 2.0f;
         public float gain = 1.0f;
         public float H = 1.0f;
         public Fractal2d.Method function = Fractal2d.Method.multiFractal;

         public float sx0 = 0;
         public float sx1 = 0;
         public float sy0 = 0;
         public float sy1 = 1;

         public float nx0 = 0;
         public float nx1 = 0;
         public float ny0 = 1;
         public float ny1 = 0;
      }
      public HeatInput heat = new HeatInput();

      public class MoistureInput
      {
         public int seed = WorldParameters.seed;
         public int octaves = WorldParameters.theMoistureOctaves;
         public float frequency = WorldParameters.theMoistureFrequency;
         public float offset = 0.0f;
         public float lacunarity = 2.0f;
         public float gain = 1.0f;
         public float H = 1.0f;
         public Fractal2d.Method function = Fractal2d.Method.multiFractal;
      }
      public MoistureInput moisture = new MoistureInput();

      public Generator()
      {
         for (int i = 0; i < 6; i++)
         {
            myElevationTex[i] = new Texture(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize, PixelInternalFormat.R32f);
            myElevationTex[i].setName(String.Format("Elevation Tex {0}", i));
            myBiomeTex[i] = new Texture(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize, PixelInternalFormat.Rgba32f);
            myBiomeTex[i].setName(String.Format("Biome Tex {0}", i));
         }

         myElevationMap = new CubemapTexture(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize, PixelInternalFormat.R32f);
         myElevationMap.setName("Elevation Cubemap");

         myBiomeMap = new CubemapTexture(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize, PixelInternalFormat.Rgba32f);
         myBiomeMap.setName("Biome Cubemap");

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
         myElevationGenerator = new ModuleTree(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize);
         myElevationFractals[0] = myElevationGenerator.addModule(Module.Type.Fractal2d, "fractal0") as Fractal2d;
         myElevationFractals[1] = myElevationGenerator.addModule(Module.Type.Fractal2d, "fractal") as Fractal2d;
         myElevationFractals[2] = myElevationGenerator.addModule(Module.Type.Fractal2d, "fractal2") as Fractal2d;
         myElevationFractals[3] = myElevationGenerator.addModule(Module.Type.Fractal2d, "fractal3") as Fractal2d;
         myElevationFractals[4] = myElevationGenerator.addModule(Module.Type.Fractal2d, "fractal4") as Fractal2d;
         myElevationFractals[5] = myElevationGenerator.addModule(Module.Type.Fractal2d, "fractal5") as Fractal2d;
         AutoCorrect Eac = myElevationGenerator.addModule(Module.Type.AutoCorrect, "autocorrect") as AutoCorrect;

         myHeatGenerator = createHeatGenerator();
         myMoistureGenerator = createMoistureGenerator();
      }

      void createElevationGenerator()
      {
         for (int i = 0; i < 6; i++)
         {
            myElevationFractals[i].face = i;
            myElevationFractals[i].seed = WorldParameters.seed;
            myElevationFractals[i].octaves = WorldParameters.theTerrainOctaves;
            myElevationFractals[i].frequency = WorldParameters.theTerrainFrequency;
         }
      }

      ModuleTree createHeatGenerator()
      {
         ModuleTree generator = new ModuleTree(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize);
         Fractal f = generator.addModule(Module.Type.Fractal, "fractal") as Fractal;
         f.output.setName("Heat Fractal");
         f.seed = WorldParameters.seed;
         f.octaves = WorldParameters.theHeatOctaves;
         f.frequency = WorldParameters.theHeatFrequency;

         AutoCorrect ac = generator.addModule(Module.Type.AutoCorrect, "autocorrect") as AutoCorrect;
         ac.output.setName("Heat Autocorrect");
         ac.source = f;

         Gradient sg = generator.addModule(Module.Type.Gradient, "south gradient") as Gradient;
         sg.output.setName("Heat South gradient");
         sg.x0 = 0; sg.x1 = 1; sg.y0 = 0; sg.y1 = 1;

         Gradient ng = generator.addModule(Module.Type.Gradient, "north gradient") as Gradient;
         ng.output.setName("Heat North gradient");
         ng.x0 = 0; ng.x1 = 1; ng.y0 = 1; ng.y1 = 0;

         //          ScaleDomain s = generator.addModule(Module.Type.ScaleDomain, "scaleDomain") as ScaleDomain;
         //          s.x = 1.0f;
         //          s.y = 0.1f;
         //          s.input = g;

         Combiner comb = generator.addModule(Module.Type.Combiner, "combiner") as Combiner;
         comb.output.setName("Heat Combiner");
         comb.inputs[0] = ac;
         comb.inputs[1] = sg;
         comb.inputs[2] = ng;
         comb.action = Combiner.CombinerType.Multiply;

         generator.final.source = comb;

         return generator;
      }

      ModuleTree createMoistureGenerator()
      {
         ModuleTree generated = new ModuleTree(WorldParameters.theWorldTextureSize, WorldParameters.theWorldTextureSize);
         Fractal f = generated.addModule(Module.Type.Fractal, "fractal") as Fractal;
         f.seed = WorldParameters.seed;
         f.octaves = WorldParameters.theMoistureOctaves;
         f.frequency = WorldParameters.theMoistureFrequency;

         AutoCorrect ac = generated.addModule(Module.Type.AutoCorect, "autocorrect") as AutoCorrect;
         ac.output.setName("Moisture Autocorrect");
         ac.source = f;

         generated.final.source = ac;

         return generated;
      }

      void updateElevation()
      {
         Renderer.device.pushDebugMarker("Update Elevation");

         AutoCorrect ac = myElevationGenerator.findModule("autocorrect") as AutoCorrect;

         ac.reset();
         
         //generate each face
         for (int i = 0; i < 6; i++)
         {
            myElevationFractals[i].output = myElevationTex[i];
            myElevationFractals[i].face = i;
            myElevationFractals[i].seed = elevation.seed;
            myElevationFractals[i].function = elevation.function;
            myElevationFractals[i].octaves = elevation.octaves;
            myElevationFractals[i].frequency = elevation.frequency;
            myElevationFractals[i].offset = elevation.offset;
            myElevationFractals[i].lacunarity = elevation.lacunarity;
            myElevationFractals[i].gain = elevation.gain;
            myElevationFractals[i].H = elevation.H;
            myElevationFractals[i].update();
         }

         //find the min/max of all the faces together
         ac.findMinMax(myElevationTex);

         //correct all the images with the global min/max
         for (int i = 0; i < 6; i++)
         {
            ac.output = myElevationTex[i];
            ac.correct(myElevationFractals[i].output);
         }

         //update the elevation height map
         myElevationMap.updateFaces(myElevationTex);
         Renderer.device.popDebugMarker();
      }

      void updateTemperature()
      {
         Renderer.device.pushDebugMarker("Update Heat");

         Fractal f = myHeatGenerator.findModule("fractal") as Fractal;
         f.seed = heat.seed;
         f.octaves = heat.octaves;
         f.frequency = heat.frequency;
         f.offset = heat.offset;
         f.lacunarity = heat.lacunarity;
         f.gain = heat.gain;
         f.H = heat.H;

         Gradient g = myHeatGenerator.findModule("south gradient") as Gradient;
         g.x0 = heat.sx0;
         g.x1 = heat.sx1;
         g.y0 = heat.sy0;
         g.y1 = heat.sy1;

         g = myHeatGenerator.findModule("north gradient") as Gradient;
         g.x0 = heat.nx0;
         g.x1 = heat.nx1;
         g.y0 = heat.ny0;
         g.y1 = heat.ny1;

         myHeatGenerator.update(true);
         Renderer.device.popDebugMarker();
      }

      void updateMoisture()
      {
         Renderer.device.pushDebugMarker("Update Moisture");
         
         Fractal f = myMoistureGenerator.findModule("fractal") as Fractal;
         f.seed = moisture.seed;
         f.octaves = moisture.octaves;
         f.frequency = moisture.frequency;
         f.offset = moisture.offset;
         f.lacunarity = moisture.lacunarity;
         f.gain = moisture.gain;
         f.H = moisture.H;

         myMoistureGenerator.update();
         Renderer.device.popDebugMarker();
      }

      void updateBiomes()
      {
         Renderer.device.pushDebugMarker("Update Biomes");
         
         for (int i = 0; i < 6; i++)
         {
            string m1 = "face " + i.ToString();
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m1.Length, m1);
            ComputeCommand cmd;
            
            String m2= "Apply Elevation";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m2.Length, m2);
            cmd = new ComputeCommand(myApplyElevationShader, myBiomeTex[i].width / 32, myBiomeTex[i].height / 32);
            cmd.addImage(myElevationTex[i], TextureAccess.ReadOnly, 0);
            cmd.addImage(myBiomeTex[i], TextureAccess.WriteOnly, 1);
            cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Int, i));
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            GL.PopDebugGroup();

            String m3 = "Apply Heat";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m3.Length, m3);
            cmd = new ComputeCommand(myApplyHeatShader, myBiomeTex[i].width / 32, myBiomeTex[i].height / 32);
            cmd.addImage(myBiomeTex[i], TextureAccess.ReadWrite, 0);
            cmd.addImage(myHeatGenerator.final.output, TextureAccess.ReadOnly, 1);
            cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Int, i));
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            GL.PopDebugGroup();

            String m4 = "Apply Moisture";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m4.Length, m4);
            cmd = new ComputeCommand(myApplyMoistureShader, myBiomeTex[i].width / 32, myBiomeTex[i].height / 32);
            cmd.addImage(myMoistureGenerator.final.output, TextureAccess.ReadOnly, 0);
            cmd.addImage(myBiomeTex[i], TextureAccess.WriteOnly, 1);
            cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Int, i));
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            GL.PopDebugGroup();

            String m5 = "Apply Biome";
            GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, m5.Length, m5);
            cmd = new ComputeCommand(myGenerateBiomeShader, myBiomeTex[i].width / 32, myBiomeTex[i].height / 32);
            cmd.addImage(myBiomeTex[i], TextureAccess.ReadWrite, 0);
            cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Int, i));
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            GL.PopDebugGroup();

            GL.PopDebugGroup();
         }

         myBiomeMap.updateFaces(myBiomeTex);
         Renderer.device.popDebugMarker();
      }

      public void update()
      {
         updateElevation();
         updateTemperature();
         updateMoisture();
         updateBiomes();
      }
   }

}
