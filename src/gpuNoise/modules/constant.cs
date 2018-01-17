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

		public override bool update()
		{
			if(didChange())
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
   }
}