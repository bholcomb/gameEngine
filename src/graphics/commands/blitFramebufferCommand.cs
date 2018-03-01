using System;
using System.Collections.Generic;

using Util;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class BlitFrameBufferCommand : StatelessRenderCommand
   {
      RenderTarget mySource;
      Rect mySourceRegion;
      Rect myDestRegion; 

      public BlitFrameBufferCommand(RenderTarget source)
      {
         mySource = source;
         mySourceRegion = new Rect(0, 0, mySource.buffers[FramebufferAttachment.ColorAttachment0].width, mySource.buffers[FramebufferAttachment.ColorAttachment0].height);
         myDestRegion = new Rect(0, 0, mySource.buffers[FramebufferAttachment.ColorAttachment0].width, mySource.buffers[FramebufferAttachment.ColorAttachment0].height);
      }

      public BlitFrameBufferCommand(RenderTarget source, Rect sourceRegion, Rect destRegion)
         : base()
      {
         mySource = source;
         mySourceRegion = sourceRegion;
         myDestRegion = destRegion;
      }

      public override void execute()
      {
			base.execute();
         
         GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, mySource.id());
         GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
         
         GL.BlitFramebuffer((int)mySourceRegion.left, (int)mySourceRegion.bottom, (int)mySourceRegion.right, (int)mySourceRegion.top,
            (int)myDestRegion.left, (int)myDestRegion.bottom, (int)myDestRegion.right, (int)myDestRegion.top,
            ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
         GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);

			Renderer.device.currentRenderTarget = null;
      }
   }
}