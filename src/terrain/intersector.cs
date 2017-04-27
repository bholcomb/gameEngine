using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;
using Physics;

namespace Terrain
{
   public class Intersector
   {
      public static Chunk chunkIntersecting(World world, Vector3 point)
      {
         UInt64 key = ChunkKey.createKeyFromWorldLocation(point);
         Chunk chunk = null;
         if (world.chunks.TryGetValue(key, out chunk) == true)
         {
            return chunk;
         }

         return null;
      }

      public static List<Chunk> chunksIntersecting(World world, Vector3 sphereCenter, float radius)
      {
         List<Chunk> ret = new List<Chunk>();

         //sphere is smaller than a chunk
         if(radius < (WorldParameters.theChunkSize /2))
         {         
            Chunk chunk = chunkIntersecting(world, sphereCenter);

            if (chunk != null)
            {
               ret.Add(chunk);
               //no need to check all the sides if the aabb totally contains the sphere
               AABox bounds = chunk.bounds();

               if (Intersections.AABBContainsSphere(sphereCenter, radius, bounds) == true)
                  return ret;

               ChunkKey.Neighbor getNeighbors=ChunkKey.Neighbor.NONE;
               if ((sphereCenter.X - radius) < bounds.myMin.X) getNeighbors |= ChunkKey.Neighbor.LEFT;
               if ((sphereCenter.X + radius) > bounds.myMax.X) getNeighbors |= ChunkKey.Neighbor.RIGHT;
               if ((sphereCenter.Y - radius) < bounds.myMin.Y) getNeighbors |= ChunkKey.Neighbor.BOTTOM;
               if ((sphereCenter.Y + radius) > bounds.myMax.Y) getNeighbors |= ChunkKey.Neighbor.TOP;
               if ((sphereCenter.Z - radius) < bounds.myMin.Z) getNeighbors |= ChunkKey.Neighbor.FRONT;
               if ((sphereCenter.Z + radius) > bounds.myMax.Z) getNeighbors |= ChunkKey.Neighbor.BACK;
               ret.AddRange(world.findNeighbors(chunk, getNeighbors));
            }
         }
         else //sphere could cover several chunks
         {
            foreach (Chunk chunk in world.chunks.Values)
            {
               if(Intersections.AABoxSphereIntersection(chunk.bounds(), sphereCenter, radius)==true)
               {
                  ret.Add(chunk);
               }
            }
         }

         return ret;
      }
   }


