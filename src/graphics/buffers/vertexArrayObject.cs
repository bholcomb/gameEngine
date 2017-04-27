using System;
using System.Reflection;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class VertexArrayObject : IDisposable
   {
      protected Int32 myId;

      public VertexArrayObject()
      {
         GL.GenVertexArrays(1, out myId);
      }

      public Int32 id { get { return myId; } }

      public void Dispose()
      {
         GL.DeleteVertexArrays(1, ref myId);
      }

      public void bind()
      {
         GL.BindVertexArray(myId);
      }

      public void unbind()
      {
         GL.BindVertexArray(0);
      }
     
		public void bindVertexFormat<T>(ShaderProgram sp)
		{
			//find the method in the vertex format
			MethodInfo attributeBindMethod = typeof(T).GetMethod("bindVertexAttribute");

			//make active
			bind();

			//get all the active attributes in this shader and bind with the vertex format
			foreach (AttributeInfo attr in sp.vertexBindings.Values)
			{
				if (attr.id == -1)
					continue;

				if (attr.name.StartsWith("gl_"))
					continue;

				//look up and call the bind functions
				attributeBindMethod.Invoke(null, new Object[] { attr.name, attr.id });
				GL.VertexAttribBinding(attr.id, 0); //always bind to 0. Assume interleaved vertex formats for now
				GL.EnableVertexAttribArray(attr.id);
			}

			//cleanup
			unbind();
		}
   }
}
