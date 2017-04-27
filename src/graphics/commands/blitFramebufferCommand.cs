using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class BlitFrameBufferCommand : StatelessRenderCommand
   {
      RenderTarget myTarget; 

      public BlitFrameBufferCommand(RenderTarget target)
         : base()
      {
         myTarget = target;
      }

      public override void execute()
      {
			base.execute();
         GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, myTarget.id());
         GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
         int w, h;
         w = myTarget.buffers[FramebufferAttachment.ColorAttachment0].width;
         h = myTarget.buffers[FramebufferAttachment.ColorAttachment0].height;
         GL.BlitFramebuffer(0, 0, w, h, 0, 0, w, h, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
         GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);

			Renderer.device.currentRenderTarget = null;
      }
   }
}