using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;

namespace GpuNoise
{
	public class ModuleTree
	{
		Dictionary<string, Module> myModules = new Dictionary<string, Module>();
		public Vector2i size { get; set; }

		public ModuleTree(int x, int y)
		{
         size = new Vector2(x, y);
		}

		public Dictionary<string, Module> modules { get { return myModules; } }

		public Module output { get; set; }

		public bool update(bool force = false)
		{
			return output.update(force);
		}

      public List<Module> moduleOrderedList()
      {
         List<Module> m = new List<Module>();
         output.appendList(m);
         return m;
      }

		public Module createModule(Module.Type moduleType, string name="")
		{
			Module m = null;
			switch(moduleType)
			{
				case Module.Type.AutoCorrect: m = new AutoCorrect(size.X, size.Y); break;
				case Module.Type.Bias: m = new Bias(size.X, size.Y); break;
				case Module.Type.Pow: m = new Pow(size.X, size.Y); break;
				case Module.Type.Combiner: m = new Combiner(size.X, size.Y); break;
				case Module.Type.Constant: m = new Constant(size.X, size.Y); break;
				case Module.Type.Fractal2d: m = new Fractal2d(size.X, size.Y); break;
				case Module.Type.Fractal3d: m = new Fractal3d(size.X, size.Y); break;
				case Module.Type.Gradient: m = new Gradient(size.X, size.Y); break;
				case Module.Type.Scale: m = new Scale(size.X, size.Y); break;
            case Module.Type.ScaleDomain: m = new ScaleDomain(size.X, size.Y); break;
				case Module.Type.Select: m = new Select(size.X, size.Y); break;
				case Module.Type.Translate: m = new Translate(size.X, size.Y); break;
            case Module.Type.Function: m = new Function(size.X, size.Y); break;
			}

			if(name == "")
				name = nextName(String.Format("{0}", m.myType));

			m.myName = name;

         addModule(m);

			return m;
		}

      public void addModule(Module m)
      {
         myModules.Add(m.myName, m);
      }

		public void removeModule(String name)
		{			
			myModules.Remove(name);
		}

		public Module findModule(String name)
		{
			Module m = null;
			if(myModules.TryGetValue(name, out m) == false)
         {
            throw new Exception(String.Format("Failed to find {0} in module tree", name));
         }

			return m;
		}

		public String nextName(String name)
		{
			int count = 0;
			String test = name;
			while (findModule(test) != null)
			{
				count++;
				test = String.Format("{0}_{1}", name, count);
			}

			return test;
		}

      public void clear()
      {
         myModules.Clear();
         output = null;
      }
	}
}