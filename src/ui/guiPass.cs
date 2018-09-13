using System;
using System.Collections.Generic;

using Graphics;

namespace GUI
{
   public class UIPass : Pass
   {
      BaseRenderQueue myRenderQueue;
      public UIPass(RenderTarget target) 
         :base ("UI", "ui")
      {
         renderTarget = target;

         PipelineState ps = new PipelineState();
         ps.blending.enabled = true;
         ps.shaderState.shaderProgram = Canvas.theShader;
         ps.blending.enabled = true;
         ps.culling.enabled = false;
         ps.depthTest.enabled = false;

         ps.vaoState.vao = new VertexArrayObject();
         ps.vaoState.vao.bindVertexFormat(ps.shaderState.shaderProgram, V2T2B4.bindings());
         ps.generateId();

         myRenderQueue = Renderer.device.createRenderQueue(ps);
         myRenderQueue.name = "UI";
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

         //process all the IMGUI commands
         myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
         myRenderQueue.addCommand(new BindCameraCommand(view.camera));

         //add the view specific commands for each render queue
         bool needsCameraRebind = false;
         foreach (RenderCommand rc in UI.getRenderCommands())
         {
            //previous command was custom and reset the pipeline for UI drawing
            if (needsCameraRebind == true && rc is UiRenderCommand)
            {
               myRenderQueue.addCommand(new SetPipelineCommand(myRenderQueue.myPipeline));
               myRenderQueue.addCommand(new BindCameraCommand(view.camera));
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

         stats.renderCalls += preCommands.Count;
         stats.renderCalls += postCommands.Count;
         foreach (BaseRenderQueue rq in myRenderQueues.Values)
         {
            stats.renderCalls += rq.commands.Count;
         }
      }
   }
}