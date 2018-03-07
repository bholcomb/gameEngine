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

		Function myLastFunction = Function.fBm;
		int myLastOctaves = 0;
		float myLastFrequency = 0.0f;
		float myLastOffset = 0.0f;
		float myLastLacunarity = 0.0f;
		float myLastGain = 0.0f;
		float myLastH = 0.0f;
		float myLastFace = 0;

		public Fractal(int x, int y) : base(Type.Fractal, x, y)
		{
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Fractal output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
			shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.fbm-cs.glsl"));
			shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.util-cs.glsl"));
			shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.noise3d-cs.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
			myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
		}

		public override bool update(bool force = false)
		{
         if (didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);
				cmd.addImage(output, TextureAccess.WriteOnly, 0);
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

				return true;
			}

			return false;
		}

		bool didChange()
		{
			bool diff =
				function != myLastFunction ||
				octaves != myLastOctaves ||
				frequency != myLastFrequency ||
				offset != myLastOffset ||
				lacunarity != myLastLacunarity ||
				gain != myLastGain ||
				H != myLastH ||
				face != myLastFace;

			if (diff)
			{
				myLastFunction = function;
				myLastOctaves = octaves;
				myLastFrequency = frequency;
				myLastOffset = offset;
				myLastLacunarity = lacunarity;
				myLastGain = gain;
				myLastH = H;
				myLastFace = face;
			}

			return diff;
		}
	}
}