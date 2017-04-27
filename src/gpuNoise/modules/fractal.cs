using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;


namespace GpuNoise
{
   public class Fractal : Module
   {
      ShaderProgram myShaderProgram;

      public enum Function { fBm, multiFractal, ridgidMultiFractal, hybridMultifractal };

      public float seed = 0;
      public Function function = Function.fBm;
      public int octaves = 5;
      public float frequency = 1.0f;
      public float offset = 0.0f;
      public float lacunarity = 2.0f;
      public float gain = 1.0f;
      public float H = 1.0f;
      public int face = 0;

      public Fractal() : base(Type.Fractal)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.fbm-cs.glsl"));
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.util-cs.glsl"));
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.noise3d-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void generate(Texture t)
      {
         ComputeCommand cmd = new ComputeCommand(myShaderProgram, t.width/32, t.height/32);
         cmd.addImage(t, TextureAccess.WriteOnly, 0);
         cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, seed));
         cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Int, (int)function));
         cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Int, octaves));
         cmd.renderState.setUniform(new UniformData(3, Uniform.UniformType.Float, frequency));
         cmd.renderState.setUniform(new UniformData(4, Uniform.UniformType.Float, lacunarity));
         cmd.renderState.setUniform(new UniformData(5, Uniform.UniformType.Float, H));
         cmd.renderState.setUniform(new UniformData(6, Uniform.UniformType.Float, gain));
         cmd.renderState.setUniform(new UniformData(7, Uniform.UniformType.Float, offset));
         cmd.renderState.setUniform(new UniformData(8, Uniform.UniformType.Int, face));
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}