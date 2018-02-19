using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class PostEffectRenderCommand : StatelessRenderCommand
   {
      static VertexArrayObject theVao;
      static PostEffectRenderCommand()
      {
         theVao = new VertexArrayObject();
      }
      public PostEffectRenderCommand(ShaderProgram sp, int width, int height)
      {
         pipelineState.shaderState.shaderProgram = sp;
         pipelineState.vaoState.vao = theVao;
         pipelineState.generateId();
         renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, (float)TimeSource.currentTime()));
         renderState.setUniform(new UniformData(1, Uniform.UniformType.Float, (float)width));
         renderState.setUniform(new UniformData(2, Uniform.UniformType.Float, (float)height));
      }

      public override void execute()
      {
         base.execute();

         //draw the mesh
         GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
      }
   }

   public class CopyFramebufferCommand : StatelessRenderCommand
   {
      static ShaderProgram theShader;
      static VertexArrayObject theVao;

      static CopyFramebufferCommand()
      {
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor("Graphics.shaders.post-vs.glsl", "Graphics.shaders.copy-ps.glsl");
         theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;
         theVao = new VertexArrayObject();
      }

      public CopyFramebufferCommand(Texture t, int width, int height)
      {
         pipelineState.shaderState.shaderProgram = theShader;
         pipelineState.vaoState.vao = theVao;
         pipelineState.depthTest.enabled = false;
         pipelineState.depthWrite.enabled = false;

         renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, (float)TimeSource.currentTime()));
         renderState.setUniform(new UniformData(1, Uniform.UniformType.Float, (float)width));
         renderState.setUniform(new UniformData(2, Uniform.UniformType.Float, (float)height));

         renderState.setTexture(t.id(), 0, t.target);
         renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
      }

      public override void execute()
      {
         base.execute();

         //draw the mesh
         GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
      }
   }
}