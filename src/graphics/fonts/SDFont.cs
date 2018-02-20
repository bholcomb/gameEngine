using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class SDFont : Font
   {
      public enum Effects { None = 0x00,  Outline = 0x01, Shadow= 0x02};
      public Effects myEffects = Effects.None;

      String myFilename;
      const int MAX_TEXT = 250;
     
      VertexBufferObject<V3T2> myVbo = new VertexBufferObject<V3T2>(BufferUsageHint.DynamicDraw);
      IndexBufferObject myIbo = new IndexBufferObject(BufferUsageHint.DynamicDraw);
      VertexArrayObject myVao = new VertexArrayObject();
      ShaderProgram myShader;

      V3T2[] myVerts = new V3T2[MAX_TEXT * 4];
      ushort[] myIndexes = new ushort[MAX_TEXT * 4];

      public SDFont(String name, String filename, int size)
         : base(name, size)
      {
         myFilename = filename;
         buildFont();

			//set shader
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\font-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\sdfont-ps.glsl", ShaderDescriptor.Source.File));
			ShaderProgramDescriptor shDesc = new ShaderProgramDescriptor(desc);

			myShader = Renderer.resourceManager.getResource(shDesc) as ShaderProgram;
         myVao.bindVertexFormat<V3T2>(myShader);
      }

      public override void setupRenderCommand(StatelessRenderCommand rc)
      {
			//setup the pipeline
			rc.pipelineState.shaderState.shaderProgram = myShader;
			rc.pipelineState.vaoState.vao = myVao;
         rc.pipelineState.generateId();

			//set renderstate
			rc.renderState.setVertexBuffer(myVbo.id, 0, 0, V3T2.stride);
			rc.renderState.setIndexBuffer(myIbo.id);
			rc.renderState.setTexture(texture.id(), 0, texture.target);
			rc.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));


			//setup the uniforms in the renderstate
         rc.renderState.setUniform(new UniformData(21, Uniform.UniformType.Vec4, Vector4.One)); //baseColor
         rc.renderState.setUniform(new UniformData(22, Uniform.UniformType.Float, 25.0f)); //ailasingFactor
			rc.renderState.setUniform(new UniformData(23, Uniform.UniformType.Int, 0x00)); //activeEffects
			rc.renderState.setUniform(new UniformData(24, Uniform.UniformType.Vec4, new Vector4(1,0,0,1))); // outlineColor
