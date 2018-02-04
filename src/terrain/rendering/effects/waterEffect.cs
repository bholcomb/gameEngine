using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;

namespace Terrain
{
	public class PerPixelWaterEffect : Effect
	{
		LightVisualizer myLightVisualizer;

		public PerPixelWaterEffect(ShaderProgram sp) : base(sp)
		{
			myFeatures = (Graphics.Material.Feature)3; // 3 means water effect 
		}

		public override void updateRenderState(Graphics.Material m, RenderState state)
		{
			if (myLightVisualizer == null)
			{
				myLightVisualizer = Renderer.visualizers["light"] as LightVisualizer;
			}

			//setup diffuse map, it should exists
			ArrayTexture tex = (m.findAttribute("texArray") as TextureAttribute).value() as ArrayTexture;
			state.setTexture((int)tex.id(), 0, TextureTarget.Texture2DArray);
			state.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));

			//setup the lights that influence this terrain
			state.setUniformBuffer(myLightVisualizer.myLightUniforBuffer.id, 1);
		}

		public override PipelineState getPipeline(Graphics.Material m)
		{
			PipelineState state = new PipelineState();
			state.culling.enabled = true;
			state.blending.enabled = false;
			state.depthWrite.enabled = true;

			state.shaderState.shaderProgram = myShader;
			state.generateId();
			return state;
		}
	}
}