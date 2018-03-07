using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;


namespace GpuNoise
{
   public abstract class Module
   {
      public enum Type {Output, AutoCorect, Bias, Combiner, Constant, Fractal, Gradient, Scale, ScaleDomain, Select, Translate };

		public Type myType;
		public Texture output;
      public string myName;
		public int myX;
		public int myY;

		public Module(Type type, int x, int y)
      {
			myType = type;
			myX = x;
			myY = y;	
      }

		//returns true if its data changed, allows connected modules to not have to rerun
		public abstract bool update(bool force = false);

		public virtual void Dispose()
		{
			output.Dispose();
		}
	}
}