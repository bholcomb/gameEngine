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
   public class Combiner : Module
   {
      ShaderProgram myShaderProgram;
      public enum CombinerType
      {
         Add,
         Multiply,
         Max,
         Min,
         Average
      }

      public CombinerType action;
      public List<Texture> inputs = new List<Texture>();
      public Texture output;
      public Combiner() : base(Type.Combiner)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.combiner-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void combine()
      {
         ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

			cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, inputs.Count));
         cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Int, (int)action));
         
         for(int i = 0; i < inputs.Count; i++)
         {
            cmd.addImage(inputs[i], TextureAccess.ReadOnly, i);
         }

         cmd.addImage(output, TextureAccess.WriteOnly, 8);

         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}