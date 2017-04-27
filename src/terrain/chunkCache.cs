using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;

using OpenTK;

using Util;

namespace Terrain
{
   public class ChunkCache
   {
      public Int32 byteCount { get; set; } //a 0 offset means it's just air
      public byte[] compresedData { get; set; }

      public ChunkCache()
      {

      }
   }

   public class TerrainCache
   {
      String myFilename;
      ConcurrentDictionary<UInt64, ChunkCache> myCacheDb = new ConcurrentDictionary<UInt64, ChunkCache>();

      public int sizeInBytes { get; set; }
      public int chunkCount { get; set; }

      public TerrainCache(String path)
      {
         myFilename = path;
         if (loadDatabase() == false)
         {
            throw new Exception("Failed to initialize game world data");
         }
      }

      public void reset()
      {
         //delete the database on disk
         File.Delete(myFilename);
         myCacheDb.Clear();
         chunkCount = 0;
         if (loadDatabase() == false)
         {
            throw new Exception("Failed to initialize game world data");
         }
      }

      public void reload()
      {
         if (loadDatabase() == false)
         {
            throw new Exception("Failed to initialize game world data");
         }
      }

      public void shutdown()
      {
         saveDatabase();
      }

      public bool containsChunk(UInt64 id)
      {
         return myCacheDb.ContainsKey(id);
      }

      public Chunk findChunk(UInt64 id)
      {
         Chunk chunk = null;

         ChunkCache ti=null;
         if (myCacheDb.TryGetValue(id, out ti) == false) // is it loaded, but compressed?
         {
            return null;  //this means that the generator needs to create it
         }
         else  //decompress it
         {
            chunk = new Chunk(Vector3.Zero);
            chunk.chunkKey = new ChunkKey(id);
            if (ti.byteCount != 0)
            {
               byte[] data = decompressChunk(ti.compresedData);
               chunk.deserialize(data);
            }
         }

         return chunk;
      }

      public byte[] compressedChunk(UInt64 id)
      {
         ChunkCache ti = null;
         if (myCacheDb.TryGetValue(id, out ti) == false) // is it loaded, but compressed?
         {
            return null;
         }

         return ti.compresedData;
      }

      public void updateChunk(Chunk chunk)
      {
         UInt64 key = chunk.key;

         if (myCacheDb.ContainsKey(key) == true)
         {
            //update metric
            sizeInBytes -= myCacheDb[key].byteCount;
         }
         else
         {
            chunkCount++;
         }

         ChunkCache ti = new ChunkCache();
         
         //save a little space on empty chunks
         //TODO: revisit this in case of dynamic chunks or variable size chunks
         if (chunk.isAir() == true)
         {
            ti.byteCount = 0;
            ti.compresedData = new byte[0];
         }
         else
         {
            byte[] data = chunk.serialize();
            data = compressChunk(data);
            ti.byteCount = data.Length;
            ti.compresedData = data;

            //update metric
            sizeInBytes += ti.byteCount;
         }
         myCacheDb[key] = ti;
      }

      public Chunk handleResponse(TerrainResponseEvent tcr)
      {
         Chunk chunk = new Chunk(Vector3.Zero);
         chunk.chunkKey = new ChunkKey(tcr.chunkId);
         byte[] data = decompressChunk(tcr.data.ToArray());
         chunk.deserialize(data);
         updateChunk(chunk);
         return chunk;
      }

      #region file handling
      bool loadDatabase()
      {
         Info.print("Loading game world: {0}", myFilename);
         if (File.Exists(myFilename) == false)
         {
            Error.print("Unable to find game world {0}", myFilename);
            return true;
         }

         using (BinaryReader reader = new BinaryReader(File.Open(myFilename, FileMode.Open)))
         {
            //read the header
            Char[] fileType = reader.ReadChars(4);
            if (fileType[0] != 'O' ||
               fileType[1] != 'C' ||
               fileType[2] != 'T' ||
               fileType[3] != 'A')
            {
               return false;
            }

            int version = reader.ReadInt32();
            if (version != 1) 
               return false;

            int indexCount = reader.ReadInt32();
            Info.print("Reading {0} chunk records", indexCount);

            for (int i = 0; i < indexCount; i++)
            {
               ChunkCache ti= new ChunkCache();
               UInt64 id = reader.ReadUInt64();
               ti.byteCount = reader.ReadInt32();
               ti.compresedData = reader.ReadBytes(ti.byteCount);
               myCacheDb[id] = ti;

               //update metric
               sizeInBytes += ti.byteCount;
               chunkCount++;
            }

            Info.print("Done");
         }

         return true;
      }

      bool saveDatabase()
      {
         using (BinaryWriter writer = new BinaryWriter(File.Open(myFilename, FileMode.Create)))
         {
            //gather some info
            Char[] octa = new Char[4] { 'O', 'C', 'T', 'A' };
            int recordCount=myCacheDb.Count;

            //Write the header
            writer.Write(octa); //4 letter identifier
            writer.Write(1); //version number
            writer.Write(recordCount); //size of the chunk
            foreach (KeyValuePair<UInt64, ChunkCache> ti in myCacheDb)
            {
               writer.Write(ti.Key);  //UInt64 chunk key
               writer.Write(ti.Value.compresedData.Length);  //size in bytes of the compressed chunk
               writer.Write(ti.Value.compresedData); //compressed chunk data
            }
         }

         return true;
      }
      #endregion

      #region compression
      byte[] compressChunk(byte[] data)
      {
         using (MemoryStream ms = new MemoryStream())
         {
            using (GZipStream gzs = new GZipStream(ms, CompressionMode.Compress))
            {
               // Write the data to the stream to compress it

               gzs.Write(BitConverter.GetBytes(data.Length), 0, sizeof(int));
               gzs.Write(data, 0, data.Length);
               gzs.Close();
            }

            // Get the compressed byte array back
            return ms.ToArray();
         }
      }

      byte[] decompressChunk(byte[] data)
      {
         using (MemoryStream ms = new MemoryStream(data))
         {
            using (GZipStream gzs = new GZipStream(ms, CompressionMode.Decompress))
            {
               byte[] dataSizeArray = new byte[sizeof(int)];
               gzs.Read(dataSizeArray, 0, dataSizeArray.Length);
               int dataSize = BitConverter.ToInt32(dataSizeArray, 0);

               byte[] ret = new byte[dataSize];
               gzs.Read(ret, 0, ret.Length);

               return ret;
            }
         }
      }
      #endregion
   }
}