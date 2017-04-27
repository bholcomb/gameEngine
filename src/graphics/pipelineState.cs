using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
	public class Blending
	{
		public Blending()
		{
			enabled = false;
			equation = BlendEquationMode.FuncAdd;
			factorSrc = BlendingFactorSrc.SrcAlpha;
			factorDest = BlendingFactorDest.OneMinusSrcAlpha;
		}

		public bool isDifferent(Blending other)
		{
			if (enabled != other.enabled) return true;
			if (equation != other.equation) return true;
			if (factorDest != other.factorDest) return true;
			if (factorSrc != other.factorSrc) return true;

			return false;
		}

		public void apply()
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
		}

		public bool enabled { get; set; }
		public BlendEquationMode equation { get; set; }
		public BlendingFactorDest factorDest { get; set; }
		public BlendingFactorSrc factorSrc { get; set; }
	}

	public class DepthTest
	{
		public DepthTest()
		{
			enabled = true;
			depthFunc = DepthFunction.Less;
		}

		public bool isDifferent(DepthTest other)
		{
			if (enabled != other.enabled) return true;
			if (depthFunc != other.depthFunc) return true;

			return false;
		}

		public void apply()
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
		}

		public bool enabled { get; set; }
		public DepthFunction depthFunc { get; set; }
	}

	public class StencilTest
	{
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

		public bool isDifferent(StencilTest other)
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
		}

		public bool enabled { get; set; }
		public StencilFunction stencilFunction { get; set; }
		public StencilOp sfail { get; set; }
		public StencilOp dfail { get; set; }
		public StencilOp dpass { get; set; }
		public int value { get; set; }
		public int mask { get; set; }
	}

	//a pipeline state is used to render with a specific shader and state functions
	//blending/culling/depth testing/etc
	//VAO is used for vertex attribute binding/input step size
	//pipeline state is serialized into a 64bit id for easy comparison and lookup
	public class PipelineState
	{
		public UInt64 id;
		//pipeline state
		public ShaderProgram shaderProgram { get; set; }
		public Blending blending { get; set; }
		public DepthTest depthTest { get; set; }
		public bool depthWrite { get; set; }
		public Culling culling { get; set; }
		public StencilTest stencilTest { get; set; }

		//input assembler
		public VertexArrayObject vao { get; set; }
		
		protected static PipelineState thePipelineState;

		static PipelineState()
		{
			thePipelineState = new PipelineState();
			thePipelineState.force();
		}

		public PipelineState()
		{
			blending = new Blending();
			depthTest = new DepthTest();
		   depthWrite = true;
			culling = new Culling();
			stencilTest = new StencilTest();
		}

		public void generateId()
		{
			//blending is most important, so it gets the hight bits
			//if blending is enabled (i.e transparent stuff) then 
			//it will be sorted behind the opaque stuff (no transparent)
			byte blendBits = blending.enabled ? (byte)1 : (byte)0;
			id |= (UInt64)blendBits << 56;
			
			id |= (UInt64)shaderProgram.id << 24;
						
			byte depthWriteBits = depthWrite == true ? (byte)0x80 : (byte)0x00;
			id |= (UInt64)depthWriteBits << 16;

			byte depthTestBits = depthTest.enabled == true ? (byte)0x80 : (byte)0x00;
			id |= (UInt64)depthTestBits << 8;
		}


		public void apply()
		{
			//shaders and uniforms
			if (shaderProgramChanged() == true)
			{
				if (shaderProgram != null)
					shaderProgram.bind();

				thePipelineState.shaderProgram = shaderProgram;
			}

			//Vertex Array
			if(vaoChanged() == true)
			{
				if (vao != null)
				{
					vao.bind();
				}
				thePipelineState.vao = vao;
			}

			//blending
			if (blending.isDifferent(thePipelineState.blending) == true)
			{
				blending.apply();
				thePipelineState.blending = blending;
			}

			//depth testing
			if (depthTest.isDifferent(thePipelineState.depthTest) == true)
			{
				depthTest.apply();
				thePipelineState.depthTest = depthTest;
			}

			//depth writing
			Renderer.device.setDepthWrite(depthWrite);

			//cull mode
			if (culling.isDifferent(thePipelineState.culling) == true)
			{
				culling.apply();
				thePipelineState.culling = culling;
			}

			//stencil test
			if (stencilTest.isDifferent(thePipelineState.stencilTest) == true)
			{
				stencilTest.apply();
				thePipelineState.stencilTest = stencilTest;
			}
		}

		public void force()
		{
			blending.apply();
			thePipelineState.blending = blending;

			//depth testing
			depthTest.apply();
			thePipelineState.depthTest = depthTest;

			//depth writing
			Renderer.device.setDepthWrite(depthWrite);

			//cull mode
			culling.apply();
			thePipelineState.culling = culling;

			//stencil test
			stencilTest.apply();
			thePipelineState.stencilTest = stencilTest;

			//shaders and uniforms
			if (shaderProgram != null)
			{
				shaderProgram.bind();
			}
			thePipelineState.shaderProgram = shaderProgram;

			//Vertex Array
			if (vao != null)
			{
				vao.bind();
			}
			thePipelineState.vao = vao;
		}

		#region pipeline state changed functions

		bool shaderProgramChanged()
		{
			if (thePipelineState.shaderProgram == null && shaderProgram == null) return false;
			if (thePipelineState.shaderProgram == null && shaderProgram != null) return true;
			if (thePipelineState.shaderProgram != null && shaderProgram == null) return true;
			if (thePipelineState.shaderProgram.id != shaderProgram.id) return true;
			return false;
		}

		bool vaoChanged()
		{
			if ((thePipelineState.vao == null) && (vao == null)) return false;
			if ((thePipelineState.vao != null) && (vao == null)) return true;
			if ((thePipelineState.vao == null) && (vao != null)) return true;
			if (thePipelineState.vao.id != vao.id) return true;
			return false;
		}

#endregion
	}
}