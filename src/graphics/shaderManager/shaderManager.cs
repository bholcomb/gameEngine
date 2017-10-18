using System;
using System.Collections.Generic;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Lua;
using Util;

namespace Graphics
{
   public class ShaderManager
   {
		List<ShaderProgram> myShaderPrograms = new List<ShaderProgram>();
      int myMaxUniformBufferBindingPoints = 0;
      int myMaxShaderStorageBufferBindingPoints = 0;
      int[] myMaxComputeWorkGroupSize = new int[3];
      int[] myMaxComputeWorkGroupCount = new int[3];
      int myMaxComputWorkGroupInvocations;
      int myMaxComputeSharedMemorySize;

      LuaState myVm;
      LuaObject myVsCompiler;
      LuaObject myPsCompiler;
      LuaObject myGsCompiler;
     
      public ShaderManager()
      {
         GL.GetInteger(GetPName.MaxUniformBufferBindings, out myMaxUniformBufferBindingPoints);
         GL.GetInteger((GetPName)All.MaxShaderStorageBufferBindings, out myMaxShaderStorageBufferBindingPoints);
         myVm = new LuaState();
         myVm.printCallback = new LuaState.PrintCallback(Debug.print);

         GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupSize, 0, out myMaxComputeWorkGroupSize[0]);
         GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupSize, 1, out myMaxComputeWorkGroupSize[1]);
         GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupSize, 2, out myMaxComputeWorkGroupSize[2]);
         GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out myMaxComputeWorkGroupCount[0]);
         GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupCount, 1, out myMaxComputeWorkGroupCount[1]);
         GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupCount, 2, out myMaxComputeWorkGroupCount[2]);
         GL.GetInteger((GetPName)All.MaxComputeWorkGroupInvocations, out myMaxComputWorkGroupInvocations);
         GL.GetInteger((GetPName)All.MaxComputeSharedMemorySize, out myMaxComputeSharedMemorySize);
         Info.print("Max work group size: {0}, {1}, {2}", myMaxComputeWorkGroupSize[0], myMaxComputeWorkGroupSize[1], myMaxComputeWorkGroupSize[2]);
         Info.print("Max work group count: {0}, {1}, {2}", myMaxComputeWorkGroupCount[0], myMaxComputeWorkGroupCount[1], myMaxComputeWorkGroupCount[2]);
         Info.print("Max work group invocation {0}", myMaxComputWorkGroupInvocations);
         Info.print("Max compute shared memory size {0}", myMaxComputeSharedMemorySize);
      }

      public void init(InitTable init)
      {
         String path = (String)init.findDataOr("shaderComponents", "../data/shaders");

         //read in the scripts to support the entity system
         //may need to move this to a resource
         myVm.doFile(path + "/shaderCompiler.lua");

         //read in all the entity files
         foreach (String file in Directory.GetFiles(path))
         {
            if (file.Contains(".comp") == true)
            {
               Debug.print("Shader compiler evaluating component file: {0}", file);
               myVm.doFile(file);
            }
         }

         myVsCompiler = myVm.findObject("compileVertexShader");
         myGsCompiler = myVm.findObject("compileGeometryShader");
         myPsCompiler = myVm.findObject("compilePixelShader");

         //createFileWatcher(path);
      }

		public void reloadShaders()
		{
			foreach(ShaderProgram sp in myShaderPrograms)
			{
				sp.reload();
			}
		}

      public ShaderProgram createShader(List<ShaderDescriptor> descriptors, List<String> defines, String objName)
      {
         List<Shader> shaders = new List<Shader>();

         foreach (ShaderDescriptor sd in descriptors)
         {
            Shader s = new Shader();
            switch (sd.mySource)
            {
               case ShaderDescriptor.Source.File: s.compileShaderFile(sd.myType, sd.myText, defines); break;
               case ShaderDescriptor.Source.String: s.compileShaderText(sd.myType, sd.myText, defines); break;
               case ShaderDescriptor.Source.Resource: s.compileShaderResource(sd.myType, sd.myText, defines); break;
            }

				if (sd.myName != "")
					s.setName(sd.myName);

				shaders.Add(s);
			}


         ShaderProgram sp = new ShaderProgram(shaders);

         if (objName != "")
            sp.setName(objName);


			myShaderPrograms.Add(sp);
         return sp;
      }

      public ShaderProgram composeShader(List<String> components, List<String> defines)
      {
         String shaderName = String.Format("{0}-{1}", Formatter.stringListHashCode(components), Formatter.stringListHashCode(defines));

         LuaObject compTable = myVm.createTable();
         for (int i = 0; i < components.Count; i++)
         {
            compTable.set<String>(components[i], i);
         }
         string vsSource = myVsCompiler.call(compTable);
         string gsSource = myGsCompiler.call(compTable);
         string psSource = myPsCompiler.call(compTable);

         if (Directory.Exists("..//data//shaders//cache") == false)
         {
            Directory.CreateDirectory("..//data//shaders//cache");
         }

         File.WriteAllText("..//data//shaders//cache//" + shaderName + ".vs.glsl", vsSource);
         File.WriteAllText("..//data//shaders//cache//" + shaderName + ".ps.glsl", psSource);

         if (gsSource != "")
            File.WriteAllText("..//data//shaders//cache//" + shaderName + ".gs.glsl", gsSource);

         Shader vs = new Shader();
         Shader ps = new Shader();
         Shader gs = gsSource == "" ? null : new Shader();

         vs.compileShaderText(ShaderType.VertexShader, vsSource, defines);
         ps.compileShaderText(ShaderType.FragmentShader, psSource, defines);

         List<Shader> shaders = new List<Shader>();
         shaders.Add(vs);
         shaders.Add(ps);

         if (gs != null)
         {
            gs.compileShaderText(ShaderType.GeometryShader, gsSource, defines);
            shaders.Add(gs);
         }

         ShaderProgram sp = new ShaderProgram(shaders);
			myShaderPrograms.Add(sp);
			return sp;
      }
   };
}