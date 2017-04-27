#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(rgba32f, binding = 0) uniform image2D texture;
layout(std430, binding = 1) buffer minMax
{
   float min;
   float max;
};

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);

   float val = imageLoad(texture, outPos).x;

   //correct the value based on the minmaxd
   float ratio = 1.0 / (max - min);
   val =  ratio * (val - min);

   //write it back out
   imageStore(texture, outPos, vec4(val));
}