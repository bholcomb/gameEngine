using System;
using System.Collections.Generic;

namespace Graphics
{
	public class DebugView : View
	{
		BaseRenderQueue myRenderQueue;
      RenderTarget myRenderTarget;

		public DebugView(string name, Camera c, Viewport v, RenderTarget rt) 
			: base(name, c, v)
		{
         myRenderTarget = rt;

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
		}

		public override void prepare()
		{
			camera.updateCameraUniformBuffer();
		}

		public override void generateRenderCommandLists()
		{
         stats.name = name;
         stats.passes = 1;

         myRenderQueue.commands.Clear();
			myRenderQueue.addCommand(new SetRenderTargetCommand(myRenderTarget));
			
			DebugRenderer.update();

			List<RenderCommand> cmds = DebugRenderer.canvas.getRenderCommands();
			foreach(RenderCommand rc in cmds)
			{
				StatelessRenderCommand src = rc as StatelessRenderCommand;
				src.renderState.setUniformBuffer(camera.uniformBufferId(), 0);
			}

			//these are stateless commands, so no need to setup a pipeline, thats part of each command (usually the same)
			myRenderQueue.commands.AddRange(cmds);
      }
	}
}