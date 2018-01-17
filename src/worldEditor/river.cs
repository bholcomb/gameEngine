using System;
using System.Collections.Generic;

namespace WorldEditor
{
   public enum Direction
   {
      Left,
      Right,
      Top,
      Bottom
   }


   public class RiverGroup
   {
      public List<River> Rivers = new List<River>();
   }

   public class River
   {
      public int myLength;
      public List<Tile> myTiles;
      public int myId;

      public int Intersections;
      public float TurnCount;
      public Direction CurrentDirection;

      public River(int id)
      {
         myId = id;
         myTiles = new List<Tile>();
      }

      public void AddTile(Tile tile)
      {
         tile.setRiverPath(this);
         myTiles.Add(tile);
      }
   }
}
