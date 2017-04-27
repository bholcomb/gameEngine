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
   public class Gradient : Module
   {
      ShaderProgram myShaderProgram;

      public float x0 = 0.0f;
      public float x1 = 1.0f;
      public float y0 = 0.0f;
      public float y1 = 1.0f;

      public Gradient() : base(Type.Gradient)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.gradient-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void generate(Texture t)
      {
         ComputeCommand cmd = new ComputeCommand(myShaderProgram, t.width / 32, t.height / 32);
         cmd.addImage(t, TextureAccess.WriteOnly, 0);
         cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, x0));
         cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Float, x1));
         cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Float, y0));
         cmd.renderState.setUniform(new UniformData(3, Uniform.UniformType.Float, y1));
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}