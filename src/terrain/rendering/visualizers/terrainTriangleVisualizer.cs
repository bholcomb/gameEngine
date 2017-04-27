using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;

namespace Terrain
{
	[StructLayout(LayoutKind.Sequential)]
	public struct TerrainVertex
	{
		static int theStride = Marshal.SizeOf(typeof(TerrainVertex));
		public UInt32 pos;
		public Byte uvx;
		public Byte uvy;
		public UInt16 texIndex;

		public UInt32 X { get { return pos & 0x3ff; } }
		public UInt32 Y { get { return (pos & 0xFFC00) >> 10; } }
		public UInt32 Z { get { return (pos & 0x3FF00000) >> 20; } }
		public UInt32 W { get { return (pos & 0xC0000000) >> 30; } }

		public static int stride { get { return theStride; } }

		public static UInt32 encode(Vector3 vert)
		{
			vert *= 10.0f;
			Vector3i iv = vert;

			UInt32 count = 0;
			if (iv.X == 1024) count++;
			if (iv.Y == 1024) count++;
			if (iv.Z == 1024) count++;

			switch (count)
			{
				case 1: //which one is max, then move other two in to x, y
					{
						if (iv.X == 1024)
							{ iv.X = iv.Y; iv.Y = iv.Z; iv.Z = 0; break; }
						if (iv.Y == 1024)
							{ iv.X = iv.X; iv.Y = iv.Z; iv.Z = 1; break; }
						if (iv.Z == 1024)
							{ iv.X = iv.X; iv.Y = iv.Y; iv.Z = 2; break; }
					}
					break;
				case 2: //which one is NOT max, then it to x
					{
						if (iv.X < 1024)
							{ iv.X = iv.X; iv.Y = 0; iv.Z = 0; break; }
						if (iv.Y < 1024)
							{ iv.X = iv.Y; iv.Y = 0; iv.Z = 1; break; }
						if (iv.Z < 1024)
							{ iv.X = iv.Z; iv.Y = 0; iv.Z = 2; break; }
					}
					break;
				case 3: // all are max
					{
						iv.X = 0; iv.Y = 0; iv.Z = 0;
					}
					break;
			}

#if DEBUG
			System.Diagnostics.Debug.Assert(count >= 0);
			System.Diagnostics.Debug.Assert(count < 4);
			System.Diagnostics.Debug.Assert(iv.X < 1024);
			System.Diagnostics.Debug.Assert(iv.Y < 1024);
			System.Diagnostics.Debug.Assert(iv.Z < 1024);
			System.Diagnostics.Debug.Assert(iv.X >= 0);
			System.Diagnostics.Debug.Assert(iv.Y >= 0);
			System.Diagnostics.Debug.Assert(iv.Z >= 0);
#endif
			UInt32 bytes = 0;
			bytes |= ((UInt32)(count)) << 30;
			bytes |= ((UInt32)(iv.Z)) << 20;
			bytes |= ((UInt32)(iv.Y)) << 10;
			bytes |= ((UInt32)(iv.X));

			return bytes;
		}

		public static void bindVertexAttribute(String fieldName, int id)
		{
			switch (fieldName)
			{
				case "position":
					GL.VertexAttribFormat(id, 4, VertexAttribType.UnsignedInt2101010Rev, false, 0);
					break;
				case "uv":
					GL.VertexAttribFormat(id, 2, VertexAttribType.UnsignedByte, true, 4);
					break;
				case "texIndex":
					GL.VertexAttribIFormat(id, 1, VertexAttribIntegerType.UnsignedShort, 6);
					break;
				default:
					throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
			}
		}
	}

	public class TriangleVisualizer : TerrainVisualizer
	{ 
		public TriangleVisualizer(TerrainRenderManager mgr) : base(mgr)
		{

		}

