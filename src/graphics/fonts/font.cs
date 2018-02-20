using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;

namespace Graphics
{
   public class Glyph
   {
      public Vector2 minTexCoord;
      public Vector2 maxTexCoord;
      public Vector2 size;       		// size inside the Texture, in pixels
      public Vector2 offset;     		// offset from the top-left corner of the pen to the glyph image
      public Vector2 advance;	   	   // tells us how much we should advance the pen after drawing this glyph
   };

   public abstract class Font
   {
      protected List<Glyph> myGlyphs = new List<Glyph>();

      protected String myName;
      protected int mySize;

      public Font(String name, int size)
      {
         myName = name;
         mySize = size;

         for (int i = 0; i < 256; i++)
         {
            myGlyphs.Add(new Glyph());
         }
      }

      public Texture texture { get; set; }
      public Glyph findGlyph(char c)
      {
         if (c < 0 || c > 255)
         {
            return null;
         }

         return myGlyphs[c];
      }

      public String name
      {
         get { return myName; }
      }

      public int size
      {
         get { return mySize; }
         set { mySize = value; }
      }

      public abstract void setupRenderCommand(StatelessRenderCommand rc);
      public abstract void updateText(String str);
      public abstract void drawText();

      public virtual void print(int x, int y, String txt, params Object[] objs)
      {
         printScreen((float)x, (float)y, String.Format(txt, objs));
      }

      public virtual void print(Vector3 pos, String txt, params Object[] objs)
      {
         print3d(pos.X, pos.Y, pos.Z, String.Format(txt, objs));
      }

      public virtual void print(float x, float y, String txt, params Object[] objs)
      {
         printScreen( x,y,String.Format(txt, objs));
      }

      public virtual void print(float x, float y, float z, String txt, params Object[] objs)
      {
         print3d(x, y, z, String.Format(txt, objs));
      }

      public virtual int width(String txt, params Object[] objs)
      {
         return width(String.Format(txt, objs));
      }

      public virtual int height(String txt, params Object[] objs)
      {
         return height(String.Format(txt, objs));
      }

      public virtual void printScreen(float x, float y, String txt)
      {
         RenderFontCommand cmd = new RenderFontCommand(this, x, y, txt, Color4.White);
         cmd.execute();
      }

      public virtual void print3d(float x, float y, float z, String txt)
      {
         RenderFontCommand cmd = new RenderFontCommand(this, new Vector3(x, y, z), txt, Color4.White);
         cmd.execute();
      }

      public abstract int width(String txt);
      public abstract int height(String txt);
   }
}