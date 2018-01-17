using System;

namespace WorldEditor
{
   public class WorldParameters
   {
      public static int seed = 101475;

      //height map generation parameters
      public static int theTerrainOctaves = 6;
      public static float theTerrainFrequency = 6.0f;

      public static int theHeatOctaves = 4;
      public static float theHeatFrequency = 3.0f;

      public static int theMoistureOctaves = 4;
      public static float theMoistureFrequency = 2.5f;


      //height classification
      public static float DeepWater = 0.1f;
      public static float ShallowWater = 0.3f;
      public static float Sand = 0.4f;
      public static float Grass = 0.5f;
      public static float Forest = 0.8f;
      public static float Rock = 0.9f;

      //heat classification
      public static float ColdestValue = 0.05f;
      public static float ColderValue = 0.18f;
      public static float ColdValue = 0.4f;
      public static float WarmValue = 0.6f;
      public static float WarmerValue = 0.8f;

      //moisture classification
      public static float DryerValue = 0.15f;
      public static float DryValue = 0.3f;
      public static float WetValue = 0.45f;
      public static float WetterValue = 0.6f;
      public static float WettestValue = 0.75f;

      public static BiomeType[,] theBiomeTable = new BiomeType[6, 6] {   
		//COLDEST        //COLDER          //COLD                               //HOT                               //HOTTER                       //HOTTEST
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland,                 BiomeType.Desert,                   BiomeType.Desert,              BiomeType.Desert },              //DRYEST
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland,                 BiomeType.Desert,                   BiomeType.Desert,              BiomeType.Desert },              //DRYER
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.TemperateSeasonalForest,   BiomeType.TemperateSeasonalForest,  BiomeType.Savanna,             BiomeType.Savanna },             //DRY
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.Taiga,                     BiomeType.TemperateSeasonalForest,  BiomeType.Savanna,             BiomeType.Savanna },             //WET
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.Taiga,                     BiomeType.TemperateSeasonalForest,  BiomeType.TropicalRainforest,  BiomeType.TropicalRainforest },  //WETTER
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.Taiga,                     BiomeType.TemperateRainforest,      BiomeType.TropicalRainforest,  BiomeType.TropicalRainforest }   //WETTEST
	};

      //river generation parameters
      public static int theRiverCount = 0; //20
      public static float theMinRiverHeight = 0.6f;
      public static int theMaxRiverAttempts = 1000;
      public static int theMinRiverTurns = 18;
      public static int theMinRiverLength = 20;
      public static int theMaxRiverIntersections = 2;

      public static int theWorldTextureSize = 1024;

      public static int theRegionSize = 128;
   }
}
