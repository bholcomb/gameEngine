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


      #region prepare phase
      //public override void prepareFrameBegin() { }
      //public override void preparePerFrame(Renderable r) { }
      //public override void preparePerViewBegin(View v) { }    

      public override void preparePerView(Renderable r, View v)
		{
			LightRenderable lr = r as LightRenderable;

			float dist = (v.camera.position - r.position).Length;

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
      //public override void preparePerViewFinalize(View v) { }  

      //public override void preparePerPassBegin(Pass p) { }
      //public override void preparePerPass(Renderable r, Pass p) { }
      //public override void preparePerPassFinalize(Pass p) { }

      public override void prepareFrameFinalize()
		{
			myLightUniforBuffer.setData(myLightData, 0, 255 * Marshal.SizeOf(typeof(LightUniformData)));
			myCurrentLightIndex = 0;
		}

      #endregion

      #region generate command phase
      //public override void generateCommandsBegin(BaseRenderQueue q) { }
      //public override void generateRenderCommand(RenderInfo r, BaseRenderQueue q) { }
      //public override void generateCommandsFinalize(BaseRenderQueue q) { }
      #endregion
   }
}