#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform image2D heatTex;
layout(rgba32f, binding = 1) uniform image2D outputTex;

#define setHeatType(x, y) bitfieldInsert(x, y, 8, 4)
#define heightType(x) bitfieldExtract(x, 12, 4)

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

//heat values
#define ColdestVal 0.05f
#define ColderVal 0.18f
#define ColdVal 0.4f
#define WarmVal 0.6f
#define WarmerVal 0.8f

uint getHeatType(float heat)
{
   if (heat < ColdestVal) return Coldest;
   if (heat < ColderVal) return Colder;
   if (heat < ColdVal) return Cold;
   if (heat < WarmVal) return Warm;
   if (heat < WarmerVal) return Warmer;

   return Warmest;
}

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);

   vec4 data = imageLoad(outputTex, outPos);
   uint bitfield = floatBitsToUint(data.a);
   uint heightType = heightType(bitfield);

   float elv = data.r;
   float heat = imageLoad(heatTex, outPos).x;

   // Adjust Heat Map based on Height - Higher == colder
   switch (heightType)
   {
      case Grass: heat -=  (0.1f * elv); break;
      case Forest: heat -= (0.2f * elv); break;
      case Rock: heat -=   (0.3f * elv); break;
      case Snow: heat -=   (0.4f * elv); break;
   }

   bitfield = setHeatType(bitfield, getHeatType(heat));

   data.g = heat;
   data.a = uintBitsToFloat(bitfield);

   imageStore(outputTex, outPos, data);
}