using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK;

using Noise;
using Graphics;
using Terrain;

namespace WorldEditor
{
   public class Region
   {
      public Tile[,] myTiles;
      int myX;
      int myY;

      public Texture myElevationMap;
      public Texture myHeatMap;
      public Texture myMoistureMap;
      public Texture myBiomeMap;

      World myWorld;
      Generator myGenerator;

      protected List<TileGroup> myWaters = new List<TileGroup>();
      protected List<TileGroup> myLands = new List<TileGroup>();
      protected List<River> myRivers = new List<River>();
      protected List<RiverGroup> myRiverGroups = new List<RiverGroup>();

      public Region(World w, int locX, int locY)
      {
         myWorld = w;
         myGenerator = w.myGenerator;
         myX = locX;
         myY = locY;

         myTiles = new Tile[WorldParameters.theRegionSize, WorldParameters.theRegionSize];
         for (int y = 0; y < WorldParameters.theRegionSize; y++)
         {
            for (int x = 0; x < WorldParameters.theRegionSize; x++)
            {
               Tile t = new Tile();
               t.X = x + myX;
               t.Y = y + myY;
               myTiles[x, y] = t;
            }
         }
      }

      public float getHeightValue(Tile tile)
      {
         if (tile == null)
            return int.MaxValue;
         else
            return tile.myHeightValue;
      }

      public void generateData()
      {
         /*
         Parallel.For(0, WorldParameters.regionSize, y =>
         //for (int y = 0; y < WorldParameters.regionSize; y++)
         {
            for (int x = 0; x < WorldParameters.regionSize; x++)
            {
               // WRAP ON BOTH AXIS
               // Sample noise at smaller intervals
               float s = (x + myX) / (float)myWorld.width;
               float t = (y + myY) / (float)myWorld.height;

               // Calculate our 4D coordinates
               float _2pi = (float)(2 * Math.PI);
               float nx = (float)Math.Cos(s * _2pi) / _2pi;
               float ny = (float)Math.Cos(t * _2pi) / _2pi;
               float nz = (float)Math.Sin(s * _2pi) / _2pi;
               float nw = (float)Math.Sin(t * _2pi) / _2pi;

               //generate the height/heat/moisture values
               float elevationData = (float)myGenerator.myElevationGenerator.Get(nx, ny, nz, nw);
               float heatData = (float)myGenerator.myHeatGenerator.Get(nx, ny, nz, nw);
               float moistureData = (float)myGenerator.myMoistureGenertator.Get(nx, ny, nz, nw);

               //get the current tile
               Tile tile = myTiles[x, y];

               //set the heightmap value
               tile.setElevation(elevationData);

               //adjust moisture based on height
               if (tile.myHeightType == HeightType.DeepWater)
               {
                  moistureData += 8f * tile.myHeightValue;
               }
               else if (tile.myHeightType == HeightType.ShallowWater)
               {
                  moistureData += 3f * tile.myHeightValue;
               }
               else if (tile.myHeightType == HeightType.Shore)
               {
                  moistureData += tile.myHeightValue;
               }
               else if (tile.myHeightType == HeightType.Sand)
               {
                  moistureData += 0.25f * tile.myHeightValue;
               }
               tile.setMoisture(moistureData);

               // Adjust Heat Map based on Height - Higher == colder
               if (tile.myHeightType == HeightType.Grass)
               {
                  heatData -= 0.1f * tile.myHeightValue;
               }
               else if (tile.myHeightType == HeightType.Forest)
               {
                  heatData -= 0.2f * tile.myHeightValue;
               }
               else if (tile.myHeightType == HeightType.Rock)
               {
                  heatData -= 0.3f * tile.myHeightValue;
               }
               else if (tile.myHeightType == HeightType.Snow)
               {
                  heatData -= 0.4f * tile.myHeightValue;
               }
               tile.setHeat(heatData);
            }
         }
         );
         */
      }

