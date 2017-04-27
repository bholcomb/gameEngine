using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Util
{
   public class OctreeElement<T> where T : class
   {
      public int myNodeId;
      public uint mySize;
      public uint myX, myY, myZ;
      public T myObject;

      public uint distanceBetween(OctreeElement<T> e2)
      {
         //Manhattan distance is cheaper but at a precision loss.
         //It is consistent though, so it should still be ok
         Vector3 p1 = new Vector3(myX, myY, myZ);
         Vector3 p2 = new Vector3(e2.myX, e2.myY, e2.myZ);
         return (uint)(p1 - p2).LengthFast;
      }

      public bool intersects(Ray r, float t0, float t1)
      {
         //algorithm from "Realtime Collision detection" by Christer Ericson
         Vector3 center = new Vector3(myX, myY, myZ);
         Vector3 m = r.myOrigin - center;
         float b = Vector3.Dot(m, r.myDirection);
         float c = Vector3.Dot(m, m) - mySize * mySize;

         if (c > 0 && b > 0) return false;
         float discr = b * b - c;

         if (discr < 0) return false;

         float t = -b - (float)Math.Sqrt(discr);

         if (t < 0) t = 0;

         //check if it's on the segment
         if (t < t0 || t > t1) return false;

         //point of intersection if you care
         //point=r.myOrigin + (t * r.myDirection);

         return true;
      }
   }

   public class OctreeElementList<T> : IEnumerable<T> where T : class
   {
      internal T[] myList;
      internal int[] myInUseList;
      const int theCount = 5;
      internal int myIterPos;

      public OctreeElementList()
      {
         myList = new T[theCount];
         myInUseList = new int[theCount];

         for (int i = 0; i < myList.Length; i++)
         {
            myList[i] = null;
            myInUseList[i] = 0;
         }

      }

      public void Dispose()
      {

      }

      public void Add(T item)
      {
         for (int i = 0; i < myInUseList.Length; i++)
         {
            //if the inuse flag is false, set to 1 in an atomic operations
            int ret = Interlocked.CompareExchange(ref myInUseList[i], 1, 0);
            if (ret == 0)
            {
               myList[i] = item;
               return;
            }
         }

         throw new Exception("Ran out of room in node list");
      }

      public void Remove(T item)
      {
         for (int i = 0; i < myList.Length; i++)
         {
            if (myList[i] == item)
            {
               myInUseList[i] = 0;
               myList[i] = null;
            }
         }
      }

      public void Clear()
      {
         for (int i = 0; i < myList.Length; i++)
         {
            myList[i] = null;
            myInUseList[i] = 0;
            myIterPos = 0;
         }
      }

      #region IEnumerable methods (NOT THREAD SAFE)
      public IEnumerator<T> GetEnumerator()
      {
         for (int i = 0; i < myInUseList.Length; i++)
         {
            if (myInUseList[i] != 0)
            {
               yield return myList[i];
            }
         }
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         throw new NotImplementedException();
      }

      #endregion
   }

   public class OctreeNode<T> where T : class
   {
      //internal List<OctreeElement<T>> myElements = new List<OctreeElement<T>>();
      //internal ConcurrentDictionary<T, OctreeElement<T>> myElements = new ConcurrentDictionary<T, OctreeElement<T>>();
      internal OctreeElementList<OctreeElement<T>> myElements = new OctreeElementList<OctreeElement<T>>();
      internal int myId = 0;
      internal int myParent = 0;
      internal int[] myChildren = new int[8];
      internal bool myIsLeaf = true;
      internal bool myInUse = false;
      internal int myDepth = 0;
      internal Octree<T> myOctree = null;
      internal int myCount = 0;

      internal Vector3 myPosition = new Vector3();
      internal uint mySize = 0;
      internal uint myMinSize = 8;

      public OctreeNode(Octree<T> oct)
      {
         myOctree = oct;

         for (int i = 0; i < 8; i++)
         {
            myChildren[i] = -1;
         }
      }

      public int count
      {
         get { return myCount; }
      }
      public bool isLeaf()
      {
         return myIsLeaf;
      }

      public int[] children
      {
         get { return myChildren; }
      }

      public uint size
      {
         get { return mySize; }
      }

      public Vector3 position
      {
         get { return myPosition; }
      }

      public OctreeElementList<OctreeElement<T>> elements
      {
         get { return myElements; }
      }

      public void getLeafs(ref List<int> leafs)
      {
         if (isLeaf() == true)
         {
            leafs.Add(myId);
            return;
         }

         for (int i = 0; i < 8; i++)
         {
            if (myChildren[i] != -1)
               myOctree.myNodes[myChildren[i]].getLeafs(ref leafs);
         }
      }

      public void insert(OctreeElement<T> e)
      {
         sbyte child = myOctree.getKey(e.myX, e.myY, e.myZ, e.mySize, myDepth);

         //if it's too big for the child or if this is the deepest we go
         if (child == -1)
         {
            myElements.Add(e);
            e.myNodeId = myId;
            System.Threading.Interlocked.Increment(ref myCount);
            return;
         }

         //insert in the child
         //create a new child node if it doesn't exist yet
         int id = myChildren[child];
         if (id == -1)
         {
            addChild(child);
         }

         myOctree.myNodes[myChildren[child]].insert(e);
         System.Threading.Interlocked.Increment(ref myCount);
      }

      public void addChild(int child)
      {
         myIsLeaf = false;
         int id;
         id = myOctree.nextAvailableNodeId();

         //use compare and swap to ensure that somebody else isn't adding the same child
         //this is doing this assignment with thread interuption fallback myChildren[child] = id;
         int ret = Interlocked.CompareExchange(ref myChildren[child], id, -1);
         if (ret != -1) //oops somebody else switched it out already-ok, use their child node, so keep going
         {
            //return the allocated node to the system
            myOctree.myNodes[id].reset();
            return;
         }

         myOctree.myNodes[id].myDepth = myDepth + 1;
         myOctree.myNodes[id].myParent = myId;
         myOctree.myNodes[id].mySize = mySize / 2;

         //calcuate the new center of the child cube
         Vector3 newPos = new Vector3(myPosition);
         if ((child & 0x4) != 0)
            newPos.X += mySize / 4;
         else
            newPos.X -= mySize / 4;
         if ((child & 0x2) != 0)
            newPos.Y += mySize / 4;
         else
            newPos.Y -= mySize / 4;
         if ((child & 0x1) != 0)
            newPos.Z += mySize / 4;
         else
            newPos.Z -= mySize / 4;

         myOctree.myNodes[id].myPosition = newPos;
      }

      public void remove(OctreeElement<T> e)
      {
         myElements.Remove(e);
         decreaseCount();
      }

      public void decreaseCount()
      {
         System.Threading.Interlocked.Decrement(ref myCount);
         if (myCount == 0)
         {
            myOctree.myEmptyNodeIndexQueue.Enqueue(myId);
         }

         //work all the way back up to root
         if (myParent != -1)
         {
            //decrement the count of the entities in the given node
            myOctree.myNodes[myParent].decreaseCount();
         }
      }
      #region intersections
      public bool containsSphere(Vector3 pos, float radius)
      {
         float dist = 0;
         float radiusSquared = radius * radius;
         float emin, emax;
         Vector3 size = new Vector3(mySize / 2);
         Vector3 min = new Vector3(myPosition) - size;
         Vector3 max = new Vector3(myPosition) + size;

         //x
         emin = pos.X - min.X;
         emax = pos.X - max.X;
         if (emin < 0)
         {
            if (emin < -radius) return false;
            dist += emin * emin;
         }
         else if (emax > 0)
         {
            if (emax > radius) return false;
            dist += emax * emax;
         }

         //y
         emin = pos.Y - min.Y;
         emax = pos.Y - max.Y;
         if (emin < 0)
         {
            if (emin < -radius) return false;
            dist += emin * emin;
         }
         else if (emax > 0)
         {
            if (emax > radius) return false;
            dist += emax * emax;
         }

         //z
         emin = pos.Z - min.Z;
         emax = pos.Z - max.Z;
         if (emin < 0)
         {
            if (emin < -radius) return false;
            dist += emin * emin;
         }
         else if (emax > 0)
         {
            if (emax > radius) return false;
            dist += emax * emax;
         }

         if (dist <= radiusSquared) return true;

         return false;
      }

      public bool containsElement(OctreeElement<T> e)
      {
         float halfSize = mySize / 2.0f;
         float maxX = myPosition.X + halfSize;
         float maxY = myPosition.Y + halfSize;
         float maxZ = myPosition.Z + halfSize;

         float dx = maxX - e.myX;
         float dy = maxY - e.myY;
         float dz = maxZ - e.myZ;

         if (dx < 0 || dx > mySize) return false;
         if (dy < 0 || dy > mySize) return false;
         if (dz < 0 || dz > mySize) return false;

         return true;
      }

      public bool intersect(Ray r, float t0, float t1)
      {
         //this algorithm is from "An efficient and Robust Ray-Box Intersection Algorithm"
         //Amy Williams, Steve Barrus, R. Keith Morley, Pete Shirley
         //University of Utah, unknown date
         //http://people.csail.mit.edu/amy/papers/box-jgt.ps

         Vector3 size = new Vector3(mySize / 2);
         Vector3[] bounds = new Vector3[2] { new Vector3(myPosition) - size, new Vector3(myPosition) + size };

         float tmin, tmax, tymin, tymax, tzmin, tzmax;

         tmin = (bounds[r.mySigns[0]].X - r.myOrigin.X) * r.myInvDirection.X;
         tmax = (bounds[1 - r.mySigns[0]].X - r.myOrigin.X) * r.myInvDirection.X;

         tymin = (bounds[r.mySigns[1]].Y - r.myOrigin.Y) * r.myInvDirection.Y;
         tymax = (bounds[1 - r.mySigns[1]].Y - r.myOrigin.Y) * r.myInvDirection.Y;

         if ((tmin > tymax) || (tymin > tmax))
            return false;
         if (tymin > tmin)
            tmin = tymin;
         if (tymax < tmax)
            tmax = tymax;

         tzmin = (bounds[r.mySigns[2]].Z - r.myOrigin.Z) * r.myInvDirection.Z;
         tzmax = (bounds[1 - r.mySigns[2]].Z - r.myOrigin.Z) * r.myInvDirection.Z;

         if ((tmin > tzmax) || (tzmin > tmax))
            return false;
         if (tzmin > tmin)
            tmin = tzmin;
         if (tzmax < tmax)
            tmax = tzmax;

         return ((tmin < t1) && (tmax > t0));
      }

      public List<int> rayIntersection(Ray r, float t0, float t1)
      {
         if (intersect(r, t0, t1) == false)
            return null;

         List<int> intersections = new List<int>();
         foreach (int i in myChildren)
         {
            if (i != -1)
            {
               List<int> nodes = myOctree.myNodes[i].rayIntersection(r, t0, t1);
               if (nodes != null)
               {
                  intersections.AddRange(nodes);
               }
            }
         }

         if (intersections.Count == 0)
         {
            intersections.Add(myId);
         }

         return intersections;
      }

      public int nodeContainingSphere(Vector3 pos, float radius)
      {
         int res = -1;

         if (containsSphere(pos, radius) == true)
         {
            res = myId;

            for (int i = 0; i < 8; i++)
            {
               int id = myChildren[i];
               if (id != -1)
               {
                  res = myOctree.myNodes[id].nodeContainingSphere(pos, radius);
                  if (res != -1)
                  {
                     return res;
                  }
               }
            }
         }

         return res;
      }

      public OctreeElement<T> nearestNeighborWithin(OctreeElement<T> e, ref uint distance)
      {
         OctreeElement<T> nearest = null;

         if (isLeaf() == false)
         {
            for (int i = 0; i < 8; i++)
            {
               int childId = myChildren[i];
               if (childId != -1)
               {
                  uint tempDistance = distance;
                  OctreeElement<T> tempNearest;
                  tempNearest = myOctree.myNodes[childId].nearestNeighborWithin(e, ref tempDistance);
                  if (tempNearest != e && tempDistance < distance)
                  {
                     distance = tempDistance;
                     nearest = tempNearest;
                  }
               }
            }
         }
         else
         {
            //check the elements stored in this node (assuming it's a leaf)
            foreach (OctreeElement<T> el in myElements)
            {
               if (el == e) continue;

               uint tempDistance = e.distanceBetween(el);
               if (tempDistance < distance)
               {
                  distance = tempDistance;
                  nearest = el;
               }
            }
         }

         return nearest;
      }
      #endregion

      public void reset()
      {
         myElements.Clear();

         for (int i = 0; i < 8; i++)
         {
            int id = myChildren[i];
            if (id != -1)
            {
               myOctree.myNodes[id].reset();
            }
            myChildren[i] = -1;
         }

         myParent = -1;

         if (myCount != 0)
         {
            throw new Exception("Error with reset");
         }
         myCount = 0;
         myDepth = 0;
         myInUse = false;
         mySize = 0;
         myIsLeaf = true;
         myPosition = Vector3.Zero;


         //add this node back to the pool of unused nodes
         myOctree.myFreeNodeIndexQueue.Enqueue(myId);
      }

#if false
      public void debugRender(ref V3[] verts, ref uint vi, ref uint[] index, ref uint ii)
      {
         //size check
         if (vi >= ((1024 * 1024) - 8) || ii >= ((1024 * 1024 * 2) - 16))
         {
            System.Console.WriteLine("Ran out of verts for octree.  Needs more than 1M verts");
            return;
         }

         float size = (float)mySize / 2.0f;

         //add the verts
         verts[vi + 0].Position = myPosition + (size * new Vector3(-1, -1, 1));
         verts[vi + 1].Position = myPosition + (size * new Vector3(1, -1, 1));
         verts[vi + 2].Position = myPosition + (size * new Vector3(1, 1, 1));
         verts[vi + 3].Position = myPosition + (size * new Vector3(-1, 1, 1));

         verts[vi + 4].Position = myPosition + (size * new Vector3(-1, -1, -1));
         verts[vi + 5].Position = myPosition + (size * new Vector3(1, -1, -1));
         verts[vi + 6].Position = myPosition + (size * new Vector3(1, 1, -1));
         verts[vi + 7].Position = myPosition + (size * new Vector3(-1, 1, -1));

         //add the indices
         index[ii + 0] = vi; index[ii + 1] = (uint)(vi + 1); index[ii + 2] = (uint)(vi + 2); index[ii + 3] = (uint)(vi + 3);
         index[ii + 4] = (uint)(vi + 5); index[ii + 5] = (uint)(vi + 4); index[ii + 6] = (uint)(vi + 7); index[ii + 7] = (uint)(vi + 6);
         index[ii + 8] = (uint)(vi + 0); index[ii + 9] = (uint)(vi + 1); index[ii + 10] = (uint)(vi + 5); index[ii + 11] = (uint)(vi + 4);
         index[ii + 12] = (uint)(vi + 3); index[ii + 13] = (uint)(vi + 2); index[ii + 14] = (uint)(vi + 6); index[ii + 15] = (uint)(vi + 7);

         //update the count
         vi += 8;
         ii += 16;

         //add the children
         for (int i = 0; i < 8; i++)
         {
            if (myChildren[i] != -1)
            {
               myOctree.myNodes[myChildren[i]].debugRender(ref verts, ref vi, ref index, ref ii);
            }
         }
      }
#endif
   }

   public class Octree<T> where T : class
   {
      internal List<OctreeNode<T>> myNodes = new List<OctreeNode<T>>();
      internal ConcurrentQueue<int> myFreeNodeIndexQueue = new ConcurrentQueue<int>();
      internal ConcurrentQueue<int> myEmptyNodeIndexQueue = new ConcurrentQueue<int>();
      int myRelevantBits;
      int myMaxDepth;
      uint mySize;
      uint myMinNodeSize;
      int myMinNodeSizeBits;

#if false
      //for debug rendering-initialize during first use, not during creation
      VertexBufferObject<V3> myVbo = null;
      IndexBufferObject myIbo = null;
#endif

      public Octree(uint size, uint minNodeSize, int nodeCacheSize)
      {
         findRelavantBits(size, minNodeSize);
         myNodes.Capacity = nodeCacheSize;
         for (int i = 0; i < nodeCacheSize; i++)
         {
            OctreeNode<T> node = new OctreeNode<T>(this);
            node.myId = i;
            myNodes.Add(node);
         }

         reset();
      }

      public void findRelavantBits(uint size, uint minNodeSize)
      {
         //find the value that has the next highest bit set that won't be used, ensuring that the relevant bits are correct and not wasted
         double val = OpenTK.MathHelper.NextPowerOfTwo((double)size);
         uint intVal = (uint)val;

         //size of the octree
         mySize = intVal;

         myRelevantBits = 0;
         while ((intVal & 1) == 0)
         {
            intVal = intVal >> 1;
            myRelevantBits++;
         }

         //find the number of bits used for the minimimum size
         val = OpenTK.MathHelper.NextPowerOfTwo((double)minNodeSize);
         intVal = (uint)val;
         myMinNodeSize = intVal;

         while ((intVal & 1) == 0)
         {
            intVal = intVal >> 1;
            myMinNodeSizeBits++;
         }

         myMaxDepth = myRelevantBits - myMinNodeSizeBits;
      }

      public sbyte getKey(uint x, uint y, uint z, uint size, int depth)
      {
         uint X, Y, Z;
         sbyte result = 0;
         int shift = myRelevantBits - depth - 1;

         //deep as we can go
         if (depth >= myMaxDepth)
         {
            return -1;
         }

         //will this size object fit in this cube
         uint maxSize = (uint)(1 << shift);
         if (size > maxSize)
         {
            return -1;
         }

         //shift the bits to get the most significant bit in the top most position
         X = x >> shift;
         Y = y >> shift;
         Z = z >> shift;

         //only care about the rightmost bit
         X = X & 0x01;
         Y = Y & 0x01;
         Z = Z & 0x01;

         //check for if the bit is set
         if (X != 0) result += 4;
         if (Y != 0) result += 2;
         if (Z != 0) result += 1;

         return result;
      }

      public int nextAvailableNodeId()
      {
         int i;
         myFreeNodeIndexQueue.TryDequeue(out i);
         return i;
      }

      public void reset()
      {
         //clean out the free node queue
         int j;
         while (myEmptyNodeIndexQueue.TryDequeue(out j)) { }

         for (int i = 0; i < myNodes.Count; i++)
         {
            myNodes[i].reset();
         }

         //set the root node to valid
         int rootNode;
         myFreeNodeIndexQueue.TryDequeue(out rootNode);

         myNodes[rootNode].myId = 0;
         myNodes[rootNode].myDepth = 0;
         myNodes[rootNode].myParent = -1;
         myNodes[rootNode].myInUse = true;
         myNodes[rootNode].mySize = mySize;
         myNodes[rootNode].myPosition = new Vector3(mySize / 2);
      }

      public void cleanUnusedNodes()
      {
         int i;
         while (myEmptyNodeIndexQueue.TryDequeue(out i))
         {
            if (myNodes[i].count == 0 && myNodes[i].myInUse!=false)
            {
               myNodes[i].reset();
            }
         }
      }

      public void insert(OctreeElement<T> e)
      {
         myNodes[0].insert(e);
      }

      public void remove(OctreeElement<T> e)
      {
         myNodes[e.myNodeId].remove(e);
      }

      public void update(OctreeElement<T> e)
      {
         if (myNodes[e.myNodeId].containsElement(e) == false)
         {
            remove(e);
            insert(e);
         }
      }

      #region "Spatial Queries"
      public OctreeNode<T> root()
      {
         return myNodes[0];
      }

      public OctreeNode<T> node(int index)
      {
         return myNodes[index];
      }

      public OctreeElement<T> nearestNeighbor(OctreeElement<T> e)
      {
         //set minimum distance to the maximum possible distance to start with
         uint minDist = 0xffffffff;

         int id = e.myNodeId;
         //while the current node only holds one element (assumed to be the one were finding the nearest neighbor to)
         //loop up and find the parent node
         while (myNodes[id].myCount == 1)
         {
            id = myNodes[id].myParent;
            //damn, hit the root node
            if (id == -1)
            {
               //couldn't find another node with any other children
               //this means there's only 1 element in the entire octree
               return null;
            }
         }

         //actually just getting the min Distance to the nearest neighbor in the
         //current node branch, but we'll need to refine this by using this as the min distance heuristic
         //and check the entire octree again using this distance
         OctreeElement<T> nn = nearestNeighborInNode(id, e, ref minDist);

         ////determine node that holds a sphere with point e.xyz and radius minDistance in octree
         ////and determine nearest neighbors within that node
         //Vector3 pos=new Vector3(e.myX, e.myY, e.myZ);
         //int secondId = nodeContainingSphere(pos, minDist);

         //if (secondId != -1 && secondId!=id)
         //{
         //   //search that node for the nearest neighbor
         //   nn = nearestNeighborInNode(secondId, e, ref minDist);
         //}

         return nn;
      }

      public List<OctreeElement<T>> neighborsWithin(OctreeElement<T> e, uint distance)
      {
         Vector3 pos = new Vector3(e.myX, e.myY, e.myZ);
         int id = myNodes[0].nodeContainingSphere(pos, distance);

         List<OctreeElement<T>> actualList = new List<OctreeElement<T>>();

         foreach (OctreeElement<T> el in myNodes[id].myElements)
         {
            if (e.distanceBetween(el) < distance)
            {
               actualList.Add(el);
            }
         }

         return actualList;
      }

      public OctreeElement<T> nearestNeighborInNode(int id, OctreeElement<T> e, ref uint minDist)
      {
         return myNodes[id].nearestNeighborWithin(e, ref minDist);
      }

      public int nodeContainingSphere(Vector3 pos, float radius)
      {
         return myNodes[0].nodeContainingSphere(pos, radius);
      }

      public List<OctreeElement<T>> rayIntersection(Ray r, float t0, float t1)
      {
         List<int> containingNodes = myNodes[0].rayIntersection(r, t0, t1);
         List<OctreeElement<T>> hits = new List<OctreeElement<T>>();

         if (containingNodes == null) return hits;

         foreach (int i in containingNodes)
         {
            if (myNodes[i].count != 0)
            {
               foreach (OctreeElement<T> element in myNodes[i].myElements)
               {
                  if (element.intersects(r, t0, t1))
                  {
                     hits.Add(element);
                  }
               }
            }
         }

         return hits;
      }

      #endregion

      #region debug
#if false
      public RenderCommand debugRender()
      {
         if (myVbo == null)
         {
            myVbo = new VertexBufferObject<V3>(BufferUsageHint.DynamicDraw);
            myIbo = new IndexBufferObject(BufferUsageHint.DynamicDraw);
         }

         uint vi = 0;
         uint ii = 0;
         V3[] verts = new V3[1024 * 1024];
         uint[] index = new uint[1024 * 1024 * 2];

         myNodes[0].debugRender(ref verts, ref vi, ref index, ref ii);

         myVbo.setData(verts, (int)vi);
         myIbo.setData(index, (int)ii);
         RenderVboCommand cmd = new RenderVboCommand(myVbo, myIbo, Color4.Blue, PrimitiveType.Quads);

         return cmd;
      }
#endif

      public float utilization()
      {
         int total = myNodes.Count;
         int notUsed = myFreeNodeIndexQueue.Count;

         float ret= (1.0f-((float)notUsed / (float)total)) * 100.0f;
         if (ret < 0.0f)
         {
            throw new Exception("Stupid error");
         }

         return ret;
      }

      #endregion
   }
}