using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public class Blending
   {
      public bool enabled { get; set; }
      public BlendEquationMode equation { get; set; }
      public BlendingFactorDest factorDest { get; set; }
      public BlendingFactorSrc factorSrc { get; set; }

      public Blending()
      {
         enabled = false;
         equation = BlendEquationMode.FuncAdd;
         factorSrc = BlendingFactorSrc.SrcAlpha;
         factorDest = BlendingFactorDest.OneMinusSrcAlpha;
      }

      bool isDifferent(Blending other)
      {
         if (enabled != other.enabled) return true;
         if (equation != other.equation) return true;
         if (factorDest != other.factorDest) return true;
         if (factorSrc != other.factorSrc) return true;

         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.thePipelineState.blending) == true)
         {
            if (enabled)
            {
               GL.Enable(EnableCap.Blend);
               GL.BlendEquation(equation);
               GL.BlendFunc(factorSrc, factorDest);
            }
            else
            {
               GL.Disable(EnableCap.Blend);
            }

            Device.thePipelineState.blending = this;
         }
      }
   }

   public class Culling
   {
      public bool enabled { get; set; }
      public CullFaceMode cullMode { get; set; }
      public FrontFaceDirection frontFaceDir { get; set; }

      public Culling()
      {
         enabled = true;
         cullMode = CullFaceMode.Back;
         frontFaceDir = FrontFaceDirection.Ccw;
      }

      bool isDifferent(Culling other)
      {
         if (enabled != other.enabled) return true;
         if (cullMode != other.cullMode) return true;
         if (frontFaceDir != other.frontFaceDir) return true;
         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.thePipelineState.culling) == true)
         {
            if (enabled)
            {
               GL.Enable(EnableCap.CullFace);
               GL.CullFace(cullMode);
               GL.FrontFace(frontFaceDir);
            }
            else
            {
               GL.Disable(EnableCap.CullFace);
            }

            Device.thePipelineState.culling = this;
         }
      }
   }

   public class DepthTest
   {
      public bool enabled { get; set; }
      public DepthFunction depthFunc { get; set; }

      public DepthTest()
      {
         enabled = true;
         depthFunc = DepthFunction.Less;
      }

      bool isDifferent(DepthTest other)
      {
         if (enabled != other.enabled) return true;
         if (depthFunc != other.depthFunc) return true;

         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.thePipelineState.depthTest) == true)
         {
            if (enabled)
            {
               GL.Enable(EnableCap.DepthTest);
               GL.DepthFunc(depthFunc);
            }
            else
            {
               GL.Disable(EnableCap.DepthTest);
            }

            Device.thePipelineState.depthTest = this;
         }
      }
   }

   public class DepthWrite
   {
      public bool enabled { get; set; }
      public DepthWrite()
      {
         enabled = true;
      }

      bool isDifferent(DepthWrite other)
      {
         if (enabled != other.enabled) return true;
         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.thePipelineState.depthWrite) == true)
         {
            GL.DepthMask(enabled);

            Device.thePipelineState.depthWrite = this;
         }
      }
   }

   public class StencilTest
   {
      public bool enabled { get; set; }
      public StencilFunction stencilFunction { get; set; }
      public StencilOp sfail { get; set; }
      public StencilOp dfail { get; set; }
      public StencilOp dpass { get; set; }
      public int value { get; set; }
      public int mask { get; set; }

      public StencilTest()
      {
         enabled = false;
         stencilFunction = StencilFunction.Equal;
         sfail = StencilOp.Keep;
         dfail = StencilOp.Keep;
         dpass = StencilOp.Keep;
         value = 0;
         mask = 0;
      }

      bool isDifferent(StencilTest other)
      {
         if (enabled != other.enabled) return true;
         if (stencilFunction != other.stencilFunction) return true;
         if (sfail != other.sfail) return true;
         if (dfail != other.dfail) return true;
         if (dpass != other.dpass) return true;
         if (value != other.value) return true;
         if (mask != other.mask) return true;
         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.thePipelineState.stencilTest) == true)
         {
            if (enabled == true)
            {
               GL.Enable(EnableCap.StencilTest);
               GL.StencilFunc(stencilFunction, value, mask);
               GL.StencilOp(sfail, dfail, dpass);
            }
            else
            {
               GL.Disable(EnableCap.StencilTest);
            }

            Device.thePipelineState.stencilTest = this;
         }
      }
   }

   public class ShaderState
   {
      public ShaderProgram shaderProgram {get; set;}

      public ShaderState()
      {

      }

      bool isDifferent(ShaderState state)
      {
         if (state.shaderProgram == null && shaderProgram == null) return false;
         if (state.shaderProgram == null && shaderProgram != null) return true;
         if (state.shaderProgram != null && shaderProgram == null) return true;
         if (state.shaderProgram.id != shaderProgram.id) return true;
         return false;
      }

      public void apply()
      {
         if(isDifferent(Device.thePipelineState.shaderState) == true)
         {
            if (shaderProgram != null)
               shaderProgram.bind();

            Device.thePipelineState.shaderState = this;
         }
      }
   }

   public class VaoState
   {
      public VertexArrayObject vao { get; set; }

      public VaoState()
      {

      }

      bool isDifferent(VaoState state)
      {
         if ((state.vao == null) && (vao == null)) return false;
         if ((state.vao != null) && (vao == null)) return true;
         if ((state.vao == null) && (vao != null)) return true;
         if ( state.vao.id != vao.id) return true;
         return false;
      }

      public void apply()
      {
         if (isDifferent(Device.thePipelineState.vaoState) == true)
         {
            if (vao != null)
            {
               vao.bind();
               Renderer.device.resetVboIboState();
            }

            Device.thePipelineState.vaoState = this;
         }
      }
   }

   //a pipeline state is used to render with a specific shader and state functions
   //blending/culling/depth testing/etc
   //VAO is used for vertex attribute binding/input step size
   //pipeline state is serialized into a 64bit id for easy comparison and lookup
   public class PipelineState
	{
		public UInt64 id;
		//pipeline state
		public ShaderState shaderState { get; set; }
		public Blending blending { get; set; }
		public DepthTest depthTest { get; set; }
      public DepthWrite depthWrite { get; set; }
		public Culling culling { get; set; }
		public StencilTest stencilTest { get; set; }

		//input assembler
		public VaoState vaoState { get; set; }
		

		public PipelineState()
		{
         shaderState = new ShaderState();
			blending = new Blending();
			depthTest = new DepthTest();
		   depthWrite = new DepthWrite();
			culling = new Culling();
			stencilTest = new StencilTest();
         vaoState = new VaoState();
		}

		public void generateId()
		{
			//blending is most important, so it gets the hight bits
			//if blending is enabled (i.e transparent stuff) then 
			//it will be sorted behind the opaque stuff (no transparent)
			byte blendBits = blending.enabled ? (byte)1 : (byte)0;
			id |= (UInt64)blendBits << 56;
			
			id |= (UInt64)shaderState.shaderProgram.id << 24;
						
			byte depthWriteBits = depthWrite.enabled == true ? (byte)0x80 : (byte)0x00;
			id |= (UInt64)depthWriteBits << 16;

			byte depthTestBits = depthTest.enabled == true ? (byte)0x80 : (byte)0x00;
			id |= (UInt64)depthTestBits << 8;
		}


		public void apply()
		{
         //apply pipeline states.  these will check that a change is necessary and update the global pipelines state in the device
         blending.apply();
         depthTest.apply();
         depthWrite.apply();
         culling.apply();
         stencilTest.apply();
         shaderState.apply();
         vaoState.apply();
		}

		public void force()
		{
         blending.apply();
         depthTest.apply();
         depthWrite.apply();
         culling.apply();
         stencilTest.apply();
         shaderState.apply();
         vaoState.apply();
		}
	}
}