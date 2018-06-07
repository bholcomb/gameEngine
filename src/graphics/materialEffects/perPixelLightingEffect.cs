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