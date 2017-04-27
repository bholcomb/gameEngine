using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	/*
  layout(std140) uniform model {
	  mat4 model;
	  mat4 normal;
	  mat4 invNormal;
	  vec4 activeLights;
  };
	*/
	[StructLayout(LayoutKind.Sequential)]
	public struct StaticModelUniformData
	{
		public Matrix4 modelMatrix;      //aligned 4N
		public Matrix4 normalMatrix;     //aligned 4N
		public Matrix4 inverseNormalMatrix; //aligned 4N
		public Vector4 activeLights;
	};

	public class StaticModelInfo : RenderInfo
	{
		public int indexOffset;
		public int indexCount;
		public int modelDataIndex;

		public StaticModelInfo() : base() { }
	}

   public class StaticModelVisualizer : Visualizer
   {
		ShaderStorageBufferObject myModelBuffer = new ShaderStorageBufferObject(BufferUsageHint.DynamicDraw);
		List<StaticModelUniformData> myModelData = new List<StaticModelUniformData>();

		public StaticModelVisualizer() : base()
      {
         myType = "staticModel";
		}

		#region extract phase
		//public override void onFrameBeginExtract() { }
		//public override void extractPerFrame(Renderable r) {}
		public override void extractPerView(Renderable r, View v)
		{
			StaticModelRenderable smr = r as StaticModelRenderable;

			StaticModelUniformData modelData = new StaticModelUniformData();
			modelData.modelMatrix = smr.model.myInitialTransform * smr.modelMatrix;
			modelData.normalMatrix = (smr.model.myInitialTransform * smr.modelMatrix).ClearTranslation();
			//modelData.inverseNormalMatrix = modelData.normalMatrix.Inverted();
			modelData.activeLights = new Vector4(0, 1, 2, 3);
			myModelData.Add(modelData);

			//save the index for this model
			int modelDataIndex = myModelData.Count - 1;

			foreach (Mesh mesh in smr.model.myMeshes)
			{
				Effect effect = getEffect(v.passType, (UInt32)mesh.material.myFeatures);
				RenderQueue<StaticModelInfo> rq = Renderer.device.getRenderQueue(effect.getPipeline(mesh.material).id) as RenderQueue<StaticModelInfo>;
				if (rq == null)
				{
					rq = Renderer.device.createRenderQueue<StaticModelInfo>(effect.getPipeline(mesh.material));
					rq.myPipeline.vao = new VertexArrayObject();
					rq.myPipeline.vao.bindVertexFormat<V3N3T2>(rq.myPipeline.shaderProgram);
					rq.visualizer = this;
					v.registerQueue(rq);
				}

				StaticModelInfo info = rq.nextInfo();

				effect.updateRenderState(mesh.material, info.renderState);
				
				float dist = (v.camera.position - r.position).Length;
				info.distToCamera = dist;
						
				info.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, modelDataIndex));
				info.renderState.setStorageBuffer(myModelBuffer.id, 2);
				info.renderState.setVertexBuffer(smr.model.myVbo.id, 0, 0, V3N3T2.stride);
				info.renderState.setIndexBuffer(smr.model.myIbo.id);
				info.indexOffset = mesh.indexBase;
				info.indexCount = mesh.indexCount;		

				info.sortId = getSortId(info);
			}
		}

		//public override void extractPerViewFinalize(BaseRenderQueue q, View v) { }
		//public override void onFrameExtractFinalize() { }
		#endregion

		#region prepare phase
		//public override void onFrameBeginPrepare() { }
		//public override void preparePerFrame(Renderable r) { }
		//public override void preparePerView(RenderInfo info, View v) { }
		public override void preparePerViewFinalize(BaseRenderQueue q, View v)
		{
			myModelBuffer.setData(myModelData);
			myModelData.Clear();
		}
		//public override void onFramePrepareFinalize() { } 
		#endregion

		#region submit phase
		//public override void onSubmitNodeBlockBegin(String technique) { }
		public override void submitRenderInfo(RenderInfo r, BaseRenderQueue q)
		{
			StaticModelInfo smi = r as StaticModelInfo;

			q.addCommand(new SetRenderStateCommand(r.renderState));
			q.addCommand(new DrawIndexedCommand(PrimitiveType.Triangles, smi.indexCount, smi.indexOffset, DrawElementsType.UnsignedShort));
		}
		//public override void onSubmitNodeBlockEnd() { }
		#endregion
   }
}