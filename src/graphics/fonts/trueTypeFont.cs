using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Util;
using FreeType;

namespace Graphics
{
   public class TTFFont : Font
   {
      String myFilename;
      const int MAX_TEXT = 255;

      VertexBufferObject<V3T2> myVbo = new VertexBufferObject<V3T2>(BufferUsageHint.DynamicDraw);
      IndexBufferObject myIbo = new IndexBufferObject(BufferUsageHint.DynamicDraw);
      VertexArrayObject myVao = new VertexArrayObject();
      ShaderProgram myShader;

      V3T2[] myVerts = new V3T2[MAX_TEXT * 4];
      ushort[] myIndexes = new ushort[MAX_TEXT * 4];

      int yCounter = 2;
      int xCounter = 0;

      public TTFFont(String name, String filename, int size)
         : base(name, size)
      {
         for (int i = 0; i < 128; i++)
         {
            myGlyphs.Add(new Glyph());
         }

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

			//set renderstate
			rc.renderState.setVertexBuffer(myVbo.id, 0, 0, V3T2.stride);
			rc.renderState.setIndexBuffer(myIbo.id);
			rc.renderState.setTexture(texture.id(), 0, texture.target);
			rc.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
		}

      public bool buildFont()
      {
         int error = 0;
         IntPtr libptr = IntPtr.Zero;
         IntPtr faceptr = IntPtr.Zero;

         error = FT.Init_FreeType(out libptr);
         if (error != 0)
         {
            System.Console.WriteLine("FT_Init_FreeType failed");
            return false;
         }

         int[] maj = new int[1];
         int[] min = new int[1];
         int[] patch = new int[1];

         FT.Library_Version(libptr, maj, min, patch);
         System.Console.WriteLine("Freetype Version {0}.{1}.{2}", maj[0], min[0], patch[0]);

         error = FT.New_Face(libptr, myFilename, 0, out faceptr);
         if (error == FT.Err_Unknown_File_Format)
         {
            System.Console.WriteLine("FT_New_Face Failed: Unknown Font Format");
            return false;
         }

         else if (error != 0)
         {
            System.Console.WriteLine("FT_New_Face Failed: Unknown Error: " + error);
            return false;
         }

         //set the size of the Font in pixels
         error = FT.Set_Pixel_Sizes(faceptr, (uint)0, (uint)mySize);
         if (error != 0)
         {
            System.Console.WriteLine("FT_Set_Pixel_Sizes Failed: Unknown Error: " + error);
            return false;
         }

         texture = new Texture(256, 256);

         uint glyphIndex;
         uint asciiCounter;

         //only going to get ascii characters at this time
         //32 128
         for (asciiCounter = 32; asciiCounter < 128; asciiCounter++)
         {
            //get the character loaded
            glyphIndex = FT.Get_Char_Index(faceptr, asciiCounter);
            error = FT.Load_Glyph(faceptr, glyphIndex, FT.LOAD_RENDER);
            if (error != 0)
            {
               System.Console.WriteLine("FT_Load_Glyph Failed: Unknown Error: " + error);
               return false;
            }

            //copy to our Texture and update the myGlyph data structure
            FaceRec f = (FaceRec)Marshal.PtrToStructure(faceptr, typeof(FaceRec));
            GlyphSlotRec g = (GlyphSlotRec)Marshal.PtrToStructure(f.glyph, typeof(GlyphSlotRec));

            addGlyph(g, (int)asciiCounter);
         }

         FT.Done_Face(faceptr);
         FT.Done_FreeType(libptr);

         return true;
      }

      public override void updateText(String txt)
      {
         int counter = 0;
         int indexCount = 0;
         float posx = 0;
         float posy = 0;
         for (int i = 0; i < txt.Length; i++)
         {
            char ch = txt[i];
            Glyph g = myGlyphs[(int)ch];

            posx += g.offset.X;
            posy = g.offset.Y;

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

      public override float width(String txt)
      {
         int size = 0;
         for (int i = 0; i < txt.Length; i++)
         {
            char c = txt[i];
            size += (int)myGlyphs[(int)c].size.X;
         }

         return size;
      }

      public override float height(String txt)
      {
         return mySize;
      }

      bool addGlyph(GlyphSlotRec glyph, int ascii)
      {
         if (glyph.bitmap.pixel_mode != (sbyte)Pixel_Mode.PIXEL_MODE_GRAY && glyph.bitmap.num_grays != 256)
         {
            System.Console.WriteLine("TtfFont::addGlyph Error: source data is wrong format ");
            return false;
         }

         if (xCounter + glyph.bitmap.width > texture.width)
         {
            xCounter = 0;
            yCounter += (int)mySize;
         }

         if (yCounter + glyph.bitmap.rows > texture.height)
         {
            System.Console.WriteLine("TtfFont::addGlyph Error: no room in Texture ");
            return false;
         }

         int byteCount = glyph.bitmap.rows * glyph.bitmap.width;
         Byte[] pixels = new Byte[byteCount * 4];
         Byte[] buffer = new Byte[glyph.bitmap.rows * glyph.bitmap.width];

         //copy the data to a managed buffer for use
         if (byteCount > 0)
            Marshal.Copy(glyph.bitmap.buffer, buffer, 0, byteCount);

         for (int y = 0; y < glyph.bitmap.rows; y++)
         {
            for (int x = 0; x < glyph.bitmap.width; x++)
            {
               int index = (y * glyph.bitmap.width) + x;
               pixels[index * 4] = buffer[index];
               pixels[index * 4 + 1] = buffer[index];
               pixels[index * 4 + 2] = buffer[index];
               pixels[index * 4 + 3] = buffer[index];
               //pixels[index * 4 + 3] = 0;
            }
         }

         //copy bitmap to the Texture
         if (byteCount > 0)
            texture.paste(pixels, new Vector2((float)xCounter, (float)yCounter),
                new Vector2((float)glyph.bitmap.width, (float)glyph.bitmap.rows), PixelFormat.Rgba);

         Glyph g = new Glyph();
         g.minTexCoord.X = (float)xCounter / (float)texture.width;
         g.minTexCoord.Y = (float)(yCounter + glyph.bitmap.rows) / (float)texture.height;

         g.maxTexCoord.X = (float)(xCounter + glyph.bitmap.width) / (float)texture.width;
         g.maxTexCoord.Y = (float)yCounter / (float)texture.height;

         g.size.X = (float)glyph.bitmap.width;
         g.size.Y = (float)glyph.bitmap.rows;
         g.offset.X = (float)glyph.bitmap_left;
         g.offset.Y = (float)glyph.bitmap_top - (float)glyph.bitmap.rows;
         g.advance.X = (float)glyph.advance.x / (float)64;
         g.advance.Y = (float)glyph.advance.y / (float)64;

         myGlyphs[ascii] = g;

         xCounter += glyph.bitmap.width;
         xCounter += 2; //for spacing between letters in bitmap;

         return true;
      }
   }
}