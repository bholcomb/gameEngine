#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(rgba32f, binding = 0) uniform readonly image2D lowImg;
layout(rgba32f, binding = 1) uniform readonly image2D highImg;
layout(rgba32f, binding = 2) uniform readonly image2D controlImg;
layout(rgba32f, binding = 3) uniform writeonly image2D outputImg;

layout(location = 0 ) uniform float lowVal;
layout(location = 1 ) uniform float highVal;
layout(location = 2 ) uniform float threshold;
layout(location = 3 ) uniform float falloff;
layout(location = 4 ) uniform bool useLowImg;
layout(location = 5 ) uniform bool useHighImg;

float quinticBlend(float t)
{
   return t * t * t * (t * (t * 6 - 15) + 10);
}

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   
   float l = useLowImg ? imageLoad(lowImg, outPos).r : lowVal;
   float h = useHighImg ? imageLoad(highImg, outPos).r : highVal;
   float c = imageLoad(controlImg, outPos).r;
   float val = 0;

   if (falloff > 0.0)
   {
      if (c < (threshold - falloff))
      {
         // Lies outside of falloff area below threshold, return first source
         val = l;
      }
      if (c >(threshold + falloff))
      {
         // Lies outside of falloff area above threshold, return second source
         val = h;
      }
      // Lies within falloff area.
      float lower = threshold - falloff;
      float upper = threshold + falloff;
      float blend = quinticBlend((c - lower) / (upper - lower));
      val = mix(l, h, blend);
   }
   else
   {
    val = c < threshold ? l : h;
   }

   imageStore(outputImg, outPos, vec4(val));
}