using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

using Util;
using Graphics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace UI2
{
   public class Glyph
   {
      public Vector2 minTexCoord;
      public Vector2 maxTexCoord;
      public Vector2 size;             // size inside the Texture, in pixels
      public Vector2 offset;           // offset from the top-left corner of the pen to the glyph image
      public Vector2 advance;          // tells us how much we should advance the pen after drawing this glyph
   };

   //used for UI rendering
   [StructLayout(LayoutKind.Sequential)]
   public struct V2T2B4
   {
      static int theStride = Marshal.SizeOf(default(V2T2B4));

      public Vector2 Position; //8 bytes
      public Vector2 TexCoord; //8 bytes
      public UInt32 Color;  // 4 bytes

      public static int stride { get { return theStride; } }

      public static void bindVertexAttribute(String fieldName, int id)
      {
         switch (fieldName)
         {
            case "position":
               GL.VertexAttribFormat(id, 2, VertexAttribType.Float, false, 0);
               break;
            case "uv":
               GL.VertexAttribFormat(id, 2, VertexAttribType.Float, false, 8);
               break;
            case "color":
               GL.VertexAttribFormat(id, 4, VertexAttribType.UnsignedByte, true, 16);
               break;
            default:
               throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
         }
      }
   }

   internal class UiRenderCommand : StatelessRenderCommand
   {
      static PipelineState thePipelineState;

      int myElementOffset;
      int myElementCount;

      static UiRenderCommand()
      {
         thePipelineState = new PipelineState();
         thePipelineState.blending.enabled = true;
         thePipelineState.shaderState.shaderProgram = Canvas.theShader;
         thePipelineState.blending.enabled = true;
         thePipelineState.culling.enabled = false;
         thePipelineState.depthTest.enabled = false;
         thePipelineState.vaoState.vao = new VertexArrayObject();
         thePipelineState.vaoState.vao.bindVertexFormat<V2T2B4>(Canvas.theShader);
         thePipelineState.generateId();
      }

      public UiRenderCommand(DrawCmd drawCmd, Matrix4 modelmatrix, VertexBufferObject<V2T2B4> vbo, IndexBufferObject ibo)
         : base()
      {
         myElementCount = (int)drawCmd.elementCount;
         myElementOffset = (int)drawCmd.elementOffset;

         pipelineState = thePipelineState;

         renderState.scissorTest.enabled = true;
         renderState.scissorTest.rect = drawCmd.clipRect;
         renderState.setVertexBuffer(vbo.id, 0, 0, V2T2B4.stride);
         renderState.setIndexBuffer(ibo.id);

         renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, 0));
         renderState.setUniform(new UniformData(0, Uniform.UniformType.Mat4, modelmatrix));
         renderState.setTexture((int)drawCmd.texture.id(), 0, TextureTarget.Texture2D);
      }

      public override void execute()
      {
         base.execute();

         GL.DrawElements(BeginMode.Triangles, myElementCount, DrawElementsType.UnsignedShort, myElementOffset * 2);  //unsigned short in bytes
      }
   }

   internal class DrawCmd
   {
      public int index;
      public int layer;
      public UInt32 elementOffset;
      public UInt32 elementCount;
      public Vector4 clipRect;
      public Texture texture;
      public RenderCommand userRenderCommand;

      public DrawCmd()
      {
         layer = 0;
         elementOffset = 0;
         elementCount = 0;
         clipRect = new Vector4(0.0f, 0.0f, 8192.0f, 8192.0f);
         texture = null;
         userRenderCommand = null;
      }
   };

   public class Canvas
   {
      [Flags]
      public enum Corners { NONE = 0, LL = 1, LR = 2, UR = 4, UL = 8, TOP = UL | UR, BOTTOM = LL | LR, LEFT = LL | UL, RIGHT = LR | UR, ALL = LL | LR | UL | UR }
      public enum Icons
      {
         CHECKBOX_UNCHECKED = 256, //start of the icons in the built in texture
         CHECKBOX_CHECKED
      };

      public static readonly Vector4 theNullClipRect = new Vector4(0.0f, 0.0f, +8192.0f, +8192.0f);
      public static readonly Vector2 uv_zero = new Vector2(0, 0);
      public static readonly Vector2 uv_one = new Vector2(1, 1);
      public static readonly Color4 col_white = new Color4(255, 255, 255, 255);

      public static ShaderProgram theShader;
      public VertexBufferObject<V2T2B4> myVbo = new VertexBufferObject<V2T2B4>(BufferUsageHint.DynamicDraw);
      public IndexBufferObject myIbo = new IndexBufferObject(BufferUsageHint.DynamicDraw);
      Vector2 myScreenSize;
      float myScale = 1.0f;
      Matrix4 myModelMatrix;

      public static Texture theDefaultTexture;
      static List<Glyph> theGlyphs;

      const int MAX_ELEMENTS = 65536;
      internal V2T2B4[] myVerts = new V2T2B4[MAX_ELEMENTS];
      internal UInt16[] myIndexes = new UInt16[MAX_ELEMENTS];
      internal UInt32 myVertCount = 0;
      internal UInt32 myIndexCount = 0;
      internal List<DrawCmd> myCmdBuffer = new List<DrawCmd>();

      List<Vector4> clipRectStack = new List<Vector4>();
      List<Texture> textureStack = new List<Texture>();
      List<int> layerStack = new List<int>();
      List<Vector2> path = new List<Vector2>(); //internal stateful path data

      DrawCmd currentCommand { get { return myCmdBuffer.Last(); } }
      Vector4 currentClipRect { get { return clipRectStack.Count > 0 ? clipRectStack.Last() : theNullClipRect; } }
      Texture currentTexture { get { return textureStack.Count > 0 ? textureStack.Last() : null; } }
      int currentLayer { get { return layerStack.Count > 0 ? layerStack.Last() : 0; } }

      #region Top level
      static Canvas()
      {
         ShaderProgramDescriptor desc = new ShaderProgramDescriptor("UI.shaders.ui-vs.glsl", "UI.shaders.ui-ps.glsl");
         theShader = Renderer.resourceManager.getResource(desc) as ShaderProgram;

         //we're expecting a sheet that is 32x32 glyphs each 32pix x 32pix in size. (1024 x 1024 texture)
         theDefaultTexture = Graphics.Util.getEmbeddedTexture("UI.data.fontIconSheet.png");
         theDefaultTexture.setMinMagFilters(TextureMinFilter.NearestMipmapNearest, TextureMagFilter.Nearest);

         theGlyphs = new List<Glyph>(1024);
         float width = theDefaultTexture.width;
         float height = theDefaultTexture.height;
         float step = 1.0f / 32.0f;
         for (int i = 0; i < 1024; i++)
         {
            float cx = (float)(i % 32) / 32.0f;					// X Position Of Current Character
            float cy = (float)(i / 32) / 32.0f;					// Y Position Of Current Character
            Glyph g = new Glyph();
            g.size = new Vector2(32, 32);
            g.minTexCoord.X = cx;
            g.minTexCoord.Y = 1 - cy - step;
            g.maxTexCoord.X = cx + step;
            g.maxTexCoord.Y = 1 - cy;
            g.advance = g.size * 0.6f;
            theGlyphs.Add(g);
         }
      }

      public Canvas()
      {
         reset();
      }

      public void reset()
      {
         myVertCount = 0;
         myIndexCount = 0;
         path.Clear();
         clipRectStack.Clear();
         textureStack.Clear();
         textureStack.Add(theDefaultTexture);
         layerStack.Clear();
         myCmdBuffer.Clear();

         addDrawCommand();
      }

      public void setScreenResolution(Vector2 screenResolution)
      {
         myScreenSize = screenResolution;
      }

      public void setScale(float scale)
      {
         myScale = scale;
      }

      public void generateCommands(ref List<RenderCommand> cmds)
      {
         myVbo.setData(myVerts, (int)myVertCount);
         myIbo.setData(myIndexes, (int)myIndexCount);


         Matrix4 transform = Matrix4.CreateTranslation(new Vector3(0, myScreenSize.Y, 0));
         Matrix4 scale = Matrix4.CreateScale(new Vector3(myScale, -myScale, 1));
         myModelMatrix = scale * transform;

         //sort based on layer depth 
         myCmdBuffer.Sort((a, b) =>
         {
            int c = a.layer.CompareTo(b.layer);
            if (c != 0)
               return c;

            return a.index.CompareTo(b.index);
         });

         foreach (DrawCmd dc in myCmdBuffer)
         {

            if (dc.userRenderCommand != null)
            {
               cmds.Add(dc.userRenderCommand);
            }
            else
            {
               if (dc.elementCount > 0)
                  cmds.Add(new UiRenderCommand(dc, myModelMatrix, myVbo, myIbo));
            }
         }
      }

      #endregion

      #region Primative Drawing
      public void addLine(Vector2 a, Vector2 b, Color4 col, float thickness = 1.0f)
      {
         if (col.A == 0.0)
            return;

         pathLineTo(a + new Vector2(0.5f, 0.5f));
         pathLineTo(b + new Vector2(0.5f, 0.5f));
         pathStroke(col, false, thickness);
      }
      public void addRect(Rect r, Color4 col, float rounding = 0.0f, Corners rounding_corners = Corners.ALL)
      {
         addRect(r.SW, r.NE, col, rounding, rounding_corners);
      }
      public void addRect(Vector2 a, Vector2 b, Color4 col, float rounding = 0.0f, Corners rounding_corners = Corners.ALL)
      {
         if (col.A == 0.0)
            return;

         pathRect(a + new Vector2(0.5f, 0.5f), b + new Vector2(0.5f, 0.5f), rounding, rounding_corners);
         pathStroke(col, true);
      }
      public void addRectFilled(Rect r, Color4 col, float rounding = 0.0f, Corners rounding_corners = Corners.ALL)
      {
         addRectFilled(r.SW, r.NE, col, rounding, rounding_corners);
      }
      public void addRectFilled(Vector2 a, Vector2 b, Color4 col, float rounding = 0.0f, Corners rounding_corners = Corners.ALL)
      {
         if (col.A == 0.0)
            return;

         if (rounding > 0)
         {
            pathRect(a, b, rounding, rounding_corners);
            pathFill(col);
         }
         else
         {
            primativeRect(a, b, col);
         }
      }
      public void addRectFilledMultiColor(Rect r, Color4 col_upr_left, Color4 col_upr_right, Color4 col_bot_right, Color4 col_bot_left)
      {
         addRectFilledMultiColor(r.SW, r.NE, col_upr_left, col_upr_right, col_bot_right, col_bot_left);
      }
      public void addRectFilledMultiColor(Vector2 a, Vector2 b, Color4 col_upr_left, Color4 col_upr_right, Color4 col_bot_right, Color4 col_bot_left)
      {
         if (col_upr_left.A == 0 && col_upr_right.A == 0.0 && col_bot_left.A == 0 && col_bot_right.A == 0)
            return;

         primativeRect(a, b, col_upr_left);

         //update the last 4 verts with the right colors
         myVerts[myVertCount - 4].Color = col_upr_left.toUInt();
         myVerts[myVertCount - 3].Color = col_upr_right.toUInt();
         myVerts[myVertCount - 2].Color = col_bot_right.toUInt();
         myVerts[myVertCount - 1].Color = col_bot_left.toUInt();

      }
      public void addTriangleFilled(Vector2 a, Vector2 b, Vector2 c, Color4 col)
      {
         if (col.A == 0.0)
            return;

         pathLineTo(a);
         pathLineTo(b);
         pathLineTo(c);
         pathFill(col);
      }
      public void addCircle(Vector2 center, float radius, Color4 col, bool filled = false, int num_segments = 12)
      {
         if (col.A == 0.0)
            return;

         float a_max = (float)Math.PI * 2.0f * ((float)num_segments - 1.0f) / (float)num_segments;
         pathArcTo(center, radius, 0.0f, a_max, num_segments);
         if (filled)
            pathFill(col);
         else
            pathStroke(col, true);
      }

      public void addText(Rect r, Color4 col, String text, Alignment align, float size = 0.0f)
      {
         Vector2 pos = r.SW;
         Vector2 textSize = new Vector2(UI.style.font.width(text), UI.style.font.height(text));
         if (align.HasFlag(Alignment.HCenter) == true) pos.X = Math.Max(pos.X, (r.SW.X + r.NE.X - textSize.X) * 0.5f);
         if (align.HasFlag(Alignment.Right) == true) pos.X = Math.Max(pos.X, r.NE.X - textSize.X);
         if (align.HasFlag(Alignment.VCenter) == true) pos.Y = Math.Max(pos.Y, (r.SW.Y + r.NE.Y - textSize.Y) * 0.5f);

         addText(pos, col, text, size);
      }

      public void addText(Vector2 pos, Color4 col, String text, float size = 0.0f)
      {
         if (col.A == 0.0)
            return;

         if (size == 0)
            size = UI.style.font.fontSize;

         primativeText(size, pos, col, text);
      }

      public void addText(Font font, float size, Vector2 pos, Color4 col, String text, float wrap_width = 0.0f)
      {
         if (col.A == 0.0)
            return;

         //addCustomRenderCommand(new RenderFontCommand(font, text, col, pos.X + myPosition.X, pos.Y + myPosition.Y));
      }

      public void addIcon(Icons icon, Rect r)
      {
         addIcon(icon, r.SW, r.NE);
      }

      public void addIcon(Icons icon, Vector2 a, Vector2 b)
      {
         Glyph g = theGlyphs[(int)icon];
         addImage(theDefaultTexture, a, b, g.minTexCoord, g.maxTexCoord, Color4.White);
      }

      public void addImage(Texture tex, Rect r, Vector2 uv0, Vector2 uv1, Color4 col)
      {
         addImage(tex, r.SW, r.NE, uv0, uv1, col);
      }
      public void addImage(Texture tex, Vector2 a, Vector2 b, Vector2 uv0, Vector2 uv1, Color4 col)
      {
         if (col.A == 0.0)
            return;

         bool pushTex = shouldPushTexture(tex);
         if (pushTex)
            pushTexture(tex);

         //fix the UV's so they're flipped appropriately for the system we're using
         Vector2 _uv0 = new Vector2(uv0.X, uv1.Y);
         Vector2 _uv1 = new Vector2(uv1.X, uv0.Y);

         primativeRectUv(a, b, _uv0, _uv1, col);

         if (pushTex)
            popTexture();
      }

      public void addPolyline(List<Vector2> points, Color4 col, bool closed, float thickness)
      {
         if (points.Count < 2)
            return;

         Vector2 uv = uv_zero;

         int count = points.Count;
         if (!closed)
            count = count - 1;

         bool thickLine = thickness > 1.0f;

         for (int i1 = 0; i1 < count; i1++)
         {
            int i2 = (i1 + 1) == points.Count ? 0 : i1 + 1;
            Vector2 p1 = points[i1];
            Vector2 p2 = points[i2];
            Vector2 diff = p2 - p1;
            diff.Normalize();

            float dx = diff.X * (thickness * 0.5f);
            float dy = diff.Y * (thickness * 0.5f);

            indexTwoNewTriangles();

            writeVertex(new Vector2(p1.X + dy, p1.Y - dx), uv, col);
            writeVertex(new Vector2(p2.X + dy, p2.Y - dx), uv, col);
            writeVertex(new Vector2(p2.X - dy, p2.Y + dx), uv, col);
            writeVertex(new Vector2(p1.X - dy, p1.Y + dx), uv, col);
         }
      }
      public void addConvexPolyFilled(List<Vector2> points, Color4 col)
      {
         Vector2 uv = uv_zero;

         int idxCount = (points.Count - 2) * 3;
         int vtxCount = points.Count;
         int vtxIdx = (int)myVertCount;

         for (int i = 0; i < vtxCount; i++)
         {
            writeVertex(points[i], uv, col);
         }
         for (int i = 2; i < points.Count; i++)
         {
            writeIndex((UInt16)vtxIdx); writeIndex((UInt16)(vtxIdx + i - 1)); writeIndex((UInt16)(vtxIdx + i));
         }
      }
      public void addBezierCurve(Vector2 pos0, Vector2 cp0, Vector2 cp1, Vector2 pos1, Color4 col, float thickness, int num_segments = 0)
      {
         if (col.A == 0.0)
            return;

         pathLineTo(pos0);
         pathBezierCurveTo(cp0, cp1, pos1, num_segments);
         pathStroke(col, false, thickness);
      }
      public void addCustomRenderCommand(StatelessRenderCommand cmd)
      {
         DrawCmd dcmd = currentCommand;
         if (dcmd == null || dcmd.elementCount != 0 || dcmd.userRenderCommand != null)
         {
            dcmd = addDrawCommand();
         }

         dcmd.userRenderCommand = cmd;
         cmd.renderState.scissorTest.enabled = true;
         cmd.renderState.scissorTest.rect = dcmd.clipRect;

         // Force a new command after us (we function this way so that the most common calls AddLine, AddRect, etc. always have a command to add to without doing any check).
         addDrawCommand();
      }
      #endregion

      #region Stateful Path API
      public void pathLineTo(Vector2 pos)
      {
         path.Add(pos);
      }
      public void pathLineToMergeDuplicate(Vector2 pos)
      {
         if (path.Count == 0 ||
            path[path.Count - 1].X != pos.X ||
            path[path.Count - 1].Y != pos.Y)
         {
            path.Add(pos);
         }
      }
      public void pathFill(Color4 col)
      {
         addConvexPolyFilled(path, col);
         path.Clear();
      }
      public void pathStroke(Color4 col, bool closed, float thickness = 1.0f)
      {
         addPolyline(path, col, closed, thickness);
         path.Clear();
      }
      public void pathArcTo(Vector2 center, float radius, float a_min, float a_max, int num_segments = 10)
      {
         if (radius == 0.0f)
         {
            path.Add(center);
         }

         for (int i = 0; i <= num_segments; i++)
         {
            float a = a_min + ((float)i / (float)num_segments) * (a_max - a_min);
            path.Add(new Vector2(center.X + (float)Math.Cos(a) * radius, center.Y + (float)Math.Sin(a) * radius));
         }
      }

      static Vector2[] theCircleVtx = new Vector2[12];
      static bool theCircleBuilt = false;
      public void pathArcToFast(Vector2 center, float radius, int a_min_of_12, int a_max_of_12)               // Use precomputed angles for a 12 steps circle
      {
         if (theCircleBuilt == false)
         {
            for (int i = 0; i < 12; i++)
            {
               float a = ((float)i / (float)12) * 2 * (float)Math.PI;
               Vector2 pt = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
               theCircleVtx[i] = pt;
            }
            theCircleBuilt = true;
         }

         if (a_min_of_12 > a_max_of_12)
            return;

         if (radius == 0.0f)
         {
            path.Add(center);
         }
         else
         {
            for (int a = a_min_of_12; a <= a_max_of_12; a++)
            {
               Vector2 c = theCircleVtx[a % 12] * radius;
               path.Add(new Vector2(center.X + c.X, center.Y + c.Y));
            }
         }
      }
      public void pathBezierToCasteljau(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, float tessTol, int level)
      {
         float dx = x4 - x1;
         float dy = y4 - y1;
         float d2 = ((x2 - x4) * dy - (y2 - y4) * dx);
         float d3 = ((x3 - x4) * dy - (y3 - y4) * dx);
         d2 = (d2 >= 0) ? d2 : -d2;
         d3 = (d3 >= 0) ? d3 : -d3;
         if ((d2 + d3) * (d2 + d3) < tessTol * (dx * dx + dy * dy))
         {
            path.Add(new Vector2(x4, y4));
         }
         else if (level < 10)
         {
            float x12 = (x1 + x2) * 0.5f, y12 = (y1 + y2) * 0.5f;
            float x23 = (x2 + x3) * 0.5f, y23 = (y2 + y3) * 0.5f;
            float x34 = (x3 + x4) * 0.5f, y34 = (y3 + y4) * 0.5f;
            float x123 = (x12 + x23) * 0.5f, y123 = (y12 + y23) * 0.5f;
            float x234 = (x23 + x34) * 0.5f, y234 = (y23 + y34) * 0.5f;
            float x1234 = (x123 + x234) * 0.5f, y1234 = (y123 + y234) * 0.5f;

            pathBezierToCasteljau(x1, y1, x12, y12, x123, y123, x1234, y1234, tessTol, level + 1);
            pathBezierToCasteljau(x1234, y1234, x234, y234, x34, y34, x4, y4, tessTol, level + 1);
         }
      }
      public void pathBezierCurveTo(Vector2 p2, Vector2 p3, Vector2 p4, int num_segments = 0)
      {
         Vector2 p1 = path.Last();
         if (num_segments == 0)
         {

         }
         else
         {
            float tStep = 1.0f / (float)num_segments;
            for (int iStep = 1; iStep <= num_segments; iStep++)
            {
               float t = tStep * iStep;
               float u = 1.0f - t;
               float w1 = u * u * u;
               float w2 = 3 * u * u * t;
               float w3 = 3 * u * t * t;
               float w4 = t * t * t;
               path.Add(new Vector2(w1 * p1.X + w2 * p2.X + w3 * p3.X + w4 * p4.X, w1 * p1.Y + w2 * p2.Y + w3 * p3.Y + w4 * p4.Y));
            }
         }
      }
      public void pathRect(Vector2 a, Vector2 b, float rounding = 0.0f, Corners rounding_corners = Corners.ALL)
      {
         float r = rounding;
         r = Math.Min(r, Math.Abs(b.X - a.X) * (((rounding_corners & Corners.BOTTOM) == Corners.BOTTOM) || ((rounding_corners & Corners.TOP) == Corners.TOP) ? 0.5f : 1.0f) - 1.0f);
         r = Math.Min(r, Math.Abs(b.Y - a.Y) * (((rounding_corners & Corners.LEFT) == Corners.LEFT) || ((rounding_corners & Corners.RIGHT) == Corners.RIGHT) ? 0.5f : 1.0f) - 1.0f);

         if (r <= 0.0f || rounding_corners == 0)
         {
            pathLineTo(a);
            pathLineTo(new Vector2(b.X, a.Y));
            pathLineTo(b);
            pathLineTo(new Vector2(a.X, b.Y));
         }
         else
         {
            float r0 = rounding_corners.HasFlag(Corners.UL) ? r : 0.0f;
            float r1 = rounding_corners.HasFlag(Corners.UR) ? r : 0.0f;
            float r2 = rounding_corners.HasFlag(Corners.LR) ? r : 0.0f;
            float r3 = rounding_corners.HasFlag(Corners.LL) ? r : 0.0f;
            pathArcToFast(new Vector2(a.X + r0, a.Y + r0), r0, 6, 9);
            pathArcToFast(new Vector2(b.X - r1, a.Y + r1), r1, 9, 12);
            pathArcToFast(new Vector2(b.X - r2, b.Y - r2), r2, 0, 3);
            pathArcToFast(new Vector2(a.X + r3, b.Y - r3), r3, 3, 6);
         }
      }
      #endregion

      #region internal helpers
      void primativeRect(Vector2 a, Vector2 c, Color4 col)
      {
         Vector2 uv = new Vector2(0, 0);
         Vector2 b = new Vector2(c.X, a.Y);
         Vector2 d = new Vector2(a.X, c.Y);

         indexTwoNewTriangles();

         UInt32 cl = col.toUInt();
         myVerts[myVertCount].Position = a; myVerts[myVertCount].TexCoord = uv; myVerts[myVertCount].Color = cl; myVertCount++;
         myVerts[myVertCount].Position = b; myVerts[myVertCount].TexCoord = uv; myVerts[myVertCount].Color = cl; myVertCount++;
         myVerts[myVertCount].Position = c; myVerts[myVertCount].TexCoord = uv; myVerts[myVertCount].Color = cl; myVertCount++;
         myVerts[myVertCount].Position = d; myVerts[myVertCount].TexCoord = uv; myVerts[myVertCount].Color = cl; myVertCount++;
      }
      void primativeRectUv(Vector2 a, Vector2 c, Vector2 uva, Vector2 uvc, Color4 col)
      {
         Vector2 b = new Vector2(c.X, a.Y);
         Vector2 d = new Vector2(a.X, c.Y);
         Vector2 uvb = new Vector2(uvc.X, uva.Y);
         Vector2 uvd = new Vector2(uva.X, uvc.Y);

         indexTwoNewTriangles();

         UInt32 cl = col.toUInt();
         myVerts[myVertCount].Position = a; myVerts[myVertCount].TexCoord = uva; myVerts[myVertCount].Color = cl; myVertCount++;
         myVerts[myVertCount].Position = b; myVerts[myVertCount].TexCoord = uvb; myVerts[myVertCount].Color = cl; myVertCount++;
         myVerts[myVertCount].Position = c; myVerts[myVertCount].TexCoord = uvc; myVerts[myVertCount].Color = cl; myVertCount++;
         myVerts[myVertCount].Position = d; myVerts[myVertCount].TexCoord = uvd; myVerts[myVertCount].Color = cl; myVertCount++;
      }
      void primativeText(float size, Vector2 pos, Color4 col, String text)
      {
         float x = (float)(int)pos.X;
         float y = (float)(int)pos.Y;

         //the built in font is 32 pixels high
         float scale = size / 32.0f;
         float lineHeight = 32.0f * scale;

         foreach (Char c in text)
         {
            if (c < 32)
            {
               if (c == '\n')
               {
                  x = pos.X;
                  y -= lineHeight;
                  continue;
               }

               if (c == '\r')
               {
                  continue;
               }
            }

            float charWidth = 0.0f;
            Glyph g = theGlyphs[c];
            if (g != null)
            {
               charWidth = g.size.X * scale;

               if ((c != ' ') && (c != '\t'))
               {
                  float x1 = x + g.offset.X * scale;
                  float y1 = y + g.offset.Y * scale;
                  float x2 = x + g.size.Y * scale;
                  float y2 = y + g.size.Y * scale;

                  //these seem backwards but only because we're doing some crazy inversion thing with UI screen space to window space
                  Vector2 uv1 = new Vector2(g.minTexCoord.X, g.maxTexCoord.Y);
                  Vector2 uv2 = new Vector2(g.maxTexCoord.X, g.minTexCoord.Y);

                  primativeRectUv(new Vector2(x1, y1), new Vector2(x2, y2), uv1, uv2, col);

                  //keep the letters a little closer together than needed
                  x += g.advance.X * scale;
               }
               else
               {
                  if (c == ' ') x += g.advance.X * scale;
                  if (c == '\t') x += g.advance.X * scale * 3;
               }
            }
         }
      }
      void primativeVert(Vector2 pos, Vector2 uv, Color4 col)
      {
         writeIndex((UInt16)myVertCount);
         myVertCount++;
         writeVertex(pos, uv, col);
      }
      void writeVertex(Vector2 a, Vector2 uv, Color4 col)
      {
         myVerts[myVertCount].Position = a;
         myVerts[myVertCount].TexCoord = uv;
         myVerts[myVertCount].Color = col.toUInt();
         myVertCount++;
      }
      void writeIndex(UInt16 idx)
      {
         myIndexes[myIndexCount] = idx;
         myIndexCount++;

         currentCommand.elementCount++;
      }
      void indexTwoNewTriangles()
      {
         UInt16 startVertex = (UInt16)myVertCount;
         writeIndex(startVertex); writeIndex((UInt16)(startVertex + 1)); writeIndex((UInt16)(startVertex + 2));
         writeIndex(startVertex); writeIndex((UInt16)(startVertex + 2)); writeIndex((UInt16)(startVertex + 3));
      }

      DrawCmd addDrawCommand()
      {
         DrawCmd cmd = new DrawCmd();
         cmd.layer = currentLayer;
         cmd.clipRect = currentClipRect;
         cmd.texture = currentTexture;
         cmd.elementOffset = myIndexCount;
         cmd.elementCount = 0;
         cmd.index = myCmdBuffer.Count;
         myCmdBuffer.Add(cmd);

         return cmd;
      }

      public void pushDrawLayer(int newLayer)
      {
         layerStack.Add(newLayer);
         updateLayer();
      }

      public void popDrawLayer()
      {
         if (layerStack.Count > 0)
         {
            layerStack.RemoveAt(layerStack.Count - 1);
            updateLayer();
         }
      }

      public void pushClipRect(Rect r)
      {
         Vector4 c;
         c.X = r.left * myScale;
         c.Y = (myScreenSize.Y - (r.top * myScale)); //convert to pixel coords from UI Screen coords
         c.Z = r.width * myScale;
         c.W = r.height * myScale;
         pushClipRect(c);
      }
      public void pushClipRect(Vector4 r)
      {
         clipRectStack.Add(r);
         updateClipRect();
      }

      public void pushClipRectFullscreen()
      {
         pushClipRect(theNullClipRect);
      }

      public void popClipRect()
      {
         if (clipRectStack.Count > 0)
         {
            clipRectStack.RemoveAt(clipRectStack.Count - 1);
            updateClipRect();
         }
      }

      public bool shouldPushTexture(Texture tex)
      {
         return textureStack.Count == 0 || tex != currentTexture;
      }

      public void pushTexture(Texture t)
      {
         textureStack.Add(t);
         updateTexture();
      }

      public void popTexture()
      {
         if (textureStack.Count > 0)
         {
            textureStack.RemoveAt(textureStack.Count - 1);
            updateTexture();
         }
      }

      void updateClipRect()
      {
         DrawCmd cmd = currentCommand;
         if (cmd == null || cmd.elementCount != 0 || cmd.userRenderCommand != null)
         {
            addDrawCommand();
         }
         else
         {
            Vector4 clipRect = currentClipRect;
            if (myCmdBuffer.Count >= 2 && (myCmdBuffer[myCmdBuffer.Count - 2].clipRect - clipRect).LengthSquared < 0.0001f)
               myCmdBuffer.RemoveAt(myCmdBuffer.Count - 1);
            else
               cmd.clipRect = clipRect;
         }
      }

      void updateTexture()
      {
         DrawCmd cmd = currentCommand;
         Texture t = currentTexture;
         if (cmd != null || (cmd.elementCount != 0 && cmd.texture != t) || cmd.userRenderCommand != null)
         {
            addDrawCommand();
         }
         else
         {
            cmd.texture = t;
         }
      }

      void updateLayer()
      {
         DrawCmd cmd = currentCommand;
         int layer = currentLayer;
         if (cmd != null || (cmd.elementCount != 0 && cmd.layer != layer) || cmd.userRenderCommand != null)
         {
            addDrawCommand();
         }
         else
         {
            cmd.layer = layer;
         }
      }

      #endregion
   }
}
