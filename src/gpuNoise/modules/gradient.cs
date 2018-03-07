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

		float lastx0 = -1.0f;
		float lastx1 = -1.0f;
		float lasty0 = -1.0f;
		float lasty1 = -1.0f;


		public Gradient(int x, int y) : base(Type.Gradient, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Gradient output");

         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.gradient-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update(bool force = false)
		{
         if (didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);
				cmd.addImage(output, TextureAccess.WriteOnly, 0);
				cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, x0));
				cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Float, x1));
				cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Float, y0));
				cmd.renderState.setUniform(new UniformData(3, Uniform.UniformType.Float, y1));
				cmd.execute();
				GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

				return true;
			}

			return false;
		}

		bool didChange()
		{
			bool diff = lastx0 != x0 ||
				lastx1 != x1 ||
				lasty0 != y0 ||
				lasty1 != y1;

			if(diff == true)
			{
				lastx0 = x0;
				lastx1 = x1;
				lasty0 = y0;
				lasty1 = y1;
			}

			return diff;
		}
   }
}