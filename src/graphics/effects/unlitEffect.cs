using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public class UnlitEffect : Effect
	{
		public UnlitEffect(ShaderProgram sp) : base(sp)
		{
			myFeatures |= Material.Feature.DiffuseMap;
		}

		public override void updateRenderState(Material m, RenderState state)
		{
			if(m.hasAttribute("diffuseMap"))
			{
				Texture tex = (m.findAttribute("diffuseMap") as TextureAttribute).value();
				state.setTexture((int)tex.id(), 0, TextureTarget.Texture2D);
				state.setUniform(new UniformData(0, Uniform.UniformType.Bool, true));
				state.setUniform(new UniformData(1, Uniform.UniformType.Int, 0));
			}
			else
				state.setUniform(new UniformData(0, Uniform.UniformType.Bool, false));

			state.setUniform(new UniformData(2, Uniform.UniformType.Color4, m.diffuse));
			state.setUniform(new UniformData(3, Uniform.UniformType.Float, m.alpha));
		}

		public override PipelineState getPipeline(Material m)
		{
			PipelineState state = new PipelineState();
			Texture tex = (m.findAttribute("diffuseMap") as TextureAttribute).value();

			//disable culling if this texture has alpha values so it can be seen from both sides
			if (tex.hasAlpha == true || m.alpha != 1.0)
			{
				state.culling.enabled = false;
				state.blending.enabled = true;
				state.depthWrite = false;
			}

			state.shaderProgram = myShader;

			state.generateId();
			return state;
		}
	}
}