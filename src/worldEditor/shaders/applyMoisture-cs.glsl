#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform image2D moistureTex;
layout(rgba32f, binding = 1) uniform image2D outputTex;


#define heightType(x) bitfieldExtract(x, 12, 4)
#define setMoistureType(x, y) bitfieldInsert(x, y, 4, 4)

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

//moisture types
#define Wettest 5
#define Wetter 4
#define Wet 3
#define Dry 2
#define Dryer 1
#define Dryest 0

//moisture values
#define DryerVal 0.15f
#define DryVal 0.3f
#define WetVal 0.45f
#define WetterVal 0.6f
#define WettestVal 0.75f

uint getMoistureType(float moist)
{
   if (moist < DryerVal) return Dryest;
   if (moist < DryVal) return Dryer;
   if (moist < WetVal) return Dry;
   if (moist < WetterVal) return Wet;
   if (moist < WettestVal) return Wetter;

   return Wettest;
}

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
  
   vec4 data = imageLoad(outputTex, outPos);
   uint bitfield = floatBitsToUint(data.a);
   uint heightType = heightType(bitfield);

   float elv = data.r;
   float moist = imageLoad(moistureTex, outPos).r;

   // Adjust Heat Map based on Height - Higher == colder
   switch (heightType)
   {
      case DeepWater: moist +=    (8.0 * elv);  break;
      case ShallowWater: moist += (3.0 * elv); break;
      case Shore: moist +=        (1.0 * elv); break;
      case Sand: moist +=         (0.25f * elv); break;
   }

   bitfield = setMoistureType(bitfield, getMoistureType(moist));

   data.b = moist;
   data.a = uintBitsToFloat(bitfield);

   imageStore(outputTex, outPos, data);
}