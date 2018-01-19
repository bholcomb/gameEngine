using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using Graphics;
using Util;

namespace WorldEditor
{

public static class TextureGenerator
{

   // Height Map Colors
   private static Color4 DeepColor = new Color4(15 / 255f, 30 / 255f, 80 / 255f, 1f);
   private static Color4 ShallowColor = new Color4(15 / 255f, 40 / 255f, 90 / 255f, 1f);
   private static Color4 RiverColor = new Color4(30 / 255f, 120 / 255f, 200 / 255f, 1f);
   private static Color4 SandColor = new Color4(198 / 255f, 190 / 255f, 31 / 255f, 1f);
   private static Color4 GrassColor = new Color4(50 / 255f, 220 / 255f, 20 / 255f, 1f);
   private static Color4 ForestColor = new Color4(16 / 255f, 160 / 255f, 0f, 1f);
   private static Color4 RockColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
   private static Color4 SnowColor = new Color4(1f, 1f, 1f, 1f);

   private static Color4 IceWater = new Color4(210 / 255f, 255 / 255f, 252 / 255f, 1f);
   private static Color4 ColdWater = new Color4(119 / 255f, 156 / 255f, 213 / 255f, 1f);
   private static Color4 RiverWater = new Color4(65 / 255f, 110 / 255f, 179 / 255f, 1f);

   // Heat Map Colors
   private static Color4 Coldest = new Color4(0f, 1f, 1f, 1f);
   private static Color4 Colder = new Color4(170 / 255f, 1f, 1f, 1f);
   private static Color4 Cold = new Color4(0f, 229 / 255f, 133 / 255f, 1f);
   private static Color4 Warm = new Color4(1f, 1f, 100 / 255f, 1f);
   private static Color4 Warmer = new Color4(1f, 100 / 255f, 0f, 1f);
   private static Color4 Warmest = new Color4(241 / 255f, 12 / 255f, 0f, 1f);

   //Moisture map
   private static Color4 Dryest = new Color4(255 / 255f, 139 / 255f, 17 / 255f, 1f);
   private static Color4 Dryer = new Color4(245 / 255f, 245 / 255f, 23 / 255f, 1f);
   private static Color4 Dry = new Color4(80 / 255f, 255 / 255f, 0 / 255f, 1f);
   private static Color4 Wet = new Color4(85 / 255f, 255 / 255f, 255 / 255f, 1f);
   private static Color4 Wetter = new Color4(20 / 255f, 70 / 255f, 255 / 255f, 1f);
   private static Color4 Wettest = new Color4(0 / 255f, 0 / 255f, 100 / 255f, 1f);

   //biome map
   private static Color4 Ice = Color4.White;
   private static Color4 Desert = new Color4(238 / 255f, 218 / 255f, 130 / 255f, 1f);
   private static Color4 Savanna = new Color4(177 / 255f, 209 / 255f, 110 / 255f, 1f);
   private static Color4 TropicalRainForest = new Color4(66 / 255f, 123 / 255f, 25 / 255f, 1f);
   private static Color4 Tundra = new Color4(96 / 255f, 131 / 255f, 112 / 255f, 1f);
   private static Color4 TemperateRainForest = new Color4(29 / 255f, 73 / 255f, 40 / 255f, 1f);
   private static Color4 Grassland = new Color4(164 / 255f, 225 / 255f, 99 / 255f, 1f);
   private static Color4 TemperateSeasonalForest = new Color4(73 / 255f, 100 / 255f, 35 / 255f, 1f);
   private static Color4 Taiga = new Color4(95 / 255f, 115 / 255f, 62 / 255f, 1f);
   private static Color4 TropicalSeasonalForest = new Color4(139 / 255f, 175 / 255f, 90 / 255f, 1f);


   public static Texture getBiomePalette()
   {
      var texture = new Texture(128, 128);
      var pixels = new Color4[128, 128];

      for (var x = 0; x < 128; x++)
      {
         for (var y = 0; y < 128; y++)
         {
            if (x < 10)
               pixels[x, y] = Ice;
            else if (x < 20)
               pixels[x, y] = Desert;
            else if (x < 30)
               pixels[x, y] = Savanna;
            else if (x < 40)
               pixels[x, y] = TropicalRainForest;
            else if (x < 50)
               pixels[x, y] = Tundra;
            else if (x < 60)
               pixels[x, y] = TemperateRainForest;
            else if (x < 70)
               pixels[x, y] = Grassland;
            else if (x < 80)
               pixels[x, y] = TemperateSeasonalForest;
            else if (x < 90)
               pixels[x, y] = Taiga;
            else if (x < 100)
               pixels[x, y] = TropicalSeasonalForest;
         }
      }

      texture.setPixels(ref pixels);
      return texture;

   }

