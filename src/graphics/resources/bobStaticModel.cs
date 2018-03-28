using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class BobStaticModelDescriptor : ResourceDescriptor
   {
      public BobStaticModelDescriptor(string descriptorFilename)
         : base()
      {
         path = Path.GetDirectoryName(descriptorFilename);
         type = "bob";
         name = descriptorFilename;
      }

      public override IResource create(ResourceManager mgr)
      {
         BobStaticModelLoader loader = new BobStaticModelLoader(mgr);
         StaticModel m = loader.loadFromFile(name);
         return m;
      }
   }

   public class BobStaticModelLoader
   {
      ResourceManager myResourceManager;
      string myRootPath;

      List<V3N3T2> myVerts = new List<V3N3T2>();
      StaticModel myModel = new StaticModel();
      List<Material> myMaterials = new List<Material>();


      public BobStaticModelLoader(ResourceManager mgr)
      {
         myResourceManager = mgr;
      }

      public StaticModel loadFromFile(String filename)
      {
         myRootPath = Path.GetDirectoryName(filename);
         BobTextFile tf = new BobTextFile();
         if (tf.loadFile(filename) == false)
         {
            Warn.print("Error loading model file {0}", filename);
            return null;
         }

         foreach (BobChunk bc in tf.chunks)
         {
            if (bc.myType == Bob.ChunkType.MODEL)
            {
               loadModel(bc as BobModelChunk);
               break;
            }
         }

         return myModel;
      }

      public void loadModel(BobModelChunk bmc)
      {
         loadMaterials(bmc.myMaterials);

         loadVerts(bmc);

         loadIndexes(bmc);

         foreach (BobMesh bm in bmc.myMeshes)
         {
            Mesh mesh = new Mesh();
            mesh.primativeType = bmc.primativeType;
            mesh.indexBase = (int)bm.indexOffset;
            mesh.indexCount = (int)bm.indexCount;
            mesh.material = findMaterial(bm.material);
            myModel.myMeshes.Add(mesh);
         }

         //should probably build a bounding box
         myModel.size = (findMax() - findMin()).Length / 2.0f;
      }

      public Material findMaterial(string matName)
      {
         foreach (Material m in myMaterials)
         {
            if (m.name == matName)
               return m;
         }

         return null;
      }

      public void loadMaterials(List<BobMaterial> mats)
      {
         foreach (BobMaterial bm in mats)
         {
            Material m = new Material(bm.name);
            m.ambient = bm.ambient;
            m.diffuse = bm.diffuse;
            m.spec = bm.spec;
            m.emission = bm.emission;
            m.shininess = bm.shininess;
            m.alpha = bm.alpha;
            if (bm.diffuseTexture != null)
            {
               Texture t = getTexture(bm.diffuseTexture);
               if (t != null)
               {
                  m.addAttribute(new TextureAttribute("diffuseMap", t));
                  m.myFeatures |= Material.Feature.DiffuseMap;
                  m.hasTransparency = t.hasAlpha;
               }
            }
            if (bm.specularTexture != null)
            {
               Texture t = getTexture(bm.specularTexture);
               if (t != null)
               {
                  m.addAttribute(new TextureAttribute("specularMap", t));
                  m.myFeatures |= Material.Feature.SpecMap;
               }
            }
            if (bm.emissionTexture != null)
            {
               Texture t = getTexture(bm.emissionTexture);
               if (t != null)
               {
                  m.addAttribute(new TextureAttribute("emissionMap", t));
                  m.myFeatures |= Material.Feature.EmmissiveMap;
               }
            }
            if (bm.normalTexture != null)
            {
               Texture t = getTexture(bm.normalTexture);
               if (t != null)
               {
                  m.addAttribute(new TextureAttribute("normalMap", t));
                  m.myFeatures |= Material.Feature.NormalMap;
               }
            }

            myMaterials.Add(m);
         }
      }

      protected Texture getTexture(string filepath)
      {
         Texture t = null;

         TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, filepath), true);
         
         t = myResourceManager.getResource(td) as Texture;

         if (t == null)
         {
            Warn.print("Failed to load texture {0}", filepath);
         }

         return t;
      }

      public void loadVerts(BobModelChunk bmc)
      {
         for (int i = 0; i < bmc.vertexCount; i++)
         {
            V3N3T2 v = new V3N3T2();
            v.Position = bmc.verts[i];
            v.Normal = bmc.normals[i];
            v.TexCoord = bmc.uvs[i];
            myVerts.Add(v);
         }

         myModel.myVbo.setData(myVerts);
      }

      public void loadIndexes(BobModelChunk bmc)
      {
         if (bmc.indexType == Bob.IndexFormat.USHORT)
         {
            myModel.myIbo.setData(bmc.indexShort);
         }
         else if (bmc.indexType == Bob.IndexFormat.UINT)
         {
            myModel.myIbo.setData(bmc.indexInt);
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