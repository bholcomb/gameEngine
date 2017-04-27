#version 430

layout(local_size_x = 32) in;

struct MinMax
{
   float min;
   float max;
};

layout(std430, binding = 1) buffer minMaxData
{
   MinMax minMax[];
};

layout(location = 0) uniform int count;

shared float smin[32];
shared float smax[32];

void main()
{
   uint offset = count / 32;
   uint gidx = gl_GlobalInvocationID.x * offset; 
   uint tid = gl_LocalInvocationID.x;
   
   //initial values
   float lmin = 1000.0;
   float lmax = -1000.0;

   for(int i=0; i < offset; i++)
   {
      float tmin = minMax[gidx + i].min;
      float tmax = minMax[gidx + i].max;
         
      lmin = min(lmin, tmin);
      lmax = max(lmax, tmax);
   }

   smin[tid] = lmin;
   smax[tid] = lmax;

   barrier(); // may not be needed due to small block size

   if(tid == 0)
   {
      lmin = 1000.0;
      lmax = -1000.0;
      for(int i=0; i< 32; i++)
      {
         lmin = min(lmin, smin[i]);
         lmax = max(lmax, smax[i]);
      }
      
      minMax[0].min = lmin;
      minMax[0].max = lmax;
   }
}
