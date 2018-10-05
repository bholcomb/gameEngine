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
   public class Select : Module
   {
      ShaderProgram myShaderProgram;

      public float threshold =0.5f;
		float lastThreshold = -1.0f;
      public float falloff = 0.5f;
		float lastFalloff = -1.0f;

      public Module low { get { return inputs[0]; } set { inputs[0] = value; } }
      public Module high { get { return inputs[1]; } set { inputs[1] = value; } }
      public Module control { get { return inputs[2]; } set { inputs[2] = value; } }

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

      public static Module create(ModuleTree tree, LuaObject config)
      {
         Select m = new Select(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         m.low = tree.findModule(config.get<String>("low"));
         m.high = tree.findModule(config.get<String>("high"));
         m.control = tree.findModule(config.get<String>("control"));

         m.threshold = config.get<float>("threshold");
         m.falloff = config.get<float>("falloff");

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         Select m = mm as Select; 
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.low.myName, "low");
         obj.set(m.high.myName, "high");
         obj.set(m.control.myName, "control");
         obj.set(m.threshold, "threhsold");
         obj.set(m.falloff, "falloff");

      }
   }
}
