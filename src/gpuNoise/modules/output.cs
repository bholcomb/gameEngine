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
   public class Output : Module
   {
		public Module source = null;

		public Output(int x, int y) : base(Type.Output, x, y)
      {
      }

		public override bool update()
		{
			if(source.update() == true)
			{
				output = source.output;
				return true;
			}

			return false;
		}
	}
}