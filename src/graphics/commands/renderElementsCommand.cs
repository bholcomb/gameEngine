using System;
using System.Collections.Generic;

using Graphics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace Graphics
{
   public class StatelessDrawElementsCommand : StatelessRenderCommand
   {
      public PrimitiveType myDrawMode;
      public int myBase;
      public int myCount;
      public IndexBufferObject.IndexBufferDatatype myIboType = IndexBufferObject.IndexBufferDatatype.UnsignedShort;

      public StatelessDrawElementsCommand(Mesh m)
			: base()
		{
			myDrawMode = m.primativeType;
			myBase = m.indexBase;
			myCount = m.indexCount;
		}

		public StatelessDrawElementsCommand(PrimitiveType drawMode, int count, int offset=0, IndexBufferObject.IndexBufferDatatype iboType = IndexBufferObject.IndexBufferDatatype.UnsignedShort)
         : base()
      {
         myDrawMode = drawMode;
         myBase = offset;
         myCount = count;
         myIboType = iboType;
      }

      public override void execute()
      {
			//setup the state
			base.execute();

         //draw the mesh
         if(myIboType == IndexBufferObject.IndexBufferDatatype.UnsignedShort)
            GL.DrawElements(myDrawMode, myCount, DrawElementsType.UnsignedShort, myBase * 2);

         if (myIboType == IndexBufferObject.IndexBufferDatatype.UnsignedInt)
            GL.DrawElements(myDrawMode, myCount, DrawElementsType.UnsignedInt, myBase * 4);
      }
   }
}

