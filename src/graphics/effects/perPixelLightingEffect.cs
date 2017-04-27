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
  layout(std140) uniform material {
		uniform vec4 matAmbientReflectivity;
		uniform vec4 matDiffuseReflectivity;
		uniform vec4 matSpecularReflectivity;
		uniform vec4 emmission;
		uniform float shininess;
		uniform float alpha;
		uniform bool hasSpecularMap;
		uniform bool hasNormalMap;		
  };
	*/
	[StructLayout(LayoutKind.Sequential)]
	public struct PerPixelUniformData
	{
		public Color4 ambientReflectivity;
		public Color4 diffuseReflectivity;
		public Color4 specularReflectivity;
		public Color4 emmission;
		public float shininess;
		public float alpha;
		public bool hasSpecularMap;
		byte pad0;
		byte pad1;
		byte pad3;
		public bool hasNormalMap;
		byte pad4;
		byte pad5;
		byte pad6;

		public static int theSize = Marshal.SizeOf(typeof(PerPixelUniformData));

		public unsafe byte[] toBytes()
		{
			byte[] bytes = new byte[theSize];

			fixed (PerPixelUniformData* srcPtr = &this)
			{
				fixed (byte* destPtr = &bytes[0])
				{
					Util.memcpy((IntPtr)destPtr, (IntPtr)srcPtr, theSize);
				}
			}
			return bytes;
		}
	};

	public class PerPixelLightinglEffect : Effect
	{
		UniformBufferObject myMaterialUniform = new UniformBufferObject(BufferUsageHint.DynamicDraw);
		LightVisualizer myLightVisualizer;

		PipelineState myTransparentPipeline = new PipelineState();
		PipelineState myOpaquePipeline = new PipelineState();

		public PerPixelLightinglEffect(ShaderProgram sp) : base(sp)
		{
			myFeatures |= Material.Feature.Lighting;
			myFeatures |= Material.Feature.DiffuseMap;
			myFeatures |= Material.Feature.SpecMap;
			myFeatures |= Material.Feature.NormalMap;

			//enable blending
			myTransparentPipeline.shaderProgram = myShader;
			myTransparentPipeline.culling.enabled = false;
			myTransparentPipeline.blending.enabled = true;
			myTransparentPipeline.depthWrite = false;
			myTransparentPipeline.generateId();

			//use default settings
			myOpaquePipeline.shaderProgram = myShader;
			myOpaquePipeline.generateId();
		}

		public override void updateRenderState(Material m, RenderState state)
		{
			if(myLightVisualizer == null)
			{
				myLightVisualizer = Renderer.visualizers["light"] as LightVisualizer;
			}

			//material properties
			PerPixelUniformData matData = new PerPixelUniformData();
			matData.ambientReflectivity = m.ambient;
			matData.diffuseReflectivity = m.diffuse;
			matData.specularReflectivity = m.spec;
			matData.emmission = m.emission;
			matData.shininess = m.shininess;
			matData.alpha = m.alpha;

			//setup diffuse map, it should exists
			Texture tex = m.myTextures[(int)Material.TextureId.Diffuse].value();
			state.setTexture((int)tex.id(), 0, TextureTarget.Texture2D);
			state.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));

			//setup specular map if it exists
			if (m.myTextures[(int)Material.TextureId.Specular] != null)
			{
				tex = m.myTextures[(int)Material.TextureId.Specular].value();
				state.setTexture((int)tex.id(), 1, TextureTarget.Texture2D);
				state.setUniform(new UniformData(21, Uniform.UniformType.Int, 1));
				matData.hasSpecularMap = true;
			}
			else
				matData.hasSpecularMap = false;

			//setup normal map if it exists
			if (m.myTextures[(int)Material.TextureId.Normal] != null)
			{
				tex = m.myTextures[(int)Material.TextureId.Normal].value();
				state.setTexture((int)tex.id(), 2, TextureTarget.Texture2D);
				state.setUniform(new UniformData(22, Uniform.UniformType.Int, 2));
				matData.hasNormalMap = true;
			}
			else
				matData.hasNormalMap = false;


			byte[] data = matData.toBytes();
			state.setUniformUpload(myMaterialUniform, data);
			state.setUniformBuffer(myMaterialUniform.id, 3);
			state.setUniformBuffer(myLightVisualizer.myLightUniforBuffer.id, 1);
		}

		public override PipelineState getPipeline(Material m)
		{
			Texture tex = m.myTextures[(int)Material.TextureId.Diffuse].value();

			//disable culling if this texture has alpha values so it can be seen from both sides
			if (tex.hasAlpha == true)
				return myTransparentPipeline;

			return myOpaquePipeline;
		}
	}
}