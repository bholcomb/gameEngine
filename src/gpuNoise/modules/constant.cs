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
   public class Constant : Module
   {
		public float val = 0.5f;
		float lastVal = -1.0f;

      public Constant(int x, int y) :  this(0.5f, x, y)  { }

		public Constant(float val, int x, int y) : base(Type.Constant, x, y)
		{
			output = new Texture(1, 1, PixelInternalFormat.R32f);
         output.setName("Constant output");

			this.val = val;
		}

		public override bool update(bool force = false)
		{
         if (didChange() == true || force == true)
			{
				float[] data = new float[] { val };

				output.paste(data, Vector2.Zero, Vector2.One, PixelFormat.Red);
				return true;
			}

			return false;
		}

		bool didChange()
		{
			if(lastVal != val)
			{
				lastVal = val;
				return true;
			}

			return false;
		}

      public static Module create(ModuleTree tree, LuaObject config)
      {
         Constant m = new Constant(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");
  
         m.val = config.get<float>("value");

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         Constant m = mm as Constant;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.val, "value");
      }
   }
}