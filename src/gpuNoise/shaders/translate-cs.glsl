#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(rgba32f, binding = 0) uniform readonly image2D transXImg;
layout(rgba32f, binding = 1) uniform readonly image2D transYImg;
layout(rgba32f, binding = 2) uniform readonly image2D inputImg;
layout(rgba32f, binding = 3) uniform writeonly image2D outputImg;

layout(location = 0) uniform float transXVal;
layout(location = 1) uniform float transYVal;
layout(location = 2) uniform bool useTransXImg;
layout(location = 3) uniform bool useTransYImg;

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy)
   float x = useTransXImg ? imageLoad(transXImg, outPos).r : transXVal;
   float y = useTransYImg ? imageLoad(transYImg, outPos).r : transYVal;

   outPos += ivec2(x,y);
   
   float val = imageLoad(inputImg, outPos).r;
   
   imageStore(outputImg, outPos, vec4(val));
}