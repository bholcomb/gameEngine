#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(rgba32f, binding = 0) uniform image2D faceTex;
layout(r32f, binding = 1) uniform image2D heatmap;

layout(location = 2) uniform int face;

#define TWO_PI 6.28318530718
#define PI 3.14159265359

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

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   vec2 texCoord = vec2(outPos) / vec2(imageSize(faceTex));
   vec3 sampleVec = generateSampleVec(face, texCoord);
   vec2 heatSample = cylinderProjection(sampleVec);
   ivec2 heatPos = ivec2(heatSample * vec2(imageSize(heatmap)));

   vec4 data = imageLoad(faceTex, outPos);
   uint bitfield = floatBitsToUint(data.a);
   uint heightType = heightType(bitfield);

   float elv = data.r;
   float heat = imageLoad(heatmap, heatPos ).r;

   // Adjust Heat Map based on Height - Higher == colder
   switch (heightType)
   {
      case Grass: heat -=  (0.1f * elv);  break;
      case Forest: heat -= (0.2f * elv); break;
      case Rock: heat -=   (0.3f * elv); break;
      case Snow: heat -=   (0.4f * elv); break;
   }

   bitfield = setHeatType(bitfield, getHeatType(heat));

   data.g = heat;
   data.a = uintBitsToFloat(bitfield);

   imageStore(faceTex, outPos, data);
}