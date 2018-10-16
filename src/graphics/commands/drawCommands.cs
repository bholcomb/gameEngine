using System;
using System.Collections;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public class RenderTexture2dCommand : StatelessRenderCommand
	{
		Vector2 myMin;
		Vector2 myMax;
		float myAlpha;
		V3T2B4[] myVerts;

		static VertexBufferObject theVBO;
		static IndexBufferObject theIBO;
		static VertexArrayObject theVAO;
		static ShaderProgram theShader;

		static RenderTexture2dCommand()
		{
			//setup the shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Graphics.shaders.draw-ps.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
			theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

			theVBO = new VertexBufferObject(BufferUsageHint.DynamicDraw);
			theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);

			ushort[] index = { 0, 1, 2, 0, 2, 3 };
			theIBO.setData(index);

			//setup the vao
			theVAO = new VertexArrayObject();
			theVAO.bindVertexFormat(theShader, V3T2B4.bindings());
		}

		public RenderTexture2dCommand(Vector2 min, Vector2 max, Texture t, float alpha = 1.0f, bool isDepthBuffer = false)
			: base()
		{
			myMin = min;
			myMax = max;
			myAlpha = alpha;

			myVerts = new V3T2B4[4];
			myVerts[0].Position = new Vector3(min.X, min.Y, 0); myVerts[0].TexCoord = new Vector2(0, 0); myVerts[0].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
			myVerts[1].Position = new Vector3(max.X, min.Y, 0); myVerts[1].TexCoord = new Vector2(1, 0); myVerts[1].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
			myVerts[2].Position = new Vector3(max.X, max.Y, 0); myVerts[2].TexCoord = new Vector2(1, 1); myVerts[2].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
			myVerts[3].Position = new Vector3(min.X, max.Y, 0); myVerts[3].TexCoord = new Vector2(0, 1); myVerts[3].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();

         bool isSingleChannel = t.pixelFormat == PixelInternalFormat.R32f;

			renderState.setTexture(t.id(), 0, t.target);
			renderState.setIndexBuffer(theIBO.id);
			renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, false));
			renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
			renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0));
			renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, isDepthBuffer));
         renderState.setUniform(new UniformData(26, Uniform.UniformType.Bool, isSingleChannel));

			pipelineState = new PipelineState();
			pipelineState.shaderState.shaderProgram = theShader;
			pipelineState.vaoState.vao = theVAO;
			pipelineState.depthTest.enabled = false;
			pipelineState.blending.enabled = alpha < 1.0f;
			pipelineState.generateId();
		}

		public RenderTexture2dCommand(Vector2 min, Vector2 max, TextureBufferObject t, float alpha = 1.0f, bool isDepthBuffer = false)
			: base()
		{
			myMin = min;
			myMax = max;
			myAlpha = alpha;

			myVerts = new V3T2B4[4];
			myVerts[0].Position = new Vector3(min.X, min.Y, 0); myVerts[0].TexCoord = new Vector2(0, 0); myVerts[0].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
			myVerts[1].Position = new Vector3(max.X, min.Y, 0); myVerts[1].TexCoord = new Vector2(1, 0); myVerts[1].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
			myVerts[2].Position = new Vector3(max.X, max.Y, 0); myVerts[2].TexCoord = new Vector2(1, 1); myVerts[2].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
			myVerts[3].Position = new Vector3(min.X, max.Y, 0); myVerts[3].TexCoord = new Vector2(0, 1); myVerts[3].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();

         renderState.setTexture(t.id, 0, TextureTarget.Texture2D);
         renderState.setIndexBuffer(theIBO.id);
         renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
         renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, false));
         renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
         renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0));         
         renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, isDepthBuffer));

         pipelineState = new PipelineState();
         pipelineState.shaderState.shaderProgram = theShader;
         pipelineState.vaoState.vao = theVAO;
         pipelineState.depthTest.enabled = false;
         pipelineState.blending.enabled = alpha < 1.0f;
         pipelineState.generateId();
      }

		public RenderTexture2dCommand(Vector2 min, Vector2 max, ArrayTexture t, int idx = 0, float alpha = 1.0f, bool isDepthBuffer = false)
			: base()
		{
			myMin = min;
			myMax = max;
			myAlpha = alpha;

			myVerts = new V3T2B4[4];
         myVerts[0].Position = new Vector3(min.X, min.Y, 0); myVerts[0].TexCoord = new Vector2(0, 0); myVerts[0].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
         myVerts[1].Position = new Vector3(max.X, min.Y, 0); myVerts[1].TexCoord = new Vector2(1, 0); myVerts[1].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
         myVerts[2].Position = new Vector3(max.X, max.Y, 0); myVerts[2].TexCoord = new Vector2(1, 1); myVerts[2].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
         myVerts[3].Position = new Vector3(min.X, max.Y, 0); myVerts[3].TexCoord = new Vector2(0, 1); myVerts[3].Color = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();

         renderState.setTexture(t.id(), 0, t.target);
         renderState.setIndexBuffer(theIBO.id);
         renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
         renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, false));
         renderState.setUniform(new UniformData(22, Uniform.UniformType.Int, 0));
         renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 2));
         renderState.setUniform(new UniformData(24, Uniform.UniformType.Int, idx));
         renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, isDepthBuffer));

         pipelineState = new PipelineState();
         pipelineState.shaderState.shaderProgram = theShader;
         pipelineState.vaoState.vao = theVAO;
         pipelineState.depthTest.enabled = false;
         pipelineState.blending.enabled = alpha < 1.0f;
         pipelineState.culling.enabled = false;
         pipelineState.generateId();
      }

		public override void execute()
		{
			//update the buffer with this command's draw data
			theVBO.setData(myVerts);

			base.execute();

			GL.DrawElements(PrimitiveType.Triangles, theIBO.count, DrawElementsType.UnsignedShort, 0);
		}
	}

	public class RenderTextureCubeCommand : StatelessRenderCommand
	{
		V3T2B4[] myVerts;

		static VertexBufferObject theVBO;
		static IndexBufferObject theIBO;
		static VertexArrayObject theVAO;
		static ShaderProgram theShader;

		static RenderTextureCubeCommand()
		{
			//setup the shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Graphics.shaders.draw-ps.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
			theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

			theVBO = new VertexBufferObject(BufferUsageHint.DynamicDraw);
			theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);

			ushort[] index ={0, 1, 2,
								0, 2, 3, //front
                        4, 5, 6,
								4,6, 7, //left
                        8,9,10,
								8,10,11, //right
                        12,13,14,
								12,14,15, // top 
                        16,17,18,
								16,18,19, // bottom
                        20,21,22,
								20,22,23  // back
                        };
			theIBO.setData(index);

			//setup the vao
			theVAO = new VertexArrayObject();
			theVAO.bindVertexFormat(theShader, V3T2B4.bindings());
		}

		public RenderTextureCubeCommand(Vector3 min, Vector3 max, Texture t, float alpha = 1.0f)
			: base()
		{
			myVerts = new V3T2B4[24];
			V3T2[] verts = new V3T2[24];
			UInt32 col = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
			//front face
			myVerts[0].Position = new Vector3(max.X, min.Y, min.Z);	myVerts[0].TexCoord = new Vector2(0, 0);	myVerts[0].Color = col;
			myVerts[1].Position = new Vector3(min.X, min.Y, min.Z);	myVerts[1].TexCoord = new Vector2(1, 0);	myVerts[1].Color = col;
			myVerts[2].Position = new Vector3(min.X, max.Y, min.Z);	myVerts[2].TexCoord = new Vector2(1, 1);	myVerts[2].Color = col;
			myVerts[3].Position = new Vector3(max.X, max.Y, min.Z);	myVerts[3].TexCoord = new Vector2(0, 1);	myVerts[3].Color = col;

			//left
			myVerts[4].Position = new Vector3(min.X, min.Y, min.Z);	myVerts[4].TexCoord = new Vector2(0, 0);	myVerts[4].Color = col;
			myVerts[5].Position = new Vector3(min.X, min.Y, max.Z);	myVerts[5].TexCoord = new Vector2(1, 0);	myVerts[5].Color = col;
			myVerts[6].Position = new Vector3(min.X, max.Y, max.Z);	myVerts[6].TexCoord = new Vector2(1, 1);	myVerts[6].Color = col;
			myVerts[7].Position = new Vector3(min.X, max.Y, min.Z);	myVerts[7].TexCoord = new Vector2(0, 1);	myVerts[7].Color = col;

			//right
			myVerts[8].Position  = new Vector3(max.X, min.Y, max.Z);  myVerts[8].TexCoord = new Vector2(0, 0);  myVerts[8].Color = col;
			myVerts[9].Position  = new Vector3(max.X, min.Y, min.Z);  myVerts[9].TexCoord = new Vector2(1, 0);  myVerts[9].Color = col;
			myVerts[10].Position = new Vector3(max.X, max.Y, min.Z);  myVerts[10].TexCoord = new Vector2(1, 1); myVerts[10].Color = col;
			myVerts[11].Position = new Vector3(max.X, max.Y, max.Z);  myVerts[11].TexCoord = new Vector2(0, 1); myVerts[11].Color = col;

			//top
			myVerts[12].Position = new Vector3(min.X, max.Y, max.Z);  myVerts[12].TexCoord = new Vector2(0, 0); myVerts[12].Color = col;
			myVerts[13].Position = new Vector3(max.X, max.Y, max.Z);  myVerts[13].TexCoord = new Vector2(1, 0); myVerts[13].Color = col;
			myVerts[14].Position = new Vector3(max.X, max.Y, min.Z);  myVerts[14].TexCoord = new Vector2(1, 1); myVerts[14].Color = col;
			myVerts[15].Position = new Vector3(min.X, max.Y, min.Z);  myVerts[15].TexCoord = new Vector2(0, 1); myVerts[15].Color = col;

			//bottom
			myVerts[16].Position = new Vector3(min.X, min.Y, min.Z);  myVerts[16].TexCoord = new Vector2(0, 0); myVerts[16].Color = col;
			myVerts[17].Position = new Vector3(max.X, min.Y, min.Z);  myVerts[17].TexCoord = new Vector2(1, 0); myVerts[17].Color = col;
			myVerts[18].Position = new Vector3(max.X, min.Y, max.Z);  myVerts[18].TexCoord = new Vector2(1, 1); myVerts[18].Color = col;
			myVerts[19].Position = new Vector3(min.X, min.Y, max.Z);  myVerts[19].TexCoord = new Vector2(0, 1); myVerts[19].Color = col;

			//back
			myVerts[20].Position = new Vector3(min.X, min.Y, max.Z);  myVerts[20].TexCoord = new Vector2(0, 0); myVerts[20].Color = col;
			myVerts[21].Position = new Vector3(max.X, min.Y, max.Z);  myVerts[21].TexCoord = new Vector2(1, 0); myVerts[21].Color = col;
			myVerts[22].Position = new Vector3(max.X, max.Y, max.Z);  myVerts[22].TexCoord = new Vector2(1, 1); myVerts[22].Color = col;
			myVerts[23].Position = new Vector3(min.X, max.Y, max.Z);  myVerts[23].TexCoord = new Vector2(0, 1); myVerts[23].Color = col;

			renderState.setTexture(t.id(), 0, t.target);
			renderState.setIndexBuffer(theIBO.id);
			renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, true));
			renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
			renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0));
			renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, false));

			pipelineState = new PipelineState();
			pipelineState.shaderState.shaderProgram = theShader;
			pipelineState.vaoState.vao = theVAO;
			pipelineState.depthTest.enabled = false;
			pipelineState.blending.enabled = t.hasAlpha || alpha < 1.0f;
			pipelineState.generateId();
		}

		public override void execute()
		{
			//update the buffer with this command's draw data
			theVBO.setData(myVerts);

			base.execute();

			GL.DrawElements(PrimitiveType.Triangles, theIBO.count, DrawElementsType.UnsignedShort, 0);
		}
	}

	public class RenderWireframeCubeCommand : StatelessRenderCommand
	{
		V3T2B4[] myVerts;

		static VertexBufferObject theVBO;
		static IndexBufferObject theIBO;
		static VertexArrayObject theVAO;
		static ShaderProgram theShader;

		static UInt16 PRIM_RESTART = 0xFFFF;

		static RenderWireframeCubeCommand()
		{
			//setup the shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Graphics.shaders.draw-ps.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
			theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

			theVBO = new VertexBufferObject(BufferUsageHint.DynamicDraw);
			theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);

			ushort[] index = {
				4, 6, 2, 3, PRIM_RESTART,
				1, 3, 7, 6, PRIM_RESTART,
				7, 5, 1, 0, PRIM_RESTART,
				2, 0, 4, 5};

			theIBO.setData(index);

			//setup the vao
			theVAO = new VertexArrayObject();
         theVAO.bindVertexFormat(theShader, V3T2B4.bindings());
      }

		public RenderWireframeCubeCommand(Vector3 min, Vector3 max, Color4 c)
			: base()
		{
			myVerts = new V3T2B4[8];
			UInt32 col = c.toUInt();
			myVerts[0].Position = new Vector3(min.X, min.Y, min.Z); myVerts[0].TexCoord = Vector2.Zero; myVerts[0].Color = col;
			myVerts[1].Position = new Vector3(max.X, min.Y, min.Z); myVerts[1].TexCoord = Vector2.Zero; myVerts[1].Color = col;
			myVerts[2].Position = new Vector3(min.X, max.Y, min.Z); myVerts[2].TexCoord = Vector2.Zero; myVerts[2].Color = col;
			myVerts[3].Position = new Vector3(max.X, max.Y, min.Z); myVerts[3].TexCoord = Vector2.Zero; myVerts[3].Color = col;
			myVerts[4].Position = new Vector3(min.X, min.Y, max.Z); myVerts[4].TexCoord = Vector2.Zero; myVerts[4].Color = col;
			myVerts[5].Position = new Vector3(max.X, min.Y, max.Z); myVerts[5].TexCoord = Vector2.Zero; myVerts[5].Color = col;
			myVerts[6].Position = new Vector3(min.X, max.Y, max.Z); myVerts[6].TexCoord = Vector2.Zero; myVerts[6].Color = col;
			myVerts[7].Position = new Vector3(max.X, max.Y, max.Z); myVerts[7].TexCoord = Vector2.Zero; myVerts[7].Color = col;

			renderState.setIndexBuffer(theIBO.id);
			renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, true)); //is 3d
			renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, -1)); //no texture
			renderState.primativeRestart.enabled = true;
			renderState.primativeRestart.value = PRIM_RESTART;

			pipelineState = new PipelineState();
			pipelineState.shaderState.shaderProgram = theShader;
			pipelineState.vaoState.vao = theVAO;
			pipelineState.depthTest.enabled = false;
			pipelineState.blending.enabled = c.A < 1.0f;
			pipelineState.generateId();
		}

		public override void execute()
		{
			//update the buffer with this command's draw data
			theVBO.setData(myVerts);

			base.execute();

			GL.DrawElements(PrimitiveType.LineStrip, theIBO.count, DrawElementsType.UnsignedShort, 0);
		}
	}

   public class RenderFrustumCommand : StatelessRenderCommand
   {
      V3T2B4[] myVerts;

      static Vector4[] theVerts = new Vector4[8];
      static VertexBufferObject theVBO;
      static IndexBufferObject theIBO;
      static VertexArrayObject theVAO;
      static ShaderProgram theShader;

      static UInt16 PRIM_RESTART = 0xFFFF;

      static RenderFrustumCommand()
      {
         //setup the shader
         List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
         desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
         desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Graphics.shaders.draw-ps.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
         theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         theVBO = new VertexBufferObject(BufferUsageHint.DynamicDraw);
         theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);

         ushort[] index = {
            0, 1, 3, 2, PRIM_RESTART,
            5, 1, 3, 7, PRIM_RESTART,
            4, 5, 7, 6, PRIM_RESTART,
            0, 4, 6, 2};

         theVerts[0] = new Vector4(-1, -1, -1, 1);
         theVerts[1] = new Vector4(1, -1, -1, 1);
         theVerts[2] = new Vector4(-1, 1, -1, 1);
         theVerts[3] = new Vector4(1, 1, -1, 1);

         theVerts[4] = new Vector4(-1, -1, 1, 1);
         theVerts[5] = new Vector4(1, -1, 1, 1);
         theVerts[6] = new Vector4(-1, 1, 1, 1);
         theVerts[7] = new Vector4(1, 1, 1, 1);

         theIBO.setData(index);

         //setup the vao
         theVAO = new VertexArrayObject();
         theVAO.bindVertexFormat(theShader, V3T2B4.bindings());
      }

      public RenderFrustumCommand(Matrix4 viewProj, Color4 c)
         : base()
      {
         Matrix4 inv = viewProj.Inverted();
         myVerts = new V3T2B4[8];
         UInt32 col = c.toUInt();

         myVerts[0].Position = (theVerts[0] * inv).Xyz; myVerts[0].TexCoord = Vector2.Zero; myVerts[0].Color = col;
         myVerts[1].Position = (theVerts[1] * inv).Xyz; myVerts[1].TexCoord = Vector2.Zero; myVerts[1].Color = col;
         myVerts[2].Position = (theVerts[2] * inv).Xyz; myVerts[2].TexCoord = Vector2.Zero; myVerts[2].Color = col;
         myVerts[3].Position = (theVerts[3] * inv).Xyz; myVerts[3].TexCoord = Vector2.Zero; myVerts[3].Color = col;
         myVerts[4].Position = (theVerts[4] * inv).Xyz; myVerts[4].TexCoord = Vector2.Zero; myVerts[4].Color = col;
         myVerts[5].Position = (theVerts[5] * inv).Xyz; myVerts[5].TexCoord = Vector2.Zero; myVerts[5].Color = col;
         myVerts[6].Position = (theVerts[6] * inv).Xyz; myVerts[6].TexCoord = Vector2.Zero; myVerts[6].Color = col;
         myVerts[7].Position = (theVerts[7] * inv).Xyz; myVerts[7].TexCoord = Vector2.Zero; myVerts[7].Color = col;

         renderState.setIndexBuffer(theIBO.id);
         renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
         renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, true)); //is 3d
         renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, -1)); //no texture
         renderState.primativeRestart.enabled = true;
         renderState.primativeRestart.value = PRIM_RESTART;

         pipelineState = new PipelineState();
         pipelineState.shaderState.shaderProgram = theShader;
         pipelineState.vaoState.vao = theVAO;
         pipelineState.depthTest.enabled = false;
         pipelineState.blending.enabled = c.A < 1.0f;
         pipelineState.generateId();
      }

      public override void execute()
      {
         //update the buffer with this command's draw data
         theVBO.setData(myVerts);

         base.execute();

         GL.DrawElements(PrimitiveType.LineStrip, theIBO.count, DrawElementsType.UnsignedShort, 0);
      }
   }


   public class RenderTexturedQuadCommand : StatelessRenderCommand
	{
		V3T2B4[] myVerts;

		static VertexBufferObject theVBO;
		static IndexBufferObject theIBO;
		static VertexArrayObject theVAO;
		static ShaderProgram theShader;

		static RenderTexturedQuadCommand()
		{
			//setup the shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Graphics.shaders.draw-ps.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
			theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

			theVBO = new VertexBufferObject(BufferUsageHint.DynamicDraw);
			theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);

			
			ushort[] index ={0,1,2,
								  0,2,3
								 };

			theIBO.setData(index);

			//setup the vao
			theVAO = new VertexArrayObject();
         theVAO.bindVertexFormat(theShader, V3T2B4.bindings());
      }

		public RenderTexturedQuadCommand(Vector3[] verts, Texture t, float alpha = 1.0f)
		{
			UInt32 col = new Color4(1.0f, 1.0f, 1.0f, alpha).toUInt();
			myVerts = new V3T2B4[4];
			myVerts[0].Position = verts[0]; myVerts[0].TexCoord = new Vector2(0, 0); myVerts[0].Color = col;
			myVerts[1].Position = verts[1]; myVerts[1].TexCoord = new Vector2(1, 0); myVerts[1].Color = col;
			myVerts[2].Position = verts[2]; myVerts[2].TexCoord = new Vector2(1, 1); myVerts[2].Color = col;
			myVerts[3].Position = verts[3]; myVerts[3].TexCoord = new Vector2(0, 1); myVerts[3].Color = col;

			renderState.setTexture(t.id(), 0, t.target);

			pipelineState = new PipelineState();
			if (t.hasAlpha == true || alpha < 1.0f)
				pipelineState.blending.enabled = true;
			pipelineState.shaderState.shaderProgram = theShader;
			pipelineState.vaoState.vao = theVAO;
			pipelineState.depthTest.enabled = false;
			pipelineState.culling.enabled = false;
			pipelineState.generateId();

			renderState.setIndexBuffer(theIBO.id);
			renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, true));
			renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
			renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0));
			renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, false));
		}

		public override void execute()
		{
			//update the buffer with this command's draw data
			theVBO.setData(myVerts);

			base.execute();

			GL.DrawElements(PrimitiveType.Triangles, theIBO.count, DrawElementsType.UnsignedShort, 0);
		}
	}

	public class RenderQuadCommand : StatelessRenderCommand
	{
		V3T2B4[] myVerts;

		static VertexBufferObject theVBO;
		static IndexBufferObject theIBO;
		static VertexArrayObject theVAO;
		static ShaderProgram theShader;

		static RenderQuadCommand()
		{
			//setup the shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Graphics.shaders.draw-ps.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
			theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

			theVBO = new VertexBufferObject(BufferUsageHint.DynamicDraw);
			theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);


			ushort[] index ={0,1,2,
								  0,2,3
								 };

			theIBO.setData(index);

			//setup the vao
			theVAO = new VertexArrayObject();
         theVAO.bindVertexFormat(theShader, V3T2B4.bindings());
      }

		public RenderQuadCommand(Vector3[] verts, Color4 c)
		{
			UInt32 col = c.toUInt();
			myVerts = new V3T2B4[4];
			myVerts[0].Position = verts[0]; myVerts[0].TexCoord = new Vector2(0, 0); myVerts[0].Color = col;
			myVerts[1].Position = verts[1]; myVerts[1].TexCoord = new Vector2(0, 1); myVerts[1].Color = col;
			myVerts[2].Position = verts[2]; myVerts[2].TexCoord = new Vector2(1, 1); myVerts[2].Color = col;
			myVerts[3].Position = verts[3]; myVerts[3].TexCoord = new Vector2(1, 0); myVerts[3].Color = col;

			pipelineState = new PipelineState();
			pipelineState.blending.enabled = c.A < 1.0f;
			pipelineState.shaderState.shaderProgram = theShader;
			pipelineState.vaoState.vao = theVAO;
			pipelineState.depthTest.enabled = false;
			pipelineState.generateId();

			renderState.setIndexBuffer(theIBO.id);
			renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, true));
			renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
			renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0));
			renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, false));
		}

		public override void execute()
		{
			//update the buffer with this command's draw data
			theVBO.setData(myVerts);

			base.execute();

			GL.DrawElements(PrimitiveType.Triangles, theIBO.count, DrawElementsType.UnsignedShort, 0);
		}
	}

	public class RenderLineCommand : StatelessRenderCommand
	{
		V3T2B4[] myVerts;

		static VertexBufferObject theVBO;
		static IndexBufferObject theIBO;
		static VertexArrayObject theVAO;
		static ShaderProgram theShader;

		static RenderLineCommand()
		{
			//setup the shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Graphics.shaders.draw-ps.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
			theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

			theVBO = new VertexBufferObject(BufferUsageHint.DynamicDraw);
			theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);


			ushort[] index ={0,1};

			theIBO.setData(index);

			//setup the vao
			theVAO = new VertexArrayObject();
         theVAO.bindVertexFormat(theShader, V3T2B4.bindings());
      }

		public RenderLineCommand(Vector3 start, Vector3 end, Color4 c)
		{
			UInt32 col = c.toUInt();
			myVerts = new V3T2B4[2];
			myVerts[0].Position = start; myVerts[0].TexCoord = new Vector2(0, 0); myVerts[0].Color = col;
			myVerts[1].Position = end; myVerts[1].TexCoord = new Vector2(0, 1); myVerts[1].Color = col;
			
			pipelineState = new PipelineState();
			pipelineState.blending.enabled = c.A < 1.0f;
			pipelineState.shaderState.shaderProgram = theShader;
			pipelineState.vaoState.vao = theVAO;
			pipelineState.depthTest.enabled = false;
			pipelineState.generateId();

			renderState.setIndexBuffer(theIBO.id);
			renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, true));
			renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, -1));
		}

		public override void execute()
		{
			//update the buffer with this command's draw data
			theVBO.setData(myVerts);

			base.execute();

			GL.DrawElements(PrimitiveType.Lines, theIBO.count, DrawElementsType.UnsignedShort, 0);
		}
	}

	public class RenderSphereCommand : StatelessRenderCommand
	{
		V3T2B4[] myVerts;

		static int theSphereVertCount = 121;
		static int theSphereIndexCount = 600;
		static Vector3[] theSphereVerts = new Vector3[theSphereVertCount];
		static ushort[] theSphereIndexes = new ushort[theSphereIndexCount];
		static VertexBufferObject theVBO;
		static IndexBufferObject theIBO;
		static VertexArrayObject theVAO;
		static ShaderProgram theShader;

		static RenderSphereCommand()
		{
			//setup the shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Graphics.shaders.draw-vs.glsl"));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Graphics.shaders.draw-ps.glsl"));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
			theShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

			theVBO = new VertexBufferObject(BufferUsageHint.DynamicDraw);
			theIBO = new IndexBufferObject(BufferUsageHint.StaticDraw);

			createTheSphere();

			theIBO.setData(theSphereIndexes);

			//setup the vao
			theVAO = new VertexArrayObject();
         theVAO.bindVertexFormat(theShader, V3T2B4.bindings());
      }

		static void createTheSphere()
		{
			int lats = 10;
			int longs = 10;
			int vertIdx = 0;
			for (int i = 0; i <= lats; i++)
			{
				float theta = i * (float)Math.PI / (float)lats;
				float sinTheta = (float)Math.Sin(theta);
				float cosTheta = (float)Math.Cos(theta);

				for (int j = 0; j <= longs; j++)
				{
					float phi = j * (float)(Math.PI * 2.0) / (float)longs;
					float sinPhi = (float)Math.Sin(phi);
					float cosPhi = (float)Math.Cos(phi);

					float x = cosPhi * sinTheta;
					float y = sinPhi * sinTheta;
					float z = cosTheta;
					//          float u = 1 - (j / longs);
					//          float v = 1- (i / lats);

					theSphereVerts[vertIdx++] = new Vector3(x, y, z);
				}
			}

			int indexIdx = 0;
			for (int i = 0; i < lats; i++)
			{
				for (int j = 0; j < longs; j++)
				{
					ushort first = (ushort)((i * (longs + 1)) + j);
					ushort second = (ushort)(first + longs + 1);
					theSphereIndexes[indexIdx++] = first; theSphereIndexes[indexIdx++] = second; theSphereIndexes[indexIdx++] = (ushort)(first + 1);
					theSphereIndexes[indexIdx++] = second; theSphereIndexes[indexIdx++] = (ushort)(second + 1); theSphereIndexes[indexIdx++] = (ushort)(first + 1);
				}
			}
		}

		public RenderSphereCommand(Vector3 position, float radius, Color4 c)
		{
			UInt32 col = c.toUInt();
			myVerts = new V3T2B4[theSphereVertCount];
			for (int i = 0; i < theSphereVertCount; i++)
			{
				myVerts[i].Position = position + (theSphereVerts[i] * radius); myVerts[i].TexCoord = Vector2.Zero; myVerts[i].Color = col;
			}

			pipelineState = new PipelineState();
			pipelineState.blending.enabled = c.A < 1.0f;
			pipelineState.shaderState.shaderProgram = theShader;
			pipelineState.vaoState.vao = theVAO;
			pipelineState.depthTest.enabled = false;
			pipelineState.generateId();

			renderState.setIndexBuffer(theIBO.id);
			renderState.setVertexBuffer(theVBO.id, 0, 0, V3T2B4.stride);
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, true));
			renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, -1));
		}

		public override void execute()
		{
			//update the buffer with this command's draw data
			theVBO.setData(myVerts);

			base.execute();

			GL.DrawElements(PrimitiveType.Triangles, theIBO.count, DrawElementsType.UnsignedShort, 0);
		}
	}

   /*
public class RenderCubemap2dCommand: StatelessRenderCommand
{
   vertexBuffer[] = {
{ -0.25f, 0.375f, 0.0f,
   -1,  1, -1 },
{   0.0f, 0.375f, 0.0f,
    1,  1, -1 },
{  -0.5f, 0.125f, 0.0f,
   -1,  1, -1 },
{ -0.25f, 0.125f, 0.0f,
   -1,  1,  1 },
{   0.0f, 0.125f, 0.0f,
    1,  1,  1 },
{  0.25f, 0.125f, 0.0f,
    1,  1, -1 },
{   0.5f, 0.125f, 0.0f,
   -1,  1, -1 },
{  -0.5f, -0.125f, 0.0f,
   -1, -1, -1 },
{ -0.25f, -0.125f, 0.0f,
   -1, -1,  1 },
{   0.0f, -0.125f, 0.0f,
    1, -1,  1 },
{  0.25f, -0.125f, 0.0f,
   1, -1, -1 },
{   0.5f, -0.125f, 0.0f,
   -1, -1, -1 },
{ -0.25f, -0.375f, 0.0f,
   -1, -1, -1 },
{   0.0f, -0.375f, 0.0f,
    1, -1, -1 }
};
}
*/
}
