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
   public class Bias : Module
   {
      ShaderProgram myShaderProgram;

      public float bias = 0.5f;

      public Bias() : base(Type.Bias)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.bias-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void generate(Texture tex)
      {
         ComputeCommand cmd = new ComputeCommand(myShaderProgram, tex.width / 32, tex.height / 32);
         
			cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, bias));
         cmd.addImage(tex, TextureAccess.ReadWrite, 0);

         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}