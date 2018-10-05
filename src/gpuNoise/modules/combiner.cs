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
   public class Combiner : Module
   {
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

      public Combiner(int x, int y) : base(Type.Combiner, x, y)
      {
			output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("Combiner output");

			List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.combiner-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

		public override bool update(bool force = false)
		{
         bool needsUpdate = false;
			foreach (Module m in inputs)
			{
				if (m != null && m.update(force) == true)
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

      public static Module create(ModuleTree tree, LuaObject config)
      {
         Combiner m = new Combiner(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         LuaObject inputs = config["inputs"];
         for(int i=1; i<=inputs.count(); i++)
         {
            m.inputs[i - 1] = tree.findModule(inputs[i]);
         }

         CombinerType action;
         Enum.TryParse(config.get<String>("action"), out action);
         m.action = action;

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         Combiner m = mm as Combiner;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.action.ToString(), "action");
         LuaObject i = obj.state.createTable();
         obj["inputs"] = i;

         for (int j = 0; j < m.inputs.Length; j++)
         {
            if (m.inputs[j] != null)
            {
               i.set(m.inputs[j].myName, j + 1);
            }
         }
      }
   }
}