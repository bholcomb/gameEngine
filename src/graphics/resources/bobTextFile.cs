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
   public class BobChunk
   {
      public Bob.ChunkType myType;
      public UInt32 myVersion;
      public UInt32 myFlags;

      public BobChunk()
      {

      }

      public virtual bool parse(LuaObject data)
      {
         myVersion = data.get<UInt32>("version");
         myFlags = data.get<UInt32>("flags");
         return true;
      }
   }

   public class BobMaterial
   {
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

      public BobMaterial()
      {
         diffuseTexture = null;
         specularTexture = null;
         emissionTexture = null;
         alphaTexture = null;
         normalTexture = null;
      }
      
      public bool load(LuaObject data)
      {
         name = data.get<String>("name");
         ambient = data.get<LuaObject>("ambient");
         diffuse = data.get<LuaObject>("diffuse");
         spec = data.get<LuaObject>("spec");
         emission = data.get<LuaObject>("emission");
         shininess = data.get<float>("shininess");
         alpha = data.get<float>("alpha");
         if (data.contains("diffuseTexture") == true)
            diffuseTexture = data.get<String>("diffuseTexture");
         if (data.contains("specularTexture") == true)
            specularTexture = data.get<String>("specularTexture");
         if (data.contains("emissionTexture") == true)
            emissionTexture = data.get<String>("emissionTexture");
         if (data.contains("alphaTexture") == true)
            alphaTexture = data.get<String>("alphaTexture");
         if (data.contains("normalTexture") == true)
            normalTexture = data.get<String>("normalTexture");

         return true;
      }
   }

   public class BobMesh
   {
      public string name { get; set; }
      public uint indexCount { get; set; }
      public uint indexOffset { get; set; }
      public string material { get; set; }

      public BobMesh()
      {
      }

      public bool parse(LuaObject data)
      {
         name = data.get<String>("name");
         material = data.get<String>("material");
         indexOffset = data.get<UInt32>("indexStart");
         indexCount = data.get<UInt32>("indexCount");
         return true;
      }
   }

   public class BobModelChunk : BobChunk
   {
      public List<BobMesh> myMeshes = new List<BobMesh>();
      public List<BobMaterial> myMaterials = new List<BobMaterial>();

      public PrimitiveType primativeType { get; set; }
      public Bob.VertexFormat vertexFormat { get; set; }
      public Bob.IndexFormat indexType { get; set; }

      public UInt32 vertexCount { get; set; }
      public List<Vector3> verts { get; set; }
      public List<Vector3> normals { get; set; }
      public List<Vector2> uvs { get; set; }
      public List<Vector4> boneIdx { get; set; }
      public List<Vector4> boneWeights { get; set; }

      public UInt32 indexCount { get; set; }
      public List<UInt16> indexShort { get; set; }
      public List<UInt32> indexInt { get; set; }

      public BobModelChunk()
         : base()
      {
         myType = Bob.ChunkType.MODEL;
      }

      public override bool parse(LuaObject data)
      {
         base.parse(data);
         String pt=data.get<String>("primativeType");
         switch(pt)
         {
            case "TRI": primativeType = PrimitiveType.Triangles; break;
            case "TRISTRIP": primativeType = PrimitiveType.TriangleStrip; break;
         }

         String vf=data.get<String>("vertexFormat");
         switch(vf)
         {
            case "V3N3T2": vertexFormat = Bob.VertexFormat.V3N3T2; break;
            case "V3N3T2B4W4": vertexFormat = Bob.VertexFormat.V3N3T2B4W4; break;
         }

         String iff=data.get<String>("indexFormat");
         switch(iff)
         {
            case "UInt16": indexType = Bob.IndexFormat.USHORT; break;
            case "UInt32": indexType = Bob.IndexFormat.UINT; break;
         }
         
         vertexCount = data.get<UInt32>("vertexCount");
         indexCount = data.get<UInt32>("indexCount");

         LuaObject meshData=data.get<LuaObject>("meshes");
         for (int i = 1; i <= meshData.count(); i++)
         {
            BobMesh mesh = new BobMesh();
            if (mesh.parse(meshData[i]) == true)
            {
               myMeshes.Add(mesh);
            }
         }

         LuaObject materialData = data.get<LuaObject>("materials");
         for (int i = 1; i <= materialData.count(); i++)
         {
            BobMaterial material = new BobMaterial();
            if (material.load(materialData[i]) == true)
            {
               myMaterials.Add(material);
            }
         }

         LuaObject vertData=data.get<LuaObject>("verts");
         switch (vertexFormat)
         {
            case Bob.VertexFormat.V3N3T2:
               {
                  verts = new List<Vector3>();
                  normals = new List<Vector3>();
                  uvs = new List<Vector2>();

                  int i = 1;
                  for(int vert = 0; vert < vertexCount; vert++)
                  {
                     verts.Add(new Vector3(vertData[i++], vertData[i++], vertData[i++]));
                     normals.Add(new Vector3(vertData[i++], vertData[i++], vertData[i++]));
                     uvs.Add(new Vector2(vertData[i++], vertData[i++]));
                  }
                  break;
               }

            case Bob.VertexFormat.V3N3T2B4W4:
               {
                  verts = new List<Vector3>();
                  normals = new List<Vector3>();
                  uvs = new List<Vector2>();
                  boneIdx = new List<Vector4>();
                  boneWeights = new List<Vector4>();

                  int i = 1;
                  for (int vert = 0; vert < vertexCount; vert++)
                  {
                     verts.Add(new Vector3(vertData[i++], vertData[i++], vertData[i++]));
                     normals.Add(new Vector3(vertData[i++], vertData[i++], vertData[i++]));
                     uvs.Add(new Vector2(vertData[i++], vertData[i++]));
                     boneIdx.Add(new Vector4(vertData[i++], vertData[i++], vertData[i++], vertData[i++]));
                     boneWeights.Add(new Vector4(vertData[i++], vertData[i++], vertData[i++], vertData[i++]));
                  }
                  break;                 
               }
         }

         LuaObject indexData=data.get<LuaObject>("indexes");
         switch (indexType)
         {
            case Bob.IndexFormat.USHORT:
               {
                  indexShort = new List<UInt16>();
                  int i=1;
                  for (int idx = 0; idx < indexCount; idx++)
                  {
                     indexShort.Add((UInt16)((float)indexData[i++]));
                  }
                  break;
               }
            case Bob.IndexFormat.UINT:
               {
                  indexInt = new List<UInt32>();
                  int i = 1;
                  for (int idx = 0; idx < indexCount; idx++)
                  {
                     indexInt.Add((UInt32)((float)indexData[i++]));
                  }
                  break;
               }
         }

         return true;
      }
   }

   public class BobTextFile
   {
      //header
      char[] myMagicNumber = new char[4];
      UInt32 myVersion;
      UInt32 myHeaderSize;
      //chunk name <->chunk ID
      Dictionary<String, UInt32> myRegistry = new Dictionary<String, UInt32>();

      //chunks
      List<BobChunk> myChunks = new List<BobChunk>();

      public List<BobChunk> chunks { get { return myChunks; } }

      public BobTextFile()
      {

      }

      public bool loadFile(string filename)
      {
         LuaState vm = new LuaState();
         try
         {
            if(vm.doFile(filename)==false)
            {
               Warn.print("Unable to open BOB file {0}", filename);
               return false;
            }

            LuaObject data= vm.findObject("BOB");
            if(data == null)
            {
               Warn.print("Unable to find BOB data in file {0}", filename);
               return false;
            }
            
            myVersion = data.get<UInt32>("version");
            
            //read the registry
            LuaObject registry= data.get<LuaObject>("registry");

            //read the chunks
            LuaObject chunks = data.get<LuaObject>("chunks");
            for(int i=1; i <= chunks.count(); i++)
            {
               LuaObject chunk = chunks[i];
               switch(chunk.get<String>("type"))
               {
                  case "model":
                     BobModelChunk model = new BobModelChunk();
                     model.parse(chunk);
                     myChunks.Add(model);
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
   }
}