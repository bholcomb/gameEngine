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
   public class Select : Module
   {
      ShaderProgram myShaderProgram;

      public float threshold =0.5f;
		float lastThreshold = -1.0f;
      public float falloff = 0.5f;
		float lastFalloff = -1.0f;

      public Module low;
      public Module high;
      public Module control;

      public Select(int x, int y) : base(Type.Select, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Select output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.select-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update(bool force = false)
		{
			if(didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

				cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, threshold));
				cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Float, falloff));
				
				cmd.addImage(low.output, TextureAccess.ReadOnly, 0);
				cmd.addImage(high.output, TextureAccess.ReadOnly, 1);
				cmd.addImage(control.output, TextureAccess.ReadOnly, 2);
				cmd.addImage(output, TextureAccess.WriteOnly, 3);				

				cmd.execute();
				GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

				return true;
			}

			return false;
		}

		bool didChange()
		{
			bool needsUpdate = false;
			if (low.update()) needsUpdate = true;
			if (high.update()) needsUpdate = true;
			if (control.update()) needsUpdate = true;
			if (threshold != lastFalloff || falloff != lastFalloff)
			{
				lastThreshold = threshold;
				lastFalloff = falloff;
				needsUpdate = true;
			}

			return needsUpdate;
		}
   }
}