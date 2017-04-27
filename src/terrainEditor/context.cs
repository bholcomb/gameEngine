using System;
using System.Collections.Generic;

using OpenTK;

using Terrain;
using Util;

namespace Editor
{
   public class Context
   {
      public Context()
      {

      }

      public Node currentNode { get; set; }
      public Terrain.Face currentFace { get; set; }
      public Terrain.Face previousFace { get; set; }
      public int currentEdge { get; set; }
      public int currentVert { get; set; }
      public List<NodeLocation> selectedNodes { get; set; }
      public int currentSelectionDepth { get; set; }
      public NodeLocation currentLocation { get; set; }
      public String currentMaterial { get; set; }
   }
}