using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public class SkydomeVisualizer : Visualizer
   {    
		SkydomeEffect myCurrentEffect;

      public SkydomeVisualizer() 
         : base("skydome")
      {
      }

      #region prepare phase
      //public override void prepareFrameBegin() { }
      //public override void preparePerFrame(Renderable r) { }
      //public override void preparePerViewBegin(View v) { }
      //public override void preparePerView(Renderable r, View v) { }
      //public override void preparePerViewFinalize(View v) { }
      //public override void preparePerPassBegin(Pass p) { }
      
      public override void preparePerPass(Renderable r, Pass p)
		{
			SkydomeRenderable renderable = r as SkydomeRenderable;
         Model model = renderable.skydome;
         Mesh mesh = model.myMeshes[0];

			MaterialEffect effect= getEffect(p.technique, (UInt32)Material.Feature.Skydome);
         PipelineState pipeline = effect.createPipeline(mesh.material);

         RenderQueue<StaticModelInfo> rq = p.findRenderQueue(pipeline.id) as RenderQueue<StaticModelInfo>;
			if (rq == null)
			{
				rq = Renderer.device.createRenderQueue<StaticModelInfo>(effect.createPipeline(mesh.material));
            rq.name = rq.myPipeline.shaderState.shaderProgram.name;
            rq.myPipeline.vaoState.vao = new VertexArrayObject();
				rq.myPipeline.vaoState.vao.bindVertexFormat(rq.myPipeline.shaderState.shaderProgram, V3.bindings());
				rq.visualizer = this;
	         p.registerQueue(rq);
			}

         StaticModelInfo info = rq.nextInfo();

			effect.updateRenderState(mesh.material, info.renderState);

         info.renderState.setUniformBuffer(p.view.camera.uniformBufferId(), 0);
         info.renderState.setVertexBuffer(model.myVbos[0].id, 0, 0, V3N3T2.stride);
         info.renderState.setIndexBuffer(model.myIbo.id);
         info.renderState.setUniform(new UniformData(0, Uniform.UniformType.Vec3, renderable.sunPosition));
         Matrix4 rot = Matrix4.CreateRotationX(renderable.starRotation);
         info.renderState.setUniform(new UniformData(1, Uniform.UniformType.Mat4, rot));
         info.renderState.setUniform(new UniformData(26, Uniform.UniformType.Float, renderable.weatherIntensity));
         info.renderState.setUniform(new UniformData(27, Uniform.UniformType.Float, renderable.weatherSpeed));
         info.renderState.setUniform(new UniformData(28, Uniform.UniformType.Float, renderable.sunSize));
         info.renderState.setUniform(new UniformData(29, Uniform.UniformType.Float, renderable.moonSize));
         info.indexOffset = mesh.indexBase;
         info.indexCount = mesh.indexCount;
      }

      //public override void preparePerPassFinalize(Pass p) { }
      //public override void preparePerView(Renderable r, View v) { }
      //public override void prepareFrameFinalize() { }

      #endregion

      #region generate command phase
      //public override void generateCommandsBegin(BaseRenderQueue q) { }

      public override void generateRenderCommand(RenderInfo r, BaseRenderQueue q)
		{
			StaticModelInfo skyInfo = r as StaticModelInfo;
         q.addCommand(new SetRenderStateCommand(r.renderState));
         q.addCommand(new DrawIndexedCommand(PrimitiveType.Triangles, skyInfo.indexCount, skyInfo.indexOffset, DrawElementsType.UnsignedShort));
      }

      //public override void generateCommandsFinalize(BaseRenderQueue q) { }
      #endregion
   }
}