using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class PostEffectPass : Pass
   {
      List<PostEffect> myEffects = new List<PostEffect>();

      List<RenderTarget> myTempRenderTargets = new List<RenderTarget>();
      int myCurrentRenderTarget = 0;

      RenderTarget mySourceFbo;

      public PostEffectPass(RenderTarget sourceFbo, View v)
         : base("postEffect", "post")
      {
         view = v;

         if (sourceFbo is DefaultRenderTarget)
         {
            throw new Exception("need fbo as input");
         }

         mySourceFbo = sourceFbo;

         //create temp render targets for ping pong type action
         for (int i = 0; i < 2; i++)
         {
            RenderTarget rt;
            view.camera.viewport().notifier += new Viewport.ViewportNotifier(handle_viewportChanged);
            Vector2 size = view.camera.viewport().size;
            rt = new RenderTarget();
            rt.attachTarget(FramebufferAttachment.ColorAttachment0, RenderTarget.createTextureBuffer((int)size.X, (int)size.Y, SizedInternalFormat.Rgba8));
            if (rt.checkFrameBufferStatus() == false)
            {
               throw new Exception("oops");
            }
            myTempRenderTargets.Add(rt);
         }
      }

      public void handle_viewportChanged(int x, int y, int w, int h)
      {
         myTempRenderTargets.Clear();

         for (int i = 0; i < 2; i++)
         {
            RenderTarget rt;
            Vector2 size = view.camera.viewport().size;
            rt = new RenderTarget();
            rt.attachTarget(FramebufferAttachment.ColorAttachment0, RenderTarget.createTextureBuffer((int)size.X, (int)size.Y, SizedInternalFormat.Rgba8));
            if (rt.checkFrameBufferStatus() == false)
            {
               throw new Exception("oops");
            }
            myTempRenderTargets.Add(rt);
         }
      }

      public bool hasEffect(String name)
      {
         foreach (PostEffect e in myEffects)
         {
            if (e.name == name)
            {
               return true;
            }
         }

         return false;
      }

      public void addEffect(String name)
      {
         if (hasEffect(name) == true)
         {
            Warn.print("Effect {0} already in pipeline", name);
            return;
         }

         PostEffect effect = PostProcessingFactory.create(name);
         if (effect == null)
         {
            Warn.print("Error creating effect {0}", name);
            return;
         }

         effect.postPass = this;
         myEffects.Add(effect);
         resetIndexes();
      }

      void resetIndexes()
      {
         int i = 0;
         foreach(PostEffect e in myEffects)
         {
            e.index = i++;
         }
      }

      public void removeEffect(String name)
      {
         foreach (PostEffect e in myEffects)
         {
            if (e.name == name)
            {
               myEffects.Remove(e);
               break;
            }
         }

         resetIndexes();
      }

      public PostEffect findEffect(String name)
      {
         foreach (PostEffect e in myEffects)
         {
            if (e.name == name)
            {
               return e;
            }
         }

         return null;
      }

      public Texture sourceColorBuffer(int layer = 0)
      {
         return mySourceFbo.buffers[FramebufferAttachment.ColorAttachment0 + layer];
      }

      public Texture sourceDepthBuffer()
      {
         return mySourceFbo.buffers[FramebufferAttachment.DepthAttachment];
      }

      public RenderTarget nextRenderTarget()
      {
         myCurrentRenderTarget = (myCurrentRenderTarget + 1) % 2;
         return myTempRenderTargets[myCurrentRenderTarget];
      }

      public RenderTarget currentRenderTarget()
      {
         return myTempRenderTargets[myCurrentRenderTarget];
      }

      public PostEffect previousEffect(PostEffect currentEffect)
      {
         int i = currentEffect.index;
         if(i > 0)
         {
            return myEffects[i - 1];
         }

         return null;
      }

      public Texture previousEffectOutput(PostEffect currentEffect)
      {
         PostEffect effect = previousEffect(currentEffect);
         while(effect != null)
         {
            if(effect.enabled == true)
            {
               return effect.output;
            }

            effect = previousEffect(effect);
         }

         return sourceColorBuffer();
      }

      public override void getRenderCommands(List<RenderCommandList> renderCmdLists)
      {
         renderCmdLists.Add(preCommands);

         RenderCommandList cmds = new RenderCommandList();

         List<PostEffect> activeEffects = new List<PostEffect>();
         foreach(PostEffect e in myEffects)
         {
            if(e.enabled == true)
            {
               activeEffects.Add(e);
            }
         }

         foreach (PostEffect e in activeEffects)
         {
            cmds.AddRange(e.getCommands());
         }

         if (activeEffects.Count > 0)
         {
            cmds.Add(new SetRenderTargetCommand(renderTarget));
            cmds.Add(new CopyFramebufferCommand(activeEffects[activeEffects.Count - 1].output, view.viewport.width, view.viewport.height));
         }

         renderCmdLists.Add(cmds);
         renderCmdLists.Add(postCommands);
      }

      public void debug()
      {
         float fsize = 150;
         Vector2 screenSize = view.camera.viewport().size;

         Vector2 size = new Vector2(fsize, fsize);
         Vector2 step = new Vector2(fsize + 10, 0);
         Vector2 min = new Vector2(10, screenSize.Y - 10 - size.Y);

         DebugRenderer.addTexture(min, min + size, renderTarget.buffers[FramebufferAttachment.ColorAttachment0], false, 0.0);
         DebugRenderer.addTexture(min += step, min + size, renderTarget.buffers[FramebufferAttachment.DepthAttachment], true, 0.0);
         DebugRenderer.addTexture(min += step, min + size, myTempRenderTargets[0].buffers[FramebufferAttachment.ColorAttachment0], false, 0.0);
         DebugRenderer.addTexture(min += step, min + size, myTempRenderTargets[1].buffers[FramebufferAttachment.ColorAttachment0], false, 0.0);
      }
   }
}