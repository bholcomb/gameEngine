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
   public class Select : Module
   {
      ShaderProgram myShaderProgram;

      public float threshold = 0.5f;
      public float falloff = 0.0f;

      public Texture lowImg = null;
      public Texture highImg = null;
      public Texture controlImg = null;
      
      public float lowVal = 0.0f;
      public float highVal = 1.0f;

      public Select() : base(Type.Select)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.select-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void generate(Texture output)
      {
         ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);
         cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, lowVal));
         cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Float, highVal));
         cmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Float, threshold));
         cmd.renderState.setUniform(new UniformData(3, Uniform.UniformType.Float, falloff));

         cmd.renderState.setUniform(new UniformData(4, Uniform.UniformType.Bool, lowImg != null));
         if (lowImg != null)
         {
            cmd.addImage(lowImg, TextureAccess.ReadOnly, 0);
         }

         cmd.renderState.setUniform(new UniformData(5, Uniform.UniformType.Bool, highImg != null));
         if (highImg != null)
         {
            cmd.addImage(highImg, TextureAccess.ReadOnly, 1);
         }
         
         cmd.addImage(controlImg, TextureAccess.ReadOnly, 2);
         cmd.addImage(output, TextureAccess.WriteOnly, 3);

         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}