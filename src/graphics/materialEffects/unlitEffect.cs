using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public class UnlitEffect : MaterialEffect
	{
		public UnlitEffect(ShaderProgram sp) : base(sp)
		{
			myFeatures |= Material.Feature.DiffuseMap;
		}

		public override void updateRenderState(Material m, RenderState state)
		{

         if (m.myTextures[(int)Material.TextureId.Diffuse] != null)
         {
            Texture tex = m.myTextures[(int)Material.TextureId.Diffuse].value();
            state.setTexture((int)tex.id(), 0, TextureTarget.Texture2D);
            state.setUniform(new UniformData(20, Uniform.UniformType.Bool, true));
            state.setUniform(new UniformData(21, Uniform.UniformType.Int, 0));
         }
         else
         {
            state.setUniform(new UniformData(20, Uniform.UniformType.Bool, false));
         }

			state.setUniform(new UniformData(22, Uniform.UniformType.Color4, m.diffuse));
			state.setUniform(new UniformData(23, Uniform.UniformType.Float, m.alpha));
		}

		public override PipelineState createPipeline(Material m)
		{
			PipelineState state = new PipelineState();
			Texture tex = m.myTextures[(int)Material.TextureId.Diffuse].value();

			//disable culling if this texture has alpha values so it can be seen from both sides
			if (tex.hasAlpha == true || m.alpha != 1.0)
			{
				state.culling.enabled = false;
				state.blending.enabled = true;
				state.depthWrite.enabled = false;
			}

			state.shaderState.shaderProgram = myShader;

			state.generateId();
			return state;
		}
	}
}
