using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Assimp;
using Assimp.Configs;

using Util;

namespace Graphics
{
   public class AssimpModelDescriptor : ResourceDescriptor
   {
      public AssimpModelDescriptor(string name)
         : base(name)
      {
         type = "AssimpModel";
      }

      public override IResource create(ResourceManager mgr)
      {
         AssimpModelLoader loader = new AssimpModelLoader(mgr);
         Model m = loader.loadFromFile(name);
         return m;
      }
   }

   public class AssimpModelLoader : AssimpLoader
   {
      List<V3N3T2> myVerts = new List<V3N3T2>();
      List<ushort> index = new List<ushort>();
      int currIndexOffset = 0;
      int currVertOffset = 0;
      Model myModel = new Model();

      public AssimpModelLoader(ResourceManager mgr) :base(mgr)
      {
      }

      public Model loadFromFile(string filename)
      {
         Info.print("Loading Assimp model {0}", filename);

         if (File.Exists(filename) == false)
         {
            Warn.print("Cannot find file {0}", filename);
            return null;
         }

         myRootPath = Path.GetDirectoryName(filename);
         try
         {
            myScene = theAssimpContext.ImportFile(filename, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals);
            Node rootNode = myScene.RootNode;

            //load static meshes
            createMeshes(rootNode);
         }
         catch
         {
            Warn.print("Failed to load model {0}", filename);
            return null;
         }

         myModel.myBindings = V3N3T2.bindings();
         VertexBufferObject vbo = new VertexBufferObject(BufferUsageHint.StaticDraw);
         vbo.setData(myVerts);
         myModel.myVbos.Add(vbo);
         myModel.myIbo.setData(index);

         //should probably build a bounding box
         myModel.size = (findMax() - findMin()).Length / 2.0f;

         return myModel;
      }

      void createMeshes(Node node)
      {
         if (node.HasMeshes)
         {
            foreach (int meshIndex in node.MeshIndices)
            {
               Assimp.Mesh amesh = myScene.Meshes[meshIndex];

               //create the material
               Graphics.Material mat = createMaterial(myScene.Materials[amesh.MaterialIndex]);

               //create the geometry
               Graphics.Mesh mesh = new Graphics.Mesh();
               mesh.primativeType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
               mesh.indexBase = currIndexOffset;
               mesh.indexCount = amesh.FaceCount * 3;
               mesh.material = mat;

               //get the indices
               foreach (Face face in amesh.Faces)
               {
                  for (int i = 0; i < 3; i++)
                  {
                     index.Add((UInt16)(face.Indices[i] + currVertOffset));
                     currIndexOffset++;
                  }
               }

               //get the verts
               for (int i = 0; i < amesh.VertexCount; i++)
               {
                  V3N3T2 v = new V3N3T2();
                  v.Position = toVector(amesh.Vertices[i]);

                  if (amesh.HasNormals == true)
                  {
                     v.Normal = toVector(amesh.Normals[i]);
                  }

                  if (amesh.HasTextureCoords(0) == true)
                  {
                     v.TexCoord = toVector2D(amesh.TextureCoordinateChannels[0][i]);
                  }

                  myVerts.Add(v);
               }

               currVertOffset += amesh.VertexCount;

               myModel.myMeshes.Add(mesh);
            }
         }

         if (node.HasChildren)
         {
            foreach(Node child in node.Children)
            {
               createMeshes(child);
            }
         }
      }

      #region helper functions
      public Vector3 findMin()
      {
         Vector3 min = new Vector3(float.MaxValue);
         foreach (V3N3T2 vert in myVerts)
         {
            Vector3 v = vert.Position;
            if (v.X < min.X) min.X = v.X;
            if (v.Y < min.Y) min.Y = v.Y;
            if (v.Z < min.Z) min.Z = v.Z;
         }

         return min;
      }

      public Vector3 findMax()
      {
         Vector3 max = new Vector3(float.MinValue);
         foreach (V3N3T2 vert in myVerts)
         {
            Vector3 v = vert.Position;
            if (v.X > max.X) max.X = v.X;
            if (v.Y > max.Y) max.Y = v.Y;
            if (v.Z > max.Z) max.Z = v.Z;
         }

         return max;
      }

      #endregion
   }
}