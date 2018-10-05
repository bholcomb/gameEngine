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
	public class Function : Module
	{
		ShaderProgram myShaderProgram;

      String myFunction = "";
		public string function { get { return myFunction; } set { myFunction = value; updateShader(); } }
		public Module source { get { return inputs[0]; } set { inputs[0] = value; } }

      string shader = "float function(float x) { FUNC; return x; }";

		public Function(int x, int y) : base(Type.Function, x, y)
		{
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Function output");
		}

      public void updateShader()
      {
         String src = shader.Replace("FUNC", myFunction);
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, src, ShaderDescriptor.Source.String));
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.function-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update(bool force = false)
		{
         if (didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, source.output.width / 32, source.output.height / 32);

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
			return false;
		}

      public static Module create(ModuleTree tree, LuaObject config)
      {
         Function m = new Function(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         m.source = tree.findModule(config.get<String>("source"));
         m.function = config.get<string>("function");

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         Function m = mm as Function;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.source.myName, "source");
         obj.set(m.function, "function");
      }
   }
}