   public static Texture getBumpMap(Tile[,] tiles)
   {
      int width = tiles.GetLength(0);
      int height = tiles.GetLength(1);
      var texture = new Texture(width, height);
      var pixels = new Color4[width , height];

      for (var x = 0; x < width; x++)
      {
         for (var y = 0; y < height; y++)
         {
            switch (tiles[x, y].myHeightType)
            {
               case HeightType.DeepWater:
                  pixels[x, y]= new Color4(0, 0, 0, 1);
                  break;
               case HeightType.ShallowWater:
                  pixels[x, y]= new Color4(0, 0, 0, 1);
                  break;
               case HeightType.Sand:
                  pixels[x, y]= new Color4(0.3f, 0.3f, 0.3f, 1);
                  break;
               case HeightType.Grass:
                  pixels[x, y]= new Color4(0.45f, 0.45f, 0.45f, 1);
                  break;
               case HeightType.Forest:
                  pixels[x, y]= new Color4(0.6f, 0.6f, 0.6f, 1);
                  break;
               case HeightType.Rock:
                  pixels[x, y]= new Color4(0.75f, 0.75f, 0.75f, 1);
                  break;
               case HeightType.Snow:
                  pixels[x, y]= new Color4(1, 1, 1, 1);
                  break;
               case HeightType.River:
                  pixels[x, y]= new Color4(0, 0, 0, 1);
                  break;
            }

            if (!tiles[x, y].myIsLand)
            {
               pixels[x, y]= ColorExt.mix(Color4.White, Color4.Black, tiles[x, y].myHeightValue * 2);
            }
         }
      }

      texture.setPixels(ref pixels);
      return texture;
   }

   public static Texture getHeightMapTexture(Tile[,] tiles)
   {
      int width = tiles.GetLength(0);
      int height = tiles.GetLength(1);
      var texture = new Texture(width, height);
      var pixels = new Color4[width, height];

      for (var x = 0; x < width; x++)
      {
         for (var y = 0; y < height; y++)
         {
            switch (tiles[x, y].myHeightType)
            {
//                case HeightType.DeepWater:
//                   pixels[x, y] = new Color4(0f, 0f, 0f, 1f);
//                   break;
//                case HeightType.ShallowWater:
//                   pixels[x, y]= new Color4(0f, 0f, 0f, 1f);
//                   break;
//                case HeightType.Shore:
//                   pixels[x, y] = new Color4(0.15f, 0.15f, 0.15f, 1f);
//                   break;
//                case HeightType.Sand:
//                   pixels[x, y]= new Color4(0.3f, 0.3f, 0.3f, 1f);
//                   break;
//                case HeightType.Grass:
//                   pixels[x, y]= new Color4(0.45f, 0.45f, 0.45f, 1f);
//                   break;
//                case HeightType.Forest:
//                   pixels[x, y]= new Color4(0.6f, 0.6f, 0.6f, 1f);
//                   break;
//                case HeightType.Rock:
//                   pixels[x, y]= new Color4(0.75f, 0.75f, 0.75f, 1f);
//                   break;
//                case HeightType.Snow:
//                   pixels[x, y]= new Color4(1f, 1f, 1f, 1f);
//                   break;
//                case HeightType.River:
//                   pixels[x, y] = new Color4(0f, 0f, 0f, 1f);
//                   break;

//                case HeightType.DeepWater:
//                case HeightType.ShallowWater:
//                case HeightType.River:
//                   pixels[x, y] = Color4.Black;
//                   break;
               default:
                  pixels[x,y] = ColorExt.mix(Color4.Black, Color4.White, tiles[x, y].myHeightValue);
                  break;
            }

             //darken the color if a edge tile
            if (tiles[x, y].myBitmask != 15)
               pixels[x, y] = ColorExt.mix(pixels[x, y], Color4.Black, 0.4f);
         }
      }

      texture.setPixels(ref pixels);

      return texture;
   }

   public static Texture getHeatMapTexture(Tile[,] tiles)
   {
      int width = tiles.GetLength(0);
      int height = tiles.GetLength(1);
      var texture = new Texture(width, height);
      var pixels = new Color4[width, height];

      for (var x = 0; x < width; x++)
      {
         for (var y = 0; y < height; y++)
         {
            switch (tiles[x, y].myHeatType)
            {
               case HeatType.Coldest:
                  pixels[x, y]= Coldest;
                  break;
               case HeatType.Colder:
                  pixels[x, y]= Colder;
                  break;
               case HeatType.Cold:
                  pixels[x, y]= Cold;
                  break;
               case HeatType.Warm:
                  pixels[x, y]= Warm;
                  break;
               case HeatType.Warmer:
                  pixels[x, y]= Warmer;
                  break;
               case HeatType.Warmest:
                  pixels[x, y]= Warmest;
                  break;
            }

            //darken the Color4 if a edge tile
            if ((int)tiles[x, y].myHeightType > 2 && tiles[x, y].myBitmask != 15)
               pixels[x, y]= ColorExt.mix(pixels[x,y], Color4.Black, 0.4f);
         }
      }

      texture.setPixels(ref pixels);
      return texture;
   }

