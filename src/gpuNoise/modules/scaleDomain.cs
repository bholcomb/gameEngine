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
   public class ScaleDomain : Module
   {
      ShaderProgram myShaderProgram;

      public float x = 1.0f;
      public float y = 1.0f;

      float myLastX = 1.0f;
      float myLastY = 1.0f;

		public Module source { get { return inputs[0]; } set { inputs[0] = value; } }

      public ScaleDomain(int x, int y) : base(Type.ScaleDomain, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Scale Domain");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.scaleDomain-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update(bool force = false)
		{
         if (didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

				cmd.addImage(source.output, TextureAccess.ReadOnly, 0);
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
         if (source.update() ||
            myLastX != x ||
            myLastY != y)
         {
            myLastX = x;
            myLastY = y;
            return true;
         }

			return false;
		}

      public static Module create(ModuleTree tree, LuaObject config)
      {
         ScaleDomain m = new ScaleDomain(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         m.source = tree.findModule(config.get<String>("source"));
         m.x = config.get<float>("x");
         m.y = config.get<float>("y");

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         ScaleDomain m = mm as ScaleDomain;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.x, "x");
         obj.set(m.y, "y");
      }
   }
}