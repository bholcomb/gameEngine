using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public enum Fill { SOLID, TRANSPARENT, WIREFRAME};
	public enum Projection { ORTHO, PERSPECTIVE };

	//used for debug rendering
	[StructLayout(LayoutKind.Sequential)]
   public struct V3T2B4
   {
      static int theStride = Marshal.SizeOf(default(V3T2B4));

      public Vector3 Position; //8 bytes
      public Vector2 TexCoord; //8 bytes
      public UInt32 Color;  // 4 bytes

      public static int stride { get { return theStride; } }

      public static void bindVertexAttribute(String fieldName, int id)
      {
         switch (fieldName)
         {
            case "position":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 0);
               break;
            case "uv":
					GL.VertexAttribFormat(id, 2, VertexAttribType.Float, false, 12);
               break;
            case "color":
					GL.VertexAttribFormat(id, 4, VertexAttribType.UnsignedByte, true, 20);
               break;
            default:
               throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
         }
      }
   }

   public class CanvasRenderCommand : StatelessRenderCommand
   {
      public PrimitiveType myPrimative;
		public Projection myProjection;
		public int myTextureId;
      public int myOffset;
      public int myCount;
      public bool myClip;
		public Fill myFill;

      public CanvasRenderCommand(PrimitiveType prim, Projection projection, int textureId, int offset, int count, bool clip, Fill fill)
         : base()
      {
         myOffset = offset;
         myCount = count;
         myPrimative = prim;
			myClip = clip;
			myFill = fill;
			myProjection = projection;
			myTextureId = textureId;
		
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Bool, myProjection == Projection.PERSPECTIVE)); //layout(location = 0) uniform bool is3d;
			renderState.wireframe.enabled = myFill == Fill.WIREFRAME;
			renderState.primativeRestart.enabled = true;
			renderState.primativeRestart.value = DebugCanvas.PRIM_RESTART;
	
			GL.Enable(EnableCap.PrimitiveRestart);

			pipelineState.blending.enabled = myFill == Fill.TRANSPARENT;
			pipelineState.depthTest.enabled = myClip;
		}

      public override void execute()
      {
			base.execute();

			GL.DrawElements(myPrimative, myCount, DrawElementsType.UnsignedInt, myOffset * 4);  //unsigned int in bytes
      }
   }

   public class DebugCanvas
   {
      public ShaderProgram myShader;
		public VertexBufferObject<V3T2B4> myVbo;
		public IndexBufferObject myIbo;
		public VertexArrayObject myVao;
		public Texture myTexture;
		public Font myFont;

      Vector2 uvOne = Vector2.One;
      Vector2 uvZero = Vector2.Zero;
		Vector2 uvSolid = new Vector2(1, 0);

      V3T2B4[] myVerts = new V3T2B4[1024 * 1024]; //1M verts
      UInt32[] myIndexes = new UInt32[1024 * 1024 * 6]; //6M indexs
      List<StatelessRenderCommand> myDrawCmds = new List<StatelessRenderCommand>();
      int myVertCount;
      int myIndexCount;

      public const UInt32 PRIM_RESTART = 0xFFFFFFFF;

      static DebugCanvas()
      {
         createTheSphere();
      }

      public DebugCanvas()
      {
      }

      public void init()
      {
			myVbo = new VertexBufferObject<V3T2B4>(BufferUsageHint.DynamicDraw);
			myIbo = new IndexBufferObject(BufferUsageHint.DynamicDraw);
			myVao = new VertexArrayObject();

			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\debug-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\debug-ps.glsl", ShaderDescriptor.Source.File));
			ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
         myShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

			myVao.bindVertexFormat<V3T2B4>(myShader);

			myTexture = Graphics.Util.getEmbeddedTexture("Graphics.data.debugFont.png");
			myTexture.setMinMagFilters(TextureMinFilter.NearestMipmapNearest, TextureMagFilter.Nearest);
			myFont = FontManager.findFont("DEFAULT");
		}

      public void reset()
      {
         myVertCount = 0;
         myIndexCount = 0;
         myDrawCmds.Clear();
      }

      public void updateBuffers()
      {
         myVbo.setData(myVerts, 0, myVertCount * V3T2B4.stride);
         myIbo.setData(myIndexes, 0, myIndexCount * 4);
      }

      public List<RenderCommand> getRenderCommands()
      {
         List<RenderCommand> ret = new List<RenderCommand>();
         ret.AddRange(myDrawCmds);
         return ret;
      }

      public void addLine(Vector3 start, Vector3 end, Color4 color, Fill fill, bool clip)
      {
			if (fill == Fill.WIREFRAME)
				fill = Fill.SOLID;

         CanvasRenderCommand cmd = nextCommand(PrimitiveType.Lines, Projection.PERSPECTIVE, myTexture.id(), fill, clip);
         UInt32 baseVertex = (UInt32)myVertCount;

         addVertex(start, uvSolid, color);
         addVertex(end, uvSolid, color);

         addIndex(cmd, baseVertex);
         addIndex(cmd, baseVertex + 1);
      }

      public void addSphere(Vector3 position, float radius, Color4 color, Fill fill, bool clip)
      {
         CanvasRenderCommand cmd = nextCommand(PrimitiveType.Triangles, Projection.PERSPECTIVE, myTexture.id(),fill, clip);
         UInt32 baseVertex = (UInt32)myVertCount;

			if (fill == Fill.TRANSPARENT)
				color.A = 0.5f;

         for (int i = 0; i < theSphereVertCount; i++)
         {
            Vector3 v = theSphereVerts[i] * radius + position;
            addVertex(v, uvSolid, color);
         }

         addIndexListWithOffset(cmd, theSphereIndexes, baseVertex);
      }

      public void addCube(Vector3 min, Vector3 max, Color4 color, Fill fill, bool clip)
      {
         UInt32 baseVertex = (UInt32)myVertCount;

			if(fill == Fill.WIREFRAME)
			{
				CanvasRenderCommand cmd = nextCommand(PrimitiveType.LineStrip, Projection.PERSPECTIVE, myTexture.id(), fill, clip);
				addVertex(min, uvSolid, color);
				addVertex(new Vector3(max[0], min[1], min[2]), uvSolid, color);
				addVertex(new Vector3(min[0], max[1], min[2]), uvSolid, color);
				addVertex(new Vector3(max[0], max[1], min[2]), uvSolid, color);
				addVertex(new Vector3(min[0], min[1], max[2]), uvSolid, color);
				addVertex(new Vector3(max[0], min[1], max[2]), uvSolid, color);
				addVertex(new Vector3(min[0], max[1], max[2]), uvSolid, color);
				addVertex(max, uvSolid, color);

				List<UInt32> indexes = new List<UInt32> {
								4, 6, 2, 3, PRIM_RESTART,
 								1, 3, 7, 6, PRIM_RESTART,
 								7, 5, 1, 0, PRIM_RESTART,
 								2, 0, 4, 5
				};

				addIndexListWithOffset(cmd, indexes, baseVertex);
			}
			else
			{
				CanvasRenderCommand cmd = nextCommand(PrimitiveType.Triangles, Projection.PERSPECTIVE, myTexture.id(), fill, clip);
				if (fill == Fill.TRANSPARENT)
					color.A = 0.5f;

				addVertex(min, uvSolid, color);
				addVertex(new Vector3(max[0], min[1], min[2]), uvSolid, color);
				addVertex(new Vector3(min[0], max[1], min[2]), uvSolid, color);
				addVertex(new Vector3(max[0], max[1], min[2]), uvSolid, color);
				addVertex(new Vector3(min[0], min[1], max[2]), uvSolid, color);
				addVertex(new Vector3(max[0], min[1], max[2]), uvSolid, color);
				addVertex(new Vector3(min[0], max[1], max[2]), uvSolid, color);
				addVertex(max, uvSolid, color);

				List<UInt32> indexes = new List<UInt32> {
								0, 1, 5, 0, 5, 4,
								1, 3, 7, 1, 7, 5,
								3, 2, 6, 3, 6, 7,
								2, 0, 4, 2, 4, 6,
								4, 5, 7, 4, 7, 6,
								2, 3, 1, 2, 1, 0
							};

				addIndexListWithOffset(cmd, indexes, baseVertex);
			}
		}

		public void addLine(Vector2 start, Vector2 end, Color4 color, Fill fill, bool clip)
		{
			if (fill == Fill.WIREFRAME)
				fill = Fill.SOLID;

			if (fill == Fill.TRANSPARENT)
				color.A = 0.5f;

			CanvasRenderCommand cmd = nextCommand(PrimitiveType.Lines, Projection.ORTHO, myTexture.id(), fill, clip);
			UInt32 baseVertex = (UInt32)myVertCount;

			addVertex(new Vector3(start), uvSolid, color);
			addVertex(new Vector3(end), uvSolid, color);

			addIndex(cmd, baseVertex);
			addIndex(cmd, baseVertex + 1);
		}

		public void addRect2d(Vector2 start, Vector2 end, Color4 color, Fill fill, bool clip)
		{
			CanvasRenderCommand cmd = nextCommand(PrimitiveType.Triangles, Projection.ORTHO, myTexture.id(), fill, clip);
			UInt32 baseVertex = (UInt32)myVertCount;

			if (fill == Fill.TRANSPARENT)
				color.A = 0.5f;

			addVertex(new Vector3(start), uvSolid, color);
			addVertex(new Vector3(end), uvSolid, color);

			addIndex(cmd, baseVertex);
			addIndex(cmd, baseVertex + 1);
		}

		public void addTexture2d(Vector2 start, Vector2 end, Texture t, int layer = 0, bool linearDepth = false)
		{
			CanvasRenderCommand cmd = nextCommand(PrimitiveType.Triangles, Projection.ORTHO, t.id(), Fill.SOLID, false);
			switch(t.target)
			{
				case TextureTarget.Texture2D:
					cmd.renderState.setTexture(t.id(), 0, t.target);
					cmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
					cmd.renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0));
					cmd.renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, linearDepth));
					break;
				case TextureTarget.TextureCubeMap:
					cmd.renderState.setTexture(t.id(), 0, t.target);
					cmd.renderState.setUniform(new UniformData(21, Uniform.UniformType.Int, 0));
					cmd.renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 1));
					cmd.renderState.setUniform(new UniformData(24, Uniform.UniformType.Int, layer));
					cmd.renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, linearDepth));
					break;
				case TextureTarget.Texture2DArray:
					cmd.renderState.setTexture(t.id(), 0, t.target);
					cmd.renderState.setUniform(new UniformData(22, Uniform.UniformType.Int, 0));
					cmd.renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 2));
					cmd.renderState.setUniform(new UniformData(24, Uniform.UniformType.Int, layer));
					cmd.renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, linearDepth));
					break;
			}

			cmd.renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, linearDepth));

			UInt32 baseVertex = (UInt32)myVertCount;

			addVertex(new Vector3(start), uvZero, Color4.White);
			addVertex(new Vector3(end), uvOne, Color4.White);

			addIndex(cmd, baseVertex);
			addIndex(cmd, baseVertex + 1);
		}

		public void addTexture2d(Vector2 start, Vector2 end, TextureBufferObject t, bool LinearDepth)
		{
			CanvasRenderCommand cmd = nextCommand(PrimitiveType.Triangles, Projection.ORTHO, t.textureId, Fill.SOLID, false);
			cmd.renderState.setTexture(t.textureId, 0, TextureTarget.TextureBuffer);
			cmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
			cmd.renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0));

			UInt32 baseVertex = (UInt32)myVertCount;

			addVertex(new Vector3(start), uvZero, Color4.White);
			addVertex(new Vector3(end), uvOne, Color4.White);

			addIndex(cmd, baseVertex);
			addIndex(cmd, baseVertex + 1);
		}

		public void addText2d(Vector2 position, String text, Color4 color)
		{
			RenderFontCommand cmd = new RenderFontCommand(myFont, new Vector3(position), text, color);
			myDrawCmds.Add(cmd);
		}

		public void addFrustum(Matrix4 invertedClip, Color4 color, bool clip)
		{
			//frustum after view/projection transform, in clip space
			Vector4[] points = new Vector4[8];
			points[0] = new Vector4(-1, -1, 0, 1); //near 
			points[1] = new Vector4( 1, -1, 0, 1);
			points[2] = new Vector4( 1,  1, 0, 1);
			points[3] = new Vector4(-1,  1, 0, 1);

			points[4] = new Vector4(-1, -1, 1, 1); //far
			points[5] = new Vector4( 1, -1, 1, 1);
			points[6] = new Vector4( 1,  1, 1, 1);
			points[7] = new Vector4(-1,  1, 1, 1);

			//transform back into world space
			for (int i = 0; i < 8; i++)
			{
				points[i] = points[i] * invertedClip;
				points[i] /= points[i].W;
			}

			//draw the frustum
			UInt32 baseVertex = (UInt32)myVertCount;

			CanvasRenderCommand cmd = nextCommand(PrimitiveType.Lines, Projection.PERSPECTIVE, myTexture.id(), Fill.WIREFRAME, clip);
			for (int i = 0; i < 8; i++)
				addVertex(points[i].Xyz, uvSolid, color);
			
			List<UInt32> indexes = new List<UInt32> {
							0, 1, //near plane
							1, 2,
							2, 3,
							3, 0,
							4, 5, //far plane
							5, 6, 
							6, 7,
							7, 4,
							0, 4, //sides
							1, 5, 
							2, 6,
							3, 7
			};

			addIndexListWithOffset(cmd, indexes, baseVertex);
		}

		#region helper commands
		static int theSphereVertCount = 121;
      static int theSphereIndexCount = 600;
      static Vector3[] theSphereVerts = new Vector3[theSphereVertCount];
      static UInt32[] theSphereIndexes = new UInt32[theSphereIndexCount];
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
               UInt32 first = (UInt32) ((i * (longs + 1)) + j);
               UInt32 second = (UInt32)(first + longs + 1);
               theSphereIndexes[indexIdx++] = first; theSphereIndexes[indexIdx++] = second; theSphereIndexes[indexIdx++] = first + 1;
               theSphereIndexes[indexIdx++] = second; theSphereIndexes[indexIdx++] = second + 1; theSphereIndexes[indexIdx++] = first + 1;
            }
         }
      }

		CanvasRenderCommand createCommand(PrimitiveType prim, Projection projection, int textureId, Fill fill, bool clip)
		{
			CanvasRenderCommand cmd = new CanvasRenderCommand(prim, projection, textureId, myIndexCount, 0, clip, fill);
			cmd.renderState.setVertexBuffer(myVbo.id, 0, 0, V3T2B4.stride);
			cmd.renderState.setIndexBuffer(myIbo.id);
			cmd.renderState.setTexture(myTexture.id(), 0, TextureTarget.Texture2D);
			cmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
			cmd.renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0));
			cmd.renderState.wireframe.enabled = fill == Fill.WIREFRAME;
			cmd.pipelineState.shaderState.shaderProgram = myShader;
			cmd.pipelineState.vaoState.vao = myVao;
			cmd.pipelineState.blending.enabled = true;
			cmd.pipelineState.generateId();

			return cmd;
		}

		CanvasRenderCommand nextCommand(PrimitiveType prim, Projection projection, int textureId, Fill fill, bool clip)
      {
         CanvasRenderCommand cmd = null;
         if (myDrawCmds.Count == 0)
         {
				cmd = createCommand(prim, projection, textureId, fill, clip);
				myDrawCmds.Add(cmd);
            return cmd;
         }

         CanvasRenderCommand currentCmd = myDrawCmds[myDrawCmds.Count - 1] as CanvasRenderCommand;

         //this command profile matches, so keep using it
         if (currentCmd != null &&
				currentCmd.myPrimative == prim &&
				currentCmd.myProjection == projection &&
				currentCmd.myTextureId == textureId &&
				currentCmd.myFill == fill &&
            currentCmd.myClip == clip)
         {
            if (prim == PrimitiveType.TriangleStrip || prim == PrimitiveType.LineStrip)
            {
               addIndex(currentCmd, PRIM_RESTART);
            }
            return currentCmd;
         }

			//new type of command
			cmd = createCommand(prim, projection, textureId, fill, clip);
			myDrawCmds.Add(cmd);

         return cmd;
      }

      void addVertex(Vector3 vert, Vector2 uv, Color4 c)
      {
         V3T2B4 v = new V3T2B4();
         v.Position = vert;
         v.TexCoord = uv;
         v.Color = c.toUInt();
         myVerts[myVertCount] = v;
         myVertCount++;
      }

      void addIndex(CanvasRenderCommand cmd, UInt32 idx)
      {
         myIndexes[myIndexCount] = idx;
         myIndexCount++;
         cmd.myCount++;
      }

      void addIndexListWithOffset(CanvasRenderCommand cmd, List<UInt32> idx, UInt32 offset)
      {
         for (int i = 0; i < idx.Count; i++)
         {
            myIndexes[myIndexCount] = idx[i] == PRIM_RESTART ? idx[i] : idx[i] + offset;
            myIndexCount++;
         }

         cmd.myCount += idx.Count;
      }

      void addIndexListWithOffset(CanvasRenderCommand cmd, UInt32[] idx, UInt32 offset)
      {
         for (int i = 0; i < idx.Length; i++)
         {
            myIndexes[myIndexCount] = idx[i] + offset;
            myIndexCount++;
         }

         cmd.myCount += idx.Length;
      }
      #endregion
   }
}