using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	/*glsl struct 
   struct Model {
		mat4 model;
		mat4 normal;
		mat4 invNormal;
		vec4 activeLights;
		int boneCount;
		int currentFrame;
		int nextFrame;C:\source\gameEngine\src\graphics\visualizers\lightVisualizer.cs
		float interpolation;
	};
*/
	[StructLayout(LayoutKind.Sequential)]
	public struct SkinnedModelUniformData
	{
		public Matrix4 modelMatrix;      //aligned 4N
		public Matrix4 normalMatrix;     //aligned 4N
		public Matrix4 inverseNormalMatrix; //aligned 4N
		public Vector4 activeLights;
		public int boneCount;
		public int currentFrame;
		public int nextFrame;
		public float interpolation;

	};

	public class SkinnedModelInfo : RenderInfo
	{
		public int indexOffset;
		public int indexCount;
		public int modelDataIndex;
		public SkinnedModelInfo() : base() { }
	}

	public class SkinnedModelVisualizer : Visualizer
   {
		ShaderStorageBufferObject myModelBuffer = new ShaderStorageBufferObject(BufferUsageHint.DynamicDraw);
		List<SkinnedModelUniformData> myModelData = new List<SkinnedModelUniformData>();

		public SkinnedModelVisualizer() 
         : base("skinnedModel")
      {
		}

      #region prepare phase
      //public override void prepareFrameBegin() { }
      //public override void preparePerFrame(Renderable r) { }
      //public override void preparePerViewBegin(View v) { }
      //public override void preparePerView(Renderable r, View v) { }
      //public override void preparePerViewFinalize(View v) { }
      //public override void preparePerPassBegin(Pass p) { }
      //public override void preparePerPass(Renderable r, Pass p) { }

      public override void preparePerPass(Renderable r, Pass p)
		{
			SkinnedModelRenderable smr = r as SkinnedModelRenderable;

			SkinnedModelUniformData modelData = new SkinnedModelUniformData();
			modelData.modelMatrix = smr.model.myInitialTransform * smr.modelMatrix;
			modelData.normalMatrix = (smr.model.myInitialTransform * smr.modelMatrix).ClearTranslation();
			//modelData.inverseNormalMatrix = modelData.normalMatrix.Inverted();
			modelData.activeLights = new Vector4(0, 1, 2, 3);
			modelData.currentFrame = (smr.findController("animation") as AnimationController).animation.currentFrame;
			modelData.nextFrame = (smr.findController("animation") as AnimationController).animation.nextFrame;
			modelData.interpolation = (smr.findController("animation") as AnimationController).animation.interpolation;
			modelData.boneCount = smr.model.boneCount;
			myModelData.Add(modelData);

			//save the index for this model
			int modelDataIndex = myModelData.Count - 1;

			foreach (Mesh mesh in smr.model.myMeshes)
			{
				MaterialEffect effect = getEffect(p.technique, (UInt32)mesh.material.myFeatures);
            PipelineState pipeline = effect.createPipeline(mesh.material);
            RenderQueue<SkinnedModelInfo> rq = p.findRenderQueue(pipeline.id) as RenderQueue<SkinnedModelInfo>;
				if (rq == null)
				{
					rq = Renderer.device.createRenderQueue<SkinnedModelInfo>(effect.createPipeline(mesh.material));
               rq.name = rq.myPipeline.shaderState.shaderProgram.name + "-" + (mesh.material.hasTransparency == true ? "transparent" : "opaque"); 
               rq.myPipeline.vaoState.vao = new VertexArrayObject();
					rq.myPipeline.vaoState.vao.bindVertexFormat<V3N3T2B4W4>(rq.myPipeline.shaderState.shaderProgram);
					rq.visualizer = this;
			      p.registerQueue(rq);
				}

				SkinnedModelInfo info = rq.nextInfo();

				effect.updateRenderState(mesh.material, info.renderState);

				float dist = (p.view.camera.position - r.position).Length;
				info.distToCamera = dist;

				info.indexCount = mesh.indexCount;
				info.indexOffset = mesh.indexBase;

				info.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, modelDataIndex));
				info.renderState.setStorageBuffer(myModelBuffer.id, 0);
				info.renderState.setStorageBuffer(smr.model.myFrames.id, 1);
				info.renderState.setVertexBuffer(smr.model.myVbo.id, 0, 0, V3N3T2B4W4.stride);
				info.renderState.setIndexBuffer(smr.model.myIbo.id);

				info.sortId = getSortId(info);
			}
		}
      
      //public override void preparePerPassFinalize(Pass p) { }

      public override void prepareFrameFinalize()
      {
         myModelBuffer.setData(myModelData);
         myModelData.Clear();
      }
      #endregion


      #region generate command phase
      //public override void generateCommandsBegin(BaseRenderQueue q) { }

      public override void generateRenderCommand(RenderInfo r, BaseRenderQueue q)
		{
			SkinnedModelInfo smi = r as SkinnedModelInfo;

			q.addCommand(new SetRenderStateCommand(r.renderState));
			q.addCommand(new DrawIndexedCommand(PrimitiveType.Triangles, smi.indexCount, smi.indexOffset, DrawElementsType.UnsignedShort));
		}

      //public override void generateCommandsFinalize(BaseRenderQueue q) { }
      #endregion
   }
}
 
 