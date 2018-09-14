using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Terrain
{
   public class Chunk
   {
      public Vector3 myLocation;
      public ChunkKey myChunkKey;
      public float mySize = WorldParameters.theChunkSize;
      public Node myRoot;
      bool myIsDirty = true;

      public World world { get; set; }

      public Int32 changeNumber = 0;

      public Chunk(Vector3 location)
      {
         myLocation = location;
         myChunkKey = new ChunkKey(myLocation);
         reset();
      }

      public void setDirty()
      {
         myIsDirty = true;
      }

      public ChunkKey chunkKey
      {
         get { return myChunkKey; }
         set
         {
            myChunkKey = value;
            myLocation = myChunkKey.myLocation;
            reset();
         }
      }

      public UInt64 key { get { return myChunkKey.myKey; } }

      public bool isAir()
      {
         return myRoot.isLeaf && myRoot.materialId == 0; //air is material 0
      }

      public bool isUnderground()
      {
         Chunk above = world.findChunk(myChunkKey.neighbor(ChunkKey.Neighbor.TOP));
         if(above != null)
         {
            return !above.isAir();
         }

         return true;
      }

      public int nodeCount { get; set; }
      public int visibleFaceCount { get; set; }
      public int visibleNodeCount { get; set; }

      #region generation
      public void reset()
      {
         myRoot = new Node(this);
         //set the root node to valid
         myRoot.myParent = null;
         myRoot.mySize = WorldParameters.theChunkSize;
         myRoot.min = myLocation;
         myRoot.max = myLocation + new Vector3(mySize);
         setDirty();
      }

      public Vector3 localToWorldLocation(float x, float y, float z)
      {
         return myLocation + new Vector3(x, y, z);
      }

      public void fromPointCloud(UInt32[, ,] pc)
      {
         UInt32 water = Hash.hash("water");

         int count = pc.GetLength(0);  // this must be a power of 2
         if (MathHelper.NextPowerOfTwo(count) != count)
         {
            throw new Exception(String.Format("Point cloud size must be power of two, found {0}", count));
         }
         float minSize = WorldParameters.theChunkSize / (float)count;
         int depth = 0;

         //figure out how deep to subdivide to for this point cloud
         int tempCount = count;
         while (tempCount != 1)
         {
            tempCount = tempCount / 2;
            depth++;
         }

         //split the octree into the right number of nodes to match the point cloud
         myRoot.uniformSplit(depth);

         //now assign the point cloud's textures to each of the newly formed cubes and face visibility
         for (int x = 0; x < count; x++)
         {
            for (int y = 0; y < count; y++)
            {
               for (int z = 0; z < count; z++)
               {
                  Vector3 loc = localToWorldLocation(x * minSize, y * minSize, z * minSize);
                  Node n = findNodeContaining(loc);
                  n.materialId = pc[x, y, z];
               }
            }
         }

         //relax the entire octree combining nodes that have the same type of children to conserve space
         relaxNode(myRoot);

         //set the visibility flags on nodes
         updateVisisbility();
      }

      public void relaxNode(Node n)
      {
         //can't relax something without children
         if (n == null)
            return;

         n.relax();

         setDirty();
      }

      public void updateVisisbility()
      {
         visibleFaceCount = 0;
         visibleNodeCount = 0;
         myRoot.updateVisibility();
      }

      #endregion

      #region spatial
      public AABox bounds()
      {
         return new AABox(myRoot.min, myRoot.max);
      }

      public bool contains(Vector3 loc)
      {
         Vector3 localLoc = loc - myLocation;

         if (localLoc.X >= 0 &&
            localLoc.Y >= 0 &&
            localLoc.Z >= 0 &&
            localLoc.X < WorldParameters.theChunkSize &&
            localLoc.Y < WorldParameters.theChunkSize &&
            localLoc.Z < WorldParameters.theChunkSize)
         {
            return true;
         }

         return false;
      }

      public Node findNode(NodeKey nodeKey)
      {
         return myRoot.findNode(nodeKey);
      }

      public Node findNodeContaining(NodeKey nodekey)
      {
         return myRoot.findNodeContaining(nodekey);
      }

      public Node findNodeContaining(Vector3 loc)
      {
         if (contains(loc) == false)
            return null;

         Vector3 localLoc = loc - myLocation;
         localLoc = (localLoc / WorldParameters.theChunkSize) * WorldParameters.theMaxUnits;
         Vector3i normalizedLoc = new Vector3i(
            (Int32)Math.Floor(localLoc.X + 0.5f),
            (Int32)Math.Floor(localLoc.Y + 0.5f),
            (Int32)Math.Floor(localLoc.Z + 0.5f));

         return myRoot.findNodeContaining(normalizedLoc);
      }

      public List<NodeHit> intersect(Ray r, float t0, float t1, Material.Property materialFlags = Material.Property.ALL)
      {
         return myRoot.rayIntersection(r, t0, t1, materialFlags);
      }

      public List<NodeHit> intersect(Vector3 sphereCenter, float radius, Material.Property materialFlags = Material.Property.ALL)
      {
         return myRoot.sphereIntersection(sphereCenter, radius, materialFlags);
      }
      #endregion

      #region editing functions
      public void splitToCube(Vector3 loc, int depth)
      {
         //clamp for sanity
         if (depth < 0) depth = 0;
         if (depth > WorldParameters.theMaxDepth) depth = WorldParameters.theMaxDepth;

         Node n = findNodeContaining(loc);
         if (n == null)
            return;

         while (n.myKey.depth < depth)
         {
            n.split();
            setDirty();
            Node tn = findNodeContaining(loc);
            if (tn == n)
            {
               return;
            }

            n = tn;
         }
      }

      public void setNodeMaterial(NodeKey nk, UInt32 matIndex)
      {
         changeNumber++;
         Node n = myRoot.getOrCreateNode(nk);
         n.materialId = matIndex;
         setDirty();
      }

      public void deleteNode(NodeKey nk)
      {
         changeNumber++;
         Node n = myRoot.getOrCreateNode(nk);
         n.reset();

         setDirty();
      }

      public void adjustVert(NodeKey nk, int edge, int vert, int amount)
      {
         changeNumber++;
         Node n = myRoot.getOrCreateNode(nk);
         int[] ie = Node.edgeIndices(edge);
         EdgeEnd end = vert == ie[0] ? EdgeEnd.START : EdgeEnd.END;
         int edgeValue;
         if (end == EdgeEnd.START)
         {
            edgeValue = (int)n.edgeStart(edge);
            n.setEdgeStart(edge, edgeValue + amount);
         }
         else
         {
            edgeValue = (int)n.edgeStop(edge);
            n.setEdgeStop(edge, edgeValue + amount);
         }

         setDirty();
      }

      public void adjustEdge(NodeKey nk, int edge, Face face, int amount)
      {
         changeNumber++;
         Node n = myRoot.getOrCreateNode(nk);
         int[] attachedEdges = Node.attachedEdges(face, edge);

         EdgeEnd end = ((attachedEdges[0] > 0) || (attachedEdges[1] > 0)) ? EdgeEnd.START : EdgeEnd.END;

         if (end == EdgeEnd.START)
         {
            int edgeValue = (int)n.edgeStart(attachedEdges[0]);
            n.setEdgeStart(attachedEdges[0], edgeValue + amount);
            edgeValue = (int)n.edgeStart(attachedEdges[1]);
            n.setEdgeStart(attachedEdges[1], edgeValue + amount);
         }
         else
         {
            //these are stored as negative values, so negate them again to avoid index errors
            int edgeValue = (int)n.edgeStop(-attachedEdges[0]);
            n.setEdgeStop(-attachedEdges[0], edgeValue + amount);
            edgeValue = (int)n.edgeStop(-attachedEdges[1]);
            n.setEdgeStop(-attachedEdges[1], edgeValue + amount);
         }

         setDirty();
      }

      public void adjustFace(NodeKey nk, Face face, int amount)
      {
         changeNumber++;
         Node n = myRoot.getOrCreateNode(nk);
         int[] attachedEdges = Node.attachedEdges(face);

         //if the face's 1 bit is set, then it's a negative direction on the axis, and all edges
         //ends are the start 
         EdgeEnd end = ((int)face & 0x1) != 0 ? EdgeEnd.START : EdgeEnd.END;

         if (end == EdgeEnd.START)
         {
            for (int i = 0; i < 4; i++)
            {
               int edgeValue = (int)n.edgeStart(attachedEdges[i]);
               n.setEdgeStart(attachedEdges[i], edgeValue + amount);
            }
         }
         else
         {
            for (int i = 0; i < 4; i++)
            {
               int edgeValue = (int)n.edgeStop(attachedEdges[i]);
               n.setEdgeStop(attachedEdges[i], edgeValue + amount);
            }
         }

         setDirty();
      }

      public void splitNode(NodeKey nk)
      {
         changeNumber++;
         Node n = myRoot.getOrCreateNode(nk);
         n.split();

         setDirty();
      }

      public void joinNode(NodeKey nk)
      {
         changeNumber++;
         Node n = myRoot.getOrCreateNode(nk);
         n.join();

         setDirty();
      }

      public void resetNode(NodeKey nk)
      {
         changeNumber++;
         Node n = myRoot.getOrCreateNode(nk);
         //n.reset();
         n.updateVisibility();

         setDirty();
      }


      #endregion


      #region Serialize/Deserialize
      public byte[] serialize()
      {
         int size = calcSize();
         int leafCount = myRoot.leafCount();

         MemoryStream ms = new MemoryStream(size);
         BinaryWriter writer = new BinaryWriter(ms);

         writer.Write(1); //version number
         writer.Write(mySize); //size of the chunk
         writer.Write(leafCount);

         myRoot.serialize(ref writer);

         return ms.ToArray();
      }

      public bool deserialize(byte[] data)
      {
         //reset the chunk
         reset();

         if (data.Length > 0)
         {
            MemoryStream ms = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(ms);
            int version = reader.ReadInt32();
            if (version != 1)
               return false;

            mySize = reader.ReadSingle();

            int leafCount = reader.ReadInt32();
            for (int i = 0; i < leafCount; i++)
            {
               UInt32 nk = reader.ReadUInt32();
               NodeKey nodeKey = new NodeKey(nk);
               UInt32 matId = reader.ReadUInt32();
               byte[] edges = reader.ReadBytes(12);
               byte faceVisibility = reader.ReadByte();

               Node n = myRoot.getOrCreateNode(nodeKey);
               n.materialId = matId;
               n.myEdgeSpans = edges;
               n.myFaceVisibilty = faceVisibility;
            }

            nodeCount = leafCount;
         }

         return true;
      }

      int calcSize()
      {
         int size = 0;
         size += 4; //version number
         size += 4; //size of chunk
         size += 4; //leaf count
         size += myRoot.calcSize(); //leaf size * num leafs
         return size;
      }
      #endregion
   };
}