#version 430

smooth in vec3 texCoord;

layout(location = 20) uniform samplerCube cubemap;

layout(location = 21) uniform bool showElevation;
layout(location = 22) uniform bool showWater;
layout(location = 23) uniform bool showHeat;
layout(location = 24) uniform bool showMoisture;
layout(location = 25) uniform bool showBiome;

out vec4 FragColor;

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
#define ShallowOcean 10
#define DeepOcean 11

#define biomeType(x) bitfieldExtract(x, 0, 4)
#define moistureType(x) bitfieldExtract(x, 4, 4)
#define heatType(x) bitfieldExtract(x, 8, 4)
#define heightType(x) bitfieldExtract(x, 12, 4)
#define groupType(x) bitfieldExtract(x, 16, 4)

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
   vec4(66 / 255f, 123 / 255f, 25 / 255f, 1f), //TropicalRainForest
   vec4(15 / 255f, 30 / 255f, 80 / 255f, 1f), //ShallowOcean
   vec4(15 / 255f, 40 / 255f, 90 / 255f, 1f) //DeepOcean
};

void main()
{
   vec4 data = texture(cubemap, texCoord);

   float elevation = data.r;
   float heat = data.g;
   float moisture = data.b;
   uint bitfield = floatBitsToUint(data.a);

   uint group = groupType(bitfield);
   uint height = heightType(bitfield);
   uint temp = heatType(bitfield);
   uint moist = moistureType(bitfield);
   uint biome = biomeType(bitfield);

   vec4 color = vec4(1,1,1,1);

   if(showElevation)
      color *= vec4(elevation,elevation, elevation, 1);

   if(showWater && group == 0)
      color = vec4(0,0,1,1);

   if(showHeat)
      color *= vec4(heat, heat, heat, 1);

   if(showMoisture)
      color *= vec4(moisture, moisture, moisture, 1);

   if(showBiome)
      color *= biomeColors[biome];

   color.a = 1;
   FragColor = color;
}