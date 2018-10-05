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
   public class Scale : Module
   {
      ShaderProgram myShaderProgram;

      public Module source  { get { return inputs[0]; } set { inputs[0] = value; } }
      public Module scale { get { return inputs[1]; } set { inputs[1] = value; } }

      public Scale(int x, int y) : base(Type.Scale, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Scale output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.scale-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update(bool force = false)
		{
         if (didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

				cmd.addImage(source.output, TextureAccess.ReadOnly, 0);
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
			if (source.update() || scale.update()) return true;

			return false;
		}

      public static Module create(ModuleTree tree, LuaObject config)
      {
         Scale m = new Scale(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         m.scale = tree.findModule(config.get<String>("scale"));
         m.source = tree.findModule(config.get<String>("source"));

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         Scale m = mm as Scale;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.scale.myName, "scale");
         obj.set(m.source.myName, "source");
      }
   }
}