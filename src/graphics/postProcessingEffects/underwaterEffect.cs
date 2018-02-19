using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class UnderwaterEffect : PostEffect
   {
      ShaderProgram myShader; 

      public UnderwaterEffect()
         : base()
      {
         name = "underwater";

         ShaderProgramDescriptor sd=new ShaderProgramDescriptor("Graphics.shaders.post-vs.glsl", "Graphics.shaders.underwater-ps.glsl");
         myShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public override List<RenderCommand> getCommands()
      {
         List<RenderCommand> ret=new List<RenderCommand>();
         ret.Add(new PushDebugMarkerCommand("underwater effect"));
         StatelessRenderCommand cmd;

         RenderTarget targetFbo = postPass.nextRenderTarget();
         output = targetFbo.buffers[FramebufferAttachment.ColorAttachment0];
         ret.Add(new SetRenderTargetCommand(targetFbo));

         cmd = new PostEffectRenderCommand(myShader, postPass.view.viewport.width, postPass.view.viewport.height);
         cmd.renderState.setTexture(postPass.previousEffectOutput(this).id(), 0, TextureTarget.Texture2D);
         cmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
         ret.Add(cmd);

         ret.Add(new PopDebugMarkerCommand());
         return ret;
      }
   }
}