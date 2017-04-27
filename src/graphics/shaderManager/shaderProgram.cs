using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class ShaderDescriptor
   {
      public enum Source { Resource, File, String }
      public ShaderType myType;
      public Source mySource;
      public String myText;
      public String myName = "";

      public ShaderDescriptor(ShaderType t, String txt, Source s = Source.Resource, String name = "")
      {
         myType = t;
         myText = txt;
         mySource = s;
         myName = name;
      }

      public override int GetHashCode()
      {
         return (int)Hash.hash(myText);
      }
   }

   public class Shader : IDisposable
   {
      ShaderType myType = ShaderType.VertexShader;
      int myId = -1;
      bool myIsCompiled = false;
		string myFilename;
		List<String> myDefines;

		public Shader()
      {
#if DEBUG
         errorReporting = true;
#else 
         errorReporting=false;
#endif
      }

      public static bool errorReporting { get; set; }

      public void Dispose()
      {
         GL.DeleteShader(myId);
      }

      public bool compileShaderResource(ShaderType target, String resName, List<String> defines)
      {
         //check for embedded file
         if (Util.hasEmbeddedResource(resName) == true)
         {
            Info.print("Compiling {0}: {1}", target, resName);
            return compileShaderText(target, Util.getString(resName), defines);
         }

         //can't find it
         return false;
      }

      public bool compileShaderFile(ShaderType target, String filename, List<String> defines)
      {
         //check the disk
         if (File.Exists(filename) == true)
         {
				Info.print("Compiling {0}: {1}", target, filename);
				myFilename = filename;
				myDefines = defines;
            StreamReader streamReader = new StreamReader(filename);
            string text = streamReader.ReadToEnd();
            streamReader.Close();

            return compileShaderText(target, text, defines);
         }

			//can't find it
			Warn.print("Cannot find {0} shader {1}", target, filename);
			return false;
      }

      public bool compileShaderText(ShaderType target, String text, List<String> defines)
      {
         myType = target;
         myIsCompiled = false;
			if(myId == -1)
				myId = GL.CreateShader(myType);

         if (myId == 0)
            return myIsCompiled;

         //prepend the defines to the shader
         if (defines != null)
         {
            String prepend = "";
            foreach (String s in defines)
            {
               prepend += s + "\n";
            }
            text = prepend + "\n" + text;
         }
         GL.ShaderSource(myId, text);
         GL.CompileShader(myId);

         // check if shader compiled
         int status = 0;
         GL.GetShader(myId, ShaderParameter.CompileStatus, out status);
         if (status != 1)
            myIsCompiled = false;
         else
            myIsCompiled = true;

         if (myIsCompiled == false)
         {
            if (errorReporting == true)
            {
               String error;
               GL.GetShaderInfoLog(myId, out error);
					Warn.print("Shader (type: {0}) compile failed: \n{1}", target, error);
            }

            GL.DeleteShader(myId);
				myId = -1;
            return false;
         }
         return myIsCompiled;
      }

		public void reload()
		{
			if (myFilename == null)
				return;

			compileShaderFile(myType, myFilename, myDefines);
		}

      public bool compiled
      {
         get { return myIsCompiled; }
      }

      public int id
      {
         get { return myId; }
      }

      public void setName(String name)
      {
         GL.ObjectLabel(ObjectLabelIdentifier.Shader, myId, name.Length, name);
      }
   }


   public class ShaderProgramDescriptor : ResourceDescriptor
   {
      List<ShaderDescriptor> myShaderDesc = new List<ShaderDescriptor>();
      List<string> myDefines = null;
      String myObjName = "";
      

      public ShaderProgramDescriptor(string vsName, string psName, List<string> defines = null, String objName="")
         : base("")
      {
         myShaderDesc.Add(new ShaderDescriptor(ShaderType.VertexShader, vsName));
         myShaderDesc.Add(new ShaderDescriptor(ShaderType.FragmentShader, psName));
         myDefines = defines;
         myObjName = objName;
         name = generateName();
      }

      public ShaderProgramDescriptor(string vsName, string gsName, string psName, List<string> defines = null, String objName = "")
         : base("")
      {
         myShaderDesc.Add(new ShaderDescriptor(ShaderType.VertexShader, vsName));
         myShaderDesc.Add(new ShaderDescriptor(ShaderType.GeometryShader, gsName));
         myShaderDesc.Add(new ShaderDescriptor(ShaderType.FragmentShader, psName));
         myDefines = defines;
         myObjName = objName;
         name = generateName();
      }

      public ShaderProgramDescriptor(List<ShaderDescriptor> shaders, List<string> defines = null, String objName = "")
         : base("")
      {
         myShaderDesc = shaders;
         myDefines = defines;
         myObjName = objName;
         name = generateName();
      }

      public String generateName()
      {
         String name = "";
         foreach (ShaderDescriptor sd in myShaderDesc)
         {
            if (sd.myName == "")
               name += sd.GetHashCode().ToString();
            else
               name += sd.myName;

            if(sd != myShaderDesc[myShaderDesc.Count -1])
            {
               name += "-";
            }
         }

         return name;
      }

      public override IResource create(ResourceManager mgr)
      {
         ShaderProgram sp = Renderer.shaderManager.createShader(myShaderDesc, myDefines, myObjName);
         return sp;
      }
   }

   public class ShaderProgram : IResource
   {
      int myId = -1;
      bool myIsLinked = false;
      List<Shader> myShaders = new List<Shader>();

      Dictionary<string, AttributeInfo> myVertexBindings = new Dictionary<string, AttributeInfo>();
      Dictionary<string, UniformInfo> myUniformInfo = new Dictionary<string, UniformInfo>();
      Dictionary<string, int> myUniformBlockIndexes = new Dictionary<string, int>();
      Dictionary<string, int> myShaderStorageBlockIndexes = new Dictionary<string, int>();
		List<Uniform> myUniforms = new List<Uniform>();
		
      public Dictionary<string, AttributeInfo> vertexBindings { get { return myVertexBindings; } }
      public Dictionary<string, UniformInfo> UniformInfo { get { return myUniformInfo; } }
      public Dictionary<string, int> uniformBlockIndexes { get { return myUniformBlockIndexes; } }
      public Dictionary<string, int> shaderStorageBlockIndexes { get { return myShaderStorageBlockIndexes; } }
      public static bool errorReporting { get; set; }
      public bool isLinked { get { return myIsLinked; } }
      public bool hasUniformBlocks { get { return myUniformBlockIndexes.Count > 0; } }
      public bool hasShaderStorageBlocks { get { return myShaderStorageBlockIndexes.Count > 0; } }

      public ShaderProgram(List<Shader> shaders)
      {
         myShaders = shaders;

#if DEBUG
         errorReporting = true;
#else 
         errorReporting=false;
#endif

         foreach(Shader s in myShaders)
         {
            if(s.compiled == false)
            {
               String err = String.Format("Tried to use uncompiled shader {0}", s.id);
               Warn.print(err);
            }
         }

         linkShaders(myShaders);
      }

      public void Dispose()
      {
         //detach shaders
         foreach(Shader s in myShaders)
         {
            GL.DetachShader(myId, s.id);
         }
         
         //delete program
         GL.DeleteProgram(myId);
      }

      public bool bind()
      {
         if (myIsLinked == false)
         {
            return false;
         }

         GL.UseProgram(myId);

         return true;
      }

      public void unbind()
      {
         GL.UseProgram(0);
      }

      public int id
      {
         get { return myId; }
      }

      public void setName(String name)
      {
         GL.ObjectLabel(ObjectLabelIdentifier.Program, myId, name.Length, name);
      }

		public void reload()
		{
			myVertexBindings.Clear();
			myUniformBlockIndexes.Clear();
			myShaderStorageBlockIndexes.Clear();
			myUniformInfo.Clear();
			myUniforms.Clear();
			
			foreach (Shader shader in myShaders)
			{
				GL.DetachShader(myId, shader.id);
			}

			foreach (Shader s in myShaders)
			{
				s.reload();
				if (s.compiled == false)
				{
					String err = String.Format("Tried to use uncompiled shader {0}", s.id);
					Warn.print(err);
					return;
				}
			}

			linkShaders(myShaders);
		}

      protected bool linkShaders(List<Shader> shaders)
      {
         myIsLinked = false;
         int program = GL.CreateProgram();
         foreach(Shader shader in shaders)
         {
            GL.AttachShader(program, shader.id);
         }
         GL.LinkProgram(program);

         if (errorReporting == true)
         {
            // Get error log.
            String error;
            GL.GetProgramInfoLog(program, out error);
            if (error != "")
            {
               System.Console.WriteLine("Shader Program link error: {0}", error);
            }
         }

         // Test linker result.
         int linkSucceed = 0;
         GL.GetProgram(program, GetProgramParameterName.LinkStatus, out linkSucceed);

         if (linkSucceed == 0)
         {
            GL.DeleteProgram(program);
            return false;
         }

         myIsLinked = true;
         myId = program;
         findAttributes();
         findUniforms();
         createUniforms();
         return myIsLinked;
      }

      void findAttributes()
      {
         int max;
         GL.GetProgram(myId, GetProgramParameterName.ActiveAttributes, out max);
         for (int i = 0; i < max; i++)
         {
            AttributeInfo binding;
            string name = GL.GetActiveAttrib(myId, i, out binding.size, out binding.type);
            binding.id = GL.GetAttribLocation(myId, name);
            binding.name = name;

            if (name == "gl_VertexID") //skip this built in attribute
               continue;
            myVertexBindings.Add(name, binding);
         }
      }

      void findUniforms()
      {
         int max;
         GL.GetProgram(myId, GetProgramParameterName.ActiveUniforms, out max);
         for (int i = 0; i < max; i++)
         {
            UniformInfo binding;
            string name = GL.GetActiveUniform(myId, i, out binding.size, out binding.type);
            binding.id = GL.GetUniformLocation(myId, name);
            binding.name = name;

            //don't create uniform objects for things that may not be uniforms
            if(binding.id >= 0)
               myUniformInfo.Add(name, binding);
         }
      }

      public void createUniforms()
      {
			List<UniformInfo> infos = new List<UniformInfo>();
         foreach (UniformInfo ui in myUniformInfo.Values)
         {
				myUniforms.Add(new Uniform(this, ui));
         }

			//sort so that when we set the uniforms (assuming 
			myUniforms.Sort((a, b) => a.location.CompareTo(b.location));
      }

		int myNextSearchUniform = 0;
		public void setUniform(UniformData uni)
      {
			Uniform uniform = null;
			for(int i=0; i< myUniforms.Count; i++)
			{
				int loc = (i + myNextSearchUniform) % myUniforms.Count;
				if (myUniforms[loc].location == uni.location)
				{
					myNextSearchUniform = loc + 1;
					uniform = myUniforms[loc];
					break;
				}
			}
			
			if(uniform != null)
         {
            switch (uni.type)
            {
               case Uniform.UniformType.Bool:
                  uniform.setValue((bool)uni.data);
                  break;
               case Uniform.UniformType.Int:
                  uniform.setValue((int)uni.data);
                  break;
               case Uniform.UniformType.Float:
                  uniform.setValue((float)uni.data);
                  break;
               case Uniform.UniformType.Vec2:
                  uniform.setValue((Vector2)uni.data);
                  break;
					case Uniform.UniformType.IVec2:
						uniform.setValue((Vector2)uni.data);
						break;
					case Uniform.UniformType.Vec3:
                  uniform.setValue((Vector3)uni.data);
                  break;
					case Uniform.UniformType.IVec3:
						uniform.setValue((Vector3)uni.data);
						break;
					case Uniform.UniformType.Vec4:
                  uniform.setValue((Vector4)uni.data);
                  break;
					case Uniform.UniformType.IVec4:
						uniform.setValue((Vector4)uni.data);
						break;
               case Uniform.UniformType.Color4:
                  uniform.setValue((Color4)uni.data);
                  break;
               case Uniform.UniformType.Mat4:
                  uniform.setValue((Matrix4)uni.data);
                  break;
               case Uniform.UniformType.Mat4Array:
                  uniform.setValue((Matrix4[])uni.data);
                  break;
            }

            uniform.apply();
         }
      }
   }
}