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
			ps.shaderState.shaderProgram = DebugRenderer.canvas.myShader;
			ps.vaoState.vao = DebugRenderer.canvas.myVao;
			ps.generateId();

			myRenderQueue = Renderer.device.getRenderQueue(ps.id);
			if (myRenderQueue == null)
			{
				myRenderQueue = Renderer.device.createRenderQueue(ps);
            myRenderQueue.name = "debug";
			}
		}

		public override void prepare()
		{
         Renderer.device.pushDebugMarker(String.Format("View {0}-prepare", name));

         onPrePrepare();

			camera.updateCameraUniformBuffer();

         onPostPrepare();

         Renderer.device.popDebugMarker();
		}

		public override void generateRenderCommandLists()
		{
         preCommands.Clear();
         postCommands.Clear();

         preCommands.Add(new PushDebugMarkerCommand(String.Format("View {0}-execute", name)));

         onPreGenerateCommands();

         //reset the device so this view can update as appropriate
         preCommands.Add(new DeviceResetCommand());


         myRenderQueue.commands.Clear();
         myRenderQueue.addCommand(new DeviceResetCommand());
         myRenderQueue.addCommand(new SetRenderTargetCommand(myRenderTarget));
         myRenderQueue.addCommand(new BindCameraCommand(camera));

         DebugRenderer.update();

			List<RenderCommand> cmds = DebugRenderer.canvas.getRenderCommands();
			foreach(RenderCommand rc in cmds)
			{
				StatelessRenderCommand src = rc as StatelessRenderCommand;
				src.renderState.setUniformBuffer(camera.uniformBufferId(), 0);
			}

			//these are stateless commands, so no need to setup a pipeline, thats part of each command (usually the same)
			myRenderQueue.commands.AddRange(cmds);

         onPostGenerateCommands();

         postCommands.Add(new PopDebugMarkerCommand());
      }

      public override List<RenderCommandList> getRenderCommandLists()
      {
         //update stats
         stats.name = name;
         stats.passStats.Clear();
         stats.passStats.Add(new PassStats());
         stats.passStats[0].name = "Debug";
         stats.passStats[0].technique = "debug";
         stats.passStats[0].queueCount = 1;
         stats.passStats[0].renderCalls = myRenderQueue.commands.Count;

         myRenderCommandLists.Clear();

         myRenderCommandLists.Add(preCommands);

         myRenderCommandLists.Add(myRenderQueue.commands);

         myRenderCommandLists.Add(postCommands);

         stats.commandLists = myRenderCommandLists.Count;

         return myRenderCommandLists;
      }
   }
}