using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Terrain
{
   public class NodeLocation
   {
      static UInt32 theMiddle = 2147483136; //when converted to chunk and node key, this represents 0,0,0 in the world
      
      public UInt32 nx = theMiddle;
      public UInt32 ny = theMiddle;
      public UInt32 nz = theMiddle;

      ChunkKey myChunk;
      NodeKey myNode;

      public NodeLocation()
      {

      }

      public NodeLocation(Vector3 worldLoc, int depth)
      {
         myChunk = new ChunkKey(worldLoc);
         Vector3 local = worldLoc - myChunk.myLocation;
         UInt32 lx,ly,lz;
         lx=(UInt32)(local.X * WorldParameters.theLeafRatio);
         ly=(UInt32)(local.Y * WorldParameters.theLeafRatio);
         lz=(UInt32)(local.Z * WorldParameters.theLeafRatio);

         myNode = NodeKey.combineCode(lx, ly, lz, depth);

         updateInternalValues();
      }
      
      public NodeLocation(UInt32 x, UInt32 y, UInt32 z, int depth)
      {
         nx = x;
         ny = y;
         nz = z;

         UInt32 lx, ly, lz;
         UInt64 cx, cy, cz;
 
         //get the bottom 10 bits for the local node location (within a chunk)
         lx = nx & 0x3ff;
         ly = ny & 0x3ff;
         lz = nz & 0x3ff;

         myNode = NodeKey.combineCode(lx, ly, lz, depth);

         //this is the chunk biased number
         //get the top 21 bits
         cx = (nx >> 10) & 0x1fffff;
         cy = (ny >> 10) & 0x1fffff;
         cz = (nz >> 10) & 0x1fffff;
         UInt64 chunkKey = (cx << 42) + (cy << 21) + cz;
         myChunk = new ChunkKey(chunkKey);
      }

      public NodeLocation(ChunkKey chunk, NodeKey key)
      {
         myChunk = chunk;
         myNode = key;

         updateInternalValues();
      }

      void updateInternalValues()
      {
         Vector3i biasedId = chunk.biasedId;
         Vector3i keyLoc = myNode.splitCode();
         nx = (UInt32)((biasedId.X << 10) + keyLoc.X);
         ny = (UInt32)((biasedId.Y << 10) + keyLoc.Y);
         nz = (UInt32)((biasedId.Z << 10) + keyLoc.Z);
      }

      public NodeLocation getNeighborLocation(Face n)
      {
         UInt32 x, y, z;
         x = nx;
         y = ny;
         z = nz;
         UInt32 nodeSize =  (UInt32) 1 << (NodeKey.theMaxDepth - myNode.depth);
         switch (n)
         {
            case Face.RIGHT:
               x += nodeSize;
               break;
            case Face.LEFT:
               x -= nodeSize;
               break;
            case Face.TOP:
               y += nodeSize;
               break;
            case Face.BOTTOM:
               y -= nodeSize;
               break;
            case Face.FRONT:
               z -= nodeSize;
               break;
            case Face.BACK:
               z += nodeSize;
               break;
         }

         return new NodeLocation(x,y,z, myNode.depth);
      }

      public ChunkKey chunk
      {
         get { return myChunk; }
      }

      public NodeKey node
      {
         get { return myNode; }
      }

      public Vector3 worldLocation()
      {
         Vector3 ret = new Vector3();
         ret = chunk.myLocation;
         ret += node.location;
         return ret;
      }

      //this is in world space
      public Vector3 min()
      {
         return worldLocation();
      }

      //this is local to the chunk
      public Vector3 max()
      {
         return worldLocation() + new Vector3(myNode.size());
      }

      //this returns the nodekeys size
      public float size()
      {
         return myNode.size();
      }

      public override bool Equals(object obj)
      {
         // If parameter is null return false.
         if (obj == null)
         {
            return false;
         }

         // If parameter cannot be cast to Point return false.
         NodeLocation p = obj as NodeLocation;
         if ((System.Object)p == null)
         {
            return false;
         }

         // Return true if the fields match:
         return (nx==p.nx && ny==p.ny && nz==p.nz);
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }
   }

   //a Morton encoded UInt32 that gives the position and depth of a node within an octree/chunk
   public struct NodeKey
   {
      //keys can only have a max depth of 10, otherwise, they won't fit in a UInt32.
      //this still allows for some rather small cubes
      public static int theMaxDepth = 10;

      // the key is the morton encoded path through the octree to find the particular node
      //it uses a 1 as a sentinel (the other values are groups of 3 bits) to help determine the depth
      //bit order is SENTINEL ZYX ZYX ZYX...
      public UInt32 myValue;
      int myCachedDepth;
      Vector3 myCachedLocation;

      //copy a key from its internal representation
      public NodeKey(UInt32 key)
      {
         myValue = key;
         myCachedDepth = -1;
         myCachedLocation = Vector3.Zero;
      }

      //copy a key
      public NodeKey(NodeKey orig)
      {
         myValue = orig.myValue;
         myCachedDepth = orig.myCachedDepth;
         myCachedLocation = orig.myCachedLocation;
      }

      //find the depth of this node based on its key
      public int depth
      {
         get{
            if (myCachedDepth != -1) 
               return myCachedDepth;

            int d = 0;
            UInt32 temp = myValue;
            while (temp != 1)
            {
               temp = temp >> 3;
               d++;
            }

            myCachedDepth = d;
            return d;
         }
      }

      //get the size of this node based on its depth and the size of a full octree in the world
      public float size()
      {
         return WorldParameters.theChunkSize / (1 << depth);
      }

      public bool contains(NodeKey key)
      {
         if (key.myValue == myValue)
            return true;

         int keyDepth = key.depth;
         if (keyDepth <= depth)
            return false;

         UInt32 temp = myValue;

         while (temp != 1)
         {
            temp = temp >> 3;
            keyDepth--;

            if (temp == myValue)
               return true;

            if (keyDepth <= depth)
               return false;
         }

         return false;
      }

      public Vector3i splitCode()
      {
         Vector3i split = new Vector3i();
         int d = depth;
         UInt32 temp = myValue;
         for (int i = d; i > 0; i--)
         {
            //figure out the depth at this size
            int depthSize = (int)WorldParameters.theMaxUnits / (1 << i);

            //only look at bottom 3 bits for size
            split.Z += (int)(temp & 0x4) != 0 ? depthSize : 0;  //if the bit is set, add the offset otherwise 0
            split.Y += (int)(temp & 0x2) != 0 ? depthSize : 0;
            split.X += (int)(temp & 0x1) != 0 ? depthSize : 0;

            temp = temp >> 3; //shift temp over for next iterations
         }
         Vector3 loc = location;

         return split;
      }

      //get the local (to the root of the octree) location from the key
      public Vector3 location
      {
         get
         {
            if (myCachedLocation != Vector3.Zero)
               return (Vector3)myCachedLocation;

            Vector3 v = Vector3.Zero;
            int d = depth;
            UInt32 temp = myValue;
            for (int i = d; i > 0; i--)
            {
               //figure out the depth at this size
               float depthSize = WorldParameters.theChunkSize / (1 << i);

               //only look at bottom 3 bits for size
               v.Z += (temp & 0x4) != 0 ? depthSize : 0;  //if the bit is set, add the offset otherwise 0
               v.Y += (temp & 0x2) != 0 ? depthSize : 0;
               v.X += (temp & 0x1) != 0 ? depthSize : 0;

               temp = temp >> 3; //shift temp over for next iterations
            }

            myCachedLocation = v;
            return v;
         }
      }

      //create the node key for the given child of this node
      public NodeKey createChildKey(int i)
      {
         UInt32 childKey = (myValue << 3) + (UInt32)i;
         return new NodeKey(childKey);
      }

      public IEnumerable childPath()
      {
         int d = depth;
         int[] path = new int[d];
         UInt32 temp = myValue;
         
         //get all the child id's up from the smallest to the largest
         for (int i = 0; i < d; i++)
         {
            path[i]= (int)temp & 0x7;
            temp = temp >> 3;
         }

         //return them one at a time from largest to smallest
         for (int i = d - 1; i >= 0; i--)
         {
            yield return path[i];
         }
      }

      public static NodeKey combineCode(UInt32 x, UInt32 y, UInt32 z, int depth)
      {
         UInt32 key=1;
         for (int i = 1; i <= depth; i++)
         {
            UInt32 v = 0;
            UInt32 value = (UInt32)1 << (theMaxDepth - i);
            if(x >= value)
            {
               x -= value;
               v += 1;
            }
            if (y >= value)
            {
               y -= value;
               v += 2;
            }
            if (z >= value)
            {
               z -= value;
               v += 4;
            }
            key = key << 3;
            key += v;
         }

         return new NodeKey(key);
      }

      public static NodeKey createNodeKey(Vector3i normalizedChunkLocation)
      {
         return combineCode((UInt32)normalizedChunkLocation.X, 
                            (UInt32)normalizedChunkLocation.Y, 
                            (UInt32)normalizedChunkLocation.Z, 
                            theMaxDepth);
      }
   }
}