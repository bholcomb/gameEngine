using System;
using System.Collections.Generic;

namespace Graphics
{
	public class DebugPass : Pass
	{
		BaseRenderQueue myRenderQueue;

		public DebugPass(RenderTarget rt = null) 
			: base("Debug", "debug")
		{
         renderTarget = rt;

			PipelineState ps = new PipelineState();
			ps.blending.enabled = true;
			ps.shaderState.shaderProgram = DebugRenderer.canvas.myShader;
			ps.vaoState.vao = DebugRenderer.canvas.myVao;
			ps.generateId();

			myRenderQueue = Renderer.device.createRenderQueue(ps);
         myRenderQueue.name = "debug";
         registerQueue(myRenderQueue);
      }

		public override void generateRenderCommandLists()
		{
         preCommands.Clear();
         postCommands.Clear();

         preCommands.Add(new PushDebugMarkerCommand(String.Format("Pass {0}:{1}-execute", view.name, name)));

         //clear render target if needed
         if (renderTarget != null)
         {
            preCommands.Add(new SetRenderTargetCommand(renderTarget));
            if (clearTarget == true)
            {
               preCommands.Add(new ClearColorCommand(clearColor));
               preCommands.Add(new ClearCommand(clearMask));
            }
         }

         //called after setting render target so that any user commands inserted will affect (or change) the render target
         onPreGenerateCommands();

         stats.queueCount = myRenderQueues.Count;
         stats.renderCalls = 0;
         stats.name = name;
         stats.technique = technique;

         //update the debug renderer
         DebugRenderer.update();

			List<RenderCommand> cmds = DebugRenderer.canvas.getRenderCommands();
			foreach(RenderCommand rc in cmds)
			{
				StatelessRenderCommand src = rc as StatelessRenderCommand;
				src.renderState.setUniformBuffer(view.camera.uniformBufferId(), 0);
			}

			//these are stateless commands, so no need to setup a pipeline, thats part of each command (usually the same)
			myRenderQueue.commands.AddRange(cmds);

         onPostGenerateCommands();

         postCommands.Add(new PopDebugMarkerCommand());

         stats.renderCalls += preCommands.Count;
         stats.renderCalls += postCommands.Count;
         foreach (BaseRenderQueue rq in myRenderQueues.Values)
         {
            stats.renderCalls += rq.commands.Count;
         }
      }
   }
}