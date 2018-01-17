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
		int myX, myY;
		public ModuleTree(int x, int y)
		{
			myX = x;
			myY = y;

			addModule(Module.Type.Output, "output");
		}

		public Dictionary<string, Module> modules { get { return myModules; } }

		public Output final
		{
			get
			{
				return myModules["output"] as Output;
			}
		}

		public bool update()
		{
			return myModules["output"].update();
		}

		public Module addModule(Module.Type moduleType, string name="")
		{
			Module m = null;
			switch(moduleType)
			{
				case Module.Type.AutoCorect: m = new AutoCorrect(myX, myY); break;
				case Module.Type.Bias: m = new Bias(myX, myY); break;
				case Module.Type.Combiner: m = new Combiner(myX, myY); break;
				case Module.Type.Constant: m = new Constant(myX, myY); break;
				case Module.Type.Fractal: m = new Fractal(myX, myY); break;
				case Module.Type.Gradient: m = new Gradient(myX, myY); break;
				case Module.Type.Output: m = new Output(myX, myY); break;
				case Module.Type.Scale: m = new Scale(myX, myY); break;
            case Module.Type.ScaleDomain: m = new ScaleDomain(myX, myY); break;
				case Module.Type.Select: m = new Select(myX, myY); break;
				case Module.Type.Translate: m = new Translate(myX, myY); break;
			}

			if(name == "")
				name = nextName(String.Format("{0}", m.myType));

			m.myName = name;

			myModules.Add(name, m);

			return m;
		}

		public void removeModule(String name)
		{			
			myModules.Remove(name);
		}

		public Module findModule(String name)
		{
			Module m = null;
			myModules.TryGetValue(name, out m);
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
	}
}