   public static Texture getSimpleHeatMapTexture(Tile[,] tiles)
   {
      int width = tiles.GetLength(0);
      int height = tiles.GetLength(1);
      var texture = new Texture(width, height);
      var pixels = new Color4[width, height];

      for (var x = 0; x < width; x++)
      {
         for (var y = 0; y < height; y++)
         {
            pixels[x, y] = ColorExt.mix(Color4.Blue, Color4.Red, tiles[x, y].myHeatValue);

            //darken the color if a edge tile
            if (tiles[x, y].myBitmask != 15)
               pixels[x, y] = ColorExt.mix(pixels[x, y], Color4.Black, 0.4f);
         }
      }

      texture.setPixels(ref pixels);
      return texture;
   }

   public static Texture getMoistureMapTexture(Tile[,] tiles)
   {
      int width = tiles.GetLength(0);
      int height = tiles.GetLength(1);
      var texture = new Texture(width, height);
      var pixels = new Color4[width, height];

      for (var x = 0; x < width; x++)
      {
         for (var y = 0; y < height; y++)
         {
            Tile t = tiles[x, y];

            if (t.myMoistureType == MoistureType.Dryest)
               pixels[x, y]= Dryest;
            else if (t.myMoistureType == MoistureType.Dryer)
               pixels[x, y]= Dryer;
            else if (t.myMoistureType == MoistureType.Dry)
               pixels[x, y]= Dry;
            else if (t.myMoistureType == MoistureType.Wet)
               pixels[x, y]= Wet;
            else if (t.myMoistureType == MoistureType.Wetter)
               pixels[x, y]= Wetter;
            else
               pixels[x, y]= Wettest;
         }
      }

      texture.setPixels(ref pixels);
      return texture;
   }

   public static Texture getBiomeMapTexture(Tile[,] tiles, float coldest, float colder, float cold)
   {
      int width = tiles.GetLength(0);
      int height = tiles.GetLength(1);
      var texture = new Texture(width, height);
      var pixels = new Color4[width, height];

      for (var x = 0; x < width; x++)
      {
         for (var y = 0; y < height; y++)
         {
            BiomeType value = tiles[x, y].myBiomeType;

            switch (value)
            {
               case BiomeType.Ice:
                  pixels[x, y]= Ice;
                  break;
               case BiomeType.Tundra:
                  pixels[x, y]= Tundra;
                  break;
               case BiomeType.Desert:
                  pixels[x, y]= Desert;
                  break;
               case BiomeType.Grassland:
                  pixels[x, y]= Grassland;
                  break;
               case BiomeType.Savanna:
                  pixels[x, y]= Savanna;
                  break;
               case BiomeType.TemperateSeasonalForest:
                  pixels[x, y]= TemperateSeasonalForest;
                  break;
               case BiomeType.TropicalSeasonalForest:
                  pixels[x, y]= TropicalSeasonalForest;
                  break;
               case BiomeType.Taiga:
                  pixels[x, y]= Taiga;
                  break;
               case BiomeType.TemperateRainforest:
                  pixels[x, y]= TemperateRainForest;
                  break;
               case BiomeType.TropicalRainforest:
                  pixels[x, y]= TropicalRainForest;
                  break;
            }

            // Water tiles
            if (tiles[x, y].myHeightType == HeightType.DeepWater)
            {
               pixels[x, y]= DeepColor;
            }
            else if (tiles[x, y].myHeightType == HeightType.ShallowWater)
            {
               pixels[x, y]= ShallowColor;
            }

            // draw rivers
            if (tiles[x, y].myHeightType == HeightType.River)
            {
               float heatValue = tiles[x, y].myHeatValue;

               if (tiles[x, y].myHeatType == HeatType.Coldest)
                  pixels[x, y]= ColorExt.mix(IceWater, ColdWater, (heatValue) / (coldest));
               else if (tiles[x, y].myHeatType == HeatType.Colder)
                  pixels[x, y] = ColorExt.mix(ColdWater, RiverWater, (heatValue - coldest) / (colder - coldest));
               else if (tiles[x, y].myHeatType == HeatType.Cold)
                  pixels[x, y] = ColorExt.mix(RiverWater, ShallowColor, (heatValue - colder) / (cold - colder));
               else
                  pixels[x, y]= ShallowColor;
            }


            // add a outline
            if (tiles[x, y].myHeightType >= HeightType.Shore && tiles[x, y].myHeightType != HeightType.River)
            {
               if (tiles[x, y].myBiomeBitmask != 15)
                  pixels[x, y] = ColorExt.mix(pixels[x, y], Color4.Black, 0.35f);
            }
         }
      }

      texture.setPixels(ref pixels);
      return texture;
   }
}

}
