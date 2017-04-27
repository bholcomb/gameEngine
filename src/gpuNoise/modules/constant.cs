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
   public class Constant : Module
   {
      ShaderProgram myShaderProgram;

      public float val = 0.5f;

      public Constant() : base(Type.Constant)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.constant-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void generate(Texture output)
      {
         ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

			cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, val));
         
         cmd.addImage(output, TextureAccess.ReadWrite, 0);

         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}