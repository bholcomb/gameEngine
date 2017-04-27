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
   public class Translate : Module
   {
      ShaderProgram myShaderProgram;

      public float transXVal = 0.0f;
      public float transYVal = 0.0f;

      public Texture transXImg = null;
      public Texture transYImg = null;
      public Texture inputImg = null;

      public float lowVal = 0.0f;
      public float highVal = 1.0f;

      public Translate() : base(Type.Translate)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.translate-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void generate(Texture output)
      {
         ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);
         
         cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, transXVal));
         cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Float, transYVal));
         
         cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Bool, transXImg != null));
         if (transXImg != null)
         {
            cmd.addImage( transXImg, TextureAccess.ReadOnly, 0);
         }

         cmd.renderState.setUniform(new UniformData(3, Uniform.UniformType.Bool, transYImg != null));
         if (transYImg != null)
         {
            cmd.addImage(transYImg, TextureAccess.ReadOnly, 1);
         }

         cmd.addImage(inputImg, TextureAccess.ReadOnly, 2);
         cmd.addImage(output, TextureAccess.WriteOnly, 3);

         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}