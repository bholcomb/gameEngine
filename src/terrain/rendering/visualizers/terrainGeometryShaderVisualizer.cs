using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;

namespace Terrain
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct I1I1B12
	{
		static int theStride = Marshal.SizeOf(typeof(I1I1B12));

		public UInt32 Morton;
		public UInt32 MaterialId;
		public fixed byte Edges[12];

		public static int stride { get { return theStride; } }

		public static void bindVertexAttribute(String fieldName, int id)
		{
			switch (fieldName)
			{
				case "morton":
					GL.VertexAttribIFormat(id, 1, VertexAttribIntegerType.UnsignedInt, 0);
					break;
				case "material":
					GL.VertexAttribIFormat(id, 1, VertexAttribIntegerType.UnsignedInt, 4);
					break;
				case "edge1":
					GL.VertexAttribIFormat(id, 4, VertexAttribIntegerType.UnsignedByte, 8);
					break;
				case "edge2":
					GL.VertexAttribIFormat(id, 4, VertexAttribIntegerType.UnsignedByte, 12);
					break;
				case "edge3":
					GL.VertexAttribIFormat(id, 4, VertexAttribIntegerType.UnsignedByte, 16);
					break;
				default:
					throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
			}
		}
	}

	public class GeometryShaderRenderInfo : RenderInfo
	{
		public GeometryShaderRenderInfo() : base()
		{

		}
	}

	public class GeometryShaderVisualizer : TerrainVisualizer
	{
		public GeometryShaderVisualizer(TerrainRenderManager mgr) : base(mgr)
		{

		}

		public override DrawChunk bufferChunk(Chunk c)
		{
			c.updateVisisbility(); //this should be done offline
			int bytesNeeded = c.visibleNodeCount * I1I1B12.stride;
			BufferMemoryManager.Page mem = myRenderManager.memoryManager.alloc(bytesNeeded);
			if (mem == null)
			{
				Warn.print("Not enough memory to buffer chunk-flushing video memory");
				myRenderManager.removeStale();
				mem = myRenderManager.memoryManager.alloc(bytesNeeded);

				if (mem == null)
					throw new Exception("Not enough memory in buffer. This is bad");
			}

			DrawChunk drawData = new DrawChunk();
			drawData.changeNumber = c.changeNumber; //important for detecting changes
			drawData.mem = mem;
			drawData.firstOffset = (int)(mem.start.ToInt64() - myRenderManager.memoryManager.buffer.ptr.ToInt64()) / I1I1B12.stride;

			int count = 0;
			int offset = drawData.firstOffset;
			IntPtr buffer = mem.start;
			drawData.solidOffset = offset;
			count = bufferNode(ref buffer, c.myRoot, Material.Property.SOLID);
			drawData.solidCount = count;
			offset += count;

			drawData.transOffset = offset;
			count = bufferNode(ref buffer, c.myRoot, Material.Property.TRANSPARENT);
			drawData.transCount = count;
			offset += count;

			drawData.waterOffset = offset;
			count = bufferNode(ref buffer, c.myRoot, Material.Property.WATER);
			drawData.waterCount = count;
			offset += count;

			return drawData;
		}

		public unsafe int bufferNode(ref IntPtr buffer, Node n, Material.Property pass)
		{
			int count = 0;
			if (n.myChildren != null)
			{
				for (int i = 0; i < 8; i++)
				{
					count += bufferNode(ref buffer, n.myChildren[i], pass);
				}
			}
			else
			{
				if (n.isVisible() == false)
					return 0;

				if (n.myMaterial.property != pass)
					return 0;

				I1I1B12 node = new I1I1B12();
				node.Morton = n.myKey.myValue;
				node.MaterialId = n.myMaterial.packedTextures;
				for (int i = 0; i < 12; i++)
				{
					node.Edges[i] = n.myEdgeSpans[i];
				}

				Marshal.StructureToPtr(node, buffer, false);
				buffer += I1I1B12.stride;
				return 1;
			}

			return count;
		}

		public override void setVertexBuffer(RenderState rs)
		{
			rs.setVertexBuffer(myRenderManager.memoryManager.buffer.id, 0, 0, I1I1B12.stride);
		}

		#region extract phase
		// 		public override void onFrameBeginExtract() { }
		// 		public override void extractPerFrame(Renderable r) { }
		// 		public override IEnumerable<RenderInfo> extractPerView(Renderable r, View v) { }
		public override void extractPerViewFinalize(BaseRenderQueue q, View v)
		{
			base.extractPerViewFinalize(q, v);

			RenderQueue<TerrainRenderInfo> rq = q as RenderQueue<TerrainRenderInfo>;

			if (q.myPipeline.vao == null)
			{
				q.myPipeline.vao = new VertexArrayObject();
				q.myPipeline.vao.bindVertexFormat<I1I1B12>(q.myPipeline.shaderProgram);
			}
		}
		// 		public override void onFrameExtractFinalize() { }
		#endregion

		#region prepare phase
		// 		public override void onFrameBeginPrepare() { }
		// 		public override void preparePerFrame(Renderable r) { }
		// 		public override void preparePerView(RenderInfo info, View v) { }
		// 		public override void preparePerViewFinalize(RenderQueue q, View v) { }
		// 		public override void onFramePrepareFinalize() { }
		#endregion

		#region submit phase
		// 		public override void onSubmitNodeBlockBegin(RenderQueue q) { }
		// 		public override void submitRenderInfo(RenderInfo r, RenderQueue q) { }
		// 		public override void onSubmitNodeBlockEnd(RenderQueue q) { }
		#endregion

	}
}