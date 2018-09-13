using System;
using System.IO;
using System.Collections.Generic;

using Util;
using Graphics;
using Lua;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   namespace Bob
   {
      public class Material
      {
         public enum TextureFormat { PNG, JPG, BMP, DDS, RAW };

         public String name { get; set; }
         public Color4 ambient { get; set; }
         public Color4 diffuse { get; set; }
         public Color4 spec { get; set; }
         public Color4 emission { get; set; }
         public float shininess { get; set; }
         public float alpha { get; set; }
         public String diffuseTexture { get; set; }
         public String specularTexture { get; set; }
         public String emissionTexture { get; set; }
         public String alphaTexture { get; set; }
         public String normalTexture { get; set; }

         public Material()
         {
            diffuseTexture = null;
            specularTexture = null;
            emissionTexture = null;
            alphaTexture = null;
            normalTexture = null;
         }
      }

      public class Mesh
      {
         public string name { get; set; }
         public uint indexCount { get; set; }
         public uint indexOffset { get; set; }
         public string material { get; set; }

         public Mesh()
         {
         }
      }

      public class ModelChunk : Chunk
      {
         public enum VertexFormat { V3N3T2, V3N3T2B4W4 };
         public enum IndexFormat { USHORT, UINT };

         [Flags]
         enum ModelFlags
         {
            Animated = 0x01
         };

         public List<Mesh> myMeshes = new List<Mesh>();
         public List<Material> myMaterials = new List<Material>();

         public PrimitiveType primativeType { get; set; }
         public VertexFormat vertexFormat { get; set; }
         public IndexFormat indexType { get; set; }

         public UInt32 vertexCount { get; set; }
         public List<Vector3> verts { get; set; }
         public List<Vector3> normals { get; set; }
         public List<Vector2> uvs { get; set; }
         public List<Vector4> boneIdx { get; set; }
         public List<Vector4> boneWeights { get; set; }

         public UInt32 indexCount { get; set; }
         public List<UInt16> indexShort { get; set; }
         public List<UInt32> indexInt { get; set; }

         public ModelChunk()
            : base()
         {
            myType = Bob.ChunkType.MODEL;
         }

         public bool animated { get { return ((ModelFlags)myFlags & ModelFlags.Animated) != 0; } }
      }
   }
}