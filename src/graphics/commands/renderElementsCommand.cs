using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace Graphics
{
   public class StatelessDrawElementsCommand : StatelessRenderCommand
   {
      PrimitiveType myDrawMode;
      int myBase;
      int myCount;

		public StatelessDrawElementsCommand(Mesh m)
			: base()
		{
			myDrawMode = m.primativeType;
			myBase = m.indexBase;
			myCount = m.indexCount;
		}

		public StatelessDrawElementsCommand(PrimitiveType drawMode, int count, int offset=0)
         : base()
      {
         myDrawMode = drawMode;
         myBase = offset;
         myCount = count;
      }

      public override void execute()
      {
			//setup the state
			base.execute();

         //draw the mesh
         GL.DrawElements(myDrawMode, myCount, DrawElementsType.UnsignedShort, myBase * 2);
      }
   }
}