      public void updateNeighbors()
      {
         for (var x = 0; x < WorldParameters.theRegionSize; x++)
         {
            for (var y = 0; y < WorldParameters.theRegionSize; y++)
            {
               Tile t = myTiles[x, y];

               t.myTop = getTop(t);
               t.myBottom = getBottom(t);
               t.myLeft = getLeft(t);
               t.myRight = getRight(t);
            }
         }
      }

      #region river generation
      void generateRivers()
      {
         int attempts = 0;
         int rivercount = WorldParameters.theRiverCount;
         myRivers = new List<River>();
         Random rand = new Random();

         // Generate some rivers
         while (rivercount > 0 && attempts < WorldParameters.theMaxRiverAttempts)
         {

            // Get a random tile
            int x = rand.Next(0, WorldParameters.theRegionSize);
            int y = rand.Next(0, WorldParameters.theRegionSize);
            Tile tile = myTiles[x, y];

            // validate the tile
            if (!tile.myIsLand) continue;
            if (tile.Rivers.Count > 0) continue;

            if (tile.myHeightValue > WorldParameters.theMinRiverHeight)
            {
               // Tile is good to start river from
               River river = new River(rivercount);

               // Figure out the direction this river will try to flow
               river.CurrentDirection = tile.getLowestNeighbor(this);

               // Recursively find a path to water
               findPathToWater(tile, river.CurrentDirection, ref river);

               // Validate the generated river 
               if (river.TurnCount < WorldParameters.theMinRiverTurns || river.myTiles.Count < WorldParameters.theMinRiverLength || river.Intersections > WorldParameters.theMaxRiverIntersections)
               {
                  //Validation failed - remove this river
                  for (int i = 0; i < river.myTiles.Count; i++)
                  {
                     Tile t = river.myTiles[i];
                     t.Rivers.Remove(river);
                  }
               }
               else if (river.myTiles.Count >= WorldParameters.theMinRiverLength)
               {
                  //Validation passed - Add river to list
                  myRivers.Add(river);
                  tile.Rivers.Add(river);
                  rivercount--;
               }
            }
            attempts++;
         }
      }

