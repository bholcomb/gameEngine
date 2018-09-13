using System;
using System.Reflection;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public enum BindingDataType
   {
      Float,
      Integer,
      Double,
   };

   public class BufferBinding
   {
      public int bufferIndex = 0;
      public int offset = 0; 
      public int numElements = 0;
      public BindingDataType dataType = BindingDataType.Float;
      public int dataFormat = 0;
      public bool normalize = false;

      public BufferBinding()
      {

      }
   };

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

      public void bindVertexFormat(ShaderProgram sp, Dictionary<string, BufferBinding> bindings)
      {
         //make active
         bind();

         foreach (AttributeInfo attr in sp.vertexBindings.Values)
         {
            if (attr.id == -1)
               continue;

            if (attr.name.StartsWith("gl_"))
               continue;

            BufferBinding binding = null;
            if(bindings.TryGetValue(attr.name, out binding) == false)
            {
               throw new Exception(String.Format("Failed to find bind point {0}", attr.name));
            }

            switch(binding.dataType)
            {
               case BindingDataType.Float:
                  GL.VertexAttribFormat(attr.id, binding.numElements, (VertexAttribType)binding.dataFormat, binding.normalize, binding.offset);
                  break;
               case BindingDataType.Integer:
                  GL.VertexAttribIFormat(attr.id, binding.numElements, (VertexAttribIntegerType)binding.dataFormat, binding.offset);
                  break;
               case BindingDataType.Double:
                  GL.VertexAttribLFormat(attr.id, binding.numElements, (VertexAttribDoubleType)binding.dataFormat, binding.offset);

                  break;
            }

            GL.VertexAttribBinding(attr.id, binding.bufferIndex);
            GL.EnableVertexAttribArray(attr.id);
         }

         //cleanup
         unbind();
      }
   }
}
