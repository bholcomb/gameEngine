#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(rgba32f, binding = 0) uniform image2D faceTex;

layout(location = 2) uniform int face;

#define moistureType(x) bitfieldExtract(x, 4, 4)
#define heatType(x) bitfieldExtract(x, 8, 4)
#define setBiomeType(x, y) bitfieldInsert(x, y, 0, 4)
#define elevationType(x) bitfieldExtract(x, 12, 4)

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


uint biomes[6][6] = {
   //COLDEST  //COLDER  //COLD                    //HOT                     //HOTTER             //HOTTEST
   { Ice,     Tundra,   Grassland,                Desert,                   Desert,              Desert },              //DRYEST
   { Ice,     Tundra,   Grassland,                Desert,                   Desert,              Desert },              //DRYER
   { Ice,     Tundra,   TemperateSeasonalForest,  TemperateSeasonalForest,  Savanna,             Savanna },             //DRY
   { Ice,     Tundra,   Taiga,                    TemperateSeasonalForest,  Savanna,             Savanna },             //WET
   { Ice,     Tundra,   Taiga,                    TemperateSeasonalForest,  TropicalRainforest,  TropicalRainforest },  //WETTER
   { Ice,     Tundra,   Taiga,                    TemperateRainforest,      TropicalRainforest,  TropicalRainforest }   //WETTEST
};

uint getBiomeType(uint temp, uint moist, uint elev)
{
   if(elev == 1)
      return DeepOcean;
   if(elev == 2)
      return ShallowOcean;
   
   return biomes[temp][moist];   
}

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   vec4 data = imageLoad(faceTex, outPos);
   uint bitfield = floatBitsToUint(data.a);
  
   uint temp = heatType(bitfield);
   uint moist = moistureType(bitfield);
   uint elev = elevationType(bitfield);

   bitfield = setBiomeType(bitfield, getBiomeType(temp, moist, elev));
   data.a = uintBitsToFloat(bitfield);

   imageStore(faceTex, outPos, data);
}