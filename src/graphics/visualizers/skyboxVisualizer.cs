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

      public SkyboxVisualizer() : base()
      {
         myType = "skybox";
      }

		#region extract phase
		//public override void onFrameBeginExtract() { }
		//public override void extractPerFrame(Renderable r) { }
		public override void extractPerView(Renderable r, View v)
		{
			SkyboxRenderable skyboxModel = r as SkyboxRenderable;
			Effect effect= getEffect(v.passType, (UInt32)Material.Feature.Skybox);
			RenderQueue<SkyboxRenderInfo> rq = Renderer.device.getRenderQueue(effect.getPipeline(skyboxModel.model.mesh.material).id) as RenderQueue<SkyboxRenderInfo>;
			if (rq == null)
			{
				rq = Renderer.device.createRenderQueue<SkyboxRenderInfo>(effect.getPipeline(skyboxModel.model.mesh.material));
				rq.myPipeline.vao = new VertexArrayObject();
				rq.myPipeline.vao.bindVertexFormat<V3>(rq.myPipeline.shaderProgram);
				rq.visualizer = this;
				v.registerQueue(rq);
			}

			SkyboxRenderInfo info = rq.nextInfo();

			effect.updateRenderState(skyboxModel.model.mesh.material, info.renderState);

         info.renderState.setUniformBuffer(v.camera.uniformBufferId(), 0);
		}

		//public override void extractPerViewFinalize(BaseRenderQueue q, View v) {	}
		//public override void onFrameExtractFinalize() { }
		#endregion

		#region  prepare phase
		//public override void onFrameBeginPrepare() { }
		//public override void preparePerFrame(Renderable r) { }
		//public override void preparePerView(RenderInfo info, View v) { }
		//public override void preparePerViewFinalize(RenderQueue q, View v) {}
		//public override void onFramePrepareFinalize() { }
		#endregion

		#region submit phase
		//public override void onSubmitNodeBlockBegin(String technique) { }
		public override void submitRenderInfo(RenderInfo r, BaseRenderQueue q)
		{
			SkyboxRenderInfo skyInfo = r as SkyboxRenderInfo;
         q.addCommand(new SetRenderStateCommand(r.renderState));		
			q.addCommand(new DrawIndexedCommand(PrimitiveType.Triangles, 36, 0, DrawElementsType.UnsignedShort));
		}
		//public override void onSubmitNodeBlockEnd() { }
		#endregion
   }
}