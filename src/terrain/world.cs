using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;
using Physics;

namespace Terrain
{
   public static class WorldParameters
   {
      public static float theRegionSize = 1024.0f;
      public static float theMaxElevation = 512.0f; //this gives 255m above sea level and below
      public static float theNodeSize = 0.1f; //10 centimeters
      public static int theMaxDepth = 10;  //this allows for a node to be morton-key identified using 32 bits 
      public static float theChunkSize; //with a default node size of 0.1m and a depth of 10, this is 102.4m
      public static float theLeafRatio;
      public static UInt32 theMaxUnits; // the number of cubes to a side at max depth
      public static Vector3 theBias = Vector3.Zero; //the "origin" of the world, may change when camera is very far away from this point, rebias the world to closer to the camera
      public static String theDataPath = "../data/terrain/chunks";
      public static int theNodeCount = 32;
      public static float theWorldSize;

      static WorldParameters()
      {
         theMaxUnits = (UInt32)(1 << theMaxDepth);
         theChunkSize = theNodeSize * theMaxUnits;
         theLeafRatio = 1.0f / theNodeSize;
         theWorldSize = theRegionSize * theChunkSize;
      }

      public static float sizeAtDepth(int depth)
      {
         //clamp for sanity
         if (depth < 0) depth = 0;
         if (depth > theMaxDepth) depth = theMaxDepth;

         return theChunkSize / (1 << depth);
      }

      public static UInt32 unitsAtDepth(int depth)
      {
         //clamp for sanity
         if (depth < 0) depth = 0;
         if (depth > theMaxDepth) depth = theMaxDepth;

         //essentially the same thing as (1 << theMaxDepth) / (1 << depth) but with only 1 shift and no division
         return (UInt32)1 << (theMaxDepth - depth);
      }
   }
   
   //a world is the uncompressed version of the terrain database used for rendering and simulation
   //also a centralized point for terrain generation and material parameters
   public class World
   {
		public delegate void ChunkDelegate(Chunk chunkKey);
		
      public Dictionary<UInt64, Chunk> myChunks = new Dictionary<UInt64, Chunk>();
      
      TerrainSource myTerrainSource;
      TerrainPager myPager;
    
      LinkedList<TerrainCommand> myUndoStack = new LinkedList<TerrainCommand>();

		public ChunkDelegate chunkAdded;
		public ChunkDelegate chunkRemoved;
		public Action chunksReset;

      public World()
      {
         
      }

      public Dictionary<UInt64, Chunk> chunks { get { return myChunks; } }
      public TerrainCache database { get { return myTerrainSource.chunkCache; } }
      public int pendingChunks { get { return myTerrainSource.requestCount; } }
      public TerrainSource terrainSource { get { return myTerrainSource; } }

      public Int32 nodeCount { get; protected set; }

      public void init(Initializer init)
      {
         MaterialManager.init(true);

         String source = init.findDataOr("terrain.source", "file");
         switch (source)
         {
            case "file":
               myTerrainSource = new LocalFileTerrainSource(init, this);
               break;
            case "generated":
               myTerrainSource = new LocalGeneratedTerrainSource(init, this);
               break;
            case "remote":
               myTerrainSource = new RemoteTerrainSource(init, this);
               break;
         }

         myPager = new TerrainPager(this);
      }

      public void shutdown()
      {
         myTerrainSource.shutdown();
      }

      public void reset()
      {
			if (chunksReset != null)
				chunksReset();

         myChunks.Clear();
         MaterialManager.reset();
         myTerrainSource.reset();
      }

      public void loadWorld(String worldName)
      {
         reset();
         myTerrainSource.loadWorld(worldName);
      }

		public void newWorld()
		{
			reset();
         /*
			Chunk chunk;

			int numChunks = 50;
			for (int i = 0; i < numChunks; i++)
			{
				for (int j = 0; j < numChunks; j++)
				{
					int iLoc = i - numChunks / 2;
					int jLoc = j - numChunks / 2;
					//create ground chunk
					chunk = new Chunk(new Vector3(WorldParameters.theChunkSize * iLoc, -WorldParameters.theChunkSize, WorldParameters.theChunkSize * jLoc));
					chunk.setNodeMaterial(new NodeKey(1), MaterialManager.getMaterialIndex("dirt"));
					addChunk(chunk);
					myTerrainSource.chunkCache.updateChunk(chunk);

					//create air chunk above it
					chunk = new Chunk(new Vector3(-WorldParameters.theChunkSize * iLoc, 0, WorldParameters.theChunkSize * jLoc));
					chunk.setNodeMaterial(new NodeKey(1), MaterialManager.getMaterialIndex("air"));
					addChunk(chunk);
					myTerrainSource.chunkCache.updateChunk(chunk);
				}
			}
         */
		}

      public TerrainPager pager { get {return myPager;} }

