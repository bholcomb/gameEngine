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
	public class Pow : Module
	{
		ShaderProgram myShaderProgram;

		public float pow = 0.5f;
		float lastPow = 0.0f;

		public Module source { get { return inputs[0]; } set { inputs[0] = value; } }

      public Pow(int x, int y) : base(Type.Pow, x, y)
		{
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Pow output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
			shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.pow-cs.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
			myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
		}

		public override bool update(bool force = false)
		{
         if (didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, source.output.width / 32, source.output.height / 32);

				cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, pow));
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
			if (lastPow != pow || source.update() )
			{
				lastPow = pow;
				return true;
			}

			return false;
		}

      public static Module create(ModuleTree tree, LuaObject config)
      {
         Pow m = new Pow(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         m.source = tree.findModule(config.get<String>("source"));
         m.pow = config.get<float>("pow");

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         Pow m = mm as Pow;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.pow, "pow");
      }
   }
}