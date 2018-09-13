using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public class SkydomeEffect : MaterialEffect
	{
		public SkydomeEffect(ShaderProgram sp) : base(sp)
		{
			myFeatures |= Material.Feature.Skydome;
		}

		public override void updateRenderState(Material m, RenderState rs)
		{
         Texture tint1 = (m.findAttribute("tint1") as TextureAttribute).value();
         Texture tint2 = (m.findAttribute("tint2") as TextureAttribute).value();
         Texture sun = (m.findAttribute("sun") as TextureAttribute).value();
         Texture moon = (m.findAttribute("moon") as TextureAttribute).value();
         Texture clouds1 = (m.findAttribute("clouds1") as TextureAttribute).value();
         Texture clouds2 = (m.findAttribute("clouds2") as TextureAttribute).value();


         rs.setTexture((int)tint1.id(), 0, TextureTarget.Texture2D);
         rs.setTexture((int)tint2.id(), 1, TextureTarget.Texture2D);
         rs.setTexture((int)sun.id(), 2, TextureTarget.Texture2D);
         rs.setTexture((int)moon.id(), 3, TextureTarget.Texture2D);
         rs.setTexture((int)clouds1.id(), 4, TextureTarget.Texture2D);
         rs.setTexture((int)clouds2.id(), 5, TextureTarget.Texture2D);

         rs.setUniform(new UniformData(20, Uniform.UniformType.Int, 0)); 
         rs.setUniform(new UniformData(21, Uniform.UniformType.Int, 1)); 
         rs.setUniform(new UniformData(22, Uniform.UniformType.Int, 2)); 
         rs.setUniform(new UniformData(23, Uniform.UniformType.Int, 3)); 
         rs.setUniform(new UniformData(24, Uniform.UniformType.Int, 4)); 
         rs.setUniform(new UniformData(25, Uniform.UniformType.Int, 5)); 
		}

		public override PipelineState createPipeline(Material m)
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