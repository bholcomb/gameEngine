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
			myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
			myRenderQueue.addCommand(new BindCameraCommand(camera));

         bool needsCameraRebind = false;
         foreach (RenderCommand rc in ImGui.getRenderCommands())
         {
            //previous command was custom and reset the camera binding
            if(needsCameraRebind == true && rc is UiRenderCommand)
            {
               myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
               //myRenderQueue.addCommand(new BindCameraCommand(camera));
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
	}
}