using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class AlphaToCoverage
   {
      public bool enabled { get; set; }
      public AlphaToCoverage()
      {
         enabled = true;
      }

      bool isDifferent(AlphaToCoverage other)
      {
         if (enabled != other.enabled) return true;
         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.theRenderState.alphaToCoverage) == true)
         {
            if (enabled)
            {
               GL.Enable(EnableCap.SampleAlphaToCoverage);
            }
            else
            {
               GL.Disable(EnableCap.SampleAlphaToCoverage);
            }

            Device.theRenderState.alphaToCoverage = this;
         }
      }      
   }

   public class PrimativeRestart
   {
      public bool enabled { get; set; }
      public UInt32 value { get; set; }

      public PrimativeRestart()
      {
         enabled = false;
         value = 0xFFFF;
      }

      bool isDifferent(PrimativeRestart other)
      {
         if (enabled != other.enabled) return true;
         if (value != other.value) return true;
         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.theRenderState.primativeRestart) == true)
         {
            if (enabled)
            {
               GL.PrimitiveRestartIndex(value);
               GL.Enable(EnableCap.PrimitiveRestart);
            }
            else
            {
               GL.Disable(EnableCap.PrimitiveRestart);
            }

            Device.theRenderState.primativeRestart = this;
         }
      }
   }

   public class Wireframe
   {
      public Wireframe()
      {
         enabled = false;
      }

      bool isDifferent(Wireframe other)
      {
         if (enabled != other.enabled) return true;
         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.theRenderState.wireframe) == true)
         {
            if (enabled == true)
            {
               GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            else
            {
               GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            Device.theRenderState.wireframe = this;
         }
      }

      public bool enabled { get; set; }
   }

   public class PolygonOffset
   {
      public enum EnableType { NONE, FILL, LINE, POINT };
      public EnableType enableType { get; set; }
      public float factor { get; set; }
      public float units { get; set; }

      public PolygonOffset()
      {
         enableType = EnableType.NONE;
         factor = 1.0f;
         units = 1.0f;
      }

      bool isDifferent(PolygonOffset other)
      {
         if (enableType != other.enableType) return true;
         if (factor != other.factor) return true;
         if (units != other.units) return true;

         return false;
      }

      public void apply()
      {
         if(isDifferent(Device.theRenderState.polygonOffset) == true)
         {
            if (enableType != EnableType.NONE)
            {
               switch (enableType)
               {
                  case EnableType.FILL: GL.Enable(EnableCap.PolygonOffsetFill); break;
                  case EnableType.LINE: GL.Enable(EnableCap.PolygonOffsetLine); break;
                  case EnableType.POINT: GL.Enable(EnableCap.PolygonOffsetPoint); break;
               }

               GL.PolygonOffset(factor, units);
            }
            else
            {
               GL.Disable(EnableCap.PolygonOffsetFill);
               GL.Disable(EnableCap.PolygonOffsetLine);
               GL.Disable(EnableCap.PolygonOffsetPoint);
            }

            Device.theRenderState.polygonOffset = this;
         }
      }
   }

   public class ChannelMask
   {
      public bool enabled { get; set; }
      public bool depthMask { get; set; }
      public bool redMask { get; set; }
      public bool greenMask { get; set; }
      public bool blueMask { get; set; }
      public bool alphaMask { get; set; }

      public ChannelMask()
      {
         enabled = false;
         depthMask = true;
         redMask = true;
         greenMask = true;
         blueMask = true;
         alphaMask = true;
      }

      bool isDifferent(ChannelMask other)
      {
         if (enabled != other.enabled) return true;
         if (depthMask != other.depthMask) return true;
         if (redMask != other.redMask) return true;
         if (greenMask != other.greenMask) return true;
         if (blueMask != other.blueMask) return true;
         if (alphaMask != other.alphaMask) return true;

         return false;
      }

      public void apply()
      {
         if(isDifferent(Device.theRenderState.channelMask) == true)
         {
            if (enabled == true)
            {
               GL.DepthMask(depthMask);
               GL.ColorMask(redMask, greenMask, blueMask, alphaMask);
            }
            else
            {
               GL.DepthMask(true);
               GL.ColorMask(true, true, true, true);
            }

            Device.theRenderState.channelMask = this;
         }
      }
   }

   public class ScissorTest
   {
      public bool enabled { get; set; }
      public Vector4 rect { get; set; }

      public ScissorTest()
      {
         enabled = false;
         rect = new Vector4(0, 0, 8192.0f, 8192.0f);
      }

      bool isDifferent(ScissorTest other)
      {
         if (enabled != other.enabled) return true;
         if (rect != other.rect) return true;

         return false;
      }

      public void apply()
      {
         if(isDifferent(Device.theRenderState.scissorTest) == true)
         {
            if (enabled == true)
            {
               GL.Enable(EnableCap.ScissorTest);
               GL.Scissor((int)rect.X, (int)rect.Y, (int)rect.Z, (int)rect.W);
            }
            else
            {
               GL.Disable(EnableCap.ScissorTest);
            }

            Device.theRenderState.scissorTest = this;
         }
      }
   }

   public class RenderState
   {
      //Things that make up a render state
      public AlphaToCoverage alphaToCoverage { get; set; }
      public Wireframe wireframe { get; set; }
      public Color4 color { get; set; }
      public PolygonOffset polygonOffset { get; set; }
      public ChannelMask channelMask { get; set; }
      public ScissorTest scissorTest { get; set; }
      public PrimitiveType primitiveType { get; set; }
      public PrimativeRestart primativeRestart { get; set; }

      int myIndexBuffer = 0;

      struct BindBufferInfo
      {
         public int id;
         public int location;
      }

      BindBufferInfo[] myUniformBuffers = new BindBufferInfo[5];
      int currentUniformBuffer = -1;
      BindBufferInfo[] myStorageBuffers = new BindBufferInfo[5];
      int currentStoragebuffer = -1;

      struct UniformUploadData
      {
         public UniformBufferObject ubo;
         public byte[] data;
      }

      UniformUploadData[] myUniformUploadData = new UniformUploadData[5];
      int currentUniformUpload = -1;

      public UniformData[] myUniforms = new UniformData[10];
      int currentUniformData = -1;

      public struct VertexInfo
      {
         public int id;
         public int location;
         public int offset;
         public int stride;
      }

      public VertexInfo[] myVertexBuffers = new VertexInfo[5];
      public int currentVertexBuffer = -1;

      struct ImageInfo
      {
         public int id;
         public int location;
         public TextureAccess access;
         public SizedInternalFormat format;
      }

      ImageInfo[] myImageBuffers = new ImageInfo[5];
      int currentImageBuffer = -1;

      public struct TextureInfo
      {
         public int id;
         public int location;
         public TextureTarget type;
      }

      public TextureInfo[] myTextures = new TextureInfo[5];
      public int currentTextureInfo = -1;

      public RenderState()
      {
         alphaToCoverage = new AlphaToCoverage();
         wireframe = new Wireframe();
         polygonOffset = new PolygonOffset();
         channelMask = new ChannelMask();
         scissorTest = new ScissorTest();
         primativeRestart = new PrimativeRestart();
      }

      public void reset()
      {
         // 			alphaToCoverage = new AlphaToCoverage();
         // 			wireframe = new Wireframe();
         // 			polygonOffset = new PolygonOffset();
         // 			channelMask = new ChannelMask();
         // 			scissorTest = new ScissorTest();

         currentImageBuffer = -1;
         currentStoragebuffer = -1;
         currentTextureInfo = -1;
         currentUniformBuffer = -1;
         currentUniformData = -1;
         currentUniformUpload = -1;
         currentVertexBuffer = -1;
         myIndexBuffer = 0;
      }

      public void resetUniforms()
      {
         currentUniformData = -1;
      }

      public void setUniform(UniformData ud)
      {
         currentUniformData++;
         myUniforms[currentUniformData] = ud;
      }

      public void setUniformUpload(UniformBufferObject ubo, byte[] data)
      {
         currentUniformUpload++;
         UniformUploadData uData;
         uData.ubo = ubo;
         uData.data = data;
         myUniformUploadData[currentUniformUpload] = uData;
      }

      public void setIndexBuffer(int ibo)
      {
         myIndexBuffer = ibo;
      }

      public void setVertexBuffer(int id, int location, int offset, int stride)
      {
         currentVertexBuffer++;
         VertexInfo vi;
         vi.id = id;
         vi.location = location;
         vi.offset = offset;
         vi.stride = stride;
         myVertexBuffers[currentVertexBuffer] = vi;
      }

      public void setTexture(int id, int location, TextureTarget type)
      {
         currentTextureInfo++;
         TextureInfo ti;
         ti.id = id;
         ti.location = location;
         ti.type = type;
         myTextures[currentTextureInfo] = ti;
      }

      public void setImageBuffer(int id, int location, TextureAccess access, SizedInternalFormat format)
      {
         currentImageBuffer++;
         ImageInfo ii;
         ii.id = id;
         ii.location = location;
         ii.access = access;
         ii.format = format;
         myImageBuffers[currentImageBuffer] = ii;
      }

      public void setUniformBuffer(int id, int location)
      {
         currentUniformBuffer++;
         BindBufferInfo ui;
         ui.id = id;
         ui.location = location;
         myUniformBuffers[currentUniformBuffer] = ui;
      }

      public void setStorageBuffer(int id, int location)
      {
         currentStoragebuffer++;
         BindBufferInfo si;
         si.id = id;
         si.location = location;
         myStorageBuffers[currentStoragebuffer] = si;
      }

      public void apply()
      {
         //apply render states.  these will check that a change is necessary and update the global renderstate in the device
         alphaToCoverage.apply();
         wireframe.apply();
         polygonOffset.apply();
         channelMask.apply();
         scissorTest.apply();
         primativeRestart.apply();

         //uniforms
         for (int i = 0; i <= currentUniformUpload; i++)
         {
            UniformUploadData uud = myUniformUploadData[i];
            uud.ubo.setData(uud.data);
         }

         for (int i = 0; i <= currentUniformData; i++)
         {
            Renderer.device.currentPipeline.shaderState.shaderProgram.setUniform(myUniforms[i]);
         }

         for (int i = 0; i <= currentUniformBuffer; i++)
         {
            BindBufferInfo ub = myUniformBuffers[i];
            Renderer.device.bindUniformBuffer(ub.id, ub.location);
         }

         for (int i = 0; i <= currentStoragebuffer; i++)
         {
            BindBufferInfo sb = myStorageBuffers[i];
            Renderer.device.bindStorageBuffer(sb.id, sb.location);
         }

         for (int i = 0; i <= currentImageBuffer; i++)
         {
            ImageInfo ii = myImageBuffers[i];
            Renderer.device.bindImageBuffer(ii.id, ii.location, ii.access, ii.format);
         }

         for (int i = 0; i <= currentVertexBuffer; i++)
         {
            VertexInfo vi = myVertexBuffers[i];
            Renderer.device.bindVertexBuffer(vi.id, vi.location, vi.offset, vi.stride);
         }

         for (int i = 0; i <= currentTextureInfo; i++)
         {
            TextureInfo ti = myTextures[i];
            Renderer.device.bindTexture(ti.id, ti.location, ti.type);
         }

         Renderer.device.bindIndexBuffer(myIndexBuffer);
      }

      public void force()
      {
         //alpha to coverage
         alphaToCoverage.apply();
         wireframe.apply();
         polygonOffset.apply();
         channelMask.apply();
         scissorTest.apply();
         primativeRestart.apply();

         //uniforms and buffers
         // 			foreach(UniformBufferData uData in myUniformData)
         // 			{
         // 				uData.ubo.setData(uData.data);
         // 			}
         // 
         // 			foreach (UniformData uni in myUniforms)
         // 			{
         // 				Renderer.device.currentPipeline.shaderProgram.setUniform(uni);
         // 			}
         // 			theRenderState.myUniforms = myUniforms;
         //
         // 			for (int i = 0; i < myUniformBuffers.Length; i++)
         // 				Renderer.device.bindUniformBuffer(myUniformBuffers[i], i);
         // 
         // 			for (int i = 0; i < myStorageBuffers.Length; i++)
         // 				Renderer.device.bindStorageBuffer(myStorageBuffers[i], i);
         // 
         // 			for (int i = 0; i < myImageBuffers.Length; i++)
         // 				Renderer.device.bindImageBuffer(myImageBuffers[i].id, i, myImageBuffers[i].access, myImageBuffers[i].format);
         // 
         // 			for (int i = 0; i < myVertexBuffers.Length; i++)
         // 				Renderer.device.bindVertexBuffer(myVertexBuffers[i].id, i, myVertexBuffers[i].offset, myVertexBuffers[i].stride);
         // 
         // 			for (int i = 0; i < myTextures.Length; i++)
         // 				Renderer.device.bindTexture(myTextures[i].id, i, myTextures[i].type);
         // 
         // 			Renderer.device.bindIndexBuffer(myIndexBuffer);
      }
   }
}