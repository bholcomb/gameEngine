using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class Device
   {
      Dictionary<String, RenderTarget> myRenderTargets = new Dictionary<string, RenderTarget>();
      Dictionary<UInt64, BaseRenderQueue> myRenderQueues = new Dictionary<UInt64, BaseRenderQueue>();

      public static RenderState theRenderState;
      public static PipelineState thePipelineState;

      //shadow state for the device, uses commands to set these
      UInt64 myCurrentPipelineId = 0;
      Int32[] myBoundUniformBuffers = new Int32[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //TODO: categorize UBOs into per-frame, per-view, per-draw groups
      Int32[] myBoundStorageBuffers = new Int32[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      Int32[] myBoundImageBuffers = new Int32[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
      Int32[] myBoundVertexBuffers = new Int32[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

      struct TextureUnitInfo
      {
         public int id;
         public TextureTarget type;
      }
      TextureUnitInfo[] myBoundTextures = new TextureUnitInfo[10];

      Int32 myBoundIndexBuffer = 0;

      public PipelineState currentPipeline;
      public RenderTarget currentRenderTarget;

      public Device()
      {
         //initialize all the things
         theRenderState = new RenderState();
         theRenderState.force();

         thePipelineState = new PipelineState();
         thePipelineState.apply();
      }

      public bool init()
      {
         setupDebugCapture();
         return true;
      }

      public bool shutdown()
      {
         return true;
      }

      public RenderTarget getRenderTarget(String name)
      {
         return myRenderTargets[name]; //throws if not found
      }

      public void executeCommandList(List<RenderCommand> cmds)
      {
         foreach (RenderCommand rc in cmds)
         {
            rc.execute();
         }
      }

      public void executeRenderQueue(BaseRenderQueue rq)
      {
         bindPipeline(rq.myPipeline);

         foreach (RenderCommand rc in rq.commands)
         {
            rc.execute();
         }
      }

      public RenderTarget createRenderTarget(String name, Vector2i size, List<RenderTargetDescriptor> desc)
      {
         RenderTarget rt = new RenderTarget(size.X, size.Y, desc);
         RenderTarget temp = null;
         if (myRenderTargets.TryGetValue(name, out temp) == true)
         {
            temp.Dispose();
         }

         myRenderTargets[name] = rt;
         return rt;
      }

      public BaseRenderQueue createRenderQueue(PipelineState pipeline)
      {
         BaseRenderQueue rq = new BaseRenderQueue(pipeline);
         myRenderQueues[pipeline.id] = rq;
         return rq;
      }

      public RenderQueue<T> createRenderQueue<T>(PipelineState pipeline) where T : RenderInfo, new()
      {
         RenderQueue<T> rq = new RenderQueue<T>(pipeline);
         myRenderQueues[pipeline.id] = rq;
         return rq;
      }

      #region Command Execution list
      public void setRenderTarget(RenderTarget rt)
      {
         if (rt != currentRenderTarget)
         {
            currentRenderTarget = rt;
            currentRenderTarget.bind();
         }
      }

      public void bindPipeline(PipelineState ps)
      {
         if (myCurrentPipelineId != ps.id)
         {
            //Renderer.device.clearRenderState();  //this clears per-frame UBO as well.  Need to find a way to segregate these
            ps.apply();
            myCurrentPipelineId = ps.id;
            currentPipeline = ps;
         }
      }

      //reset just the bound VBO and IBO, useful for after binding a VAO to set the input assembler vertex structure
      //but update the vbo
      public void resetVboIboState()
      {
         for (int i = 0; i < myBoundVertexBuffers.Length; i++)
         {
            bindVertexBuffer(0, i, 0, 0);
            myBoundVertexBuffers[i] = 0;
         }

         bindIndexBuffer(0);
      }

      public void resetRenderState()
      {
         //create a blank/default renderstate and apply it (this will update the global render state
         RenderState rs = new RenderState();
         rs.apply();

         //clear out any previously bound buffers
         for (int i = 0; i < myBoundUniformBuffers.Length; i++)
         {
            bindUniformBuffer(0, i);
            myBoundUniformBuffers[i] = 0;
         }

         for (int i = 0; i < myBoundStorageBuffers.Length; i++)
         {
            bindStorageBuffer(0, i);
            myBoundStorageBuffers[i] = 0;
         }

         for (int i = 0; i < myBoundImageBuffers.Length; i++)
         {
            bindImageBuffer(0, i, TextureAccess.ReadOnly, SizedInternalFormat.R32f);
            myBoundImageBuffers[i] = 0;
         }

         for (int i = 0; i < myBoundVertexBuffers.Length; i++)
         {
            bindVertexBuffer(0, i, 0, 0);
            myBoundVertexBuffers[i] = 0;
         }

         for (int i = 0; i < myBoundTextures.Length; i++)
         {
            if (myBoundTextures[i].id != 0)
               bindTexture(0, i, myBoundTextures[i].type);
         }

         bindIndexBuffer(0);
      }

      public void resetPipelineState()
      {
         PipelineState ps = new PipelineState();
         ps.apply();
         myCurrentPipelineId = ps.id;
      }

      public void reset()
      {
         resetPipelineState();
         resetRenderState();
      }

      public void clearTargets(ClearBufferMask clearMask)
      {
         GL.Clear(clearMask);
      }

      public void bindUniformBuffer(int bufferId, int location)
      {
         if (myBoundUniformBuffers[location] != bufferId)
         {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, location, bufferId);
            myBoundUniformBuffers[location] = bufferId;
         }
      }

      public void bindStorageBuffer(int bufferId, int location)
      {
         if (myBoundStorageBuffers[location] != bufferId)
         {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, location, bufferId);
            myBoundStorageBuffers[location] = bufferId;
         }
      }

      public void bindImageBuffer(int bufferId, int location, TextureAccess access, SizedInternalFormat format)
      {
         if (access == 0) access = TextureAccess.ReadOnly;
         if (format == 0) format = SizedInternalFormat.R32f;

         if (myBoundImageBuffers[location] != bufferId)
         {
            GL.BindImageTexture(location, bufferId, 0, false, 0, access, format);
            myBoundImageBuffers[location] = bufferId;
         }
      }

      public void bindTexture(int bufferId, int location, TextureTarget textureType)
      {
         if (myBoundTextures[location].id != bufferId)
         {
            GL.ActiveTexture(TextureUnit.Texture0 + location);

            //clear previous texture type in this location so we don't have more
            //than one texture type bound to a texture unit, tends to cause issues with openGL
            if (myBoundTextures[location].type != textureType && myBoundTextures[location].id != 0)
            {
               GL.BindTexture(myBoundTextures[location].type, 0);
            }

            GL.BindTexture(textureType, bufferId);
            myBoundTextures[location].id = bufferId;
            myBoundTextures[location].type = textureType;
         }
      }

      public void bindVertexBuffer(int bufferId, int location, int offset, int stride)
      {
         if (myBoundVertexBuffers[location] != bufferId)
         {
            GL.BindVertexBuffer(location, bufferId, (IntPtr)offset, stride);
            myBoundVertexBuffers[location] = bufferId;
         }
      }

      public void bindIndexBuffer(int ibo)
      {
         if (myBoundIndexBuffer != ibo)
         {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            myBoundIndexBuffer = ibo;
         }
      }

      public void drawArray(PrimitiveType type, int first, int count)
      {
         GL.DrawArrays(type, first, count);
      }

      public void drawIndexed(PrimitiveType type, int count, int offset, DrawElementsType indexType)
      {
         int byteOffset = offset * (indexType == DrawElementsType.UnsignedShort ? 2 : 4);
         GL.DrawElements(type, count, indexType, byteOffset);
      }

      #endregion

      #region debugging functions
      public void pushDebugMarker(String marker)
      {
#if DEBUG
         GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, marker.Length, marker);
#endif
      }

      public void popDebugMarker()
      {
#if DEBUG
         GL.PopDebugGroup();
#endif
      }

      uint[] disabledIds;
      DebugProc myDebugCallback;
      void setupDebugCapture()
      {
         //#if false
#if DEBUG
         GL.Enable(EnableCap.DebugOutput);
         GL.Enable(EnableCap.DebugOutputSynchronous);

         //filter out noise messages
         disabledIds = new uint[4] { 131185, 131186, 131204, 1282 };
         GL.DebugMessageControl(DebugSourceControl.DebugSourceApi, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, disabledIds.Length, disabledIds, false);
         //filter out debug push/pop messages (too much console spew)
         GL.DebugMessageControl(DebugSourceControl.DebugSourceApplication, DebugTypeControl.DebugTypePushGroup, DebugSeverityControl.DebugSeverityNotification, 0, (uint[])null, false);
         GL.DebugMessageControl(DebugSourceControl.DebugSourceApplication, DebugTypeControl.DebugTypePopGroup, DebugSeverityControl.DebugSeverityNotification, 0, (uint[])null, false);

         myDebugCallback = new DebugProc(debugOutput);
         GL.DebugMessageCallback(myDebugCallback, IntPtr.Zero);
#endif
      }

      void debugOutput(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
      {
         string msg = Marshal.PtrToStringAnsi(message, length);
         Info.print("GL Debug Message: {0} \n\tSource:{1} \n\tType:{2} \n\tSeverity:{3} \n\tID:{4} ", msg, source, type, severity, id);
      }
      #endregion

   }
}