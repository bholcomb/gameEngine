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
   public class AssimpAnimatedModelDescriptor : ResourceDescriptor
   {
      public AssimpAnimatedModelDescriptor(string name)
         : base(name)
      {
         type = "AssimpModel";
      }

      public override IResource create(ResourceManager mgr)
      {
         AssimpAnimatedModelLoader loader = new AssimpAnimatedModelLoader(mgr);
         SkinnedModel m = loader.loadFromFile(name);
         return m;
      }
   }

   public class AssimpAnimatedModelLoader : AssimpLoader
   {
      List<V3N3T2B4W4> myVerts = new List<V3N3T2B4W4>();
      List<ushort> index = new List<ushort>();
      int currIndexOffset = 0;
      int currVertOffset = 0;

      List<string> boneNames = new List<string>();
      List<Matrix4> bones = new List<Matrix4>();

      SkinnedModel myModel = new SkinnedModel();

      public AssimpAnimatedModelLoader(ResourceManager mgr) : base(mgr)
      {
      }

      public SkinnedModel loadFromFile(string filename)
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
            loadBoneData();
            loadAnimationData();
         }
         catch
         {
            Warn.print("Failed to load model {0}", filename);
            return null;
         }

         myModel.myBindings = V3N3T2B4W4.bindings();
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
                  V3N3T2B4W4 v = new V3N3T2B4W4();
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

               //get the bone data
               if(amesh.HasBones)
               {
                  getVertexBoneData(amesh);
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

      void getVertexBoneData(Assimp.Mesh mesh)
      {
         int weightsPerVertex = 4;

         //we should set this once
         if (myModel.boneCount == 0)
         {
            myModel.boneCount = mesh.BoneCount;
            foreach (Bone b in mesh.Bones)
            {
               if (boneNames.Contains(b.Name) == false)
               {
                  boneNames.Add(b.Name);
                  bones.Add(toMatrix(b.OffsetMatrix));
               }
            }
         }
         else
         {
            if(myModel.boneCount != mesh.BoneCount)
            {
               Warn.print("Weird model.  Meshes have different bones");
               throw new Exception("Weird Model");
            }
         }

         for(int i = 0; i < mesh.BoneCount; i++)
         {
            Bone b = mesh.Bones[i];

            if (b.HasVertexWeights == false)
               continue;

            foreach(VertexWeight weight in b.VertexWeights)
            {
               V3N3T2B4W4 vert = myVerts[weight.VertexID + currVertOffset];

               //find the next available weight (if there is space)
               for(int k = 0; k < weightsPerVertex; k++)
               {
                  if(vert.BoneWeight[k] == 0.0f)
                  {
                     vert.BoneId[k] = (float)i;
                     vert.BoneWeight[k] = weight.Weight;
                     break;
                  }
               }

               myVerts[weight.VertexID + currVertOffset] = vert;
            }
         }
      }

      void loadBoneData()
      {
         
      }

      void loadAnimationData()
      {
         if (myScene.HasAnimations == false)
            return;

//          //count how many total frames of animation there are
//          int keyframeCount = 0;
//          foreach (Assimp.Animation a in myScene.Animations)
//          {
//             keyframeCount += a.DurationInTicks;
//          }
// 
//          //create matrix buffer to hold all the bone's keyframes
//          Matrix4[] jointKeyframes = new Matrix4[myModel.boneCount * keyframeCount];
//          
//          foreach(Assimp.Animation a in myScene.Animations)
//          {
//             //get the bone data
//             foreach(NodeAnimationChannel data in a.NodeAnimationChannels)
//             {
//                string boneName = data.NodeName;
//                data.
//             }
// 
//             Graphics.Animation ani = new Graphics.Animation(a.Name, 0, 0, (float)a.TicksPerSecond, true);
//             myModel.animations[a.Name] = ani;
//          }
      }
      
      #region helper functions
      public Vector3 findMin()
      {
         Vector3 min = new Vector3(float.MaxValue);
         foreach (V3N3T2B4W4 vert in myVerts)
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
         foreach (V3N3T2B4W4 vert in myVerts)
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