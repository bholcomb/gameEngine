using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   [StructLayout(LayoutKind.Sequential)]
   public struct V3
   {
      static int theStride = Marshal.SizeOf(default(V3)); 

      public Vector3 Position;

      public static int stride { get { return theStride; } }

      static Dictionary<string, BufferBinding> theBindings = null;
      public static Dictionary<string, BufferBinding> bindings()
      {
         if (theBindings == null)
         {
            theBindings = new Dictionary<string, BufferBinding>();
            theBindings["position"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 3, offset = 0 };
         }

         return theBindings;
      }
   }

   [StructLayout(LayoutKind.Sequential)]
   public struct V3T2
   {
      static int theStride = Marshal.SizeOf(default(V3T2)); 

      public Vector3 Position;
      public Vector2 TexCoord;

      public static int stride { get { return theStride; } }

      static Dictionary<string, BufferBinding> theBindings = null;
      public static Dictionary<string, BufferBinding> bindings()
      {
         if (theBindings == null)
         {
            theBindings = new Dictionary<string, BufferBinding>();
            theBindings["position"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 3, offset = 0 };
            theBindings["uv"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 2, offset = 12 };
         }

         return theBindings;
      }
   }

   [StructLayout(LayoutKind.Sequential)]
   public struct V3N3T2
   {
      static int theStride = Marshal.SizeOf(default(V3N3T2)); 

      public Vector3 Position;
      public Vector3 Normal;
      public Vector2 TexCoord;

      public static int stride { get { return theStride; } }

      static Dictionary<string, BufferBinding> theBindings = null;
      public static Dictionary<string, BufferBinding> bindings()
      {
         if (theBindings == null)
         {
            theBindings = new Dictionary<string, BufferBinding>();
            theBindings["position"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 3, offset = 0 };
            theBindings["normal"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 3, offset = 12 };
            theBindings["uv"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 2, offset = 24 };
         }

         return theBindings;
      }
   }

   //used for MD2 where the vertex data is stored on the card already
   [StructLayout(LayoutKind.Sequential)]
   public struct T2
   {
      static int theStride = Marshal.SizeOf(default(T2));

      public Vector2 TexCoord;

      public static int stride { get { return theStride; } }

      static Dictionary<string, BufferBinding> theBindings = null;
      public static Dictionary<string, BufferBinding> bindings()
      {
         if (theBindings == null)
         {
            theBindings = new Dictionary<string, BufferBinding>();
            theBindings["uv"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 2, offset = 0 };
         }

         return theBindings;
      }
   }

   //used for MS3D format
   [StructLayout(LayoutKind.Sequential)]
   public struct V3N3T2B4W4
   {
      static int theStride= Marshal.SizeOf(default(V3N3T2B4W4)); 

      public Vector3 Position;
      public Vector3 Normal;
      public Vector2 TexCoord;
      public Vector4 BoneId;
      public Vector4 BoneWeight;

      public static int stride { get { return theStride; } }

      static Dictionary<string, BufferBinding> theBindings = null;
      public static Dictionary<string, BufferBinding> bindings()
      {
         if (theBindings == null)
         {
            theBindings = new Dictionary<string, BufferBinding>();
            theBindings["position"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 3, offset = 0 };
            theBindings["normal"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = true, numElements = 3, offset = 12 };
            theBindings["uv"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 2, offset = 24 };
            theBindings["boneId"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 4, offset = 32 };
            theBindings["boneWeight"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 4, offset = 48 };
         }

         return theBindings;
      }
   }

   //this is used for particle systems
   [StructLayout(LayoutKind.Sequential)]
   public struct V3C4S3R
   {
      static int theStride = Marshal.SizeOf(typeof(V3C4S3R)); 

      public Vector3 Position;
      public Vector4 Color;
      public Vector3 Size;
      public float Rotation;

      public static int stride { get { return theStride; } }

      public static void bindVertexAttribute(String fieldName, int id)
      {
         switch (fieldName)
         {
            case "position":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 0);
					break;
            case "color":
					GL.VertexAttribFormat(id, 4, VertexAttribType.Float, false, 12);
               break;
            case "size":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 28);
               break;
            case "rotation":
					GL.VertexAttribFormat(id, 1, VertexAttribType.Float, false, 40);
               break;
            default:
               throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
         }
      }

      static Dictionary<string, BufferBinding> theBindings = null;
      public static Dictionary<string, BufferBinding> bindings()
      {
         if (theBindings == null)
         {
            theBindings = new Dictionary<string, BufferBinding>();
            theBindings["position"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 3, offset = 0 };
            theBindings["color"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = true, numElements = 4, offset = 12 };
            theBindings["size"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 3, offset = 28 };
            theBindings["rotation"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 1, offset = 40 };           
         }

         return theBindings;
      }
   }
}