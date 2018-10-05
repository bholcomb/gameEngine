using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;
using Lua;


namespace GpuNoise
{
	public class Fractal2d : Module
	{
		ShaderProgram myShaderProgram;

		public enum Method { fBm, multiFractal, ridgidMultiFractal, hybridMultifractal };

		public float seed = 0;

		public Method method = Method.fBm;
		public int octaves = 5;
		public float frequency = 1.0f;
		public float offset = 0.0f;
		public float lacunarity = 2.0f;
		public float gain = 1.0f;
		public float H = 1.0f;

		Method myLastFunction = Method.fBm;
		int myLastOctaves = 0;
		float myLastFrequency = 0.0f;
		float myLastOffset = 0.0f;
		float myLastLacunarity = 0.0f;
		float myLastGain = 0.0f;
		float myLastH = 0.0f;

		public Fractal2d(int x, int y) : base(Type.Fractal2d, x, y)
		{
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Fractal output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
			shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.fbm2d-cs.glsl"));
			shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.util-cs.glsl"));
			shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.noise2d-cs.glsl"));
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
				cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Int, (int)method));
				cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Int, octaves));
				cmd.renderState.setUniform(new UniformData(3, Uniform.UniformType.Float, frequency));
				cmd.renderState.setUniform(new UniformData(4, Uniform.UniformType.Float, lacunarity));
				cmd.renderState.setUniform(new UniformData(5, Uniform.UniformType.Float, H));
				cmd.renderState.setUniform(new UniformData(6, Uniform.UniformType.Float, gain));
				cmd.renderState.setUniform(new UniformData(7, Uniform.UniformType.Float, offset));
				cmd.execute();
				GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

				return true;
			}

			return false;
		}

		bool didChange()
		{
         bool diff =
            method != myLastFunction ||
            octaves != myLastOctaves ||
            frequency != myLastFrequency ||
            offset != myLastOffset ||
            lacunarity != myLastLacunarity ||
            gain != myLastGain ||
            H != myLastH;

			if (diff)
			{
				myLastFunction = method;
				myLastOctaves = octaves;
				myLastFrequency = frequency;
				myLastOffset = offset;
				myLastLacunarity = lacunarity;
				myLastGain = gain;
				myLastH = H;
			}

			return diff;
		}

      public static Module create(ModuleTree tree, LuaObject config)
      {
         Fractal2d m = new Fractal2d(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         Method func;
         Enum.TryParse(config.getOr<String>("method", "fBm"), out func);
         m.method = func;
         m.seed = config.getOr<float>("seed", 101475.0f);
         m.octaves = config.getOr<int>("octaves", 5);
         m.frequency = config.getOr<float>("frequency", 1.0f);
         m.offset = config.getOr<float>("offset", 0.0f);
         m.lacunarity = config.getOr<float>("lacunarity", 2.0f);
         m.gain = config.getOr<float>("gain", 1.0f);
         m.H = config.getOr<float>("H", 1.0f);

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         Fractal2d m = mm as Fractal2d;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.method.ToString(), "method");
         obj.set(m.octaves, "octaves");
         obj.set(m.frequency, "frequency");
         obj.set(m.offset, "offset");
         obj.set(m.lacunarity, "lacunarity");
         obj.set(m.gain, "gain");
         obj.set(m.H, "H");
      }
   }
}