   /*
   public class NodeHit
   {
      public Vector3 location;
      public Vector3 surfaceNormal;
      public Node node;
      public Face face;
      public int edge;
      public int vert;

      public NodeHit()
      {
         node = null;
         face = Face.NONE;
         edge = -1;
         vert = -1;
      }
   };

   public class NodeIntersector
   {
#region intersections
      public static bool contains(Vector3 worldLoc, Vector3 min, Vector3 max)
      {
         if (worldLoc.X >= min.X &&
             worldLoc.Y >= min.Y &&
             worldLoc.Z >= min.Z &&
             worldLoc.X < max.X &&
             worldLoc.Y < max.Y &&
             worldLoc.Z < max.Z)
         {
            return true;
         }

         return false;
      }

      public static bool contains(NodeLocation nl, Vector3 min, Vector3 max)
      {
         return contains(nl.worldLocation(), min, max);
      }

      public Node findNodeContaining(Vector3 loc)
      {
         Node ret = null;
         if (contains(loc) == true)
         {
            ret = this;

            if (myChildren != null)
            {
               foreach (Node child in myChildren)
               {
                  if (child != null)
                  {
                     Node temp = child.findNodeContaining(loc);
                     if (temp != null)
                     {
                        return temp;
                     }
                  }
               }
            }
         }

         return ret;
      }

      public Node findNodeContaining(Vector3i loc)
      {
         Node ret = this;
         NodeKey nk = NodeKey.createNodeKey(loc);

         foreach (int child in nk.childPath())
         {
            if(ret.myIsLeaf==true)
               break;

            ret = ret.myChildren[child];
         }

         return ret;
      }

      public Node findOrCreateNode(NodeKey nk)
      {
         if (nk.myKey == myKey.myKey)
            return this;

         Node n = this;
         foreach (int child in nk.childPath())
         {
            n = n.findOrCreateChild(child);
         }

         return n;
      }

      public Node findOrCreateChild(int child)
      {
         if (myIsLeaf == true)
            split();

         return myChildren[child];
      }

      public Node findNode(NodeKey nk)
      {
         if (nk.myKey == myKey.myKey)
            return this;

         Node n = this;
         foreach (int child in nk.childPath())
         {
            n = n.findNode(child);
            if (n == null)
            {
               return null;
            }
         }

         return n;
      }

      public Node findNode(int child)
      {
         if (myIsLeaf == true)
            return null;

         return myChildren[child];
      }

      public NodeHit getHitInfo(Ray r, float t)
      {
         NodeHit hit = new NodeHit();
         hit.node = this;
         hit.location = r.myOrigin + (r.myDirection * t);
         hit.surfaceNormal = Vector3.Zero;
         hit.face = Face.NONE;
         hit.edge = -1;
         hit.vert = -1;

         float tolerence = mySize / 12.0f; //on a 6" cube, this means within 1/2" of the edge

         //determine face/edge/vert hit
         //this is a fake loop so we can break out of it quickly once we get a good hit'
         
         //TODO:  Revisit this and use the ray signs to determine which faces are possible to be seen and only test those
         //this should speed things up a little
         for (int i = 0; i < 1; i++)
         {
            #region "X plane"
            if (FloatEquality.EQ(hit.location.X, min.X))
            {
               hit.face = Face.LEFT;
               hit.surfaceNormal = theFaceNormals[(int)Face.LEFT];
               if (FloatEquality.Within(hit.location.Y, min.Y, tolerence / 2.0f))
               {
                  hit.edge = 11;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 4;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, max.Y, tolerence / 2.0f))
               {
                  hit.edge = 10;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 2;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 6;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, min.Z, tolerence / 2.0f))
               {
                  hit.edge = 6;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 2;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, max.Z, tolerence / 2.0f))
               {
                  hit.edge = 5;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 4;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 6;
                  break;
               }
               break;
            }

            if (FloatEquality.EQ(hit.location.X, max.X))
            {
               hit.face = Face.RIGHT;
               hit.surfaceNormal = theFaceNormals[(int)Face.RIGHT];
               if (FloatEquality.Within(hit.location.Y, min.Y, tolerence / 2.0f))
               {
                  hit.edge = 8;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 1;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 5;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, max.Y, tolerence / 2.0f))
               {
                  hit.edge = 9;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 3;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 7;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, min.Z, tolerence / 2.0f))
               {
                  hit.edge = 7;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 1;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 3;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, max.Z, tolerence / 2.0f))
               {
                  hit.edge = 4;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 5;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 7;
                  break;
               }
               break;
            }
         #endregion

            #region "Y plane"
            if (FloatEquality.EQ(hit.location.Y, min.Y))
            {
               hit.face = Face.BOTTOM;
               hit.surfaceNormal = theFaceNormals[(int)Face.BOTTOM];
               if (FloatEquality.Within(hit.location.X, min.X, tolerence / 2.0f))
               {
                  hit.edge = 11;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 4;
                  break;
               }
               if (FloatEquality.Within(hit.location.X, max.X, tolerence / 2.0f))
               {
                  hit.edge = 8;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 1;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 5;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, min.Z, tolerence / 2.0f))
               {
                  hit.edge = 3;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 1;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, max.Z, tolerence / 2.0f))
               {
                  hit.edge = 0;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 4;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 5;
                  break;
               }
               break;
            }

            if (FloatEquality.EQ(hit.location.Y, max.Y))
            {
               hit.face = Face.TOP;
               hit.surfaceNormal = theFaceNormals[(int)Face.TOP];
               if (FloatEquality.Within(hit.location.X, min.X, tolerence / 2.0f))
               {
                  hit.edge = 10;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 2;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 6;
                  break;
               }
               if (FloatEquality.Within(hit.location.X, max.X, tolerence / 2.0f))
               {
                  hit.edge = 9;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 3;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 7;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, min.Z, tolerence / 2.0f))
               {
                  hit.edge = 2;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 2;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 3;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, max.Z, tolerence / 2.0f))
               {
                  hit.edge = 1;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 6;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 7;
                  break;
               }
               break;
            }
            #endregion

            #region "Z plane"
            if (FloatEquality.EQ(hit.location.Z, min.Z))
            {
               hit.face = Face.FRONT;
               hit.surfaceNormal = theFaceNormals[(int)Face.FRONT];
               if (FloatEquality.Within(hit.location.X, min.X, tolerence / 2.0f))
               {
                  hit.edge = 6;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 2;
                  break;
               }
               if (FloatEquality.Within(hit.location.X, max.X, tolerence / 2.0f))
               {
                  hit.edge = 7;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 1;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 3;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, min.Y, tolerence / 2.0f))
               {
                  hit.edge = 3;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 1;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, max.Y, tolerence / 2.0f))
               {
                  hit.edge = 2;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 2;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 3;
                  break;
               }
               break;
            }

            if (FloatEquality.EQ(hit.location.Z, max.Z))
            {
               hit.face = Face.BACK;
               hit.surfaceNormal = theFaceNormals[(int)Face.BACK];
               if (FloatEquality.Within(hit.location.X, min.X, tolerence / 2.0f))
               {
                  hit.edge = 5;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 4;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 6;
                  break;
               }
               if (FloatEquality.Within(hit.location.X, max.X, tolerence / 2.0f))
               {
                  hit.edge = 4;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 5;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 7;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, min.Y, tolerence / 2.0f))
               {
                  hit.edge = 0;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 4;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 5;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, max.Y, tolerence / 2.0f))
               {
                  hit.edge = 1;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 6;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 7;
                  break;
               }
               break;
            }
            #endregion
         }
         return hit;
      }

      public NodeHit getHitInfo(Vector3 sphereCenter, float radius, Vector3 hitNormal, float hitDepth)
      {
         NodeHit hit = new NodeHit();
         hit.node = this;
         hit.location = r.myOrigin + (r.myDirection * t);
         hit.surfaceNormal = Vector3.Zero;
         hit.face = Face.NONE;
         hit.edge = -1;
         hit.vert = -1;

         float tolerence = mySize / 12.0f; //on a 6" cube, this means within 1/2" of the edge

         //determine face/edge/vert hit
         //this is a fake loop so we can break out of it quickly once we get a good hit'

         //TODO:  Revisit this and use the ray signs to determine which faces are possible to be seen and only test those
         //this should speed things up a little
         for (int i = 0; i < 1; i++)
         {
            #region "X plane"
            if (FloatEquality.EQ(hit.location.X, min.X))
            {
               hit.face = Face.LEFT;
               hit.surfaceNormal = theFaceNormals[(int)Face.LEFT];
               if (FloatEquality.Within(hit.location.Y, min.Y, tolerence / 2.0f))
               {
                  hit.edge = 11;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 4;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, max.Y, tolerence / 2.0f))
               {
                  hit.edge = 10;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 2;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 6;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, min.Z, tolerence / 2.0f))
               {
                  hit.edge = 6;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 2;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, max.Z, tolerence / 2.0f))
               {
                  hit.edge = 5;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 4;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 6;
                  break;
               }
               break;
            }

            if (FloatEquality.EQ(hit.location.X, max.X))
            {
               hit.face = Face.RIGHT;
               hit.surfaceNormal = theFaceNormals[(int)Face.RIGHT];
               if (FloatEquality.Within(hit.location.Y, min.Y, tolerence / 2.0f))
               {
                  hit.edge = 8;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 1;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 5;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, max.Y, tolerence / 2.0f))
               {
                  hit.edge = 9;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 3;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 7;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, min.Z, tolerence / 2.0f))
               {
                  hit.edge = 7;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 1;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 3;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, max.Z, tolerence / 2.0f))
               {
                  hit.edge = 4;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 5;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 7;
                  break;
               }
               break;
            }
            #endregion

            #region "Y plane"
            if (FloatEquality.EQ(hit.location.Y, min.Y))
            {
               hit.face = Face.BOTTOM;
               hit.surfaceNormal = theFaceNormals[(int)Face.BOTTOM];
               if (FloatEquality.Within(hit.location.X, min.X, tolerence / 2.0f))
               {
                  hit.edge = 11;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 4;
                  break;
               }
               if (FloatEquality.Within(hit.location.X, max.X, tolerence / 2.0f))
               {
                  hit.edge = 8;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 1;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 5;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, min.Z, tolerence / 2.0f))
               {
                  hit.edge = 3;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 1;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, max.Z, tolerence / 2.0f))
               {
                  hit.edge = 0;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 4;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 5;
                  break;
               }
               break;
            }

            if (FloatEquality.EQ(hit.location.Y, max.Y))
            {
               hit.face = Face.TOP;
               hit.surfaceNormal = theFaceNormals[(int)Face.TOP];
               if (FloatEquality.Within(hit.location.X, min.X, tolerence / 2.0f))
               {
                  hit.edge = 10;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 2;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 6;
                  break;
               }
               if (FloatEquality.Within(hit.location.X, max.X, tolerence / 2.0f))
               {
                  hit.edge = 9;
                  if (FloatEquality.Within(hit.location.Z, min.Z, tolerence)) hit.vert = 3;
                  if (FloatEquality.Within(hit.location.Z, max.Z, tolerence)) hit.vert = 7;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, min.Z, tolerence / 2.0f))
               {
                  hit.edge = 2;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 2;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 3;
                  break;
               }
               if (FloatEquality.Within(hit.location.Z, max.Z, tolerence / 2.0f))
               {
                  hit.edge = 1;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 6;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 7;
                  break;
               }
               break;
            }
            #endregion

            #region "Z plane"
            if (FloatEquality.EQ(hit.location.Z, min.Z))
            {
               hit.face = Face.FRONT;
               hit.surfaceNormal = theFaceNormals[(int)Face.FRONT];
               if (FloatEquality.Within(hit.location.X, min.X, tolerence / 2.0f))
               {
                  hit.edge = 6;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 2;
                  break;
               }
               if (FloatEquality.Within(hit.location.X, max.X, tolerence / 2.0f))
               {
                  hit.edge = 7;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 1;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 3;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, min.Y, tolerence / 2.0f))
               {
                  hit.edge = 3;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 0;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 1;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, max.Y, tolerence / 2.0f))
               {
                  hit.edge = 2;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 2;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 3;
                  break;
               }
               break;
            }

            if (FloatEquality.EQ(hit.location.Z, max.Z))
            {
               hit.face = Face.BACK;
               hit.surfaceNormal = theFaceNormals[(int)Face.BACK];
               if (FloatEquality.Within(hit.location.X, min.X, tolerence / 2.0f))
               {
                  hit.edge = 5;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 4;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 6;
                  break;
               }
               if (FloatEquality.Within(hit.location.X, max.X, tolerence / 2.0f))
               {
                  hit.edge = 4;
                  if (FloatEquality.Within(hit.location.Y, min.Y, tolerence)) hit.vert = 5;
                  if (FloatEquality.Within(hit.location.Y, max.Y, tolerence)) hit.vert = 7;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, min.Y, tolerence / 2.0f))
               {
                  hit.edge = 0;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 4;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 5;
                  break;
               }
               if (FloatEquality.Within(hit.location.Y, max.Y, tolerence / 2.0f))
               {
                  hit.edge = 1;
                  if (FloatEquality.Within(hit.location.X, min.X, tolerence)) hit.vert = 6;
                  if (FloatEquality.Within(hit.location.X, max.X, tolerence)) hit.vert = 7;
                  break;
               }
               break;
            }
            #endregion
         }
         return hit;
      }

      public List<NodeHit> rayIntersection(Ray r, float t0, float t1, Material.Property materialFlags = Material.Property.ALL)
      {
         if (myIsLeaf == true && myMaterial.property == Material.Property.AIR)
            return null;

         float t;
         if (Intersections.rayBoxIntersection(r, t0, t1, min, max, out t) == false)
            return null;

         List<NodeHit> intersections = new List<NodeHit>();

         //since we passed the hit test and we are a leaf, just return us
         if (myIsLeaf == true)
         {
            if ((myMaterial.property & materialFlags) != Material.Property.NONE)
            {
               intersections.Add(getHitInfo(r, t));
            }
            
            return intersections;
         }

         //check the children
         for (int i = 0; i < 8; i++ )
         {
            if (myChildren[i] != null)
            {
               List<NodeHit> nodes = myChildren[i].rayIntersection(r, t0, t1, materialFlags);
               if (nodes != null)
               {
                  intersections.AddRange(nodes);
               }
            }
         }

         if (intersections.Count == 0)
         {
            return null;
         }

         return intersections;
      }

      public List<NodeHit> sphereIntersection(Vector3 sphereCenter, float radius, Material.Property materialFlags = Material.Property.ALL)
      {
         if (myIsLeaf == true && myMaterial.property == Material.Property.AIR)
            return null;

         Vector3 hitNormal;
         float hitDepth;
         if (Intersections.AABoxSphereIntersection(sphereCenter, radius, new AABox(min, max), out hitNormal, out hitDepth) == false)
            return null;

         List<NodeHit> intersections = new List<NodeHit>();

         //since we passed the hit test and we are a leaf, just return us
         if (myIsLeaf == true)
         {
            if ((myMaterial.property & materialFlags) != Material.Property.NONE)
            {
               intersections.Add(getHitInfo(sphereCenter, radius, hitNormal, hitDepth));
            }

            return intersections;
         }

         //check the children
         for (int i = 0; i < 8; i++)
         {
            if (myChildren[i] != null)
            {
               List<NodeHit> nodes = myChildren[i].sphereIntersection(sphereCenter, radius, materialFlags);
               if (nodes != null)
               {
                  intersections.AddRange(nodes);
               }
            }
         }

         if (intersections.Count == 0)
         {
            return null;
         }

         return intersections;

      }

      public bool obscures(Face f)
      {
         if (myIsLeaf == false)
         {
            int[] childIds = childrenOnFace(f);
            for (int i = 0; i < 4; i++)
            {
               if (myChildren[childIds[i]].obscures(f) == false)
                  return false;
            }

            return true;
         }

         if (myMaterial.property == Material.Property.SOLID)
         {
            return true;
         }

         return false;
      }
#endregion

   };
    */
}