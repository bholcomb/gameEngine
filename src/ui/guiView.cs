using System;
using System.Collections.Generic;

using Graphics;

namespace UI
{
	public class GuiView : View
	{
		BaseRenderQueue myRenderQueue;
      RenderTarget myRenderTarget;

		public GuiView(string name, Camera c, Viewport v, RenderTarget rt) 
			: base(name, c, v)
		{
         myRenderTarget = rt;
			PipelineState ps = new PipelineState();
			ps.blending.enabled = true;
			ps.shaderState.shaderProgram = UI.Canvas.theShader;
			ps.blending.enabled = true;
			ps.culling.enabled = false;
			ps.depthTest.enabled = false;

			ps.vaoState.vao = new VertexArrayObject();
			ps.vaoState.vao.bindVertexFormat<V2T2B4>(ps.shaderState.shaderProgram);
			ps.generateId();

			myRenderQueue = Renderer.device.getRenderQueue(ps.id);
			if (myRenderQueue == null)
			{
				myRenderQueue = Renderer.device.createRenderQueue(ps);
            myRenderQueue.name = "UI";
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
			myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
			myRenderQueue.addCommand(new BindCameraCommand(camera));

         bool needsCameraRebind = false;
         foreach (RenderCommand rc in ImGui.getRenderCommands())
         {
            //previous command was custom and reset the pipeline for UI drawing
            if (needsCameraRebind == true && rc is UiRenderCommand)
            {
               myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
               myRenderQueue.addCommand(new BindCameraCommand(camera));
            }

            //add the command
            myRenderQueue.addCommand(rc);

            if (rc is StatelessRenderCommand)
            {
               needsCameraRebind = true;
            }
         }

         onPostGenerateCommands();

         postCommands.Add(new PopDebugMarkerCommand());
      }

      public override List<RenderCommandList> getRenderCommandLists()
      {
         //update stats
         stats.name = name;
         stats.passStats.Clear();
         stats.passStats.Add(new PassStats());
         stats.passStats[0].name = "UI";
         stats.passStats[0].technique = "UI";
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