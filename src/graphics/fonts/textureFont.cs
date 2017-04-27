using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class TextureFont : Font
   {
      const int MAX_TEXT = 255;

      VertexBufferObject<V3T2> myVbo = new VertexBufferObject<V3T2>(BufferUsageHint.DynamicDraw);
      IndexBufferObject myIbo = new IndexBufferObject(BufferUsageHint.DynamicDraw);
      VertexArrayObject myVao = new VertexArrayObject();
      ShaderProgram myShader;

      V3T2[] myVerts = new V3T2[MAX_TEXT * 4];
      ushort[] myIndexes = new ushort[MAX_TEXT * 4];

      public TextureFont(String name, String textureName, int size)
         : base(name, size)
      {
         TextureDescriptor td = new TextureDescriptor(textureName, true);
         texture = Renderer.resourceManager.getResource(td) as Texture;
         buildFont();

			//set shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\font-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\font-ps.glsl", ShaderDescriptor.Source.File));
			ShaderProgramDescriptor shDesc = new ShaderProgramDescriptor(desc);
         myShader = Renderer.resourceManager.getResource(shDesc) as ShaderProgram;
			myVao.bindVertexFormat<V3T2>(myShader);
      }

      public bool buildFont()
      {
         float cx;								            // Holds Our X Character Coord
         float cy;								            // Holds Our Y Character Coord

         for (int loop = 0; loop < 256; loop++)				// Loop Through All 256 Lists
         {
            cx = (float)(loop % 16) / 16.0f;					// X Position Of Current Character
            cy = (float)(loop / 16) / 16.0f;					// Y Position Of Current Character
            Glyph g = new Glyph();
            g.minTexCoord.X = cx;
            g.minTexCoord.Y = 1 - cy - 0.0625f;
            g.maxTexCoord.X = cx + 0.0625f;
            g.maxTexCoord.Y = 1 - cy;
            myGlyphs[loop] = g;
         }												      // Loop Until All 256 Are Built

         return true;
      }

      public override void setupRenderCommand(StatelessRenderCommand rc)
      {
			//setup the pipeline
			rc.pipelineState.shaderProgram = myShader;
			rc.pipelineState.vao = myVao;

			//set renderstate
			rc.renderState.setVertexBuffer(myVbo.id, 0, 0, V3T2.stride);
			rc.renderState.setIndexBuffer(myIbo.id);
			rc.renderState.setTexture(texture.id(), 0, texture.target);
			rc.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
		}

      public override void drawText(String txt)
      {
         int counter = 0;
         int indexCount = 0;
         float posx = 0;
         float posy = 0;
         float advance = mySize * 0.6f;
         for (int i = 0; i < txt.Length; i++)
         {
            char ch = txt[i];
            Glyph g = myGlyphs[(int)ch - 32];

            myVerts[counter * 4].Position.X = posx;
            myVerts[counter * 4].Position.Y = posy;
            myVerts[counter * 4].Position.Z = 0.0f;
            myVerts[counter * 4].TexCoord.X = g.minTexCoord.X;
            myVerts[counter * 4].TexCoord.Y = g.minTexCoord.Y;

            myVerts[counter * 4 + 1].Position.X = posx + mySize;
            myVerts[counter * 4 + 1].Position.Y = posy;
            myVerts[counter * 4 + 1].Position.Z = 0.0f;
            myVerts[counter * 4 + 1].TexCoord.X = g.maxTexCoord.X;
            myVerts[counter * 4 + 1].TexCoord.Y = g.minTexCoord.Y;

            myVerts[counter * 4 + 2].Position.X = posx + mySize;
            myVerts[counter * 4 + 2].Position.Y = posy + mySize;
            myVerts[counter * 4 + 2].Position.Z = 0.0f;
            myVerts[counter * 4 + 2].TexCoord.X = g.maxTexCoord.X;
            myVerts[counter * 4 + 2].TexCoord.Y = g.maxTexCoord.Y;

            myVerts[counter * 4 + 3].Position.X = posx;
            myVerts[counter * 4 + 3].Position.Y = posy + mySize;
            myVerts[counter * 4 + 3].Position.Z = 0.0f;
            myVerts[counter * 4 + 3].TexCoord.X = g.minTexCoord.X;
            myVerts[counter * 4 + 3].TexCoord.Y = g.maxTexCoord.Y;

            //indices to draw as tris
            myIndexes[indexCount++] = (ushort)(0 + (counter * 4));
            myIndexes[indexCount++] = (ushort)(1 + (counter * 4));
            myIndexes[indexCount++] = (ushort)(3 + (counter * 4));
            myIndexes[indexCount++] = (ushort)(2 + (counter * 4));

            posx += advance;
            counter++;
         }

         //update the VBO
         myVbo.setData(myVerts, 0, (counter * 4 * V3T2.stride));
         myIbo.setData(myIndexes, 0, (indexCount * 2));

			//draw
			Renderer.device.bindVertexBuffer(myVbo.id, 0, 0, V3T2.stride);
			Renderer.device.bindIndexBuffer(myIbo.id);
			Renderer.device.drawIndexed(PrimitiveType.TriangleStrip, myIbo.count, 0, DrawElementsType.UnsignedShort);
      }

      public override int width(String txt)
      {
         //estimate for the width of a character
         return txt.Length * mySize - (mySize / 2);
      }

      public override int height(String txt)
      {
         return mySize;
      }

   }
}