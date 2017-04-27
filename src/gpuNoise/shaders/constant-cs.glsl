#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(rgba32f, binding = 0) uniform image2D outputImg;

layout(location = 0) uniform float constant;

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
  
   imageStore(outputImg, outPos, vec4(constant));
}