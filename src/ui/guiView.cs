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
			}
		}

		public override void prepare()
		{
			camera.updateCameraUniformBuffer();
		}

		public override void generateRenderCommandLists()
		{
         myRenderQueue.commands.Clear();
         myRenderQueue.addCommand(new DeviceResetCommand());
			myRenderQueue.addCommand(new SetRenderTargetCommand(myRenderTarget));
			myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
			myRenderQueue.addCommand(new BindCameraCommand(camera));

         bool needsCameraRebind = false;
         foreach (RenderCommand rc in ImGui.getRenderCommands())
         {
            //previous command was custom and reset the pipeline for UI drawing
            if(needsCameraRebind == true && rc is UiRenderCommand)
            {
               myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
               myRenderQueue.addCommand(new BindCameraCommand(camera));
            }

            //add the command
            myRenderQueue.addCommand(rc);

            if(rc is StatelessRenderCommand)
            {
               needsCameraRebind = true;
            }
         }

         stats.renderCalls = myRenderQueue.commands.Count;
		}

      public override List<RenderCommandList> getRenderCommandLists()
      {
         //update stats
         stats.name = name;
         stats.passes = 1;

         myRenderCommandLists.Clear();

         myRenderCommandLists.Add(preCommands);

         myRenderCommandLists.Add(myRenderQueue.commands);

         myRenderCommandLists.Add(postCommands);

         //update render call stats
         foreach (RenderCommandList rcl in myRenderCommandLists)
         {
            stats.renderCalls += rcl.Count;
         }

         return myRenderCommandLists;
      }
   }
}