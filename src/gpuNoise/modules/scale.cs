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
   public class Scale : Module
   {
      ShaderProgram myShaderProgram;

      public Module scale = null;
		public Module input = null;

      public Scale(int x, int y) : base(Type.Scale, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.scale-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update()
		{
			if(didChange() == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

				cmd.addImage(input.output, TextureAccess.ReadOnly, 0);
				cmd.addImage(scale.output, TextureAccess.ReadOnly, 1);
				cmd.addImage(output, TextureAccess.WriteOnly, 2);

				cmd.execute();
				GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
				return true;
			}

			return false;
		}

		bool didChange()
		{
			if (input.update() || scale.update()) return true;

			return false;
		}
   }
}