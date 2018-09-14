using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Util;
using GUI;
using Engine;
using Terrain;

namespace Editor
{
   public class FaceMode : Mode
   {
      public FaceMode(Editor e)
         : base(e, "Face Mode")
      {
      }

      public override void onGui()
      {
         if (UI.hoveredWindow != null)
            return;


         if (UI.mouse.wheelDelta != 0.0)
         {
            if (UI.keyboard.keyPressed(Key.ShiftLeft) == true)
            {
               if (UI.mouse.wheelDelta > 0)
                  createMultiBlocks();
               else
                  removeMutliBlock();
            }
            else
            {
               myEditor.cursorDepth -= (int)UI.mouse.wheelDelta;
               if (myEditor.cursorDepth < 0) myEditor.cursorDepth = 0;
               if (myEditor.cursorDepth > WorldParameters.theMaxDepth) myEditor.cursorDepth = WorldParameters.theMaxDepth;
            }
         }
      }


      public void createMultiBlocks()
      {
         //get current material
         UInt32 mat = MaterialManager.getMaterialIndex(myEditor.context.currentMaterial);
         List<NodeLocation> newNodes = new List<NodeLocation>();

         foreach (NodeLocation n in myEditor.context.selectedNodes)
         {
            NodeLocation nl = n;
            nl = nl.getNeighborLocation(myEditor.context.currentFace);
            newNodes.Add(nl);

            //send create blocks cmd
            AddBlockCommand cmd = new AddBlockCommand(nl, mat);
            myEditor.world.dispatch(cmd);
         }

         myEditor.context.selectedNodes.Clear();
         myEditor.context.selectedNodes.AddRange(newNodes);
      }

      public void removeMutliBlock()
      {
         List<NodeLocation> newNodes = new List<NodeLocation>();
         foreach (NodeLocation n in myEditor.context.selectedNodes)
         {
            NodeLocation nl = n;
            //determine the opposite face
            int faceId=(int)myEditor.context.currentFace;
            if ((faceId & 0x1) == 1)
               faceId--;
            else
               faceId++;

            newNodes.Add(nl.getNeighborLocation((Face)faceId));
            //send delete blocks command
            DeleteBlockCommand cmd = new DeleteBlockCommand(nl);
            myEditor.world.dispatch(cmd);
         }

         myEditor.context.selectedNodes.Clear();
         myEditor.context.selectedNodes.AddRange(newNodes);
      }
   }
}