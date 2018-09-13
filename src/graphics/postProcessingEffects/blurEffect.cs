using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class BlurEffect : PostEffect
   {
      ShaderProgram myShader;

      public BlurEffect()
         : base()
      {
         name = "blur";

         ShaderProgramDescriptor sd = new ShaderProgramDescriptor("Graphics.shaders.post-vs.glsl", "Graphics.shaders.blur-ps.glsl");
         myShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public override void getCommands(List<RenderCommand> cmds)
      {
         StatelessRenderCommand cmd;

         cmds.Add(new PushDebugMarkerCommand("blur effect"));

         RenderTarget target1;
         RenderTarget target2;
         target1 = postPass.nextRenderTarget();
         target2 = postPass.nextRenderTarget();

         //target a new render target
         cmds.Add(new SetRenderTargetCommand(target1));

         //blur the input image in the horizontal direction
         cmd = new PostEffectRenderCommand(myShader, postPass.view.viewport.width, postPass.view.viewport.height);
         cmd.renderState.setTexture(postPass.previousEffectOutput(this).id(), 0, TextureTarget.Texture2D);
         cmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
         cmd.renderState.setTexture(postPass.sourceDepthBuffer().id(), 1, TextureTarget.Texture2D);
         cmd.renderState.setUniform(new UniformData(21, Uniform.UniformType.Int, 1));
         cmd.renderState.setUniform(new UniformData(22, Uniform.UniformType.Int, 0)); //blur horizontal
         cmds.Add(cmd);

         //target a new render target
         cmds.Add(new SetRenderTargetCommand(target2));

         //blur the previous render target in the vertical direction
         cmd = new PostEffectRenderCommand(myShader, postPass.view.viewport.width, postPass.view.viewport.height);
         cmd.renderState.setTexture(target1.buffers[FramebufferAttachment.ColorAttachment0].id(), 0, TextureTarget.Texture2D);
         cmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
         cmd.renderState.setTexture(postPass.sourceDepthBuffer().id(), 1, TextureTarget.Texture2D);
         cmd.renderState.setUniform(new UniformData(21, Uniform.UniformType.Int, 1));
         cmd.renderState.setUniform(new UniformData(22, Uniform.UniformType.Int, 1)); //blur vertical
         cmds.Add(cmd);

         output = target2.buffers[FramebufferAttachment.ColorAttachment0];

         cmds.Add(new PopDebugMarkerCommand());
      }
   }
}