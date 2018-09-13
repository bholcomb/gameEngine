using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public class SkyboxRenderInfo : RenderInfo
	{
		public SkyboxRenderInfo() : base() { }
	}

	public class SkyboxVisualizer : Visualizer
   {    
		SkyboxEffect myCurrentEffect;

      public SkyboxVisualizer() 
         : base("skybox")
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
			SkyboxRenderable skyboxModel = r as SkyboxRenderable;
			MaterialEffect effect= getEffect(p.technique, (UInt32)Material.Feature.Skybox);
         PipelineState pipeline = effect.createPipeline(skyboxModel.model.mesh.material);

         RenderQueue<SkyboxRenderInfo> rq = p.findRenderQueue(pipeline.id) as RenderQueue<SkyboxRenderInfo>;
			if (rq == null)
			{
				rq = Renderer.device.createRenderQueue<SkyboxRenderInfo>(effect.createPipeline(skyboxModel.model.mesh.material));
            rq.name = rq.myPipeline.shaderState.shaderProgram.name;
            rq.myPipeline.vaoState.vao = new VertexArrayObject();
				rq.myPipeline.vaoState.vao.bindVertexFormat(rq.myPipeline.shaderState.shaderProgram, V3.bindings());
				rq.visualizer = this;
	         p.registerQueue(rq);
			}

			SkyboxRenderInfo info = rq.nextInfo();

			effect.updateRenderState(skyboxModel.model.mesh.material, info.renderState);

         info.renderState.setUniformBuffer(p.view.camera.uniformBufferId(), 0);
		}

      //public override void preparePerPassFinalize(Pass p) { }
      //public override void preparePerView(Renderable r, View v) { }
      //public override void prepareFrameFinalize() { }

      #endregion

      #region generate command phase
      //public override void generateCommandsBegin(BaseRenderQueue q) { }

      public override void generateRenderCommand(RenderInfo r, BaseRenderQueue q)
		{
			SkyboxRenderInfo skyInfo = r as SkyboxRenderInfo;
         q.addCommand(new SetRenderStateCommand(r.renderState));		
			q.addCommand(new DrawIndexedCommand(PrimitiveType.Triangles, 36, 0, DrawElementsType.UnsignedShort));
		}

      //public override void generateCommandsFinalize(BaseRenderQueue q) { }
      #endregion
   }
}