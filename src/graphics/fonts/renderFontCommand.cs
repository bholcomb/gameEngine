using System;

using OpenTK;
using OpenTK.Graphics;

namespace Graphics
{
   public class RenderFontCommand : StatelessRenderCommand
   {
      Font myFont;
      String myString;
      Vector3 myPosition;
      Camera myCamera;
      bool myIs3d = false;

      public RenderFontCommand(Font f, Vector3 position, String s, Color4 color)
         : base()
      {
         myFont = f;
         myString = s;
         myPosition = position;
			renderState.setUniform(new UniformData(21, Uniform.UniformType.Color4, color));
			pipelineState.blending.enabled = true;
			pipelineState.depthTest.enabled = false;

			myFont.setupRenderCommand(this);


         Matrix4 model = Matrix4.CreateTranslation(myPosition);

         //add the model view projection matrix
         renderState.setUniform(new UniformData(0, Uniform.UniformType.Mat4, model));
         renderState.setUniform(new UniformData(1, Uniform.UniformType.Bool, true));
      }

      public RenderFontCommand(Font f, Vector2 pos, String s, Color4 color)
         :base()
      {
         myFont = f;
         myString = s;
         myPosition = new Vector3(pos.X, pos.Y, 0.0f);
         renderState.setUniform(new UniformData(21, Uniform.UniformType.Color4, color));
         pipelineState.blending.enabled = true;
         pipelineState.depthTest.enabled = false;

         myFont.setupRenderCommand(this);

         Matrix4 model = Matrix4.CreateTranslation(myPosition);

         //add the model view projection matrix
         renderState.setUniform(new UniformData(0, Uniform.UniformType.Mat4, model));
         renderState.setUniform(new UniformData(1, Uniform.UniformType.Bool, false));
      }

      public RenderFontCommand(Font f, float x, float y, String s, Color4 color)
         : base()
      {
         myFont = f;
         myString = s;
         myPosition = new Vector3(x,y,0.0f);
			renderState.setUniform(new UniformData(21, Uniform.UniformType.Color4, color));
			pipelineState.blending.enabled = true;
			pipelineState.depthTest.enabled = false;

			myFont.setupRenderCommand(this);

			Matrix4 model = Matrix4.CreateTranslation(myPosition);

			//add the model view projection matrix
			renderState.setUniform(new UniformData(0, Uniform.UniformType.Mat4, model));
			renderState.setUniform(new UniformData(1, Uniform.UniformType.Bool, false));
		}

      public override void execute()
      {
         myFont.updateText(myString);
			base.execute();
         myFont.drawText();
      }
   }
}