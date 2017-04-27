#version 430

layout(local_size_x = 8, local_size_y = 8) in;

layout(r32f, binding = 0) uniform readonly image2D inputImg;

struct MinMax
{
   float min;
   float max;
};

layout(std430, binding = 1) buffer minMaxData
{
   MinMax minMax[];
};

shared float smin[64];
shared float smax[64];

void main()
{
   uint groupSize = gl_WorkGroupSize.x * gl_WorkGroupSize.y;
   uint groupId = gl_WorkGroupID.x + (gl_NumWorkGroups.x * gl_WorkGroupID.y);
   uint tid = gl_LocalInvocationIndex;
   ivec2 loc = ivec2(gl_GlobalInvocationID) * 4;

   //initial values
   float lmin = 1000.0;
   float lmax = -1000.0;

   //check 16 pixels in each thread
   for(int i=0; i< 4; i++)
   {
      for(int j=0; j< 4; j++)
      {
         float val = imageLoad(inputImg, loc + ivec2(i, j)).r;
         lmin = min(lmin, val);
         lmax = max(lmax, val);
      }
   }

   //store local min/max in shared memory
   smin[tid] = lmin;
   smax[tid] = lmax;

   barrier(); // may not be needed due to small block size (ATI=64 threads/warp Nvidia=32 threads/warp)

   int j=1;
   for(uint i = groupSize / 4; i > 0; i >>= 2)
   {
      if(tid < i)
      {
         uint idx = tid * (j * 4);
         float v1, v2, v3, v4;
         v1 = smin[idx];
         v2 = smin[idx + (1 * j)];
         v3 = smin[idx + (2 * j)];
         v4 = smin[idx + (3 * j)];
         smin[idx] = min(min(v1, v2), min(v3, v4));  
         
         v1 = smax[idx];
         v2 = smax[idx + (1 * j)];
         v3 = smax[idx + (2 * j)];
         v4 = smax[idx + (3 * j)];
         smax[idx] = max(max(v1, v2), max(v3, v4));
      }

      j *= 4;
   }

   if(tid == 0)
   {
      minMax[groupId].min = smin[0];
      minMax[groupId].max = smax[0];
   }
}