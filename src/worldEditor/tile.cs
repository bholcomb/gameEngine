using System;
using System.Collections.Generic;

using OpenTK.Graphics;

namespace WorldEditor
{
#region enums
   public enum HeightType
   {
      DeepWater = 1,
      ShallowWater = 2,
      Shore = 3,
      Sand = 4,
      Grass = 5,
      Forest = 6,
      Rock = 7,
      Snow = 8,
      River = 9
   }

   public enum HeatType
   {
      Coldest = 0,
      Colder = 1,
      Cold = 2,
      Warm = 3,
      Warmer = 4,
      Warmest = 5
   }

   public enum MoistureType
   {
      Wettest = 5,
      Wetter = 4,
      Wet = 3,
      Dry = 2,
      Dryer = 1,
      Dryest = 0
   }

   public enum BiomeType
   {
      //low moisture
      Ice,
      Tundra,
      Desert,

      //medium moisture
      Grassland,
      Savanna,
      TemperateSeasonalForest,
      TropicalSeasonalForest,

      //high moisture
      Taiga,
      TemperateRainforest,
      TropicalRainforest
   }

   public enum TileGroupType
   {
      Water,
      Land
   }

#endregion

   public class TileGroup
   {
      public TileGroupType myType;
      public List<Tile> myTiles;

      public TileGroup()
      {
         myTiles = new List<Tile>();
      }
   }

   public class Tile
   {
      public HeightType myHeightType;
      public HeatType myHeatType;
      public MoistureType myMoistureType;
      public BiomeType myBiomeType;

      public float myHeightValue;
      public float myHeatValue;
      public float myMoistureValue;
      public int X, Y;
      public int myBitmask;
      public int myBiomeBitmask;

      public Tile myLeft;
      public Tile myRight;
      public Tile myTop;
      public Tile myBottom;

      public bool myIsLand;
      public bool myFloodFilled;

      public Color4 myColor = Color4.Black;

      public List<River> Rivers = new List<River>();

      public int RiverSize { get; set; }

      public Tile()
      {
      }

      
      public void setElevation(float elevation)
      {
         myHeightValue = elevation;
         if (myHeightValue < WorldParameters.DeepWater)
         {
            myHeightType = HeightType.DeepWater;
            myIsLand = false;
         }
         else if (myHeightValue < WorldParameters.ShallowWater)
         {
            myHeightType = HeightType.ShallowWater;
            myIsLand = false;
         }
         else if (myHeightValue < WorldParameters.Sand)
         {
            myHeightType = HeightType.Sand;
            myIsLand = true;
         }
         else if (myHeightValue < WorldParameters.Grass)
         {
            myHeightType = HeightType.Grass;
            myIsLand = true;
         }
         else if (myHeightValue < WorldParameters.Forest)
         {
            myHeightType = HeightType.Forest;
            myIsLand = true;
         }
         else if (myHeightValue < WorldParameters.Rock)
         {
            myHeightType = HeightType.Rock;
            myIsLand = true;
         }
         else
         {
            myHeightType = HeightType.Snow;
            myIsLand = true;
         }
      }

      public void setMoisture(float moisture)
      {
         myMoistureValue = moisture;

         if (myMoistureValue < WorldParameters.DryerValue) myMoistureType = MoistureType.Dryest;
         else if (myMoistureValue < WorldParameters.DryValue) myMoistureType = MoistureType.Dryer;
         else if (myMoistureValue < WorldParameters.WetValue) myMoistureType = MoistureType.Dry;
         else if (myMoistureValue < WorldParameters.WetterValue) myMoistureType = MoistureType.Wet;
         else if (myMoistureValue < WorldParameters.WettestValue) myMoistureType = MoistureType.Wetter;
         else myMoistureType = MoistureType.Wettest;
      }

      public void setHeat(float heat)
      {
         myHeatValue = heat;

         if (myHeatValue < WorldParameters.ColdestValue) myHeatType = HeatType.Coldest;
         else if (myHeatValue < WorldParameters.ColderValue) myHeatType = HeatType.Colder;
         else if (myHeatValue < WorldParameters.ColdValue) myHeatType = HeatType.Cold;
         else if (myHeatValue < WorldParameters.WarmValue) myHeatType = HeatType.Warm;
         else if (myHeatValue < WorldParameters.WarmerValue) myHeatType = HeatType.Warmer;
         else myHeatType = HeatType.Warmest;
      }

      public void updateBiomeBitmask()
      {
         int count = 0;

         if (myIsLand && myTop.myBiomeType == myBiomeType)
            count += 1;
         if (myIsLand && myRight.myBiomeType == myBiomeType)
            count += 2;
         if (myIsLand && myBottom.myBiomeType == myBiomeType)
            count += 4;
         if (myIsLand && myLeft.myBiomeType == myBiomeType)
            count += 8;

         myBiomeBitmask = count;
      }

      public void updateBitmask()
      {
         int count = 0;

         if (myIsLand && myTop.myHeightType == myHeightType)
            count += 1;
         if (myIsLand && myRight.myHeightType == myHeightType)
            count += 2;
         if (myIsLand && myBottom.myHeightType == myHeightType)
            count += 4;
         if (myIsLand && myLeft.myHeightType == myHeightType)
            count += 8;

         myBitmask = count;
      }

