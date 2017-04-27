using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Util;
using UI;
using Events;
using Terrain;

namespace Editor
{
   public class EdgeMode: Mode
   {
      Node activeNode = null;
      int activeVertex = -1;
      int activeEdge = -1;

      public EdgeMode(Editor e)
         : base(e, "Edge Mode")
      {
      }

      public override void onGui()
      {
         if (ImGui.hoveredWindow != null)
            return;

         if (ImGui.mouse.wheelDelta != 0.0)
         {
            if (myEditor.context.currentEdge == -1 && myEditor.context.currentVert == -1 && myEditor.context.currentFace != Face.NONE)
            {
               AdjustFaceCommand cmd = new AdjustFaceCommand(myEditor.context.currentLocation, myEditor.context.currentFace, (int)ImGui.mouse.wheelDelta);
               myEditor.world.dispatch(cmd);
            }
            else if (myEditor.context.currentEdge != -1 && myEditor.context.currentVert == -1 && myEditor.context.currentFace != Face.NONE)
            {
               AdjustEdgeCommand cmd = new AdjustEdgeCommand(myEditor.context.currentLocation, myEditor.context.currentEdge, myEditor.context.currentFace, (int)ImGui.mouse.wheelDelta);
               myEditor.world.dispatch(cmd);
            }
            else if (myEditor.context.currentEdge != -1 && myEditor.context.currentVert != -1)
            {
               AdjustVertCommand cmd = new AdjustVertCommand(myEditor.context.currentLocation, myEditor.context.currentEdge, myEditor.context.currentVert, (int)ImGui.mouse.wheelDelta);
               myEditor.world.dispatch(cmd);
            }
         }
      }
   }
}