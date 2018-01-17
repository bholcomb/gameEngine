#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform writeonly image2D tex;

layout(location = 0) uniform float x0;
layout(location = 1) uniform float x1;
layout(location = 2) uniform float y0;
layout(location = 3) uniform float y1;

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   vec2 normalizedCoord = vec2(outPos) / vec2(imageSize(tex));

   float mx = x1 - x0;
   float my = y1 - y0;
   float len = (mx * mx + my * my);
   
   float dx = normalizedCoord.x - x0;
   float dy = normalizedCoord.y - y0;
   float dp = dx * mx + dy * my;
   dp /= len;
   
   imageStore(tex, outPos, vec4(dp));
}