      void findPathToWater(Tile tile, Direction direction, ref River river)
      {
         if (tile.Rivers.Contains(river))
            return;

         // check if there is already a river on this tile
         if (tile.Rivers.Count > 0)
            river.Intersections++;

         river.AddTile(tile);

         // get neighbors
         Tile left = getLeft(tile);
         Tile right = getRight(tile);
         Tile top = getTop(tile);
         Tile bottom = getBottom(tile);

         float leftValue = int.MaxValue;
         float rightValue = int.MaxValue;
         float topValue = int.MaxValue;
         float bottomValue = int.MaxValue;

         // query height values of neighbors
         if (left != null && left.getRiverNeighborCount(river) < 2 && !river.myTiles.Contains(left))
            leftValue = left.myHeightValue;
         if (right != null && right.getRiverNeighborCount(river) < 2 && !river.myTiles.Contains(right))
            rightValue = right.myHeightValue;
         if (top != null && top.getRiverNeighborCount(river) < 2 && !river.myTiles.Contains(top))
            topValue = top.myHeightValue;
         if (bottom != null && bottom.getRiverNeighborCount(river) < 2 && !river.myTiles.Contains(bottom))
            bottomValue = bottom.myHeightValue;

         // if neighbor is existing river that is not this one, flow into it
         if (bottom != null && bottom.Rivers.Count == 0 && !bottom.myIsLand)
            bottomValue = 0;
         if (top != null && top.Rivers.Count == 0 && !top.myIsLand)
            topValue = 0;
         if (left != null && left.Rivers.Count == 0 && !left.myIsLand)
            leftValue = 0;
         if (right != null && right.Rivers.Count == 0 && !right.myIsLand)
            rightValue = 0;

         // override flow direction if a tile is significantly lower
         if (direction == Direction.Left)
            if (Math.Abs(rightValue - leftValue) < 0.1)
               rightValue = int.MaxValue;
         if (direction == Direction.Right)
            if (Math.Abs(rightValue - leftValue) < 0.1)
               leftValue = int.MaxValue;
         if (direction == Direction.Top)
            if (Math.Abs(topValue - bottomValue) < 0.1)
               bottomValue = int.MaxValue;
         if (direction == Direction.Bottom)
            if (Math.Abs(topValue - bottomValue) < 0.1)
               topValue = int.MaxValue;

         // find minimum
         float min = Math.Min(Math.Min(Math.Min(leftValue, rightValue), topValue), bottomValue);

         // if no minimum found - exit
         if (min == int.MaxValue)
            return;

         //Move to next neighbor
         if (min == leftValue)
         {
            if (left != null && left.myIsLand)
            {
               if (river.CurrentDirection != Direction.Left)
               {
                  river.TurnCount++;
                  river.CurrentDirection = Direction.Left;
               }
               findPathToWater(left, direction, ref river);
            }
         }
         else if (min == rightValue)
         {
            if (right != null && right.myIsLand)
            {
               if (river.CurrentDirection != Direction.Right)
               {
                  river.TurnCount++;
                  river.CurrentDirection = Direction.Right;
               }
               findPathToWater(right, direction, ref river);
            }
         }
         else if (min == bottomValue)
         {
            if (bottom != null && bottom.myIsLand)
            {
               if (river.CurrentDirection != Direction.Bottom)
               {
                  river.TurnCount++;
                  river.CurrentDirection = Direction.Bottom;
               }
               findPathToWater(bottom, direction, ref river);
            }
         }
         else if (min == topValue)
         {
            if (top != null && top.myIsLand)
            {
               if (river.CurrentDirection != Direction.Top)
               {
                  river.TurnCount++;
                  river.CurrentDirection = Direction.Top;
               }
               findPathToWater(top, direction, ref river);
            }
         }
      }

      void buildRiverGroups()
      {
         //loop each tile, checking if it belongs to multiple rivers
         for (var x = 0; x < WorldParameters.theRegionSize; x++)
         {
            for (var y = 0; y < WorldParameters.theRegionSize; y++)
            {
               Tile t = myTiles[x, y];

               if (t.Rivers.Count > 1)
               {
                  // multiple rivers == intersection
                  RiverGroup group = null;

                  // Does a rivergroup already exist for this group?
                  for (int n = 0; n < t.Rivers.Count; n++)
                  {
                     River tileriver = t.Rivers[n];
                     for (int i = 0; i < myRiverGroups.Count; i++)
                     {
                        for (int j = 0; j < myRiverGroups[i].Rivers.Count; j++)
                        {
                           River river = myRiverGroups[i].Rivers[j];
                           if (river.myId == tileriver.myId)
                           {
                              group = myRiverGroups[i];
                           }
                           if (group != null) break;
                        }
                        if (group != null) break;
                     }
                     if (group != null) break;
                  }

                  // existing group found -- add to it
                  if (group != null)
                  {
                     for (int n = 0; n < t.Rivers.Count; n++)
                     {
                        if (!group.Rivers.Contains(t.Rivers[n]))
                           group.Rivers.Add(t.Rivers[n]);
                     }
                  }
                  else   //No existing group found - create a new one
                  {
                     group = new RiverGroup();
                     for (int n = 0; n < t.Rivers.Count; n++)
                     {
                        group.Rivers.Add(t.Rivers[n]);
                     }
                     myRiverGroups.Add(group);
                  }
               }
            }
         }
      }

      void digRiverGroups()
      {
         for (int i = 0; i < myRiverGroups.Count; i++)
         {
            RiverGroup group = myRiverGroups[i];
            River longest = null;

            //Find longest river in this group
            for (int j = 0; j < group.Rivers.Count; j++)
            {
               River river = group.Rivers[j];
               if (longest == null)
                  longest = river;
               else if (longest.myTiles.Count < river.myTiles.Count)
                  longest = river;
            }

            if (longest != null)
            {
               //Dig out longest path first
               digRiver(longest);

               for (int j = 0; j < group.Rivers.Count; j++)
               {
                  River river = group.Rivers[j];
                  if (river != longest)
                  {
                     digRiver(river, longest);
                  }
               }
            }
         }
      }

