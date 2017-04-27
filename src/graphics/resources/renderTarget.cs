using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public struct RenderTargetDescriptor
   {
      public FramebufferAttachment attach;
      //these are all pretty much mutually exclusive
      public SizedInternalFormat format;
      public Texture tex;
      public int bpp;
   }

   public class RenderTarget : IDisposable
   {
      protected Int32 myId;
      List<DrawBuffersEnum> myTargets = new List<DrawBuffersEnum>();
      Dictionary<FramebufferAttachment, Texture> myBuffers = new Dictionary<FramebufferAttachment, Texture>();
      Dictionary<FramebufferAttachment, uint> myRenderBuffers = new Dictionary<FramebufferAttachment, uint>();

      public RenderTarget()
      {
         myId = GL.GenFramebuffer();
      }

      public int id()
      {
         return myId;
      }

      public RenderTarget(int width, int height, List<RenderTargetDescriptor> desc) : this()
      {
         update(width, height, desc);
      }

      public void update(int width, int height, List<RenderTargetDescriptor> desc)
      {
         myTargets.Clear();

         foreach (RenderTargetDescriptor d in desc)
         {
            if (d.tex == null)
            {
               if (d.attach >= FramebufferAttachment.ColorAttachment0 && d.attach <= FramebufferAttachment.ColorAttachment15)
               {
                  Texture t = createTextureBuffer(width, height, d.format);
                  attachTarget(d.attach, t);
               }
               if (d.attach == FramebufferAttachment.DepthAttachment)
               {
                  uint tid = createDepthRenderBuffer(width, height, d.bpp);
                  attachRenderBuffer(d.attach, tid);
               }
               if (d.attach == FramebufferAttachment.StencilAttachment)
               {
                  uint tid = createStencilRenderBuffer(width, height, d.bpp);
                  attachRenderBuffer(d.attach, tid);
               }
               if (d.attach == FramebufferAttachment.DepthStencilAttachment)
               {
                  uint tid = createDepthStencilRenderBuffer(width, height, d.bpp);
                  attachRenderBuffer(d.attach, tid);
               }

            }
            else
            {
               attachTarget(d.attach, d.tex);
            }
         }

         if (checkFrameBufferStatus() == false)
         {
            throw new Exception("Cannot complete framebuffer");
         }
      }

      public void Dispose()
      {
         GL.DeleteFramebuffer(myId);
      }

      public virtual void bind()
      {
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, myId);
      }

      public virtual void unbind()
      {
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
      }

      public Dictionary<FramebufferAttachment, Texture> buffers { get { return myBuffers; } }

      public virtual void attachTarget(FramebufferAttachment attachTarget, Texture tex)
      {
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, myId);
         GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachTarget, TextureTarget.Texture2D, tex.id(), 0);
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

         myBuffers[attachTarget] = tex;

         //if it's a color buffer set it up as a draw buffer
         if (attachTarget >= FramebufferAttachment.ColorAttachment0 && attachTarget <= FramebufferAttachment.ColorAttachment15)
         {
            myTargets.Add((DrawBuffersEnum)attachTarget);
         }
      }

      public virtual void attachRenderBuffer(FramebufferAttachment attachTarget, uint renderbufferId)
      {
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, myId);
         GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachTarget, RenderbufferTarget.Renderbuffer, renderbufferId);
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

         myRenderBuffers[attachTarget] = renderbufferId;
      }

      public virtual bool checkFrameBufferStatus()
      {
         bind();
         FramebufferErrorCode err = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
         if (err != FramebufferErrorCode.FramebufferComplete)
         {
            unbind();
            System.Console.WriteLine("Error with framebuffer: {0}", err);
            return false;
         }

         DrawBuffersEnum[] targets = myTargets.ToArray();
         GL.DrawBuffers(myTargets.Count, targets);

         unbind();
         return true;
      }

      #region static create buffer functions

      public static uint createDepthRenderBuffer(int width, int height, int depth)
      {
         uint id;
         GL.GenRenderbuffers(1, out id);
         GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, id);
         switch (depth)
         {
            case 16: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent16, width, height); break;
            case 24: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height); break;
            case 32: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32, width, height); break;
            default: throw new Exception("Depth is 16, 24, or 32");
         }

         return id;
      }

      public static uint createStencilRenderBuffer(int width, int height, int depth)
      {
         uint id;
         GL.GenRenderbuffers(1, out id);
         GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, id);
         switch (depth)
         {
            case 1: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.StencilIndex1, width, height); break;
            case 4: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.StencilIndex4, width, height); break;
            case 8: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.StencilIndex8, width, height); break;
            case 16: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.StencilIndex16, width, height); break;
            default: throw new Exception("Stencil depth is 1, 4, 8, or 16");
         }

         return id;
      }

      public static uint createDepthStencilRenderBuffer(int width, int height, int depth)
      {
         uint id;
         GL.GenRenderbuffers(1, out id);
         GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, id);
         switch (depth)
         {
            case 24: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height); break;
            case 32: GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth32fStencil8, width, height); break;
            default: throw new Exception("DepthStencil depth is 24 or 32");
         }

         return id;
      }

      public static Texture createTextureBuffer(int width, int height, SizedInternalFormat format)
      {
         if (format == 0)
            throw new Exception("Texture buffer needs an internal format");

         int tex;
         tex = GL.GenTexture();
         GL.BindTexture(TextureTarget.Texture2D, tex);
         GL.TexStorage2D(TextureTarget2d.Texture2D, 1, format, width, height);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
         GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


         Texture t = new Texture(tex);
         return t;
      }

      #endregion


      public void debug()
      {
         float fsize = 150;
         Vector2 min = new Vector2(10, 10);
         Vector2 step = new Vector2(fsize + 10, 0);
         Vector2 size = new Vector2(fsize, fsize);

         foreach (KeyValuePair<FramebufferAttachment, Texture> kv in myBuffers)
         {
            if (kv.Key == FramebufferAttachment.DepthAttachment)
            {
               //DebugRenderer.addTexture(min, min + size, kv.Value, true, false, 0.0);
            }
            else
            {
               //DebugRenderer.addTexture(min, min + size, kv.Value, false, false, 0.0);
            }

            min += step;
         } 
      }
   }

   public class DefaultRenderTarget : RenderTarget
   {
      public DefaultRenderTarget()
      {
         //no op
      }

      public new void Dispose()
      {
         //no op
      }

      //use the default one provided by the window system
      public override void bind()
      {
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
      }

      public override void unbind()
      {
         //no op.
      }

      //don't do this either
      public override void attachTarget(FramebufferAttachment attachTarget, Texture target)
      {
         throw new Exception("Cannot bind a target to the default frame buffer");
      }

      public override bool checkFrameBufferStatus()
      {
         return true;
      }
   }

}
