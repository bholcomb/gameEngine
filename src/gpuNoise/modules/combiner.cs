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
		const int MAX_INPUTS = 4;
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
      public Module[] inputs = new Module[MAX_INPUTS]; //max inputs

      public Combiner(int x, int y) : base(Type.Combiner, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Combiner output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.combiner-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update()
		{
			bool needsUpdate = false;
			foreach (Module m in inputs)
			{
				if (m != null && m.update() == true)
				{
					needsUpdate = true;
				}
			}

			if (needsUpdate == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

				
				cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Int, (int)action));

				int inputCount = 0;
				for (int i = 0; i < MAX_INPUTS; i++)
				{
					if (inputs[i] != null)
					{
						cmd.addImage(inputs[i].output, TextureAccess.ReadOnly, i);
						inputCount++;
					}
				}

				cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, inputCount));

				cmd.addImage(output, TextureAccess.WriteOnly, 4);

				cmd.execute();
				GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

				return true;
			}

			return false;
		}
   }
}