      void digRiver(River river, River parent)
      {
         int intersectionID = 0;
         int intersectionSize = 0;
         Random rand = new Random();

         // determine point of intersection
         for (int i = 0; i < river.myTiles.Count; i++)
         {
            Tile t1 = river.myTiles[i];
            for (int j = 0; j < parent.myTiles.Count; j++)
            {
               Tile t2 = parent.myTiles[j];
               if (t1 == t2)
               {
                  intersectionID = i;
                  intersectionSize = t2.RiverSize;
               }
            }
         }

         int counter = 0;
         int intersectionCount = river.myTiles.Count - intersectionID;
         int size = rand.Next(intersectionSize, 5);
         river.myLength = river.myTiles.Count;

         // randomize size change
         int two = river.myLength / 2;
         int three = two / 2;
         int four = three / 2;
         int five = four / 2;

         int twomin = two / 3;
         int threemin = three / 3;
         int fourmin = four / 3;
         int fivemin = five / 3;

         // randomize length of each size
         int count1 = rand.Next(fivemin, five);
         if (size < 4)
         {
            count1 = 0;
         }
         int count2 = count1 + rand.Next(fourmin, four);
         if (size < 3)
         {
            count2 = 0;
            count1 = 0;
         }
         int count3 = count2 + rand.Next(threemin, three);
         if (size < 2)
         {
            count3 = 0;
            count2 = 0;
            count1 = 0;
         }
         int count4 = count3 + rand.Next(twomin, two);

         // Make sure we are not digging past the river path
         if (count4 > river.myLength)
         {
            int extra = count4 - river.myLength;
            while (extra > 0)
            {
               if (count1 > 0) { count1--; count2--; count3--; count4--; extra--; }
               else if (count2 > 0) { count2--; count3--; count4--; extra--; }
               else if (count3 > 0) { count3--; count4--; extra--; }
               else if (count4 > 0) { count4--; extra--; }
            }
         }

         // adjust size of river at intersection point
         if (intersectionSize == 1)
         {
            count4 = intersectionCount;
            count1 = 0;
            count2 = 0;
            count3 = 0;
         }
         else if (intersectionSize == 2)
         {
            count3 = intersectionCount;
            count1 = 0;
            count2 = 0;
         }
         else if (intersectionSize == 3)
         {
            count2 = intersectionCount;
            count1 = 0;
         }
         else if (intersectionSize == 4)
         {
            count1 = intersectionCount;
         }
         else
         {
            count1 = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;
         }

         // dig out the river
         for (int i = river.myTiles.Count - 1; i >= 0; i--)
         {

            Tile t = river.myTiles[i];

            if (counter < count1)
            {
               t.digRiver(river, 4);
            }
            else if (counter < count2)
            {
               t.digRiver(river, 3);
            }
            else if (counter < count3)
            {
               t.digRiver(river, 2);
            }
            else if (counter < count4)
            {
               t.digRiver(river, 1);
            }
            else
            {
               t.digRiver(river, 0);
            }
            counter++;
         }
      }