      public int getRiverNeighborCount(River river)
      {
         int count = 0;
         if (myLeft != null && myLeft.Rivers.Count > 0 && myLeft.Rivers.Contains(river))
            count++;
         if (myRight != null && myRight.Rivers.Count > 0 && myRight.Rivers.Contains(river))
            count++;
         if (myTop != null && myTop.Rivers.Count > 0 && myTop.Rivers.Contains(river))
            count++;
         if (myBottom != null && myBottom.Rivers.Count > 0 && myBottom.Rivers.Contains(river))
            count++;
         return count;
      }

      public Direction getLowestNeighbor(Region region)
      {
         float left = region.getHeightValue(myLeft);
         float right = region.getHeightValue(myRight);
         float bottom = region.getHeightValue(myBottom);
         float top = region.getHeightValue(myTop);

         if (left < right && left < top && left < bottom)
            return Direction.Left;
         else if (right < left && right < top && right < bottom)
            return Direction.Right;
         else if (top < left && top < right && top < bottom)
            return Direction.Top;
         else if (bottom < top && bottom < right && bottom < left)
            return Direction.Bottom;
         else
            return Direction.Bottom;
      }

      public void setRiverPath(River river)
      {
         if (!myIsLand)
            return;

         if (!Rivers.Contains(river))
         {
            Rivers.Add(river);
         }
      }

      private void setRiverTile(River river)
      {
         setRiverPath(river);
         myHeightType = HeightType.River;
         myHeightValue = 0;
         myIsLand = false;
      }

      public void digRiver(River river, int size)
      {
         setRiverTile(river);
         RiverSize = size;

         if (size == 1)
         {
            if (myBottom != null)
            {
               myBottom.setRiverTile(river);
               if (myBottom.myRight != null) myBottom.myRight.setRiverTile(river);
            }
            if (myRight != null) myRight.setRiverTile(river);
         }

         if (size == 2)
         {
            if (myBottom != null)
            {
               myBottom.setRiverTile(river);
               if (myBottom.myRight != null) myBottom.myRight.setRiverTile(river);
            }
            if (myRight != null)
            {
               myRight.setRiverTile(river);
            }
            if (myTop != null)
            {
               myTop.setRiverTile(river);
               if (myTop.myLeft != null) myTop.myLeft.setRiverTile(river);
               if (myTop.myRight != null) myTop.myRight.setRiverTile(river);
            }
            if (myLeft != null)
            {
               myLeft.setRiverTile(river);
               if (myLeft.myBottom != null) myLeft.myBottom.setRiverTile(river);
            }
         }

         if (size == 3)
         {
            if (myBottom != null)
            {
               myBottom.setRiverTile(river);
               if (myBottom.myRight != null) myBottom.myRight.setRiverTile(river);
               if (myBottom.myBottom != null)
               {
                  myBottom.myBottom.setRiverTile(river);
                  if (myBottom.myBottom.myRight != null) myBottom.myBottom.myRight.setRiverTile(river);
               }
            }
            if (myRight != null)
            {
               myRight.setRiverTile(river);
               if (myRight.myRight != null)
               {
                  myRight.myRight.setRiverTile(river);
                  if (myRight.myRight.myBottom != null) myRight.myRight.myBottom.setRiverTile(river);
               }
            }
            if (myTop != null)
            {
               myTop.setRiverTile(river);
               if (myTop.myLeft != null) myTop.myLeft.setRiverTile(river);
               if (myTop.myRight != null) myTop.myRight.setRiverTile(river);
            }
            if (myLeft != null)
            {
               myLeft.setRiverTile(river);
               if (myLeft.myBottom != null) myLeft.myBottom.setRiverTile(river);
            }
         }

         if (size == 4)
         {

            if (myBottom != null)
            {
               myBottom.setRiverTile(river);
               if (myBottom.myRight != null) myBottom.myRight.setRiverTile(river);
               if (myBottom.myBottom != null)
               {
                  myBottom.myBottom.setRiverTile(river);
                  if (myBottom.myBottom.myRight != null) myBottom.myBottom.myRight.setRiverTile(river);
               }
            }
            if (myRight != null)
            {
               myRight.setRiverTile(river);
               if (myRight.myRight != null)
               {
                  myRight.myRight.setRiverTile(river);
                  if (myRight.myRight.myBottom != null) myRight.myRight.myBottom.setRiverTile(river);
               }
            }
            if (myTop != null)
            {
               myTop.setRiverTile(river);
               if (myTop.myRight != null)
               {
                  myTop.myRight.setRiverTile(river);
                  if (myTop.myRight.myRight != null) myTop.myRight.myRight.setRiverTile(river);
               }
               if (myTop.myTop != null)
               {
                  myTop.myTop.setRiverTile(river);
                  if (myTop.myTop.myRight != null) myTop.myTop.myRight.setRiverTile(river);
               }
            }
            if (myLeft != null)
            {
               myLeft.setRiverTile(river);
               if (myLeft.myBottom != null)
               {
                  myLeft.myBottom.setRiverTile(river);
                  if (myLeft.myBottom.myBottom != null) myLeft.myBottom.myBottom.setRiverTile(river);
               }

               if (myLeft.myLeft != null)
               {
                  myLeft.myLeft.setRiverTile(river);
                  if (myLeft.myLeft.myBottom != null) myLeft.myLeft.myBottom.setRiverTile(river);
                  if (myLeft.myLeft.myTop != null) myLeft.myLeft.myTop.setRiverTile(river);
               }

               if (myLeft.myTop != null)
               {
                  myLeft.myTop.setRiverTile(river);
                  if (myLeft.myTop.myTop != null) myLeft.myTop.myTop.setRiverTile(river);
               }
            }
         }
      }

   }
}