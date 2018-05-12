using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class PerPixelLightinglEffect : MaterialEffect
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
         myFeatures |= Material.Feature.ParallaxMap;

         //transparent drawing
         myTransparentPipeline.shaderState.shaderProgram = myShader;
         myTransparentPipeline.culling.enabled = false;
         myTransparentPipeline.blending.enabled = true;
         myTransparentPipeline.depthTest.enabled = true;
         myTransparentPipeline.depthWrite.enabled = true;
         myTransparentPipeline.generateId();

         //opaque drawing 
         myOpaquePipeline.shaderState.shaderProgram = myShader;
         myOpaquePipeline.culling.enabled = true;
         myOpaquePipeline.blending.enabled = false;
         myTransparentPipeline.depthTest.enabled = true;
         myOpaquePipeline.depthWrite.enabled = true;
         myOpaquePipeline.generateId();
      }

      public override void updateRenderState(Material m, RenderState state)
      {
         if (myLightVisualizer == null)
         {
            myLightVisualizer = Renderer.visualizers["light"] as LightVisualizer;
         }

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
         }

         //setup normal map if it exists
         if (m.myTextures[(int)Material.TextureId.Normal] != null)
         {
            tex = m.myTextures[(int)Material.TextureId.Normal].value();
            state.setTexture((int)tex.id(), 2, TextureTarget.Texture2D);
            state.setUniform(new UniformData(22, Uniform.UniformType.Int, 2));
         }

         state.setUniformBuffer(m.myMaterialUniformBuffer.id, 2);
         state.setUniformBuffer(myLightVisualizer.myLightUniforBuffer.id, 1);
      }

      public override PipelineState createPipeline(Material m)
      {
         Texture tex = m.myTextures[(int)Material.TextureId.Diffuse].value();

         //disable culling if this texture has alpha values so it can be seen from both sides
         if (tex.hasAlpha == true)
            return myTransparentPipeline;

         return myOpaquePipeline;
      }
   }
}