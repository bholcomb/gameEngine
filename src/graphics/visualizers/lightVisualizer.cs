using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   /* glsl struct
   struct lightData{
      vec4 color;	
      vec4 position;
      vec4 direction;
		int lightType;
      float constantAttenuation;
      float linearAttenuation;
      float quadraticAttenuation;
      float spotAngle;
      float spotExponential;
      float pad1;
      float pad2;
   };
   */
   [StructLayout(LayoutKind.Sequential)]
	public struct LightUniformData
	{
		public Vector4 color;
		public Vector4 position;
      public Vector4 direction;
      public int lightType;
		public float constantAttenuation;
		public float linearAttenuation;
		public float quadraticAttenuation;
		public float spotAngle;
		public float spotExponential;
		public float pad1;
		public float pad2;
	};

	public class LightInfo : RenderInfo
	{
		public Color4 color;
		public Vector4 position;
      public Vector4 direction;
		public int lightType;
		public float constantAttenuation;
		public float linearAttenuation;
		public float quadraticAttenuation;
		public float spotAngle;
		public float spotExponential;
	}

	public class LightVisualizer : Visualizer
   {
		public UniformBufferObject myLightUniforBuffer;
		LightUniformData[] myLightData = new LightUniformData[255];
		int myCurrentLightIndex = 0;
	
		public LightVisualizer() : base()
      {
         myType = "light";

			// room for 255 lights, light 0 is the sun
			for (int i = 0; i < 255; i++)
				myLightData[i] =new LightUniformData();

			myLightUniforBuffer = new UniformBufferObject(BufferUsageHint.DynamicDraw);
		}
		

		#region extract phase
		//public override void onFrameBeginExtract() { }
		//public override void extractPerFrame(Renderable r) { }
		public override void extractPerView(Renderable r, View v)
		{
			LightRenderable lr = r as LightRenderable;

			float dist = (v.camera.position - r.position).Length;

			PipelineState pipelineState = new PipelineState();
			RenderQueue<LightInfo> rq = Renderer.device.getRenderQueue(pipelineState.id) as RenderQueue<LightInfo>;
			if (rq == null)
			{
				rq = Renderer.device.createRenderQueue<LightInfo>(pipelineState);
				rq.visualizer = this;
				v.registerQueue(rq);
			}

			switch (lr.myLightType)
			{
				case LightRenderable.Type.DIRECTIONAL:
					myLightData[myCurrentLightIndex].lightType = 0;
					myLightData[myCurrentLightIndex].color = new Vector4(lr.color.R, lr.color.G, lr.color.B, 1.0f);
					myLightData[myCurrentLightIndex].position = new Vector4(lr.position.X, lr.position.Y, lr.position.Z, 0.0f);
					break;
				case LightRenderable.Type.POINT:
					myLightData[myCurrentLightIndex].lightType = 1;
					myLightData[myCurrentLightIndex].color = new Vector4(lr.color.R, lr.color.G, lr.color.B, 1.0f);
					myLightData[myCurrentLightIndex].position = new Vector4(lr.position.X, lr.position.Y, lr.position.Z, 1.0f);
					myLightData[myCurrentLightIndex].linearAttenuation = lr.linearAttenuation;
					myLightData[myCurrentLightIndex].quadraticAttenuation = lr.quadraticAttenuation;
					break;
				case LightRenderable.Type.SPOT:
					myLightData[myCurrentLightIndex].lightType = 2;
					myLightData[myCurrentLightIndex].color = new Vector4(lr.color.R, lr.color.G, lr.color.B, 1.0f);
					myLightData[myCurrentLightIndex].position = new Vector4(lr.position.X, lr.position.Y, lr.position.Z, 1.0f);
					myLightData[myCurrentLightIndex].direction = new Vector4(lr.direction.X, lr.direction.Y, lr.direction.Z, 1.0f);
					myLightData[myCurrentLightIndex].linearAttenuation = lr.linearAttenuation;
					myLightData[myCurrentLightIndex].quadraticAttenuation = lr.quadraticAttenuation;
					myLightData[myCurrentLightIndex].spotAngle = lr.spotAngle;
					myLightData[myCurrentLightIndex].spotExponential = lr.spotExponential;
					break;
			}
			myCurrentLightIndex++;
		}
		//public override void extractPerViewFinalize(RenderQueue q, View v){}
		//public override void onFrameExtractFinalize() { }
		#endregion

		#region prepare phase
		//public override void onFrameBeginPrepare() { }
		//public override void preparePerFrame(Renderable r) { }
		//public override void preparePerView(RenderInfo info, View v) { }
		public override void preparePerViewFinalize(BaseRenderQueue q, View v)
		{
			myLightUniforBuffer.setData(myLightData, 0, 255 * Marshal.SizeOf(typeof(LightUniformData)));
			myCurrentLightIndex = 0;
		}
		
		//public override void onFramePrepareFinalize() { } 
		#endregion

		#region submit phase
		//public override void onSubmitNodeBlockBegin(BaseRenderQueue q) {	}
		//public override void submitRenderInfo(RenderInfo r, RenderQueue q) { }
		//public override void onSubmitNodeBlockEnd(RenderQueue q) { }
		#endregion
	}
}