      void digRiver(River river)
      {
         int counter = 0;
         Random rand = new Random();

         // How wide are we digging this river?
         int size = rand.Next(1, 5);
         river.myLength = river.myTiles.Count;

         // randomize size change
         int two = river.myLength / 2;
         int three = two / 2;
         int four = three / 2;
         int five = four / 2;

         int twomin = two / 3;
         int threemin = three / 3;
         int fourmin = four / 3;
         int fivemin = five / 3;

         // randomize length of each size
         int count1 = rand.Next(fivemin, five);
         if (size < 4)
         {
            count1 = 0;
         }
         int count2 = count1 + rand.Next(fourmin, four);
         if (size < 3)
         {
            count2 = 0;
            count1 = 0;
         }
         int count3 = count2 + rand.Next(threemin, three);
         if (size < 2)
         {
            count3 = 0;
            count2 = 0;
            count1 = 0;
         }
         int count4 = count3 + rand.Next(twomin, two);

         // Make sure we are not digging past the river path
         if (count4 > river.myLength)
         {
            int extra = count4 - river.myLength;
            while (extra > 0)
            {
               if (count1 > 0) { count1--; count2--; count3--; count4--; extra--; }
               else if (count2 > 0) { count2--; count3--; count4--; extra--; }
               else if (count3 > 0) { count3--; count4--; extra--; }
               else if (count4 > 0) { count4--; extra--; }
            }
         }

         // Dig it out
         for (int i = river.myTiles.Count - 1; i >= 0; i--)
         {
            Tile t = river.myTiles[i];

            if (counter < count1)
            {
               t.digRiver(river, 4);
            }
            else if (counter < count2)
            {
               t.digRiver(river, 3);
            }
            else if (counter < count3)
            {
               t.digRiver(river, 2);
            }
            else if (counter < count4)
            {
               t.digRiver(river, 1);
            }
            else
            {
               t.digRiver(river, 0);
            }
            counter++;
         }
      }

      void adjustMoistureMap()
      {
         for (var x = 0; x < WorldParameters.theRegionSize; x++)
         {
            for (var y = 0; y < WorldParameters.theRegionSize; y++)
            {

               Tile t = myTiles[x, y];
               if (t.myHeightType == HeightType.River)
               {
                  addMoisture(t, (int)60);
               }
            }
         }
      }

      private void addMoisture(Tile t, int radius)
      {
//          int startx = wrap(t.X - radius, WorldParameters.regionSize);
//          int endx = wrap(t.X + radius, WorldParameters.regionSize);
//          Vector2 center = new Vector2(t.X, t.Y);
//          int curr = radius;
// 
//          while (curr > 0)
//          {
// 
//             int x1 = wrap(t.X - curr, WorldParameters.regionSize);
//             int x2 = wrap(t.X + curr, WorldParameters.regionSize);
//             int y = t.Y;
// 
//             addMoisture(myTiles[x1, y], 0.025f / (center - new Vector2(x1, y)).Length);
// 
//             for (int i = 0; i < curr; i++)
//             {
//                addMoisture(myTiles[x1, wrap(y + i + 1, WorldParameters.regionSize)], 0.025f / (center - new Vector2(x1, wrap(y + i + 1, WorldParameters.regionSize))).Length);
//                addMoisture(myTiles[x1, wrap(y - (i + 1), WorldParameters.regionSize)], 0.025f / (center - new Vector2(x1, wrap(y - (i + 1), WorldParameters.regionSize))).Length);
// 
//                addMoisture(myTiles[x2, wrap(y + i + 1, WorldParameters.regionSize)], 0.025f / (center - new Vector2(x2, wrap(y + i + 1, WorldParameters.regionSize))).Length);
//                addMoisture(myTiles[x2, wrap(y - (i + 1), WorldParameters.regionSize)], 0.025f / (center - new Vector2(x2, wrap(y - (i + 1), WorldParameters.regionSize))).Length);
//             }
//             curr--;
//          }
      }

      private void addMoisture(Tile t, float amount)
      {
//          float currentVal = myMoistureData.get(t.X, t.Y);
//          myMoistureData.set(t.X, t.Y, currentVal += amount);
//          t.setMoisture(myMoistureData.get(t.X, t.Y));
      }
      #endregion

      public void generateBiomes()
      {
         for (var x = 0; x < WorldParameters.theRegionSize; x++)
         {
            for (var y = 0; y < WorldParameters.theRegionSize; y++)
            {
               if (!myTiles[x, y].myIsLand) continue;

               Tile t = myTiles[x, y];
               t.myBiomeType = WorldParameters.theBiomeTable[(int)t.myMoistureType, (int)t.myHeatType];
            }
         }
      }

