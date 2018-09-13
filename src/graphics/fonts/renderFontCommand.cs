using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public class RenderFontCommand : StatelessRenderCommand
   {
      Font myFont;
      String myString;
      Vector3 myPosition;
      VertexBufferObject myVbo = new VertexBufferObject(BufferUsageHint.DynamicDraw);
      IndexBufferObject myIbo = new IndexBufferObject(BufferUsageHint.DynamicDraw);

      public RenderFontCommand(Font f, Vector3 position, String s, Color4 color, bool is3d = true)
         : base()
      {
         myFont = f;
         myString = s;
         myPosition = position;
			renderState.setUniform(new UniformData(21, Uniform.UniformType.Color4, color));
			pipelineState.blending.enabled = true;
			pipelineState.depthTest.enabled = false;

         myFont.setupRenderCommand(this);
         myFont.updateText(myString, myVbo, myIbo);
         renderState.setVertexBuffer(myVbo.id, 0, 0, V3T2.stride);
         renderState.setIndexBuffer(myIbo.id);

         Matrix4 model = Matrix4.CreateTranslation(myPosition);

         //add the model view projection matrix
         renderState.setUniform(new UniformData(0, Uniform.UniformType.Mat4, model));
         renderState.setUniform(new UniformData(1, Uniform.UniformType.Bool, is3d));
      }

      public RenderFontCommand(Font f, Vector2 pos, String s, Color4 color) : this(f, new Vector3(pos.X, pos.Y, 0.0f), s, color, false) { }


      public RenderFontCommand(Font f, float x, float y, String s, Color4 color) : this(f, new Vector3(x, y, 0.0f), s, color, false) { }

      public override void execute()
      {
			base.execute();
         Renderer.device.bindVertexBuffer(myVbo.id, 0, 0, V3T2.stride);
         Renderer.device.bindIndexBuffer(myIbo.id);
         Renderer.device.drawIndexed(PrimitiveType.Triangles, myIbo.count, 0, DrawElementsType.UnsignedShort);

         myVbo.Dispose();
         myIbo.Dispose();
      }
   }
}
