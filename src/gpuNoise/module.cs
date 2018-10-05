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
      public const int MAX_INPUTS = 5;
      public enum Type { AutoCorrect, Bias, Pow, Combiner, Constant, Fractal2d, Fractal3d, Gradient, Scale, ScaleDomain, Select, Translate, Function };

		public Type myType;
      public Module[] inputs = new Module[MAX_INPUTS];
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

      public void appendList(List<Module> list)
      {
         for(int i= 0; i< inputs.Length; i++)
         {
            if(inputs[i] != null)
            {
               inputs[i].appendList(list);
            }
         }

         list.Add(this);
      }
	}
}