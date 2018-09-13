using System;
using System.IO;
using System.Collections.Generic;

using Util;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   /*
   public class BobChunk
   {
      public Bob.ChunkType myType;
      public UInt32 myVersion;
      public UInt32 myFlags;
      public UInt32 mySize;

      public BobChunk()
      {

      }

      public virtual bool load(BinaryReader reader)
      {
         bool ret;
         ret = loadHeader(reader);
         if (ret == false)
         {
            Warn.print("Failed to read chuck header");
            return false;
         }

         return true;
      }

      public bool loadHeader(BinaryReader reader)
      {
         myType = (Bob.ChunkType)reader.ReadUInt32();
         myVersion = reader.ReadUInt32();
         myFlags = reader.ReadUInt32();
         mySize = reader.ReadUInt32();

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

      public BobMaterial()
      {
      }
      
      public bool load(BinaryReader reader)
      {
         name = reader.ReadString();
         Color4 a = Color4.White;
         a.R = reader.ReadSingle();
         a.G = reader.ReadSingle();
         a.B = reader.ReadSingle();
         a.A = reader.ReadSingle();
         ambient = a;
         Color4 d = Color4.White;
         d.R = reader.ReadSingle();
         d.G = reader.ReadSingle();
         d.B = reader.ReadSingle();
         d.A = reader.ReadSingle();
         diffuse = d;
         Color4 s = Color4.White;
         s.R = reader.ReadSingle();
         s.G = reader.ReadSingle();
         s.B = reader.ReadSingle();
         s.A = reader.ReadSingle();
         spec = s;
         Color4 e = Color4.White;
         e.R = reader.ReadSingle();
         e.G = reader.ReadSingle();
         e.B = reader.ReadSingle();
         e.A = reader.ReadSingle();
         emission = e;
         shininess = reader.ReadSingle();
         alpha = reader.ReadSingle();
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

      public bool load(BinaryReader reader)
      {
         name = reader.ReadString();
         indexCount = reader.ReadUInt32();
         indexOffset = reader.ReadUInt32();
         material = reader.ReadString();

         return true;
      }
   }

   public class BobModelChunk : BobChunk
   {
      List<BobMesh> myMeshes = new List<BobMesh>();
      List<BobMaterial> myMaterials = new List<BobMaterial>();

      public Bob.PrimativeType primativeType { get; set; }
      public Bob.VertexFormat vertexFormat { get; set; }
      public Bob.IndexFormat indexType { get; set; }

      public BaseVertexBufferObject VBO { get; set; }
      public IndexBufferObject IBO { get; set; }

      public BobModelChunk()
         : base()
      {
      }

      public override bool load(BinaryReader reader)
      {
         base.load(reader);
         uint numMeshes = reader.ReadUInt32();
         uint numMaterials = reader.ReadUInt32();
         Bob.PrimativeType primativeType = (Bob.PrimativeType)reader.ReadByte();
         Bob.VertexFormat vertexFormat = (Bob.VertexFormat)reader.ReadByte();
         Bob.IndexFormat indexType = (Bob.IndexFormat)reader.ReadByte();
         int vertCount;
         int indexCount;

         for (int i = 0; i < numMeshes; i++)
         {
            BobMesh mesh = new BobMesh();
            if (mesh.load(reader) == true)
               myMeshes.Add(mesh);
         }

         for (int i = 0; i < numMaterials; i++)
         {
            BobMaterial material = new BobMaterial();
            if (material.load(reader) == true)
               myMaterials.Add(material);
         }

         vertCount = reader.ReadUInt32;
         switch (vertexFormat)
         {
            case Bob.VertexFormat.V3N3T2:
               {
                  VBO = new VertexBufferObject<V3N3T2>(BufferUsageHint.StaticDraw);
                  V3N3T2[] verts = new V3N3T2[vertCount];
                  for (int i = 0; i < vertCount; i++)
                  {
                     verts[i].Position.X = reader.ReadSingle();
                     verts[i].Position.Y = reader.ReadSingle();
                     verts[i].Position.Z = reader.ReadSingle();
                     verts[i].Normal.X = reader.ReadSingle();
                     verts[i].Normal.Y = reader.ReadSingle();
                     verts[i].Normal.Z = reader.ReadSingle();
                     verts[i].TexCoord.X = reader.ReadSingle();
                     verts[i].TexCoord.Y = reader.ReadSingle();
                  }
                  VBO.setData(verts);
                  break;
               }

            case Bob.VertexFormat.V3N3T2B4W4:
               {
                  VBO = new VertexBufferObject<V3N3T2B4W4>(BufferUsageHint.StaticDraw);
                  V3N3T2B4W4[] verts = new V3N3T2B4W4[vertCount];
                  for (int i = 0; i < vertCount; i++)
                  {
                     verts[i].Position.X = reader.ReadSingle();
                     verts[i].Position.Y = reader.ReadSingle();
                     verts[i].Position.Z = reader.ReadSingle();
                     verts[i].Normal.X = reader.ReadSingle();
                     verts[i].Normal.Y = reader.ReadSingle();
                     verts[i].Normal.Z = reader.ReadSingle();
                     verts[i].TexCoord.X = reader.ReadSingle();
                     verts[i].TexCoord.Y = reader.ReadSingle();
                     verts[i].BoneId.X = reader.ReadSingle();
                     verts[i].BoneId.Y = reader.ReadSingle();
                     verts[i].BoneId.Z = reader.ReadSingle();
                     verts[i].BoneId.W = reader.ReadSingle();
                     verts[i].BoneWeight.X = reader.ReadSingle();
                     verts[i].BoneWeight.Y = reader.ReadSingle();
                     verts[i].BoneWeight.Z = reader.ReadSingle();
                     verts[i].BoneWeight.W = reader.ReadSingle();
                  }
                  VBO.setData(verts);
                  break;
               }
         }

         IBO = new IndexBufferObject(BufferUsageHint.StaticDraw);
         switch (indexType)
         {
            case Bob.IndexFormat.USHORT:
               {
                  ushort[] indexes = new ushort[indexCount];
                  for (int i = 0; i < indexCount; i++)
                  {
                     indexes[i] = reader.ReadUInt16();
                  }
                  IBO.setData(indexes);
                  break;
               }
            case Bob.IndexFormat.UINT:
               {
                  uint[] indexes = new uint[indexCount];
                  for (int i = 0; i < indexCount; i++)
                  {
                     indexes[i] = reader.ReadUInt32();
                  }
                  IBO.setData(indexes);
                  break;
               }
         }

         return true;
      }
   }

   public class BobScriptChunk : BobChunk
   {
      public string myName;
      public string myData;

      public override bool load(BinaryReader reader)
      {
         base.load(reader);

         myName = reader.ReadString();
         myData = reader.ReadString();

         return true;
      }
   }

   public class BobFile
   {
      //header
      char[] myMagicNumber = new char[4];
      UInt32 myVersion;
      UInt32 myHeaderSize;
      //chunk name <->chunk ID
      Dictionary<String, UInt32> myRegistry = new Dictionary<String, UInt32>();

      //chunks
      List<BobChunk> myChunks = new List<BobChunk>();

      public BobFile()
      {

      }

      public bool loadFile(string filename)
      {
         System.IO.FileStream stream = null;
         System.IO.BinaryReader reader = null;
         try
         {
            // Open the specified file as a stream.
            stream = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read);

            // Pipe the stream in to a binary reader so we can work at the byte-by-byte level.
            reader = new System.IO.BinaryReader(stream);

            //load the header
            myMagicNumber = reader.ReadChars(4);
            if (myMagicNumber.ToString() == "BOB!")
            {
               Warn.print("{0} is not a valid BOB file", filename);
               return false;
            }

            myVersion = reader.ReadUInt32();
            myHeaderSize = reader.ReadUInt32();

            //read the registry
            UInt32 regCount = reader.ReadUInt32();
            for (int i = 0; i < regCount; i++)
            {
               String name = reader.ReadString();
               UInt32 offset = reader.ReadUInt32();
            }

            while (stream.Position != stream.Length - 1)
            {
               BobChunk chunk = new BobChunk();
               chunk.load(reader);
            }
         }
         catch (Exception ex)
         {
            throw new Exception("Error while loading BOB model from definition file ( " + filename + " ).", ex);
         }
         finally
         {
            if (reader != null) { reader.Close(); }
            if (stream != null)
            {
               stream.Close();
               stream.Dispose();
            }
         }
         return true;
      }
   }
    */
}