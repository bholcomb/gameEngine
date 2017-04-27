using System;
using System.Collections.Generic;

namespace Graphics
{
	public class DebugView : View
	{
		BaseRenderQueue myRenderQueue;

		public DebugView(Camera c, Viewport v, RenderTarget rt) 
			: base("debug view", c, v, rt, false)
		{
			PipelineState ps = new PipelineState();
			ps.blending.enabled = true;
			ps.shaderProgram = DebugRenderer.canvas.myShader;
			ps.vao = DebugRenderer.canvas.myVao;
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
			
			DebugRenderer.update();

			List<RenderCommand> cmds = DebugRenderer.canvas.getRenderCommands();
			foreach(RenderCommand rc in cmds)
			{
				StatelessRenderCommand src = rc as StatelessRenderCommand;
				src.renderState.setUniformBuffer(camera.uniformBufferId(), 0);
			}

			//these are stateless commands, so no need to setup a pipeline, thats part of each command (usually the same)
			myRenderQueue.commands.AddRange(cmds);

         stats.renderCalls = myRenderQueue.commands.Count;
      }
	}
}