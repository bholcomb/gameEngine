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
   public class Translate : Module
   {
      ShaderProgram myShaderProgram;

      public Module x = null;
      public Module y = null;
      public Module source { get { return inputs[0]; } set { inputs[0] = value; } }

      public Translate(int x, int y) : base(Type.Translate, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Translate output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.translate-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update(bool force = false)
		{
			if(didChange() == true || force == true)
			{
				ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);

				cmd.addImage(x.output, TextureAccess.ReadOnly, 0);
				cmd.addImage(y.output, TextureAccess.ReadOnly, 1);
				cmd.addImage(source.output, TextureAccess.ReadOnly, 2);
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
			if (x.update()) needsUpdate = true;
			if (y.update()) needsUpdate = true;
			if (source.update()) needsUpdate = true;

			return needsUpdate;
		}

      public static Module create(ModuleTree tree, LuaObject config)
      {
         Translate m = new Translate(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         m.source = tree.findModule(config.get<String>("source"));
         m.x = tree.findModule(config.get<String>("x"));
         m.y = tree.findModule(config.get<String>("y"));

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         Translate m = mm as Translate;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.x, "x");
         obj.set(m.y, "y");
      }
   }
}