using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;
using Lua;

namespace GpuNoise
{
   public static class ModuleFactory
   {
      public delegate Module CreateModule(ModuleTree tree, LuaObject config);
      public delegate void SerializeModule(Module m, LuaObject config);

      static Dictionary<Module.Type, CreateModule> myCreators = new Dictionary<Module.Type, CreateModule>();
      static Dictionary<Module.Type, SerializeModule> mySerializers = new Dictionary<Module.Type, SerializeModule>();

      static ModuleFactory()
      {
         installModuleCreators();
         installModuleSerializers();
      }

      public static ModuleTree create(LuaObject treeConfig)
      {
         int x = treeConfig.get<int>("size[1]");
         int y = treeConfig.get<int>("size[2]");

         ModuleTree tree = new ModuleTree(x, y);
         LuaObject nodes = treeConfig["nodes"];
         for(int i = 1; i <= nodes.count(); i++)
         {
            LuaObject nodeConfig = nodes[i];
            string type = nodeConfig.get<String>("type");

            CreateModule creator = null;
            Module.Type mType;
            Enum.TryParse(type, out mType);
            if (myCreators.TryGetValue(mType, out creator) == true)
            {
               Module m = creator(tree, nodeConfig);
            }
            else
            {
               throw new Exception(String.Format("Failed to find creator {0}", type));
            }
         }

         string outputName = treeConfig.get<String>("output");
         Module outputModule = tree.findModule(outputName);
         tree.output = outputModule;

         return tree;
      }

      static void installModuleCreators()
      {
         myCreators.Add(Module.Type.AutoCorrect, AutoCorrect.create);
         myCreators.Add(Module.Type.Bias, Bias.create);
         myCreators.Add(Module.Type.Pow, Pow.create);
         myCreators.Add(Module.Type.Combiner, Combiner.create);
         myCreators.Add(Module.Type.Constant, Constant.create);
         myCreators.Add(Module.Type.Fractal2d, Fractal2d.create);
         myCreators.Add(Module.Type.Fractal3d, Fractal3d.create);
         myCreators.Add(Module.Type.Gradient, Gradient.create);
         myCreators.Add(Module.Type.Scale, Scale.create);
         myCreators.Add(Module.Type.ScaleDomain, ScaleDomain.create);
         myCreators.Add(Module.Type.Select, Select.create);
         myCreators.Add(Module.Type.Translate, Translate.create);
         myCreators.Add(Module.Type.Function, Function.create);
      }

      static void installModuleSerializers()
      {
         mySerializers.Add(Module.Type.AutoCorrect, AutoCorrect.serialize);
         mySerializers.Add(Module.Type.Bias, Bias.serialize);
         mySerializers.Add(Module.Type.Pow, Pow.serialize);
         mySerializers.Add(Module.Type.Combiner, Combiner.serialize);
         mySerializers.Add(Module.Type.Constant, Constant.serialize);
         mySerializers.Add(Module.Type.Fractal2d, Fractal2d.serialize);
         mySerializers.Add(Module.Type.Fractal3d, Fractal3d.serialize);
         mySerializers.Add(Module.Type.Gradient, Gradient.serialize);
         mySerializers.Add(Module.Type.Scale, Scale.serialize);
         mySerializers.Add(Module.Type.ScaleDomain, ScaleDomain.serialize);
         mySerializers.Add(Module.Type.Select, Select.serialize);
         mySerializers.Add(Module.Type.Translate, Translate.serialize);
         mySerializers.Add(Module.Type.Function, Function.serialize);
      }

      public static void serialize(ModuleTree tree, LuaObject obj)
      {
         LuaObject size = obj.state.createTable();
         size.set(tree.size.X, 1);
         size.set(tree.size.Y, 2);
         obj["size"] = size;

         LuaObject nodes = obj.state.createTable();
         obj["nodes"] = nodes;


         int idx = 1;
         foreach(Module m in tree.moduleOrderedList())
         {
            LuaObject mobj = obj.state.createTable();
            SerializeModule ser = null;
            if (mySerializers.TryGetValue(m.myType, out ser) == true)
            {
               ser(m, mobj);
               nodes.set(mobj, idx++);
            }
            else
            {
               throw new Exception(String.Format("Failed to find creator {0}", m.myType));
            }
         }

         obj.set(tree.output.myName, "output");
      }
   }
}