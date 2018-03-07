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
		float lastBias = 0.0f;
		public Module source = null;

		public Bias(int x, int y) : base(Type.Bias, x, y)
		{
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Bias output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
			shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.bias-cs.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
			myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
		}

		public override bool update(bool force = false)
		{
         if (didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, source.output.width / 32, source.output.height / 32);

				cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, bias));
				cmd.addImage(source.output, TextureAccess.ReadOnly, 0);
				cmd.addImage(output, TextureAccess.WriteOnly, 1);

				cmd.execute();
				GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

				return true;
			}

			return false;
		}

		bool didChange()
		{
			if (lastBias != bias || source.update() )
			{
				lastBias = bias;
				return true;
			}

			return false;
		}
	}
}