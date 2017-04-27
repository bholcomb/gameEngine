using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Graphics;
using Terrain;
using Util;
using Events;
using UI;

namespace Editor
{
   public class BlockMode : Mode
   {
      public BlockMode(Editor e)
         : base(e, "Block Mode")
      {
      }

      public override void onGui()
      {
         if (ImGui.hoveredWindow != null)
            return;

         if (ImGui.mouse.buttonReleased[(int)MouseButton.Left] == true && ImGui.keyboard.keyPressed(Key.ShiftLeft) == false)
         {
            createBlock();
         }

         else if (ImGui.mouse.buttonReleased[(int)MouseButton.Left] == true && ImGui.keyboard.keyPressed(Key.ShiftLeft) == true)
         {
            removeBlock();
         }

         else if (ImGui.mouse.buttonReleased[(int)MouseButton.Left] == true &&
            ImGui.keyboard.keyPressed(Key.ShiftLeft) == true && 
            ImGui.keyboard.keyPressed(Key.ControlLeft) == true)
         {
            resetBlock();
         }

         if (ImGui.mouse.wheelDelta != 0.0f)
         {
            if (ImGui.keyboard.keyPressed(Key.ControlLeft) == true)
            {
               if (ImGui.mouse.wheelDelta > 0)
                  splitBlock();
               else
                  joinBlock();
            }
            else
            {

					myEditor.cursorDepth += ImGui.mouse.wheelDelta < 0 ? -1 : 1;
               if (myEditor.cursorDepth < 0) myEditor.cursorDepth = 0;
               if (myEditor.cursorDepth > WorldParameters.theMaxDepth) myEditor.cursorDepth = WorldParameters.theMaxDepth;

            }
         }
      }

      public void createBlock()
      {
         //get current material
         UInt32 mat = Terrain.MaterialManager.getMaterialIndex(myEditor.context.currentMaterial);

         //get block selection
         NodeLocation nl = myEditor.context.currentLocation;
         if (nl != null)
         {
            nl = nl.getNeighborLocation(myEditor.context.currentFace);

            //send create blocks cmd
            AddBlockCommand cmd = new AddBlockCommand(nl, mat);
            myEditor.world.dispatch(cmd);
         }
      }

      

      public void removeBlock()
      {
         //get block selection
         NodeLocation nl = myEditor.context.currentLocation;
         if (nl != null)
         {
            //send create blocks cmd
            DeleteBlockCommand cmd = new DeleteBlockCommand(nl);
            myEditor.world.dispatch(cmd);
         }
      }

      public void splitBlock()
      {
         //get block selection
         NodeLocation nl = myEditor.context.currentLocation;
         if (nl != null)
         {
            //send create blocks cmd
            SplitBlockCommand cmd = new SplitBlockCommand(nl);
            myEditor.world.dispatch(cmd);
         }
      }

      public void joinBlock()
      {
         //get block selection
         NodeLocation nl = myEditor.context.currentLocation;
         if (nl != null)
         {
            //send create blocks cmd
            JoinBlockCommand cmd = new JoinBlockCommand(nl);
            myEditor.world.dispatch(cmd);
         }
      }

      public void resetBlock()
      {
         //get block selection
         NodeLocation nl = myEditor.context.currentLocation;
         if (nl != null)
         {
            //send create blocks cmd
            ResetBlockCommand cmd = new ResetBlockCommand(nl);
            myEditor.world.dispatch(cmd);
         }
      }
   }
}