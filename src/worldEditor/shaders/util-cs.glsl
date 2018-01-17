#version 430

#define TWO_PI 6.28318530718

//height values
#define DeepWaterVal 0.1f
#define ShallowWaterVal 0.3f
#define SandVal 0.4f
#define GrassVal 0.5f
#define ForestVal 0.8f
#define RockVal 0.9f

//heat values
#define ColdestVal 0.05f
#define ColderVal 0.18f
#define ColdVal 0.4f
#define WarmVal 0.6f
#define WarmerVal 0.8f

//moisture values
#define DryerVal 0.15f
#define DryVal 0.3f
#define WetVal 0.45f
#define WetterVal 0.6f
#define WettestVal 0.75f

//height types
#define DeepWater  1
#define ShallowWater 2
#define Shore 3
#define Sand 4 
#define Grass 5
#define Forest  6
#define Rock 7
#define Snow 8
#define River 9

//temp types
#define Coldest 0
#define Colder 1
#define Cold 2
#define Warm 3
#define Warmer 4
#define Warmest 5

//moisture types
#define Wettest 5
#define Wetter 4
#define Wet 3
#define Dry 2
#define Dryer 1
#define Dryest 0

//group types
#define Water 0
#define Land 1

//biome types
#define Ice 0
#define Tundra 1
#define Desert 2
#define Grassland 3
#define Savanna 4
#define TemperateSeasonalForest 5
#define TropicalSeasonalForest 6
#define Taiga 7
#define TemperateRainforest 8
#define TropicalRainforest 9

#define biomeType(x) bitfieldExtract(x, 0, 4)
#define moistureType(x) bitfieldExtract(x, 4, 4)
#define heatType(x) bitfieldExtract(x, 8, 4)
#define elevationType(x) bitfieldExtract(x, 12, 4)
#define groupType(x) bitfieldExtract(x, 16, 4)

#define setBiomeType(x, y) bitfieldInsert(x, y, 0, 4)
#define setMoistureType(x, y) bitfieldInsert(x, y, 4, 4)
#define setHeatType(x, y) bitfieldInsert(x, y, 8, 4)
#define setElevationType(x, y) bitfieldInsert(x, y, 12, 4)
#define setGroupType(x, y) bitfieldInsert(x, y, 16, 4)

uint biomes[6][6] = {
   //COLDEST  //COLDER  //COLD                    //HOT                     //HOTTER             //HOTTEST
   { Ice,     Tundra,   Grassland,                Desert,                   Desert,              Desert },              //DRYEST
   { Ice,     Tundra,   Grassland,                Desert,                   Desert,              Desert },              //DRYER
   { Ice,     Tundra,   TemperateSeasonalForest,  TemperateSeasonalForest,  Savanna,             Savanna },             //DRY
   { Ice,     Tundra,   Taiga,                    TemperateSeasonalForest,  Savanna,             Savanna },             //WET
   { Ice,     Tundra,   Taiga,                    TemperateSeasonalForest,  TropicalRainforest,  TropicalRainforest },  //WETTER
   { Ice,     Tundra,   Taiga,                    TemperateRainforest,      TropicalRainforest,  TropicalRainforest }   //WETTEST
};

//biome colors
vec4 biomeColors[] = {
   vec4(1,1,1,1), //ice
   vec4(96 / 255f, 131 / 255f, 112 / 255f, 1f), //tundra
   vec4(238 / 255f, 218 / 255f, 130 / 255f, 1f), //desert
   vec4(164 / 255f, 225 / 255f, 99 / 255f, 1f), //Grassland
   vec4(177 / 255f, 209 / 255f, 110 / 255f, 1f), //Savanna
   vec4(73 / 255f, 100 / 255f, 35 / 255f, 1f), //TemperateSeasonalForest
   vec4(139 / 255f, 175 / 255f, 90 / 255f, 1f), //TropicalSeasonalForest
   vec4(95 / 255f, 115 / 255f, 62 / 255f, 1f), //Taiga
   vec4(29 / 255f, 73 / 255f, 40 / 255f, 1f), //TemperateRainForest
   vec4(66 / 255f, 123 / 255f, 25 / 255f, 1f) //TropicalRainForest
};



vec3 generateSampleVec(int f, vec2 uv)
{
   //map input coord from 0..1 to -1..1
   vec2 t = uv * 2 - 1.0;

   switch (f)
   {
   case 0: // +x
      return vec3(1, t.y, -t.x);
   case 1: // -x
      return vec3(-1, t.y, t.x);
   case 2: // +y
      return vec3(t.x, -1, t.y);
   case 3: // -y
      return vec3(t.x, 1, -t.y);
   case 4: // +z
      return vec3(t.x, t.y, 1);
   case 5: // -z
      return vec3(-t.x, t.y, -1);
   }
}

ivec2 cylinderProjection(vec3 dir, ivec2 size)
{
   float theta = 0.0;
   if (dir.x != 0)
      theta = atan(dir.z, dir.x);

   if (theta < 0)
   {
      theta += TWO_PI;
   }

   float x = theta / TWO_PI;
   float y = (dir.y + 1) / 2.0;
   x *= size.x;
   y *= size.y;

   return ivec2(x, y);
}

uint getElevationType(float elv)
{
   if (elv < DeepWaterVal) return DeepWater;
   if (elv < ShallowWaterVal) return ShallowWater;
   if (elv < SandVal) return Sand;
   if (elv < GrassVal) return Grass;
   if (elv < ForestVal) return Forest;
   if (elv < RockVal) return Rock;
   
   return Snow;
}

uint getHeatType(float heat)
{
   if (heat < ColdestVal) return Coldest;
   if (heat < ColderVal) return Colder;
   if (heat < ColdVal) return Cold;
   if (heat < WarmVal) return Warm;
   if (heat < WarmerVal) return Warmer;

   return Warmest;
}

uint getMoistureType(float moist)
{
   if (moist < DryerVal) return Dryest;
   if (moist < DryVal) return Dryer;
   if (moist < WetVal) return Dry;
   if (moist < WetterVal) return Wet;
   if (moist < WettestVal) return Wetter;

   return Wettest;
}

uint getGroupType(float elev)
{
   if (elev < ShallowWaterVal) return Water;

   return Land;
}

uint getBiomeType(uint temp, uint moist)
{
   return biomes[temp][moist];
}