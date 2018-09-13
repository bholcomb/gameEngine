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

      public override void getCommands(List<RenderCommand> cmds)
      {
         cmds.Add(new PushDebugMarkerCommand("underwater effect"));
         StatelessRenderCommand cmd;

         RenderTarget targetFbo = postPass.nextRenderTarget();
         output = targetFbo.buffers[FramebufferAttachment.ColorAttachment0];
         cmds.Add(new SetRenderTargetCommand(targetFbo));

         cmd = new PostEffectRenderCommand(myShader, postPass.view.viewport.width, postPass.view.viewport.height);
         cmd.renderState.setTexture(postPass.previousEffectOutput(this).id(), 0, TextureTarget.Texture2D);
         cmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
         cmds.Add(cmd);

         cmds.Add(new PopDebugMarkerCommand());
      }
   }
}