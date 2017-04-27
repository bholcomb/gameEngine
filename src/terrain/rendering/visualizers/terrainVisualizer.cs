using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using System.Collections.Concurrent;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;

namespace Terrain
{
	public class TerrainRenderInfo : RenderInfo
	{
		public enum Type { SOLID, TRANS, WATER };
		public Type myType;
		public int offset;
		public int count;
		public Matrix4 model;

		public TerrainRenderInfo() : base()
		{
		}
	}


	public abstract class TerrainVisualizer : Visualizer
	{
		protected TerrainRenderManager myRenderManager;

		int[] myOffset = new int[1024];
		int[] myCount = new int[1024];
		Matrix4[] myInstanceModelMatrix = new Matrix4[1024];
		UniformBufferObject myInstanceMatrixBuffer = new UniformBufferObject(BufferUsageHint.DynamicDraw);
		int myDrawCount;

		public TerrainVisualizer(TerrainRenderManager mgr) : base()
		{
			myRenderManager = mgr;
		}

		public void addDraw(int offset, int count, Matrix4 modelMatrix)
		{
			myOffset[myDrawCount] = offset;
			myCount[myDrawCount] = count;
			myInstanceModelMatrix[myDrawCount] = modelMatrix;

			myDrawCount++;
		}

		public abstract DrawChunk bufferChunk(Chunk chunk);
		public abstract void setVertexBuffer(RenderState rs);

		public override UInt64 getSortId(RenderInfo info)
		{
			UInt64 sortId = 0;
			if (info.pipeline.blending.enabled == true) //this is a transparent object
			{
				//sort these back to front so transparent objects draw properly
				//regardless of material or object type
				sortId = (UInt64)((1.0f / info.distToCamera) * UInt64.MaxValue);
			}
			else
			{
				sortId = (UInt64)info.distToCamera;
			}

			return sortId;
		}

		#region extract phase
		public override void onFrameBeginExtract()
		{
			myDrawCount = 0;
		}
		//public override void extractPerFrame(Renderable r) { }
		public override void extractPerView(Renderable r, View v)
		{
			TerrainRenderable tr = r as TerrainRenderable;
			Chunk c = tr.myChunk;
			if (myRenderManager.isLoaded(c, bufferChunk) == false)
			{
				myRenderManager.loadChunk(c, bufferChunk);
				return;
			}

			//DebugRenderer.addOffsetCube(c.myLocation, c.mySize, Color4.Blue, Fill.WIREFRAME, false, 0.0f);

			DrawChunk dc = myRenderManager.findDrawChunk(c);
			if (dc != null)
			{
				Matrix4 m = Matrix4.CreateTranslation(c.myLocation);
				
				//add solid render info
				if (dc.solidCount > 0)
				{
					Effect effect = getEffect(v.passType, 1);
					RenderQueue<TerrainRenderInfo> rq = Renderer.device.getRenderQueue(effect.getPipeline(MaterialManager.visualMaterial).id) as RenderQueue<TerrainRenderInfo>;
					if (rq == null)
					{
						rq = Renderer.device.createRenderQueue<TerrainRenderInfo>(effect.getPipeline(MaterialManager.visualMaterial));
						rq.visualizer = this;
						v.registerQueue(rq);
					}

					TerrainRenderInfo info = rq.nextInfo();
					info.distToCamera = (v.camera.position - c.myLocation).Length;
					info.model = m;
					info.myType = TerrainRenderInfo.Type.SOLID;
					info.count = dc.solidCount;
					info.offset = dc.solidOffset;
					info.sortId = getSortId(info);
					effect.updateRenderState(MaterialManager.visualMaterial, info.renderState);
				}

				//add transparent render info
				if (dc.transCount > 0)
				{
					Effect effect = getEffect(v.passType, 2);
					RenderQueue<TerrainRenderInfo> rq = Renderer.device.getRenderQueue(effect.getPipeline(MaterialManager.visualMaterial).id) as RenderQueue<TerrainRenderInfo>;
					if (rq == null)
					{
						rq = Renderer.device.createRenderQueue<TerrainRenderInfo>(effect.getPipeline(MaterialManager.visualMaterial));
						v.registerQueue(rq);
					}

					TerrainRenderInfo info = rq.nextInfo();
					info.distToCamera = (v.camera.position - c.myLocation).Length;
					info.model = m;
					info.myType = TerrainRenderInfo.Type.TRANS;
					info.count = dc.solidCount;
					info.offset = dc.solidOffset;
					info.sortId = getSortId(info);
					effect.updateRenderState(MaterialManager.visualMaterial, info.renderState);
				}

				//add water renderinfo
				if (dc.waterCount > 0)
				{
					Effect effect = getEffect(v.passType, 3);
					RenderQueue<TerrainRenderInfo> rq = Renderer.device.getRenderQueue(effect.getPipeline(MaterialManager.visualMaterial).id) as RenderQueue<TerrainRenderInfo>;
					if (rq == null)
					{
						rq = Renderer.device.createRenderQueue<TerrainRenderInfo>(effect.getPipeline(MaterialManager.visualMaterial));
						v.registerQueue(rq);
					}

					TerrainRenderInfo info = rq.nextInfo();
					info.distToCamera = (v.camera.position - c.myLocation).Length;
					info.model = m;
					info.myType = TerrainRenderInfo.Type.WATER;
					info.count = dc.solidCount;
					info.offset = dc.solidOffset;
					info.sortId = getSortId(info);
					effect.updateRenderState(MaterialManager.visualMaterial, info.renderState);
				}
			}
		}

		public override void extractPerViewFinalize(BaseRenderQueue q, View v) {	}

		//public override void onFrameExtractFinalize() {	}
		#endregion

		#region prepare phase
		//public override void onFrameBeginPrepare() { }
		//public override void preparePerFrame(Renderable r) { }
		public override void preparePerView(RenderInfo info, View v)
		{
			TerrainRenderInfo ti = info as TerrainRenderInfo;
			addDraw(ti.offset, ti.count, ti.model);
		}

		public override void preparePerViewFinalize(BaseRenderQueue q, View v)
		{
			myInstanceMatrixBuffer.setData(myInstanceModelMatrix, 0, myDrawCount * 16 * 4);
		}

		//public override void onFramePrepareFinalize() { }
		#endregion

		#region submit phase
		//public override void onSubmitNodeBlockBegin(BaseRenderQueue q) { }
		//public override void submitRenderInfo(RenderInfo r, RenderQueue q) { }
		public override void onSubmitNodeBlockEnd(BaseRenderQueue q)
		{
			RenderQueue<TerrainRenderInfo> rq = q as RenderQueue<TerrainRenderInfo>;
			if (rq.myInfoCount > 0)
			{
				//these should all have the same renderstate
				TerrainRenderInfo firstTi = rq.myInfos[0] as TerrainRenderInfo;
				firstTi.renderState.setUniformBuffer(myInstanceMatrixBuffer.id, 2);
				setVertexBuffer(firstTi.renderState);
				q.addCommand(new SetRenderStateCommand(firstTi.renderState));
				q.commands.Add(new MultiDrawArraysCommand(PrimitiveType.Triangles, myOffset, myCount, myDrawCount));	
			}
		}
		#endregion
	}
}