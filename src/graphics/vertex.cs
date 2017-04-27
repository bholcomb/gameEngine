using System;
using System.Runtime.InteropServices;

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

      public static void bindVertexAttribute(String fieldName, int id)
      {
         switch (fieldName)
         {
            case "position":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 0);
               break;
            default:
               throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
         }
      }
   }

   [StructLayout(LayoutKind.Sequential)]
   public struct V3T2
   {
      static int theStride = Marshal.SizeOf(default(V3T2)); 

      public Vector3 Position;
      public Vector2 TexCoord;

      public static int stride { get { return theStride; } }

      public static void bindVertexAttribute(String fieldName, int id)
      {
         switch (fieldName)
         {
            case "position":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 0);
               break;
            case "uv":
					GL.VertexAttribFormat(id, 2, VertexAttribType.Float, false, 12);
               break;
            default:
               throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
         }
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

      public static void bindVertexAttribute(String fieldName, int id)
      {
         switch (fieldName)
         {
            case "position":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 0);
					break;
            case "normal":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 12);
					break;
            case "uv":
					GL.VertexAttribFormat(id, 2, VertexAttribType.Float, false, 24);
               break;
            default:
               throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
         }
      }
   }

   //used for MD2 where the vertex data is stored on the card already
   [StructLayout(LayoutKind.Sequential)]
   public struct T2
   {
      static int theStride = Marshal.SizeOf(default(T2));

      public Vector2 TexCoord;

      public static int stride { get { return theStride; } }

      public static void bindVertexAttribute(String fieldName, int id)
      {
         switch (fieldName)
         {
            case "uv":
					GL.VertexAttribFormat(id, 2, VertexAttribType.Float, false, 0);
               break;
            default:
               throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
         }
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

      public static void bindVertexAttribute(String fieldName, int id)
      {
         switch (fieldName)
         {
            case "position":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 0);
               break;
            case "normal":
					GL.VertexAttribFormat(id, 3, VertexAttribType.Float, false, 12);
               break;
            case "uv":
					GL.VertexAttribFormat(id, 2, VertexAttribType.Float, false, 24);
               break;
            case "boneId":
					GL.VertexAttribFormat(id, 4, VertexAttribType.Float, false, 32);
               break;
            case "boneWeight":
					GL.VertexAttribFormat(id, 4, VertexAttribType.Float, false, 48);
               break;
            default:
               throw new Exception(String.Format("Unknown attribute field: {0}", fieldName));
         }
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
   }
}