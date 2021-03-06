using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class BobModelDescriptor : ResourceDescriptor
   {
      public BobModelDescriptor(string descriptorFilename)
         :base()
      {
         path = Path.GetDirectoryName(descriptorFilename);
         type = "bob";
         name = descriptorFilename;
      }
      public override IResource create(ResourceManager mgr)
      {
         Bob.TextFile tf = new Bob.TextFile();
         if (tf.loadFile(name) == false)
         {
            Warn.print("Error loading model file {0}", name);
            return null;
         }

         BobModelLoader loader = new BobModelLoader(mgr, Path.GetDirectoryName(name));
         Model m = loader.load(tf);

         if (m != null)
         {
            return m;
         }

         return null;
      }
   }

   public class BobModelLoader
   {
      ResourceManager myResourceManager;
      string myRootPath;

      List<Material> myMaterials = new List<Material>();

      public BobModelLoader(ResourceManager mgr, string rootPath)
      {
         myResourceManager = mgr;
         myRootPath = rootPath;
      }

      public Model load(Bob.TextFile tf)
      {
         Model model = null;
         Skeleton skel = null;
         Dictionary<string, Animation> anims = new Dictionary<string, Animation>();
         foreach(Bob.Chunk c in tf.chunks)
         {
            switch(c.myType)
            {
               case Bob.ChunkType.SKELETON:
                  skel = loadSkeleton(c as Bob.SkeletonChunk);
                  break;
               case Bob.ChunkType.ANIMATION:
                  Animation a = loadAnimation(c as Bob.AnimationChunk);
                  anims[a.name] = a;
                  break;
               case Bob.ChunkType.MODEL:
                  model = loadModel(c as Bob.ModelChunk);
                  break;
            }
         }

         if(model is SkinnedModel)
         {
            (model as SkinnedModel).skeleton = skel;
            (model as SkinnedModel).createNullAnimation();
            
            foreach(KeyValuePair<string, Animation> anim in anims)
            {
               anim.Value.skeleton = skel;
               (model as SkinnedModel).animations.Add(anim.Key, anim.Value);
            }
         }

         return model;
      }

      Skeleton loadSkeleton(Bob.SkeletonChunk sc)
      {
         Skeleton s = new Skeleton();
         s.myName = sc.myName;
         
         foreach(Bob.Bone b in sc.myBones)
         {
            Bone bb = new Bone();
            bb.myName = b.myName;
            bb.myParent = b.myParent;
            bb.myRelativeBindMatrix = b.myRelativeBindMatrix;

            if(b.myParent == -1)
            {
               bb.myWorldBindMatrix = bb.myRelativeBindMatrix;
            }
            else
            {
               bb.myWorldBindMatrix = bb.myRelativeBindMatrix * s.myBones[bb.myParent].myWorldBindMatrix;

            }
            bb.myInvWorldBindMatrix = bb.myWorldBindMatrix.Inverted();

            s.myBones.Add(bb);
         }

         return s;
      }

      Animation loadAnimation(Bob.AnimationChunk ac)
      {
         Animation a = new Animation();
         a.name = ac.myName;
         a.loop = ac.loop;
         a.duration = ac.duration;
         a.loop = ac.loop;
         a.events = ac.events;
         a.channels = ac.channels;
         return a;
      }

      Model loadModel(Bob.ModelChunk mc)
      {
         Model model = null;
         if(mc.animated == true)
         {
            model = new SkinnedModel();
         }
         else
         {
            model = new Model();
         }

         loadMaterials(model, mc);
         loadVerts(model, mc);
         loadIndexes(model, mc);
         loadMeshes(model, mc);
         model.size = findSize(mc);

         return model;
      }

      Material findMaterial(string matName)
      {
         foreach (Material m in myMaterials)
         {
            if (m.name == matName)
               return m;
         }

         return null;
      }

      void loadMaterials(Model m, Bob.ModelChunk bmc)
      {
         foreach (Bob.Material bm in bmc.myMaterials)
         {
            Material mat = new Material(bm.name);
            mat.myFeatures |= Material.Feature.Lighting;
            mat.ambient = bm.ambient;
            mat.diffuse = bm.diffuse;
            mat.spec = bm.spec;
            mat.emission = bm.emission;
            mat.shininess = bm.shininess;
            mat.alpha = bm.alpha;
            if (bm.diffuseTexture != null)
            {
               Texture t = getTexture(bm.diffuseTexture);
               if (t != null)
               {
                  mat.addAttribute(new TextureAttribute("diffuseMap", t));
                  mat.myFeatures |= Material.Feature.DiffuseMap;
                  mat.hasTransparency = t.hasAlpha;
               }
            }
            if (bm.specularTexture != null)
            {
               Texture t = getTexture(bm.specularTexture);
               if (t != null)
               {
                  mat.addAttribute(new TextureAttribute("specularMap", t));
                  mat.myFeatures |= Material.Feature.SpecMap;
               }
            }
            if (bm.emissionTexture != null)
            {
               Texture t = getTexture(bm.emissionTexture);
               if (t != null)
               {
                  mat.addAttribute(new TextureAttribute("emissionMap", t));
                  mat.myFeatures |= Material.Feature.EmmissiveMap;
               }
            }
            if (bm.normalTexture != null)
            {
               Texture t = getTexture(bm.normalTexture);
               if (t != null)
               {
                  mat.addAttribute(new TextureAttribute("normalMap", t));
                  mat.myFeatures |= Material.Feature.NormalMap;
               }
            }

            mat.upload();
            myMaterials.Add(mat);
         }
      }

      Texture getTexture(string filepath)
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

      void loadVerts(Model m, Bob.ModelChunk bmc)
      {
         VertexBufferObject vbo = new VertexBufferObject(BufferUsageHint.StaticDraw);
         if (bmc.animated == false)
         {
            List<V3N3T2> verts = new List<V3N3T2>();
            for (int i = 0; i < bmc.vertexCount; i++)
            {
               V3N3T2 v = new V3N3T2();
               v.Position = bmc.verts[i];
               v.Normal = bmc.normals[i];
               v.TexCoord = bmc.uvs[i];
               verts.Add(v);
            }

            m.myBindings = V3N3T2.bindings();
            vbo.setData(verts);
         }
         else
         {
            List<V3N3T2B4W4> verts = new List<V3N3T2B4W4>();
            for (int i = 0; i < bmc.vertexCount; i++)
            {
               V3N3T2B4W4 v = new V3N3T2B4W4();
               v.Position = bmc.verts[i];
               v.Normal = bmc.normals[i];
               v.TexCoord = bmc.uvs[i];
               v.BoneId = bmc.boneIdx[i];
               v.BoneWeight = bmc.boneWeights[i];
               verts.Add(v);
            }

            m.myBindings = V3N3T2B4W4.bindings();
            vbo.setData(verts);
         }

         m.myVbos.Add(vbo);
      }

      void loadIndexes(Model m, Bob.ModelChunk bmc)
      {
         if (bmc.indexType == Bob.ModelChunk.IndexFormat.USHORT)
         {
            m.myIbo.setData(bmc.indexShort);
         }
         else if (bmc.indexType == Bob.ModelChunk.IndexFormat.UINT)
         {
            m.myIbo.setData(bmc.indexInt);
         }
      }

      void loadMeshes(Model m, Bob.ModelChunk bmc)
      {
         foreach (Bob.Mesh bm in bmc.myMeshes)
         {
            Mesh mesh = new Mesh();
            mesh.primativeType = bmc.primativeType;
            mesh.indexBase = (int)bm.indexOffset;
            mesh.indexCount = (int)bm.indexCount;
            mesh.material = findMaterial(bm.material);
            m.myMeshes.Add(mesh);
         }
      }

      float findSize(Bob.ModelChunk bmc)
      {
         Vector3 min = new Vector3(float.MaxValue);
         Vector3 max = new Vector3(float.MinValue);

         foreach (Vector3 v in bmc.verts)
         {
            if (v.X < min.X) min.X = v.X;
            if (v.X > max.X) max.X = v.X;
            if (v.Y < min.Y) min.Y = v.Y;
            if (v.Y > max.Y) max.Y = v.Y;
            if (v.Z < min.Z) min.Z = v.Z;
            if (v.Z > max.Z) max.Z = v.Z;
         }

         return (max - min).Length / 2.0f;
      }
   }
}
