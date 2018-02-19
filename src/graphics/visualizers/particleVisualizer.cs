using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class ParticleSystemInfo : RenderInfo
   {
      public int count;
      public ParticleSystemInfo() : base() { }
   }
   public class ParticleVisualizer : Visualizer
   {
      ShaderProgram myParticleShaderProgram;
      VertexArrayObject myVao;
      public ParticleVisualizer() 
         : base("particle")
      {
         ShaderProgramDescriptor spDesc = new ShaderProgramDescriptor("Graphics.shaders.particleSystem-vs.glsl", "Graphics.shaders.particleSystem-gs.glsl", "Graphics.shaders.particleSystem-ps.glsl");
         myParticleShaderProgram = Renderer.resourceManager.getResource(spDesc) as ShaderProgram;

         myVao = new VertexArrayObject();
         myVao.bindVertexFormat<V3C4S3R>(myParticleShaderProgram);
      }

      #region prepare phase
      public override void prepareFrameBegin()
      {
         ParticleManager.tick((float)TimeSource.timeThisFrame());
      }

      public override void preparePerFrame(Renderable r)
      {
         ParticleSystem ps = r as ParticleSystem;
         if (ps != null)
         {
            ps.updateVbo();
         }
      }

      //       public override void preparePerViewBegin(View v) { }
      //       public override void preparePerView(Renderable r, View v) { }
      //       public override void preparePerViewFinalize(View v) { }
      //       public override void preparePerPassBegin(Pass p) { }

      PipelineState createPipeline()
      {
         PipelineState ps = new PipelineState();
         ps.shaderState.shaderProgram = myParticleShaderProgram;
         ps.vaoState.vao = myVao;
         ps.culling.enabled = false;
         ps.blending.enabled = true;
         ps.depthTest.enabled = true;
         ps.depthWrite.enabled = false;
         ps.generateId();
         return ps;
      }

      public override void preparePerPass(Renderable r, Pass p)
      {
         ParticleSystem ps = r as ParticleSystem;

         PipelineState pipeline = createPipeline();
         RenderQueue<ParticleSystemInfo> rq = p.findRenderQueue(pipeline.id) as RenderQueue<ParticleSystemInfo>;
         if(rq == null)
         {
            rq = Renderer.device.createRenderQueue<ParticleSystemInfo>(pipeline);
            rq.name = "particle system";
            rq.visualizer = this;
            p.registerQueue(rq);
         }

         ParticleSystemInfo info = rq.nextInfo();

         float dist = (p.view.camera.position - r.position).Length;
         info.distToCamera = dist;
         info.count = ps.particles.Count;

         info.renderState.setVertexBuffer(ps.vbo.id, 0, 0, V3C4S3R.stride);
         info.renderState.alphaToCoverage.enabled = true;
         info.renderState.setUniform(new UniformData(0, Uniform.UniformType.Mat4, Matrix4.CreateTranslation(ps.position)));
         
         info.renderState.setTexture(ps.material.id(), 0, TextureTarget.Texture2D);
         info.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));

      }
      //       public override void preparePerPassFinalize(Pass p) { }
      //       public override void prepareFrameFinalize() { }
      #endregion

      #region generate command phase
      //       public override void generateRenderCommandsBegin(BaseRenderQueue q) { }
      public override void generateRenderCommand(RenderInfo r, BaseRenderQueue q)
      {
         ParticleSystemInfo psi = r as ParticleSystemInfo;

         q.addCommand(new SetRenderStateCommand(r.renderState));
         q.addCommand(new DrawArraysCommand(PrimitiveType.Points, 0, psi.count));
      }
      //       public override void generateRenderCommandsFinalize(BaseRenderQueue q) { }
      #endregion
   }
}