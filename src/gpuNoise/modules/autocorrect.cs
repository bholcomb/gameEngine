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

		public Module source = null;

		public AutoCorrect(int x, int y) : base(Type.AutoCorect, x, y)
		{
			output = new Texture(x, y, PixelInternalFormat.R32f);

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
			mySSbo.resize(4 * 2 * myX * myY); //2 floats, 1024 length array for each work group (32, 32)
		}

		public override void Dispose()
		{
			mySSbo.Dispose();
		}

		public override bool update()
		{
			if (source.update() == true)
			{
				findMinMax(source.output);
				correct(source.output);

				return true;
			}

			return false;
		}

		internal void findMinMax(Texture t)
		{
			if (mySSbo.sizeInBytes < 4 * 2 * t.width)
			{
				mySSbo.resize(4 * 2 * t.width);
			}

			ComputeCommand cmd = new ComputeCommand(myMinMaxShader, t.width / 32, t.height / 32);
			cmd.addImage(t, TextureAccess.ReadOnly, 0);
			cmd.renderState.setStorageBuffer(mySSbo.id, 1);
			cmd.execute();
			GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

			cmd = new ComputeCommand(myMinMaxPass2Shader, 1);
			cmd.renderState.setStorageBuffer(mySSbo.id, 1);
			cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, (t.width / 32) * (t.height / 32)));
			cmd.execute();
			GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
		}

      internal void findMinMax(Texture[] t)
      {
         //assumes all images are same size
         if (mySSbo.sizeInBytes < 4 * 2 * t[0].width)
         {
            mySSbo.resize(4 * 2 * t[0].width);
         }

         for(int i=0; i<t.Length; i++)
         {
            ComputeCommand cmd = new ComputeCommand(myMinMaxShader, t[i].width / 32, t[i].height / 32);
            cmd.addImage(t[i], TextureAccess.ReadOnly, 0);
            cmd.renderState.setStorageBuffer(mySSbo.id, 1);
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            cmd = new ComputeCommand(myMinMaxPass2Shader, 1);
            cmd.renderState.setStorageBuffer(mySSbo.id, 1);
            cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, (t[i].width / 32) * (t[i].height / 32)));
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
         }
      }

      internal void correct(Texture t)
		{
			ComputeCommand cmd = new ComputeCommand(myAutoCorrectShader, t.width / 32, t.height / 32);
			cmd.addImage(t, TextureAccess.ReadOnly, 0);
			cmd.addImage(output, TextureAccess.WriteOnly, 1);
			cmd.renderState.setStorageBuffer(mySSbo.id, 1);
			cmd.execute();
			GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
		}
   }
}