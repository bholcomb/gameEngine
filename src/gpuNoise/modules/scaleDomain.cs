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
   public class ScaleDomain : Module
   {
      ShaderProgram myShaderProgram;

      public float x = 1.0f;
      public float y = 1.0f;

      float myLastX = 1.0f;
      float myLastY = 1.0f;

		public Module input = null;

      public ScaleDomain(int x, int y) : base(Type.ScaleDomain, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Scale Domain");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.scaleDomain-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update()
		{
			if(didChange() == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

				cmd.addImage(input.output, TextureAccess.ReadOnly, 0);
				cmd.addImage(output, TextureAccess.WriteOnly, 1);

            cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Float, x));
            cmd.renderState.setUniform(new UniformData(3, Uniform.UniformType.Float, y));

            cmd.execute();
				GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
				return true;
			}

			return false;
		}

		bool didChange()
		{
         if (input.update() ||
            myLastX != x ||
            myLastY != y)
         {
            myLastX = x;
            myLastY = y;
            return true;
         }

			return false;
		}
   }
}