		public override DrawChunk bufferChunk(Chunk c)
		{
			c.updateVisisbility(); //this should be done offline
			int bytesNeeded = c.visibleFaceCount * TerrainVertex.stride * 6;
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
			drawData.firstOffset = (int)(mem.start.ToInt64() - myRenderManager.memoryManager.buffer.ptr.ToInt64()) / TerrainVertex.stride;

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

		public int bufferNode(ref IntPtr buffer, Node n, Material.Property pass)
		{
			int vertCount = 0;
			if (n.isLeaf == false)
			{
				for (int i = 0; i < 8; i++)
				{
					vertCount += bufferNode(ref buffer, n.myChildren[i], pass);
				}
			}
			else
			{
				if (n.isVisible() == false)
					return 0;

				if (n.myMaterial.property != pass)
					return 0;

				TerrainVertex[] vertBuffer = new TerrainVertex[36];

				//calculated values
				Vector3[] verts = n.generateVerts();
				int idx = 0;
				for (int i = 0; i < 6; i++)
				{
					if (n.faceVisible(i) == true)
					{
						Vector2 uv;
						Vector3 v;

						//first triangle
						v = n.localVertex(i, 0, ref verts);
						uv = n.localUv(i, 0, ref verts);
						vertBuffer[idx].pos = TerrainVertex.encode(v);
						vertBuffer[idx].uvx = (Byte)(uv.X * 255); vertBuffer[idx].uvy = (Byte)(uv.Y * 255);
						vertBuffer[idx].texIndex = (UInt16)n.localMaterial(i); idx++;

						v = n.localVertex(i, 1, ref verts);
						uv = n.localUv(i, 1, ref verts);
						vertBuffer[idx].pos = TerrainVertex.encode(v);
						vertBuffer[idx].uvx = (Byte)(uv.X * 255); vertBuffer[idx].uvy = (Byte)(uv.Y * 255);
						vertBuffer[idx].texIndex = (UInt16)n.localMaterial(i); idx++;

						v = n.localVertex(i, 2, ref verts);
						uv = n.localUv(i, 2, ref verts);
						vertBuffer[idx].pos = TerrainVertex.encode(v);
						vertBuffer[idx].uvx = (Byte)(uv.X * 255); vertBuffer[idx].uvy = (Byte)(uv.Y * 255);
						vertBuffer[idx].texIndex = (UInt16)n.localMaterial(i); idx++;

						//second triangle
						v = n.localVertex(i, 0, ref verts);
						uv = n.localUv(i, 0, ref verts);
						vertBuffer[idx].pos = TerrainVertex.encode(v);
						vertBuffer[idx].uvx = (Byte)(uv.X * 255); vertBuffer[idx].uvy = (Byte)(uv.Y * 255);
						vertBuffer[idx].texIndex = (UInt16)n.localMaterial(i); idx++;

						v = n.localVertex(i, 2, ref verts);
						uv = n.localUv(i, 2, ref verts);
						vertBuffer[idx].pos = TerrainVertex.encode(v);
						vertBuffer[idx].uvx = (Byte)(uv.X * 255); vertBuffer[idx].uvy = (Byte)(uv.Y * 255);
						vertBuffer[idx].texIndex = (UInt16)n.localMaterial(i); idx++;

						v = n.localVertex(i, 3, ref verts);
						uv = n.localUv(i, 3, ref verts);
						vertBuffer[idx].pos = TerrainVertex.encode(v);
						vertBuffer[idx].uvx = (Byte)(uv.X * 255); vertBuffer[idx].uvy = (Byte)(uv.Y * 255);
						vertBuffer[idx].texIndex = (UInt16)n.localMaterial(i); idx++;
					}
				}

				for (int i = 0; i < idx; i++)
				{
					Marshal.StructureToPtr(vertBuffer[i], buffer, false);
					buffer += TerrainVertex.stride;
				}

				//return the number of verts that are added
				return idx;
			}

			return vertCount;
		}

		public override void setVertexBuffer(RenderState rs)
		{
			rs.setVertexBuffer(myRenderManager.memoryManager.buffer.id, 0, 0, TerrainVertex.stride);
		}

		#region extract phase
		// 		public override void onFrameBeginExtract() { }
		// 		public override void extractPerFrame(Renderable r) { }
		// 		public override IEnumerable<RenderInfo> extractPerView(Renderable r, View v) { }
		public override void extractPerViewFinalize(BaseRenderQueue q, View v)
		{
			base.extractPerViewFinalize(q, v);

			if (q.myPipeline.vao == null)
			{
				q.myPipeline.vao = new VertexArrayObject();
				q.myPipeline.vao.bindVertexFormat<TerrainVertex>(q.myPipeline.shaderProgram);
			}
		}
		// 		public override void onFrameExtractFinalize() { }
		#endregion

		#region prepare phase
		// 		public override void onFrameBeginPrepare() { }
		// 		public override void preparePerFrame(Renderable r) { }
		// 		public override void preparePerView(RenderInfo info, View v) { }
		//       public override void preparePerViewFinalize(BaseRenderQueue q, View v) { }
		// 		public override void onFramePrepareFinalize() { }
		#endregion

		#region submit phase
		// 		public override void onSubmitNodeBlockBegin(BaseRenderQueue q) { }
		// 		public override void submitRenderInfo(RenderInfo r, BaseRenderQueue q) { }
		// 		public override void onSubmitNodeBlockEnd(BaseRenderQueue q) { }
		#endregion

	}
}