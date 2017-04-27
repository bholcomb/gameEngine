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
   public class AutoCorrect : Module
   {
      ShaderStorageBufferObject mySSbo = new ShaderStorageBufferObject(BufferUsageHint.StreamRead);
      ShaderProgram myMinMaxShader;
      ShaderProgram myMinMaxPass2Shader;
      ShaderProgram myAutoCorrectShader;
      
      public AutoCorrect() : base(Type.AutoCorect)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.minmax-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myMinMaxShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.minmax-pass2-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myMinMaxPass2Shader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.autocorrect-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myAutoCorrectShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         reset();
      }

      public void reset()
      {        
         mySSbo.resize(4 * 2 * 1024); //2 floats, 1024 length array for each work group (32, 32)
      }

      public void Dispose()
      {
         mySSbo.Dispose();
      }

      public void findMinMax(Texture t)
      {
         if(mySSbo.sizeInBytes < 4 * 2 * t.width)
         {
            mySSbo.resize(4 * 2 * t.width);
         }

         ComputeCommand cmd = new ComputeCommand(myMinMaxShader, t.width/32, t.height/32);
         cmd.addImage(t, TextureAccess.ReadOnly, 0);
         cmd.renderState.setStorageBuffer(mySSbo.id, 1);
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

         cmd = new ComputeCommand(myMinMaxPass2Shader, 1);
			cmd.renderState.setStorageBuffer(mySSbo.id, 1);
         cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, (t.width / 32) * (t.height / 32)));
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
      }

      public void correct(Texture t)
      {
         ComputeCommand cmd = new ComputeCommand(myAutoCorrectShader, t.width/32, t.height/32);
         cmd.addImage(t, TextureAccess.ReadWrite, 0);
			cmd.renderState.setStorageBuffer(mySSbo.id, 1);
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}