using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class FogEffect : PostEffect
   {
      ShaderProgram myShader;

      public FogEffect()
         : base()
      {
         name = "fog";

         ShaderProgramDescriptor sd=new ShaderProgramDescriptor("Graphics.shaders.post-vs.glsl", "Graphics.shaders.fog-ps.glsl");
         myShader= Renderer.resourceManager.getResource(sd) as ShaderProgram;
      }

      public Vector3 fogColor { get; set; }
      public float maxDistance { get; set; }

      public override void getCommands(List<RenderCommand> cmds)
      {
         StatelessRenderCommand cmd;

         cmds.Add(new PushDebugMarkerCommand("fog effect"));

         RenderTarget targetFbo = postPass.nextRenderTarget();
         output = targetFbo.buffers[FramebufferAttachment.ColorAttachment0];
         cmds.Add(new SetRenderTargetCommand(targetFbo));

         cmd = new PostEffectRenderCommand(myShader, postPass.view.viewport.width, postPass.view.viewport.height);
         cmd.renderState.setTexture(postPass.previousEffectOutput(this).id(), 0, TextureTarget.Texture2D);
         cmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
         cmd.renderState.setTexture(postPass.sourceDepthBuffer().id(), 1, TextureTarget.Texture2D);
         cmd.renderState.setUniform(new UniformData(21, Uniform.UniformType.Int, 1));
         cmd.renderState.setUniform(new UniformData(22, Uniform.UniformType.Vec3, fogColor));
         cmd.renderState.setUniform(new UniformData(23, Uniform.UniformType.Float, maxDistance));

         cmds.Add(cmd);

         cmds.Add(new PopDebugMarkerCommand());
      }
   }
}