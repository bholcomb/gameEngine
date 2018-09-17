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
      public class TextFile
      {
         //header
         UInt32 myVersion;
         UInt32 myHeaderSize;
         //chunk name <->chunk ID
         Dictionary<String, UInt32> myRegistry = new Dictionary<String, UInt32>();

         //chunks
         List<Chunk> myChunks = new List<Chunk>();

         public List<Chunk> chunks { get { return myChunks; } }

         public TextFile()
         {

         }

         public bool loadFile(string filename)
         {
            LuaState vm = new LuaState();
            LuaExt.extendLua(vm);

            try
            {
               if (vm.doFile(filename) == false)
               {
                  Warn.print("Unable to open BOB file {0}", filename);
                  return false;
               }

               LuaObject data = vm.findObject("BOB");
               if (data == null)
               {
                  Warn.print("Unable to find BOB data in file {0}", filename);
                  return false;
               }

               myVersion = data.get<UInt32>("version");

               //read the registry
               LuaObject registry = data.get<LuaObject>("registry");
               parseRegistry(registry);

               //read the chunks
               LuaObject chunks = data.get<LuaObject>("chunks");
               for (int i = 1; i <= chunks.count(); i++)
               {
                  LuaObject chunkData = chunks[i];
                  switch (chunkData.get<String>("type"))
                  {
                     case "model":
                        ModelChunk model = new ModelChunk();
                        parseModel(model, chunkData);
                        myChunks.Add(model);
                        break;
                     case "skeleton":
                        SkeletonChunk skeleton = new SkeletonChunk();                        
                        parseSkeleton(skeleton, chunkData);
                        myChunks.Add(skeleton);
                        break;
                     case "animation":
                        AnimationChunk animation = new AnimationChunk();
                        parseAnimation(animation, chunkData);
                        myChunks.Add(animation);
                        break;
                     case "texture":
                        Warn.print("Skipping .BOB texture");
                        break;
                     case "particle":
                        Warn.print("Skipping .BOB particle system");
                        break;
                     case "audio":
                        Warn.print("Skipping .BOB audio");
                        break;
                     default:
                        Warn.print("Unknown type: {0}", chunkData.get<String>("type"));
                        break;
                  }
               }
            }
            catch (Exception ex)
            {
               throw new Exception("Error while loading BOB model from definition file ( " + filename + " ).", ex);
            }
            return true;
         }

         public bool parseChunkHeader(Chunk c, LuaObject data)
         {
            c.myVersion = data.get<UInt32>("version");
            c.myFlags = data.get<UInt32>("flags");
            c.myName = data.get<string>("name");

            return true;
         }

         public bool parseRegistry(LuaObject data)
         {
            for(int i = 0; i < data.count(); i++)
            {
               LuaObject regEntry = data[i];
               myRegistry[regEntry.get<string>("name")] = (UInt32)i;
            }

            return true;
         }

         public bool parseMaterial(Material m, LuaObject data)
         {
            m.name = data.get<String>("name");
            m.ambient = data.get<LuaObject>("ambient");
            m.diffuse = data.get<LuaObject>("diffuse");
            m.spec = data.get<LuaObject>("spec");
            m.emission = data.get<LuaObject>("emission");
            m.shininess = data.get<float>("shininess");
            m.alpha = data.get<float>("alpha");
            if (data.contains("diffuseTexture") == true)
               m.diffuseTexture = data.get<String>("diffuseTexture");
            if (data.contains("specularTexture") == true)
               m.specularTexture = data.get<String>("specularTexture");
            if (data.contains("emissionTexture") == true)
               m.emissionTexture = data.get<String>("emissionTexture");
            if (data.contains("alphaTexture") == true)
               m.alphaTexture = data.get<String>("alphaTexture");
            if (data.contains("normalTexture") == true)
               m.normalTexture = data.get<String>("normalTexture");

            return true;
         }

         public bool parseMesh(Mesh m, LuaObject data)
         {
            m.name = data.get<String>("name");
            m.material = data.get<String>("material");
            m.indexOffset = data.get<UInt32>("indexStart");
            m.indexCount = data.get<UInt32>("indexCount");

            return true;
         }

         public bool parseModel(ModelChunk m, LuaObject data)
         {
            parseChunkHeader(m, data);

            String pt = data.get<String>("primativeType");
            switch (pt)
            {
               case "TRI": m.primativeType = PrimitiveType.Triangles; break;
               case "TRISTRIP": m.primativeType = PrimitiveType.TriangleStrip; break;
            }

            String vf = data.get<String>("vertexFormat");
            switch (vf)
            {
               case "V3N3T2": m.vertexFormat = ModelChunk.VertexFormat.V3N3T2; break;
               case "V3N3T2B4W4": m.vertexFormat = ModelChunk.VertexFormat.V3N3T2B4W4; break;
            }

            String iff = data.get<String>("indexFormat");
            switch (iff)
            {
               case "UInt16": m.indexType = ModelChunk.IndexFormat.USHORT; break;
               case "UInt32": m.indexType = ModelChunk.IndexFormat.UINT; break;
            }

            m.vertexCount = data.get<UInt32>("vertexCount");
            m.indexCount = data.get<UInt32>("indexCount");

            LuaObject meshData = data.get<LuaObject>("meshes");
            for (int i = 1; i <= meshData.count(); i++)
            {
               Mesh mesh = new Mesh();
               if (parseMesh(mesh, meshData[i]) == true)
               {
                  m.myMeshes.Add(mesh);
               }
            }

            LuaObject materialData = data.get<LuaObject>("materials");
            for (int i = 1; i <= materialData.count(); i++)
            {
               Material material = new Material();
               if (parseMaterial(material, materialData[i]) == true)
               {
                  m.myMaterials.Add(material);
               }
            }

            LuaObject vertData = data.get<LuaObject>("verts");
            switch (m.vertexFormat)
            {
               case ModelChunk.VertexFormat.V3N3T2:
                  {
                     m.verts = new List<Vector3>();
                     m.normals = new List<Vector3>();
                     m.uvs = new List<Vector2>();

                     int i = 1;
                     for (int vert = 0; vert < m.vertexCount; vert++)
                     {
                        m.verts.Add(new Vector3(vertData[i++], vertData[i++], vertData[i++]));
                        m.normals.Add(new Vector3(vertData[i++], vertData[i++], vertData[i++]));
                        m.uvs.Add(new Vector2(vertData[i++], vertData[i++]));
                     }
                     break;
                  }

               case ModelChunk.VertexFormat.V3N3T2B4W4:
                  {
                     m.verts = new List<Vector3>();
                     m.normals = new List<Vector3>();
                     m.uvs = new List<Vector2>();
                     m.boneIdx = new List<Vector4>();
                     m.boneWeights = new List<Vector4>();

                     int i = 1;
                     for (int vert = 0; vert < m.vertexCount; vert++)
                     {
                        m.verts.Add(new Vector3(vertData[i++], vertData[i++], vertData[i++]));
                        m.normals.Add(new Vector3(vertData[i++], vertData[i++], vertData[i++]));
                        m.uvs.Add(new Vector2(vertData[i++], vertData[i++]));
                        m.boneIdx.Add(new Vector4(vertData[i++], vertData[i++], vertData[i++], vertData[i++]));
                        m.boneWeights.Add(new Vector4(vertData[i++], vertData[i++], vertData[i++], vertData[i++]));
                     }
                     break;
                  }
            }

            LuaObject indexData = data.get<LuaObject>("indexes");
            switch (m.indexType)
            {
               case ModelChunk.IndexFormat.USHORT:
                  {
                     m.indexShort = new List<UInt16>();
                     int i = 1;
                     for (int idx = 0; idx < m.indexCount; idx++)
                     {
                        m.indexShort.Add((UInt16)((float)indexData[i++]));
                     }
                     break;
                  }
               case ModelChunk.IndexFormat.UINT:
                  {
                     m.indexInt = new List<UInt32>();
                     int i = 1;
                     for (int idx = 0; idx < m.indexCount; idx++)
                     {
                        m.indexInt.Add((UInt32)((float)indexData[i++]));
                     }
                     break;
                  }
            }

            return true;
         }

         public bool parseBone(Bone b, LuaObject data)
         {
            b.myName = data.get<string>("name");
            b.myParent = data.get<int>("parent");
            b.myRelativeBindMatrix = data.get<Matrix4>("matrix");
            return true;
         }

         public bool parseSkeleton(SkeletonChunk s, LuaObject data)
         {
            parseChunkHeader(s, data);
            LuaObject bones = data.get<LuaObject>("bones");
            for (int i = 1; i <= bones.count(); i++)
            {
               Bone b = new Bone();
               parseBone(b, bones[i]);
               s.myBones.Add(b);
            }

            return true;
         }

         public bool parseAnimation(AnimationChunk a, LuaObject data)
         {
            parseChunkHeader(a, data);
            a.fps = data.get<int>("framerate");
            a.numFrames = data.get<int>("numFrames");
            a.skeletonName = data.get<string>("skeleton");
            a.numBones = data.get<int>("numBones");
            LuaObject events = data["events"];
            for(int i =0; i < events.count(); i++)
            {
               LuaObject e = events[i+1];
               AnimationEvent aniEvent = new AnimationEvent();
               aniEvent.frame = e.get<int>("frame");
               aniEvent.name = e.get<string>("name");
               a.events.Add(aniEvent);
            }

            
            LuaObject poseData = data["poses"];
            int next = 1;
            for (int i = 0; i < a.numFrames; i++)
            {
               AnimationFrame frame = new AnimationFrame();
               for (int b = 0; b < a.numBones; b++)
               {
                  JointPose jp = new JointPose();
                  jp.position = new Vector3(poseData[next++], poseData[next++], poseData[next++]);
                  jp.rotation = new Quaternion(poseData[next++], poseData[next++], poseData[next++], poseData[next++]);
                  frame.Add(jp);
               }

               a.poses.Add(frame);
            }

            return true;
         }
      }
   }
}