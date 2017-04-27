using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class Mesh
   {
		public PrimitiveType primativeType;
      public int indexBase;
      public int indexCount;
		public Material material;
   }

   public class StaticModel : IResource
   {
      public Matrix4 myInitialTransform = Matrix4.Identity;
      public VertexBufferObject<V3N3T2> myVbo;
      public IndexBufferObject myIbo;
      public List<Mesh> myMeshes;

      public float size;

      public StaticModel()
      {
         myVbo = new VertexBufferObject<V3N3T2>(BufferUsageHint.StaticDraw);
         myIbo = new IndexBufferObject(BufferUsageHint.StaticDraw);
         myMeshes = new List<Mesh>();
      }

      public void Dispose()
      {
         myVbo.Dispose();
         myIbo.Dispose();
      }
   }
}