//          rc.renderState.setUniform(new UniformData(25, Uniform.UniformType.Float, 0.4f)); //outlineMin_0
//          rc.renderState.setUniform(new UniformData(26, Uniform.UniformType.Float, 0.0f)); //outlineMax_0
//          rc.renderState.setUniform(new UniformData(27, Uniform.UniformType.Float, 0.0f)); //outlineMin_1
//          rc.renderState.setUniform(new UniformData(28, Uniform.UniformType.Float, 0.0f)); //outlineMax_1
			rc.renderState.setUniform(new UniformData(29, Uniform.UniformType.Vec4, new Vector4(0, 0, 0, 1))); //glowColor
			rc.renderState.setUniform(new UniformData(30, Uniform.UniformType.Vec2, Vector2.Zero)); //glowOffset
      }

      public bool buildFont()
      {
         TextureDescriptor td = new TextureDescriptor(myFilename + ".png", false);
         texture = Renderer.resourceManager.getResource(td) as Texture;

         FileInfo file = new FileInfo(myFilename + ".txt");
         StreamReader reader = file.OpenText();
         string line;
         while ((line = reader.ReadLine()) != null)
         {
            String[] splitters = { " " };
            String[] tokens = line.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            if (tokens[0] == "info")
            {
            }
            if (tokens[0] == "chars")
            {
            }
            if (tokens[0] == "char")
            {
               String[] split;

               //id
               split = tokens[1].Split('=');
               int id = Convert.ToInt32(split[1]);

               //X and Y are in pixels, so need to be converted to S/T coordinates (normalized)
               //x
               split = tokens[2].Split('=');
               myGlyphs[id].minTexCoord.X = Convert.ToSingle(split[1]) / texture.width;

               //y
               split = tokens[3].Split('=');
               myGlyphs[id].minTexCoord.Y = Convert.ToSingle(split[1]) / texture.height;

               //width and height are in pixels, need to convert to S/T coordinates
               //width
               split = tokens[4].Split('=');
               myGlyphs[id].maxTexCoord.X = myGlyphs[id].minTexCoord.X;
               myGlyphs[id].maxTexCoord.X += Convert.ToSingle(split[1]) / texture.width;
               myGlyphs[id].size.X = Convert.ToSingle(split[1]);

               //height
               split = tokens[5].Split('=');
               myGlyphs[id].maxTexCoord.Y = myGlyphs[id].minTexCoord.Y;
               myGlyphs[id].maxTexCoord.Y += Convert.ToSingle(split[1]) / texture.height;
               myGlyphs[id].size.Y = Convert.ToSingle(split[1]);

               //xoffset
               split = tokens[6].Split('=');
               myGlyphs[id].offset.X = Convert.ToSingle(split[1]);

               //yoffset
               split = tokens[7].Split('=');
               myGlyphs[id].offset.Y = Convert.ToSingle(split[1]);

               //xadvance
               split = tokens[8].Split('=');
               myGlyphs[id].advance.X = Convert.ToSingle(split[1]);
            }

         }
         return true;
      }

      public override void updateText(String txt)
      {
         //build the VBO
         ushort counter = 0;
         ushort indexCount = 0;
         float posx = 0;
         float posy = 0;

         for (int i = 0; i < txt.Length; i++)
         {
            char ch = txt[i];
            Glyph g = myGlyphs[(int)ch];

            posx += g.offset.X;
            //posy = g.offset.Y - mySize;

            myVerts[counter * 4].Position.X = posx;
            myVerts[counter * 4].Position.Y = posy;
            myVerts[counter * 4].Position.Z = 0.0f;
            myVerts[counter * 4].TexCoord.X = g.minTexCoord.X;
            myVerts[counter * 4].TexCoord.Y = g.maxTexCoord.Y;

            myVerts[counter * 4 + 1].Position.X = posx + g.size.X;
            myVerts[counter * 4 + 1].Position.Y = posy;
            myVerts[counter * 4 + 1].Position.Z = 0.0f;
            myVerts[counter * 4 + 1].TexCoord.X = g.maxTexCoord.X;
            myVerts[counter * 4 + 1].TexCoord.Y = g.maxTexCoord.Y;

            myVerts[counter * 4 + 2].Position.X = posx + g.size.X;
            myVerts[counter * 4 + 2].Position.Y = posy + g.size.Y;
            myVerts[counter * 4 + 2].Position.Z = 0.0f;
            myVerts[counter * 4 + 2].TexCoord.X = g.maxTexCoord.X;
            myVerts[counter * 4 + 2].TexCoord.Y = g.minTexCoord.Y;

            myVerts[counter * 4 + 3].Position.X = posx;
            myVerts[counter * 4 + 3].Position.Y = posy + g.size.Y;
            myVerts[counter * 4 + 3].Position.Z = 0.0f;
            myVerts[counter * 4 + 3].TexCoord.X = g.minTexCoord.X;
            myVerts[counter * 4 + 3].TexCoord.Y = g.minTexCoord.Y;

            //indicies to draw as tris
            myIndexes[indexCount++] = (ushort)(0 + (counter * 4));
            myIndexes[indexCount++] = (ushort)(1 + (counter * 4));
            myIndexes[indexCount++] = (ushort)(3 + (counter * 4));
            myIndexes[indexCount++] = (ushort)(2 + (counter * 4));

            posx += g.advance.X;
            counter++;
         }

			//update the VBO
			myVbo.setData(myVerts, 0, (counter * 4 * V3T2.stride));
			myIbo.setData(myIndexes, 0, (indexCount * 2));
		}

      public override void drawText()
      {
         Renderer.device.bindVertexBuffer(myVbo.id, 0, 0, V3T2.stride);
         Renderer.device.bindIndexBuffer(myIbo.id);
         Renderer.device.drawIndexed(PrimitiveType.TriangleStrip, myIbo.count, 0, DrawElementsType.UnsignedShort);
      }

      public override int width(String txt)
      {
         int size = 0;
         for (int i = 0; i < txt.Length; i++)
         {
            char c = txt[i];
            size += (int)myGlyphs[(int)c].size.X;
         }

         return size;
      }

      public override int height(String txt)
      {
         return mySize;
      }
   }
}