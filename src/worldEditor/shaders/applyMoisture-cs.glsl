#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform image2D moisturemap;
layout(rgba32f, binding = 1) uniform image2D faceTex;

layout(location = 2) uniform int face;

#define TWO_PI 6.28318530718

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


vec3 generateSampleVec(int f, vec2 uv)
{
   //map input coord from 0..1 to -1..1
   vec2 t = uv * 2 - 1.0;
   vec3 ret;
   switch (f)
   {
   case 0: // +x
      ret = vec3(1, t.y, -t.x); break;
   case 1: // -x
      ret = vec3(-1, t.y, t.x); break;
   case 2: // +y
      ret = vec3(t.x, -1, t.y); break;
   case 3: // -y 
      ret = vec3(t.x, 1, -t.y); break;
   case 4: // +z
      ret = vec3(t.x, t.y, 1); break;
   case 5: // -z
      ret = vec3(-t.x, t.y, -1); break;
   }

   return normalize(ret);
}

vec2 cylinderProjection(vec3 dir)
{
   float theta = 0.0;
   vec2 t = normalize(dir.xz);
   if (t.x != 0)
   {
      theta = atan(t.y, t.x);
   }

   if (theta <= 0)
   {
      theta += TWO_PI;
   }

   float x = theta / TWO_PI;
   float y = (dir.y + 1.0) / 2.0;
   return vec2(x,y);
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

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   vec2 texCoord = vec2(outPos) / vec2(imageSize(faceTex));
   vec3 sampleVec = generateSampleVec(face, texCoord);
   vec2 heatSample = cylinderProjection(sampleVec);
   ivec2 heatPos = ivec2(heatSample * vec2(imageSize(moisturemap)));

   vec4 data = imageLoad(faceTex, outPos);
   uint bitfield = floatBitsToUint(data.a);
   uint heightType = heightType(bitfield);

   float moist = imageLoad(moisturemap, heatPos).r;

   // Adjust Heat Map based on Height - Higher == colder
   switch (heightType)
   {
      case DeepWater: moist += 8.0 * data.r;  break;
      case ShallowWater: moist += 3.0 * data.r; break;
      case Shore: moist += 1.0 * data.r; break;
      case Sand: moist += 0.25f * data.r; break;
   }

   bitfield = setMoistureType(bitfield, getMoistureType(moist));

   data.b = moist;
   data.a = uintBitsToFloat(bitfield);

   imageStore(faceTex, outPos, data);
}