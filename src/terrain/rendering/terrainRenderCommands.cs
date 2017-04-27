using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Terrain
{
   /*
   public class RenderTerrainChunkCommand : RenderCommand
   {
      public Vector3 myModelLocation;
      public bool isTransparent = false;
      public UInt32 distance = 0;

      public int myOffset;
      public int myCount;

      static ShaderProgram theShader;

      static RenderTerrainChunkCommand()
      {
         ShaderProgramDescriptor spDesc = new ShaderProgramDescriptor("Terrain.shaders.terrain-vs.glsl", "Terrain.shaders.terrain-gs.glsl", "Terrain.shaders.terrain-ps.glsl");
         theShader = ResourceManager.getResource(spDesc) as ShaderProgram;

         TerrainRenderManager.myVao.bindWithShader<TerrainVertex>(theShader, TerrainRenderManager.myTerrainBuffer, null);
      }

      public RenderTerrainChunkCommand(Vector3 location, int offset, int count)
         : base()
      {
         myModelLocation = location;
         myOffset = offset;
         myCount = count;

         //setup the vao
         myRenderState.blending.enabled = true;
         myRenderState.alphaTest.enabled = true;
         myRenderState.alphaTest.limit = 0.1f;
         myRenderState.alphaToCoverage.enabled = true;
         myRenderState.shaderProgram = theShader;
         myRenderState.vao = TerrainRenderManager.myVao;
         TextureSampler ts = new TextureSampler(MaterialManager.myMaterialTextureArray);
         myRenderState.textures.Add(ts);
         myRenderState.blending.enabled = true;

         //these things don't move, just set the uniform once
         myRenderState.updateUniform(new UniformData("model", Uniform.UniformType.Mat4, Matrix4.CreateTranslation(myModelLocation)));
      }

      public override void execute()
      {
         myRenderState.apply();
         GL.DrawArrays(PrimitiveType.Triangles, myOffset, myCount);
      }
   }
   */
}