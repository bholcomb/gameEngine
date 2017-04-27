using System;
using System.Collections.Generic;

using OpenTK;

using Util;

namespace Terrain
{
   public abstract class TerrainCommand
   {
      public NodeLocation myLocation;
      public Chunk myChunk;
      public byte[] myPreviousState;

      public TerrainCommand(NodeLocation loc)
      {
         myLocation = loc;
      }

      public abstract bool execute(World w);

      public virtual void saveState(World w, NodeLocation loc)
      {
         myChunk = w.findOrCreateChunk(loc.chunk);
         if (myChunk != null)
         {
            myPreviousState = myChunk.serialize();
         }
      }

      public virtual bool undo(World w)
      {
         if(myChunk!=null && myPreviousState!=null)
         {
            myChunk.deserialize(myPreviousState);
            return true;
         }

         return false;
      }
   };

   public class AddBlockCommand : TerrainCommand
   {
      UInt32 myMaterial;

      public AddBlockCommand(NodeLocation loc, UInt32 matIndex)
         : base(loc)
      {
         myMaterial = matIndex;
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            myChunk.setNodeMaterial(myLocation.node, myMaterial);
            return true;
         }

         return false;
      }
   }

   public class DeleteBlockCommand : TerrainCommand
   {
      public DeleteBlockCommand(NodeLocation loc)
         : base(loc)
      {
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            myChunk.deleteNode(myLocation.node);
            return true;
         }

         return false;
      }
   }

   public class AssignMaterialCommand : TerrainCommand
   {
      string myMaterialName;

      public AssignMaterialCommand(NodeLocation loc, String name)
         : base(loc)
      {
         myMaterialName = name;
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            UInt32 m = MaterialManager.getMaterialIndex(myMaterialName);
            myChunk.setNodeMaterial(myLocation.node, m);
            return true;
         }

         return false;
      }
   }

   public class AdjustVertCommand : TerrainCommand
   {
      int myEdge;
      int myVert;
      int myAmount;

      public AdjustVertCommand(NodeLocation loc, int edge, int vert, int amount)
         : base(loc)
      {
         myEdge = edge;
         myVert = vert;
         myAmount = amount;
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            myChunk.adjustVert(myLocation.node, myEdge, myVert, myAmount);
            return true;
         }

         return false;
      }
   }

   public class AdjustEdgeCommand : TerrainCommand
   {
      int myEdge;
      Face myFace;
      int myAmount;

      public AdjustEdgeCommand(NodeLocation loc, int edge, Face face, int amount)
         : base(loc)
      {
         myEdge = edge;
         myFace = face;
         myAmount = amount;
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            myChunk.adjustEdge(myLocation.node, myEdge, myFace, myAmount);
            return true;
         }

         return false;
      }
   }

   public class AdjustFaceCommand : TerrainCommand
   {
      Face myFace;
      int myAmount;

      public AdjustFaceCommand(NodeLocation loc, Face face, int amount)
         : base(loc)
      {
         myFace = face;
         myAmount = amount;
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            myChunk.adjustFace(myLocation.node, myFace, myAmount);
            return true;
         }

         return false;
      }
   }

   public class SplitBlockCommand : TerrainCommand
   {
      public SplitBlockCommand(NodeLocation loc)
         : base(loc)
      {
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            myChunk.splitNode(myLocation.node);
            return true;
         }

         return false;
      }
   }

   public class JoinBlockCommand : TerrainCommand
   {
      public JoinBlockCommand(NodeLocation loc)
         : base(loc)
      {
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            myChunk.joinNode(myLocation.node);
            return true;
         }

         return false;
      }
   }

   public class ResetBlockCommand : TerrainCommand
   {
      public ResetBlockCommand(NodeLocation loc)
         : base(loc)
      {
      }

      public override bool execute(World w)
      {
         saveState(w, myLocation);
         if (myChunk != null)
         {
            myChunk.resetNode(myLocation.node);
            return true;
         }

         return false;
      }
   }
}