      public void setInterest(Vector3 pos)
      {
         myPager.setInterest(pos);
      }

      public void tick(double time)
      {
         //tick the pager
         myPager.tick();
         myTerrainSource.tick();
      }

      public void addChunk(Chunk chunk)
      {
         chunk.world = this;
         myChunks.Add(chunk.key, chunk);
         nodeCount += chunk.nodeCount;
         if (chunkAdded != null)
         {
            chunkAdded(chunk);
         }
      }

		public void removeChunk(UInt64 key)
		{
         Chunk c = null;
         myChunks.TryGetValue(key, out c);
         if(c != null)
         {
            if (chunkRemoved != null)
            {
               chunkRemoved(c);
            }

            myChunks.Remove(key);
         }
		}

      public NodeHit getNodeIntersection(Ray r, float t0, float t1, Material.Property materialFlags=Material.Property.ALL)
      {
         List<NodeHit> hits = new List<NodeHit>();
         foreach (Chunk chunk in myChunks.Values)
         {
            List<NodeHit> chunkHits = chunk.intersect(r, t0, t1, materialFlags);
            if (chunkHits != null)
            {
               hits.AddRange(chunkHits);
            }
         }

         //sort the list based on distance from ray
         hits.Sort((x, y) => (x.location - r.myOrigin).Length.CompareTo((y.location - r.myOrigin).Length));

         //get the first one hit if one exists
         if (hits.Count > 0)
            return hits[0];
         else
            return null;
      }

      public NodeHit getNodeIntersection(Vector3 sphereCenter, float radius, Material.Property materialFlags = Material.Property.ALL)
      {
         List<NodeHit> hits = new List<NodeHit>();
         foreach (Chunk chunk in myChunks.Values)
         {
            List<NodeHit> chunkHits = chunk.intersect(sphereCenter, radius, materialFlags);
            if (chunkHits != null)
            {
               hits.AddRange(chunkHits);
            }
         }

         //sort the list based on distance from ray
         hits.Sort((x, y) => (x.location - sphereCenter).Length.CompareTo((y.location - sphereCenter).Length));

         //get the first one hit if one exists
         if (hits.Count > 0)
            return hits[0];
         else
            return null;
      }