      public void generateTextures()
      {
         myElevationMap = TextureGenerator.getHeightMapTexture(myTiles);
         myHeatMap = TextureGenerator.getSimpleHeatMapTexture(myTiles);
         myMoistureMap = TextureGenerator.getMoistureMapTexture(myTiles);
         myBiomeMap = TextureGenerator.getBiomeMapTexture(myTiles, WorldParameters.ColdestValue, WorldParameters.ColderValue, WorldParameters.ColdValue);
      }

      public void updateBitmasks()
      {
         for (var x = 0; x < WorldParameters.theRegionSize; x++)
         {
            for (var y = 0; y < WorldParameters.theRegionSize; y++)
            {
               myTiles[x, y].updateBitmask();
            }
         }
      }

      public void updateBiomeBitmasks()
      {
         for (var x = 0; x < WorldParameters.theRegionSize; x++)
         {
            for (var y = 0; y < WorldParameters.theRegionSize; y++)
            {
               myTiles[x, y].updateBiomeBitmask();
            }
         }
      }

      public void floodFill()
      {
         // Use a stack instead of recursion
         Stack<Tile> stack = new Stack<Tile>();

         for (int x = 0; x < WorldParameters.theRegionSize; x++)
         {
            for (int y = 0; y < WorldParameters.theRegionSize; y++)
            {

               Tile t = myTiles[x, y];

               //Tile already flood filled, skip
               if (t.myFloodFilled) continue;

               // Land
               if (t.myIsLand)
               {
                  TileGroup group = new TileGroup();
                  group.myType = TileGroupType.Land;
                  stack.Push(t);

                  while (stack.Count > 0)
                  {
                     floodFill(stack.Pop(), ref group, ref stack);
                  }

                  if (group.myTiles.Count > 0)
                     myLands.Add(group);
               }
               // Water
               else
               {
                  TileGroup group = new TileGroup();
                  group.myType = TileGroupType.Water;
                  stack.Push(t);

                  while (stack.Count > 0)
                  {
                     floodFill(stack.Pop(), ref group, ref stack);
                  }

                  if (group.myTiles.Count > 0)
                     myWaters.Add(group);
               }
            }
         }
      }

      void floodFill(Tile tile, ref TileGroup tiles, ref Stack<Tile> stack)
      {
         // Validate
         if (tile == null)
            return;
         if (tile.myFloodFilled)
            return;
         if (tiles.myType == TileGroupType.Land && !tile.myIsLand)
            return;
         if (tiles.myType == TileGroupType.Water && tile.myIsLand)
            return;

         // Add to TileGroup
         tiles.myTiles.Add(tile);
         tile.myFloodFilled = true;

         // floodfill into neighbors
         Tile t = getTop(tile);
         if (t != null && !t.myFloodFilled && tile.myIsLand == t.myIsLand)
            stack.Push(t);
         t = getBottom(tile);
         if (t != null && !t.myFloodFilled && tile.myIsLand == t.myIsLand)
            stack.Push(t);
         t = getLeft(tile);
         if (t != null && !t.myFloodFilled && tile.myIsLand == t.myIsLand)
            stack.Push(t);
         t = getRight(tile);
         if (t != null && !t.myFloodFilled && tile.myIsLand == t.myIsLand)
            stack.Push(t);
      }

      Tile getTop(Tile t)
      {
         return myWorld.findTile(t.X, t.Y - 1);
      }
      Tile getBottom(Tile t)
      {
         return myWorld.findTile(t.X, t.Y + 1);
      }
      Tile getLeft(Tile t)
      {
         return myWorld.findTile(t.X -1, t.Y);
      }
      Tile getRight(Tile t)
      {
         return myWorld.findTile(t.X + 1, t.Y);
      }
   }
}