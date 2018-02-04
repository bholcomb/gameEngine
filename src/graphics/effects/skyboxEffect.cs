using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public class SkyboxEffect : Effect
	{
		public SkyboxEffect(ShaderProgram sp) : base(sp)
		{
			myFeatures |= Material.Feature.Skybox;
		}

		public override void updateRenderState(Material m, RenderState rs)
		{
			CubemapTexture cubemap = m.myTextures[(int)Material.TextureId.Skybox].value() as CubemapTexture;
			rs.setTexture((int)cubemap.id(), 0, TextureTarget.TextureCubeMap);
			rs.setUniform(new UniformData(20, Uniform.UniformType.Int, 0)); // we know this location from the shader
			rs.setVertexBuffer(SkyBox.theVbo.id, 0, 0, V3.stride);
			rs.setIndexBuffer(SkyBox.theIbo.id);
		}

		public override PipelineState getPipeline(Material m)
		{
			PipelineState ps = new PipelineState();
			ps.shaderState.shaderProgram = myShader;
			ps.depthTest.enabled = false;
			ps.depthWrite.enabled = false;
         ps.culling.enabled = false;
         ps.depthTest.depthFunc = DepthFunction.Lequal;
			ps.generateId();
			return ps;
		}
	}
}