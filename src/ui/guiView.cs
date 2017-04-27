using System;
using System.Collections.Generic;

using Graphics;

namespace UI
{
	public class GuiView : View
	{
		BaseRenderQueue myRenderQueue;

		public GuiView(Camera c, Viewport v, RenderTarget rt) 
			: base("gui view", c, v, rt, false)
		{
			PipelineState ps = new PipelineState();
			ps.blending.enabled = true;
			ps.shaderProgram = UI.Canvas.theShader;
			ps.blending.enabled = true;
			ps.culling.enabled = false;
			ps.depthTest.enabled = false;

			ps.vao = new VertexArrayObject();
			ps.vao.bindVertexFormat<V2T2B4>(ps.shaderProgram);
			ps.generateId();

			myRenderQueue = Renderer.device.getRenderQueue(ps.id);
			if (myRenderQueue == null)
			{
				myRenderQueue = Renderer.device.createRenderQueue(ps);
			}

			registerQueue(myRenderQueue);
		}

		public override void extract()
		{
				
		}

		public override void prepare()
		{
			camera.updateCameraUniformBuffer();
		}

		public override void submit()
		{
         stats.queueCount = 1;
         stats.renderCalls = 0;
         stats.viewName = name;

         myRenderQueue.commands.Clear();
			myRenderQueue.addCommand(new SetRenderTargetCommand(renderTarget));
			myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
			myRenderQueue.addCommand(new BindCameraCommand(camera));
			myRenderQueue.commands.AddRange(ImGui.getRenderCommands());

         stats.renderCalls = myRenderQueue.commands.Count;
		}
	}
}