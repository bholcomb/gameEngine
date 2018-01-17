#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform readonly image2D scaleImg;
layout(r32f, binding = 1) uniform image2D outputImg;

layout(location = 2) uniform float x;
layout(location = 3) uniform float y;

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   ivec2 readPos = ivec2(vec2(outPos) * vec2(x,y));
   
   float data = imageLoad(scaleImage, readPos).r;
  
   imageStore(outputImg, outPos, vec4(data));
}