      public List<Chunk> findNeighbors(Chunk chunk, ChunkKey.Neighbor neighbors)
      {
         List<Chunk> ret = new List<Chunk>();
         ChunkKey k = chunk.chunkKey;
         if ((neighbors & ChunkKey.Neighbor.LEFT)  == ChunkKey.Neighbor.LEFT)  ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.LEFT)));
         if ((neighbors & ChunkKey.Neighbor.RIGHT) == ChunkKey.Neighbor.RIGHT) ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.RIGHT)));
         if ((neighbors & ChunkKey.Neighbor.BOTTOM)== ChunkKey.Neighbor.BOTTOM)ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.BOTTOM)));
         if ((neighbors & ChunkKey.Neighbor.TOP)   == ChunkKey.Neighbor.TOP)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.TOP)));
         if ((neighbors & ChunkKey.Neighbor.FRONT) == ChunkKey.Neighbor.FRONT) ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.FRONT)));
         if ((neighbors & ChunkKey.Neighbor.BACK)  == ChunkKey.Neighbor.BACK)  ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.BACK)));
         if ((neighbors & ChunkKey.Neighbor.LBF)   == ChunkKey.Neighbor.LBF)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.LBF)));
         if ((neighbors & ChunkKey.Neighbor.LB )   == ChunkKey.Neighbor.LB )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.LB )));
         if ((neighbors & ChunkKey.Neighbor.LBB)   == ChunkKey.Neighbor.LBB)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.LBB)));
         if ((neighbors & ChunkKey.Neighbor.BF )   == ChunkKey.Neighbor.BF )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.BF )));
         if ((neighbors & ChunkKey.Neighbor.BB )   == ChunkKey.Neighbor.BB )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.BB )));
         if ((neighbors & ChunkKey.Neighbor.RBF)   == ChunkKey.Neighbor.RBF)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.RBF)));
         if ((neighbors & ChunkKey.Neighbor.RB )   == ChunkKey.Neighbor.RB )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.RB )));
         if ((neighbors & ChunkKey.Neighbor.RBB)   == ChunkKey.Neighbor.RBB)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.RBB)));
         if ((neighbors & ChunkKey.Neighbor.LF )   == ChunkKey.Neighbor.LF )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.LF )));
         if ((neighbors & ChunkKey.Neighbor.BL )   == ChunkKey.Neighbor.BL )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.BL )));
         if ((neighbors & ChunkKey.Neighbor.RF )   == ChunkKey.Neighbor.RF )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.RF )));
         if ((neighbors & ChunkKey.Neighbor.BR )   == ChunkKey.Neighbor.BR )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.BR )));
         if ((neighbors & ChunkKey.Neighbor.LTF)   == ChunkKey.Neighbor.LTF)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.LTF)));
         if ((neighbors & ChunkKey.Neighbor.LT )   == ChunkKey.Neighbor.LT )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.LT )));
         if ((neighbors & ChunkKey.Neighbor.LTB)   == ChunkKey.Neighbor.LTB)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.LTB)));
         if ((neighbors & ChunkKey.Neighbor.TF )   == ChunkKey.Neighbor.TF )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.TF )));
         if ((neighbors & ChunkKey.Neighbor.TB )   == ChunkKey.Neighbor.TB )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.TB )));
         if ((neighbors & ChunkKey.Neighbor.RTF)   == ChunkKey.Neighbor.RTF)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.RTF)));
         if ((neighbors & ChunkKey.Neighbor.RT )   == ChunkKey.Neighbor.RT )   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.RT )));
         if ((neighbors & ChunkKey.Neighbor.RTB)   == ChunkKey.Neighbor.RTB)   ret.Add(findChunk(k.neighbor(ChunkKey.Neighbor.RTB)));
         return ret;                        
      }                                     

      public void regenCurrentChunk()
      {
         ChunkKey ck = new ChunkKey(myPager.interestPoint);
         myChunks.Remove(ck.myKey);
         myTerrainSource.forceRebuild(ck.myKey);
      }

      public Chunk findChunk(ChunkKey chunkKey)
      {
         Chunk chunk = null;
         if (myChunks.TryGetValue(chunkKey.myKey, out chunk) == true)
         {
            //Warn.print("Cannot find chunk");
         }

         return chunk;
      }
     
      public Chunk findOrCreateChunk(ChunkKey chunkKey)
      {
         Chunk chunk=findChunk(chunkKey);
         if(chunk==null)
         {
            chunk = new Chunk(chunkKey.myLocation);
            addChunk(chunk);
            myTerrainSource.chunkCache.updateChunk(chunk);
         }

         return chunk;
      }

      public Material.Property propertyAt(NodeLocation loc)
      {
         Chunk chunk = findChunk(loc.chunk);
         if (chunk != null)
         {
            Node n = chunk.findNodeContaining(loc.node);
            if(n != null)
               return n.property();
         }

         return Material.Property.UNKNOWN;
      }

      public enum FindCriteria { EXACT, CONTAINS };
      public Node findNode(NodeLocation loc, FindCriteria criteria=FindCriteria.EXACT)
      {
         Chunk chunk = findChunk(loc.chunk);
         if (chunk != null)
         {
            Node n = null;
            switch (criteria)
            {
               case FindCriteria.EXACT: n = chunk.findNode(loc.node); break;
               case FindCriteria.CONTAINS: n = chunk.findNodeContaining(loc.node); break;
            }
            
            return n;
         }

         return null;
      }

      public Node findNode(Vector3 worldLocation)
      {
         UInt64 key = ChunkKey.createKeyFromWorldLocation(worldLocation);
         Chunk chunk;
         if (myChunks.TryGetValue(key, out chunk) == false)
         {
            return null;
         }

         return chunk.findNodeContaining(worldLocation);
      }

      public void addUndo(TerrainCommand cmd)
      {
         /* TODO: figure out a better way of commands to figure out if they actually need to be stored
         if (myUndoStack.Count > 0)
         {
            //check if this command has actually changes the chunk
            TerrainCommand prevCmd = myUndoStack.Last.Value;

            //could be the same 
            if (cmd.myPreviousState.Length == prevCmd.myPreviousState.Length)
            {
               bool different = false;
               for (int i = 0; i < cmd.myPreviousState.Length; i++)
               {
                  if (cmd.myPreviousState[i] != prevCmd.myPreviousState[i])
                  {
                     different = true;
                     break;
                  }
               }

               if (different == false)
               {
                  //no sense in storing the same exact state twice
                  return;
               }
            }
         }
          */

         myUndoStack.AddLast(cmd);

         while (undoUsage > 1024 * 1024 * 100) //more than 100 mb of undo memory used
         {
            myUndoStack.RemoveFirst();
         }
      }

      public void dispatch(TerrainCommand cmd)
      {
         if (cmd.execute(this) == true)
         {
            addUndo(cmd);
            myTerrainSource.chunkCache.updateChunk(cmd.myChunk);
            
         }
      }

      public void undoLastCommand()
      {
         if (myUndoStack.Count == 0)
            return;

         TerrainCommand cmd = myUndoStack.Last.Value;
         myUndoStack.RemoveLast();
         if (cmd.undo(this) == true)
         {
            myTerrainSource.chunkCache.updateChunk(cmd.myChunk);
         }
      }

      public int undoUsage
      {
         get
         {
            int usage = 0;
            foreach (TerrainCommand tc in myUndoStack)
            {
               usage += tc.myPreviousState.Length;
            }

            return usage;
         }
      }
   };
}