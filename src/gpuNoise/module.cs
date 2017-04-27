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
      public enum Type {AutoCorect, Bias, Combiner, Constant, Fractal, Gradient, Scale, Select, Translate };
      public Type myType;
      public string myName;

      
      public Module(Type type)
      {

      }
   }
}