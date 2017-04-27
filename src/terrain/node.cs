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
   public enum Face { 
      RIGHT,  // +X
      LEFT,   // -X
      TOP,    // +Y
      BOTTOM, // -Y
      BACK,   // +Z
      FRONT,  // -Z
      NONE
   };

   public enum EdgeEnd { START, END };

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

   public class Node
   {
      #region "static helper data"
      public static Vector3[] theChildOffsets = new Vector3[] {
         new Vector3(0,0,0),
         new Vector3(1,0,0),
         new Vector3(0,1,0),
         new Vector3(1,1,0),
         new Vector3(0,0,1),
         new Vector3(1,0,1),
         new Vector3(0,1,1),
         new Vector3(1,1,1)};

      public static Vector3[] theFaceNormals = new Vector3[]{ 
         new Vector3(1,0,0),   //+X  RIGHT
         new Vector3(-1,0,0),  //-X  LEFT
         new Vector3(0,1,0),   //+Y  TOP
         new Vector3(0,-1, 0), //-Y  BOTTOM
         new Vector3(0,0,1),   //+Z  BACK
         new Vector3(0,0,-1)}; //-Z  FRONT

      public static int[][] theFaceIndexes = new int[][]
               { 
                  new int[] {5,1,3,7},    //+X  RIGHT
                  new int[] {0,4,6,2},    //-X  LEFT
                  new int[] {6,7,3,2},    //+Y  TOP
                  new int[] {0,1,5,4},    //-Y  BOTTOM
                  new int[] {4,5,7,6},    //+Z  BACK
                  new int[] {1,0,2,3}     //-Z  FRONT
               };

      public static int[][] theEdgeIndexes = new int[][]
      {
         //these are from start to end
         new int[] {4,5},
         new int[] {6,7},
         new int[] {2,3},
         new int[] {0,1},
         new int[] {5,7},
         new int[] {4,6},
         new int[] {0,2},
         new int[] {1,3},
         new int[] {1,5},
         new int[] {3,7},
         new int[] {2,6},
         new int[] {0,4}
      };

      public static int[][] theVertexEdges = new int[][]
         {
            //These are edge ids in order of z,y,x axis of influence
            //if vertex id's binary rep has a 1 set for that axis (also read ZYX)
            //then that an edge end, if it is a 0, then that is an edge start
            //vertex 0 has all edges going away from it (it's the start of all its connected edges)
            //vertex 7 has all edges coming into it (it's the end of all its connected edges)
            new int[] {11, 6, 3},
            new int[] {8, 7, 3},
            new int[] {10, 6, 2},
            new int[] {9, 7, 2},
            new int[] {11, 5, 0},
            new int[] {8, 4, 0},
            new int[] {10, 5, 1},
            new int[] {9, 4, 1}
         };

      public static int[][][] theAdjoiningEdges = new int[][][]
      {
         //these are the edges that are connected to the edge used as the index for a particular face
         //positive numbers means its the start of the edge, negative means its the end of the edge
         //attached edges in order of what connects to the start/end point of the edge in question
         new int [][] {    //+X  RIGHT
            new int[] {}, //edge 0
            new int[] {}, //edge 1
            new int[] {}, //edge 2
            new int[] {}, //edge 3
            new int[] {-8, -9}, //edge 4
            new int[] {}, //edge 5
            new int[] {}, //edge 6
            new int[] {8, 9}, //edge 7
            new int[] {7, 4}, //edge 8
            new int[] {-7, -4}, //edge 9
            new int[] {}, //edge 10
            new int[] {}, //edge 11
         }, 
          new int [][] {    //-X LEFT
            new int[] {}, //edge 0
            new int[] {}, //edge 1
            new int[] {}, //edge 2
            new int[] {}, //edge 3
            new int[] {}, //edge 4
            new int[] {-11, -10}, //edge 5
            new int[] {11, 10}, //edge 6
            new int[] {}, //edge 7
            new int[] {}, //edge 8
            new int[] {}, //edge 9
            new int[] {-6, -5}, //edge 10
            new int[] {6, 5}, //edge 11
         }, 
         new int [][] {       //+Y  TOP
            new int[] {}, //edge 0
            new int[] {-10, -9}, //edge 1
            new int[] {10, 9}, //edge 2
            new int[] {}, //edge 3
            new int[] {}, //edge 4
            new int[] {}, //edge 5
            new int[] {}, //edge 6
            new int[] {}, //edge 7
            new int[] {}, //edge 8
            new int[] {-2, -1}, //edge 9
            new int[] {2, 1}, //edge 10
            new int[] {}, //edge 11
         },
         new int [][] {       //-Y  BOTTOM
            new int[] {-11, -8}, //edge 0
            new int[] {}, //edge 1
            new int[] {}, //edge 2
            new int[] {11, 8}, //edge 3
            new int[] {}, //edge 4
            new int[] {}, //edge 5
            new int[] {}, //edge 6
            new int[] {}, //edge 7
            new int[] {-3, 0}, //edge 8
            new int[] {}, //edge 9
            new int[] {}, //edge 10
            new int[] {3, 0}, //edge 11
         },
         new int [][] {    //+Z  BACK
            new int[] {5, 4}, //edge 0
            new int[] {-5, -4}, //edge 1
            new int[] {}, //edge 2
            new int[] {}, //edge 3
            new int[] {0, -1}, //edge 4
            new int[] {0, 1}, //edge 5
            new int[] {}, //edge 6
            new int[] {}, //edge 7
            new int[] {}, //edge 8
            new int[] {}, //edge 9
            new int[] {}, //edge 10
            new int[] {}, //edge 11
         },
         new int [][] {    //-Z  FRONT
            new int[] {}, //edge 0
            new int[] {}, //edge 1
            new int[] {-6, -7}, //edge 2
            new int[] {6, 7}, //edge 3
            new int[] {}, //edge 4
            new int[] {}, //edge 5
            new int[] {3, 2}, //edge 6
            new int[] {-3, -2}, //edge 7
            new int[] {}, //edge 8
            new int[] {}, //edge 9
            new int[] {}, //edge 10
            new int[] {}, //edge 11
         }
      };

      public static int[][] theFaceEdges = new int[][]{
                  new int[] {0, 3, 2, 1},    //+X  RIGHT
                  new int[] {0, 3, 2, 1},    //-X  LEFT
                  new int[] {4, 5, 6, 7},    //+Y  TOP
                  new int[] {4, 5, 6, 7},    //-Y  BOTTOM
                  new int[] {8, 9, 10, 11},    //+Z  BACK
                  new int[] {8, 9, 10, 11}     //-Z  FRONT
      };

      public static int[][] theChildrenOnFace = new int[][]{
                  new int[] {1, 5, 3, 7},    //+X  RIGHT
                  new int[] {0, 2, 4, 6},    //-X  LEFT
                  new int[] {2, 3, 6, 7},    //+Y  TOP
                  new int[] {0, 1, 4, 5},    //-Y  BOTTOM
                  new int[] {4, 5, 6, 7},    //+Z  BACK
                  new int[] {0, 1, 2, 3}     //-Z  FRONT
      };

      public static String visiblityFlagsString(byte flags)
      {
         String ret = "";

         if ((flags & (1 << (byte)Face.RIGHT)) != 0) ret += "Right | ";
         if ((flags & (1 << (byte)Face.LEFT)) != 0) ret += "Left | ";
         if ((flags & (1 << (byte)Face.TOP)) != 0) ret += "Top | ";
         if ((flags & (1 << (byte)Face.BOTTOM)) != 0) ret += "Bottom | ";
         if ((flags & (1 << (byte)Face.FRONT)) != 0) ret += "Front | ";
         if ((flags & (1 << (byte)Face.BACK)) != 0) ret += "Back | ";

         if (ret == "")
            ret = "None";
         else
            ret = ret.Substring(0, ret.Length - 3); //trim off the trailing | sign         

         return ret;
      }

      #endregion

      public Chunk myChunk = null;

      //node info
      public Node myParent = null;
      public Node[] myChildren = null;

      //leaf information
      public Material myMaterial;
      public byte[] myEdgeSpans = new byte[12];
      public byte myFaceVisibilty = 0x3f;

      //spatial info
      public NodeKey myKey = new NodeKey(1);
      public Vector3 myLocation;
      public float mySize = 0;

      //calculated values
      public Vector3 min { get; set; }  //world space
      public Vector3 max { get; set; }  //world space

      public Node(Chunk chunk)
      {
         myChunk = chunk;
         myMaterial = MaterialManager.defaultMaterial;
         myLocation = Vector3.Zero;
      }

      public bool isLeaf { get { return myChildren == null; } }

      public UInt32 materialId
      {
         get { return myMaterial.id; }
         set
         {
            myMaterial = MaterialManager.getMaterial(value);
            if (isLeaf == false)
            {
               for (int i = 0; i < 8; i++)
               {
                  myChildren[i].materialId = value;
               }
            }
         }
      }

      public NodeLocation location
      {
         get
         {
            return new NodeLocation(myChunk.chunkKey, myKey);
         }
      }

      public float size
      {
         get { return mySize; }
      }

      public int depth
      {
         get
         {
            return myKey.depth;
         }
      }

      public bool isSolidCube()
      {
         bool ret = true;
         for (int i = 0; i < 12; i++)
         {
            if (myEdgeSpans[i] != (byte)0x00)
               ret = false;
         }

         return ret;
      }

      public bool isTransparent()
      {
         return (myMaterial.property & Material.Property.SOLID) == 0;
      }

      public bool isVisible()
      {
         return myFaceVisibilty != 0;
      }

#region editing
      public void reset()
      {
         if (myChildren != null)
         {
            for (int i = 0; i < 8; i++)
            {
               myChildren[i].reset();
            }
            myChildren = null;
         }

         for (int i = 0; i < 12; i++)
         {
            myEdgeSpans[i] = 0x00;
         }

         myMaterial = MaterialManager.defaultMaterial;
      }

      public void relax()
      {
         //can't relax something without children
         if (isLeaf == true)
            return;

         //depth first attempt at relaxing nodes
         for (int i = 0; i < 8; i++)
         {
            myChildren[i].relax();
         }

         //are all the children leaves?
         for (int i = 0; i < 8; i++)
         {
            if (myChildren[i].isLeaf == false)
               return;
         }

         //now see if we can combine them
         //check children for being leaves and having the same type
         UInt32 testMaterial = myChildren[0].materialId;
         for (int i = 0; i < 8; i++)
         {
            if (myChildren[i].materialId != testMaterial)
               return;
         }

         //remove the children
         myChildren = null;

         //combine and become a leaf
         materialId = testMaterial;
      } 

      public Node findChild(int i)
      {
         //stupid check
         if (myChildren == null)
            return null;

         //may return null
         return myChildren[i];
      }

      public void removeChild(int i)
      {
         //stupid check
         if (myChildren == null)
            return;

         myChildren[i] = null;
      }

      public void uniformSplit(int depth)
      {
         if (myKey.depth >= depth || myKey.depth == WorldParameters.theMaxDepth)
            return;

         if (isLeaf == true)
         {
            split();
         }

         for (int i = 0; i < 8; i++)
         {
            myChildren[i].uniformSplit(depth);
         }
      }

      public void split()
      {
         if (isLeaf == false) return;
         if (myKey.depth == WorldParameters.theMaxDepth) return;

         //so I am a leaf and can split.  Make children
         myChildren = new Node[8];
         for (int i = 0; i < 8; i++)
         {
            myChildren[i] = new Node(myChunk);
            myChildren[i].myKey = myKey.createChildKey(i);
            myChildren[i].myParent = this;
            myChildren[i].mySize = mySize / 2;
            myChildren[i].myLocation = myLocation + (theChildOffsets[i] * (mySize / 2));
            myChildren[i].min = min + (theChildOffsets[i] * (mySize / 2));
            myChildren[i].max = min + (theChildOffsets[i] * (mySize / 2)) + new Vector3(mySize / 2);
            myChildren[i].myMaterial = myMaterial;
         }
      }

      public void join()
      {
         if (isLeaf == true) return;

         //determine material to use.  each child gets to vote.
         Dictionary<UInt32, int> mats = new Dictionary<UInt32, int>();
         mats.Add(materialId, 1);
         for(int i=0; i<8; i++)
         {
            if (mats.ContainsKey(myChildren[i].materialId) == false)
               mats.Add(myChildren[i].materialId, 1);
            else
               mats[myChildren[i].materialId]++;
         }

         UInt32 useMat = 0;
         int count=0;
         foreach (KeyValuePair<UInt32, int> mat in mats)
         {
            if (mat.Value > count)
            {
               useMat = mat.Key;
               count = mat.Value;
            }
         }

         //TODO:  figure out if the edges can be saved
         for (int i = 0; i < 8; i++)
         {
            //do something here to preserve any edges we can
         }

         //delete the children
         myChildren = null;

         materialId = useMat;
      }

      public float edgeStart(int index)
      {
         int start = myEdgeSpans[index];
         start = start >> 4;
         return (float)start;
      }

      public float edgeStop(int index)
      {
         int stop = myEdgeSpans[index];
         stop = stop & 0xf;
         return (float)stop;
      }

      public void setEdgeStart(int index, int value)
      {
         if (value < 0) value = 0;
         if (value > 15) value = 15;

         int edge = myEdgeSpans[index];
         int stop = edge & 0xf;
         int start = edge >> 4;

         //this keeps the ends from crossing each other
         //since the start and end distance are measured from their 
         //relative ends of the edge, when they are at the same point,
         //their combined value will be 15, so if their sum is more than 
         //that they have crossed and we shouldn't let that happen
         //so we reduce stop, effectively pushing it back
         while (value + stop > 15)
         {
            stop--;
            if (stop < 0)
            {
               //can't move it any further, so just stop
               return;
            }
         }

         value = value << 4;
         value += stop;
         myEdgeSpans[index]=(byte)value;
      }

      public void setEdgeStop(int index, int value)
      {
         if (value < 0) value = 0;
         if (value > 15) value = 15;

         int edge = myEdgeSpans[index];
         int start = edge >> 4;
         int stop = edge & 0xf;

         //see comment in previous function
         while (value + start  > 15)
         {
            start--;
            if (start < 0)
            {
               //can't move it any further, so just stop
               return;
            }
         }

         start = start << 4;
         start += value;
         myEdgeSpans[index] = (byte)start;
      }

      public Material.Property property()
      {
         return myMaterial.property;
      }

      public bool obscures(Face f)
      {
         if (isLeaf == false)
         {
            int[] childIds = childrenOnFace(f);
            for (int i = 0; i < 4; i++)
            {
               if (myChildren[childIds[i]].obscures(f) == false)
                  return false;
            }

            return true;
         }

         if ((myMaterial.property & Material.Property.SOLID) != 0)
         {
            return true;
         }

         return false;
      }

      public bool isFaceVisible(Face f)
      {
         NodeLocation neighborLoc = location.getNeighborLocation(f);
         Node neighbor = myChunk.world.findNode(neighborLoc, World.FindCriteria.CONTAINS);
         if (neighbor == null)
         {
            //unknown neighbor, could be visible, might not.  We should do something here
            return false;
         }

         if (neighbor.isLeaf == true)
         {
            //just use the neighbor's material property if we can see through it
            if ((neighbor.property() & Material.Property.SOLID) == 0)
            {
               return true;
            }
         }
         else //gotta ask all the children on that side of the neighbor touching this face
         {
            return !neighbor.obscures(oppositeFace(f));
         }

         //default to invisible
         return false;
      }

      public void updateVisibility()
      {
         myFaceVisibilty = 0;

         if (isLeaf == false)
         {
            for (int i = 0; i < 8; i++)
            {
               myChildren[i].updateVisibility();
            }

            //update the non-leaf nodes with visibility status based on children
            bool allSolid = true;
            bool allSame = true;
            UInt32 matId = myChildren[0].materialId;
            for (int i = 0; i < 8; i++)
            {
               if (myChildren[i].myMaterial.property != Material.Property.SOLID)
                  allSolid = false;

               if (myChildren[i].materialId != matId)
                  allSame = false;
            }

            if (allSolid && allSame)
            {
               myMaterial = myChildren[0].myMaterial;
            }
            else if (allSolid)
            {
               myMaterial = MaterialManager.compositeSolidMaterial;
            }
            else
            {
               myMaterial = MaterialManager.compositeTransparentMaterial;
            }
         }
         else
         {
            switch (myMaterial.property)
            {
               case Material.Property.AIR:
                  {
                     return;
                  }
               case Material.Property.WATER:
                  {
                     NodeLocation neighbor = location.getNeighborLocation(Face.TOP);
                     Node n = myChunk.world.findNode(neighbor);
                     if (n == null || n.isTransparent() == true) //only render water when there is nothing or transparent stuff above it
                     {
                        myChunk.visibleFaceCount++;
                        myFaceVisibilty = 1 << (int)Face.TOP;
                     }
                  }
                  break;
               case Material.Property.TRANSPARENT:  //fallthrough
               case Material.Property.SOLID:
                  {
                     for (int i = 0; i < 6; i++)
                     {
                        if(isFaceVisible((Face)i)==true)
                        {
                           //a visible face
                           myChunk.visibleFaceCount++;
                           myFaceVisibilty |= (Byte)(1 << i);
                        }
                     }
                  }
                  break;
            }

            //if at least 1 face is visible, then we are a visible node
            if (myFaceVisibilty != 0)
            {
               myChunk.visibleNodeCount++;
            }
         }
      }
#endregion

#region static indexing functions
      public static int[] faceIndices(int faceId)
      {
         return theFaceIndexes[faceId];
      }

      public static int[] edgeIndices(int edge)
      {
         return theEdgeIndexes[edge];
      }

      public static int[] vertexEdges(int vertexId)
      {
         return theVertexEdges[vertexId];
      }

      public static int[] attachedEdges(Face f, int edge)
      {
         return theAdjoiningEdges[(int)f][edge];
      }

      public static int[] attachedEdges(Face f)
      {
         return theFaceEdges[(int)f];
      }

      public static int[] childrenOnFace(Face f)
      {
         return theChildrenOnFace[(int)f];
      }

      public static Face oppositeFace(Face f)
      {
         switch(f)
         {
            case Face.LEFT: return Face.RIGHT;
            case Face.RIGHT: return Face.LEFT;
            case Face.TOP: return Face.BOTTOM;
            case Face.BOTTOM: return Face.TOP;
            case Face.FRONT: return Face.BACK;
            case Face.BACK: return Face.FRONT;
         }

         return Face.NONE;
      }
#endregion

#region intersections
      public bool contains(Vector3 worldLoc)
      {
         return Intersections.pointInAABox(min, max, worldLoc);
      }

      public bool contains(NodeLocation nl)
      {
         return contains(nl.worldLocation());
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
            if (ret.isLeaf == true)
               break;

            ret = ret.myChildren[child];
         }

         return ret;
      }

      public Node findNodeContaining(NodeKey nk)
      {
         if (nk.myValue == myKey.myValue)
            return this;

         Node n = this;
         foreach (int child in nk.childPath())
         {
            Node temp = n.findChildNode(child);
            if (temp == null)
            {
               return n;
            }
            n = temp;
         }

         return n;
      }

      public Node getOrCreateNode(NodeKey nk)
      {
         if (nk.myValue == myKey.myValue)
            return this;

         Node n = this;
         foreach (int child in nk.childPath())
         {
            n = n.getOrCreateChild(child);
         }

         return n;
      }

      public Node getOrCreateChild(int child)
      {
         if (isLeaf == true)
            split();

         return myChildren[child];
      }

      public Node findNode(NodeKey nk)
      {
         if (nk.myValue == myKey.myValue)
            return this;

         Node n = this;
         foreach (int child in nk.childPath())
         {
            n = n.findChildNode(child);
            if (n == null)
            {
               return null;
            }
         }

         return n;
      }

      public Node findChildNode(int child)
      {
         if (isLeaf == true)
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
         hit.location = Distance.closestPointOn(new AABox(min, max), sphereCenter);
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
         if (isLeaf == true && myMaterial.property == Material.Property.AIR)
            return null;

         float t;
         if (Intersections.rayBoxIntersection(r, t0, t1, min, max, out t) == false)
            return null;

         List<NodeHit> intersections = new List<NodeHit>();

         //since we passed the hit test and we are a leaf, just return us
         if (isLeaf == true)
         {
            if ((myMaterial.property & materialFlags) != Material.Property.UNKNOWN)
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
         if (isLeaf == true && myMaterial.property == Material.Property.AIR)
            return null;

         Vector3 hitNormal;
         float hitDepth;
         if (Intersections.AABoxSphereIntersection(new AABox(min, max), sphereCenter, radius, out hitNormal, out hitDepth) == false)
            return null;

         List<NodeHit> intersections = new List<NodeHit>();

         //since we passed the hit test and we are a leaf, just return us
         if (isLeaf == true)
         {
            if ((myMaterial.property & materialFlags) != Material.Property.UNKNOWN)
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

#endregion

#region rendering

      public bool faceVisible(int face)
      {
         int bit = 1 << face;
         if ((myFaceVisibilty & bit) != 0)
            return true;

         return false;
      }

      public Vector3[] generateVerts()
      {
         Vector3[] verts = new Vector3[8];
         float edgeStep = myKey.size() / 16.0f;

         for (int i = 0; i < 8; i++)
         {
            //get the base position
            verts[i] = myKey.location + (theChildOffsets[i] * mySize);

            int[] edges = vertexEdges(i);

            //the index of the vert is also a binary 3 digit key for 
            //which end of the edge influences the vert
            //a 0 means the beginning and a 1 means the end of edge
            //pushes/pulls the vert

            // bits are in order z, y, x
            if ((i & 0x1) != 0)
               verts[i].X -= edgeStep * edgeStop(edges[2]);
            else
               verts[i].X += edgeStep * edgeStart(edges[2]);

            if ((i & 0x2) != 0)
               verts[i].Y -= edgeStep * edgeStop(edges[1]);
            else
               verts[i].Y += edgeStep * edgeStart(edges[1]);

            if ((i & 0x4) != 0)
               verts[i].Z -= edgeStep * edgeStop(edges[0]);
            else
               verts[i].Z += edgeStep * edgeStart(edges[0]);
         }

         return verts;
      }

      public Vector3 localVertex(int faceId, int vertId, ref Vector3[] verts)
      {
         int[] index = faceIndices(faceId);
         int vertOffset = index[vertId];
         return verts[vertOffset];
      }

      public Vector2 localUv(int faceId, int vertId, ref Vector3[] verts)
      {
         Vector3 vert = localVertex(faceId, vertId, ref verts);
         Vector2 uv = new Vector2();
         switch (faceId)
         {
            case 0:  //+X
            case 1:  //-X
               {
                  uv.X = vert.Z / myChunk.mySize;
                  uv.Y = vert.Y / myChunk.mySize;
                  break;
               }
            case 2:  //+Y
            case 3:  //-Y
               {
                  uv.X = vert.X / myChunk.mySize;
                  uv.Y = vert.Z / myChunk.mySize;
                  break;
               }
            case 4:  //+Z
            case 5:  //-Z
               {
                  uv.X = vert.X / myChunk.mySize;
                  uv.Y = vert.Y / myChunk.mySize;
                  break;
               }
         }

         return uv;
      }

      public int localMaterial(int faceId)
      {
         if (faceId == (int)Face.TOP)
            return myMaterial.top;

         if (faceId == (int)Face.BOTTOM)
            return myMaterial.bottom;

         return myMaterial.side;
      }

//       public void renderOutline(ref List<RenderCommand> cmds)
//       {
//          if (isLeaf == false)
//          {
//             for (int i = 0; i < 8; i++)
//             {
//                myChildren[i].renderOutline(ref cmds);
//             }
//          }
//          else
//          {
//             if (isVisible() == true)
//             {
//                RenderCommand cmd = new RenderOffsetCubeCommand(min, mySize, Color4.Black);
//                cmd.renderState.depthTest.enabled = true;
//                cmd.renderState.polygonOffset.enableType = PolygonOffset.EnableType.FILL;
//                cmd.renderState.polygonOffset.factor = -1.0f;
//                cmd.renderState.polygonOffset.units = 1.0f;
//                cmds.Add(cmd);
//             }
//          }
//       }

#endregion

#region serialization
      //this only returns the count of non-air leaves
      public int leafCount()
      {
         if (isLeaf == true)
         {
            if (myMaterial.property == Material.Property.AIR)
               return 0;
            else
               return 1;
         }

         int count = 0;
         for (int i = 0; i < 8; i++)
         {
            count += myChildren[i].leafCount();
         }

         return count;
      }

      public int leafCount(Material.Property pass)
      {
         if (isLeaf == true)
         {
            if (myMaterial.property == pass)
               return 1;
            else
               return 0;
         }

         int count = 0;
         for (int i = 0; i < 8; i++)
         {
            count += myChildren[i].leafCount(pass);
         }

         return count;
      }

      public int calcSize()
      {
         int size = 0;
         int lc = leafCount();

         size += 4; //for the key
         size += 4; //material id
         size += 12; //edge span info
         size += 1; //face visibility flags

         return size * lc;
      }

      public void serialize(ref BinaryWriter writer)
      {
         if (isLeaf)
         {
            if (myMaterial.property != Material.Property.AIR)
            {
               writer.Write(myKey.myValue);
               writer.Write(materialId);
               writer.Write(myEdgeSpans);
               writer.Write(myFaceVisibilty);
            }
         }
         else
         {
            for (int i = 0; i < 8; i++)
            {
               myChildren[i].serialize(ref writer);
            }
         }
      }

#endregion
   };
}