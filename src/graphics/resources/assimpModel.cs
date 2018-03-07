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
         StaticModel m = loader.loadFromFile(name);
         return m;
      }
   }

   public class AssimpModelLoader
   {
      static AssimpContext theAssimpContext;

      ResourceManager myResourceManager;
      string myRootPath;
      Scene myScene;
      List<V3N3T2> myVerts = new List<V3N3T2>();
      List<ushort> index = new List<ushort>();
      int currIndexOffset = 0;
      int currVertOffset = 0;
      StaticModel myModel = new StaticModel();

      static AssimpModelLoader()
      {
         theAssimpContext = new AssimpContext();
         theAssimpContext.SetConfig(new NormalSmoothingAngleConfig(66.0f));
      }
      public AssimpModelLoader(ResourceManager mgr)
      {
         myResourceManager = mgr;
      }

      public StaticModel loadFromFile(string filename)
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

         myModel.myVbo.setData(myVerts);
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

      Graphics.Material createMaterial(Assimp.Material am)
      {
         Graphics.Material mat = new Material(am.Name);
         mat.myFeatures |= Material.Feature.Lighting;

         if (am.HasColorAmbient)
         {
            mat.ambient = toColor(am.ColorAmbient);
         }

         if (am.HasColorDiffuse)
         {
            mat.diffuse = toColor(am.ColorDiffuse);
         }

         if (am.HasColorSpecular)
         {
            mat.spec = toColor(am.ColorSpecular);
         }

         if (am.HasShininess)
         {
            mat.shininess = am.Shininess;
         }

         if(am.HasOpacity)
         {
            mat.alpha = am.Opacity;
         }

         if(am.HasTextureAmbient)
         {
            string textureName = am.TextureAmbient.FilePath;
            textureName = textureName.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            Texture t = myResourceManager.getResource(td) as Texture;
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("ambientMap", t));
            }
         }

         if (am.HasTextureDiffuse)
         {
            string textureName = am.TextureDiffuse.FilePath;
            textureName = textureName.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            Texture t = myResourceManager.getResource(td) as Texture;
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("diffuseMap", t));
               mat.myFeatures |= Material.Feature.DiffuseMap;
               mat.hasTransparency = t.hasAlpha;
            }
         }

         if (am.HasTextureSpecular)
         {
            string textureName = am.TextureSpecular.FilePath;
            textureName = textureName.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            Texture t = myResourceManager.getResource(td) as Texture;
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("specularMap", t));
               mat.myFeatures |= Material.Feature.SpecMap;
            }
         }

         if (am.HasTextureNormal)
         {
            string textureName = am.TextureNormal.FilePath;
            textureName = textureName.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            Texture t = myResourceManager.getResource(td) as Texture;
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("normalMap", t));
               mat.myFeatures |= Material.Feature.NormalMap;
            }
         }

         //Assimp should really be fixed instead of this workaround.  Same bug in both Blender and OBJ files
         if (am.HasTextureHeight)
         {
            string textureName = am.TextureHeight.FilePath;
            textureName = textureName.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            Texture t = myResourceManager.getResource(td) as Texture;
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("normalMap", t));
               mat.myFeatures |= Material.Feature.NormalMap;
            }
         }

         if (am.HasTextureDisplacement)
         {
            string textureName = am.TextureDisplacement.FilePath;
            textureName = textureName.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            Texture t = myResourceManager.getResource(td) as Texture;
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("displaceMap", t));
               mat.myFeatures |= Material.Feature.DisplacementMap;
            }
         }

         if(am.HasTextureEmissive)
         {
            string textureName = am.TextureEmissive.FilePath;
            textureName = textureName.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            Texture t = myResourceManager.getResource(td) as Texture;
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("emissiveMap", t));
               mat.myFeatures |= Material.Feature.EmmissiveMap;
            }
         }

         if(am.HasTextureOpacity)
         {
            string textureName = am.TextureOpacity.FilePath;
            textureName = textureName.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            Texture t = myResourceManager.getResource(td) as Texture;
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("alphaMap", t));
               mat.myFeatures |= Material.Feature.AlphaMap;
            }
         }

         return mat;
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

      private Vector2 toVector2D(Vector3D vec)
      {
         Vector2 v;
         v.X = vec.X;
         v.Y = vec.Y;
         return v;
      }
      private Vector3 toVector(Vector3D vec)
      {
         Vector3 v;
         v.X = vec.X;
         v.Y = vec.Y;
         v.Z = vec.Z;
         return v;
      }

      private Color4 toColor(Color4D color)
      {
         Color4 c;
         c.R = color.R;
         c.G = color.G;
         c.B = color.B;
         c.A = color.A;
         return c;
      }

      private Matrix4 toMatrix(Matrix4x4 mat)
      {
         Matrix4 m = new Matrix4();
         m.M11 = mat.A1;
         m.M12 = mat.A2;
         m.M13 = mat.A3;
         m.M14 = mat.A4;
         m.M21 = mat.B1;
         m.M22 = mat.B2;
         m.M23 = mat.B3;
         m.M24 = mat.B4;
         m.M31 = mat.C1;
         m.M32 = mat.C2;
         m.M33 = mat.C3;
         m.M34 = mat.C4;
         m.M41 = mat.D1;
         m.M42 = mat.D2;
         m.M43 = mat.D3;
         m.M44 = mat.D4;
         return m;
      }
      #endregion
   }
}