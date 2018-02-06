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
         int sw, sh;
         sw = myTarget.buffers[FramebufferAttachment.ColorAttachment0].width;
         sh = myTarget.buffers[FramebufferAttachment.ColorAttachment0].height;
         //TODO: fix this so we can blit from different sized textures
         GL.BlitFramebuffer(0, 0, sw, sh, 0, 0, 1280, 800, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
         GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);

			Renderer.device.currentRenderTarget = null;
      }
   }
}