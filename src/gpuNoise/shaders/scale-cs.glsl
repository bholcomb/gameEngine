#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(rgba32f, binding = 0) uniform readonly image2D scaleImg;
layout(rgba32f, binding = 1) uniform image2D outputImg;

layout(location = 0) uniform float scaleVal;
layout(location = 1) uniform bool useScaleImg;

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   
   float s = useScaleImg ? imageLoad(scaleImg, outPos).r : scaleVal;
   float val = imageLoad(outputImg, outPos).r;
   
   val *= s;

   imageStore(outputImg, outPos, vec4(val));
}