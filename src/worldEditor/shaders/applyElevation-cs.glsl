#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform image2D elvTex;
layout(rgba32f, binding = 1) uniform image2D faceTex;

layout(location = 2) uniform int face;

//height values
#define DeepWaterVal 0.1f
#define ShallowWaterVal 0.3f
#define SandVal 0.4f
#define GrassVal 0.5f
#define ForestVal 0.8f
#define RockVal 0.9f

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

//group types
#define Water 0
#define Land 1

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

uint getGroupType(float elev)
{
   if (elev < ShallowWaterVal) return Water;

   return Land;
}

#define setElevationType(x, y) bitfieldInsert(x, y, 12, 4)
#define setGroupType(x, y) bitfieldInsert(x, y, 16, 4)


void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy); 
   float elev = imageLoad(elvTex, outPos).x;
   
   vec4 data;

   //init these values
   data.r = elev;
   data.g = 0.0;
   data.b = 0.0;
   data.a = 0.0;
   
   uint bitfield = 0;
   bitfield = setElevationType(bitfield, getElevationType(elev));
   bitfield = setGroupType(bitfield, getGroupType(elev));
   data.a = uintBitsToFloat(bitfield);

   imageStore(faceTex, outPos, data);
}