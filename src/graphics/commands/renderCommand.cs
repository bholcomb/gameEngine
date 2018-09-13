using System;
using System.Collections.Generic;

using OpenTK;

namespace Graphics
{
   public class RenderCommand
   {
      public RenderCommand()
      {
			
      }
		
      public virtual void execute() { }
   }

	public class StatelessRenderCommand : RenderCommand
	{
		public StatelessRenderCommand()
		{
			renderState = new RenderState();
			pipelineState = new PipelineState();
		}

		public RenderState renderState { get; set; }
		public PipelineState pipelineState { get; set; }

		public override void execute()
		{
         //Renderer.device.resetVboIboState();
			Renderer.device.bindPipeline(pipelineState);
			renderState.apply();
		}
	}
}