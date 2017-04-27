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
   public class Scale : Module
   {
      ShaderProgram myShaderProgram;

      public float scaleVal = 1.0f;
      public Texture scaleImg = null;

      public Scale() : base(Type.Scale)
      {
         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.scale-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myShaderProgram = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public void generate(Texture output)
      {
         ComputeCommand cmd = new ComputeCommand(myShaderProgram, output.width / 32, output.height / 32);
         
         cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, scaleVal));

         cmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Bool, scaleImg != null));
         if (scaleImg != null)
         {
            cmd.addImage(scaleImg, TextureAccess.ReadOnly, 0);
         }

         cmd.addImage( output, TextureAccess.ReadWrite, 1);

         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
      }
   }
}