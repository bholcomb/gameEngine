#version 430

layout(local_size_x = 32, local_size_y = 32) in;


layout(r32f, binding = 0) uniform readonly image2D lowImg;
layout(r32f, binding = 1) uniform readonly image2D highImg;
layout(r32f, binding = 2) uniform readonly image2D controlImg;
layout(r32f, binding = 3) uniform writeonly image2D outputImg;

layout(location = 0 ) uniform float threshold;
layout(location = 1 ) uniform float falloff;

ivec2 outSize;
vec2 ratio;

float quinticBlend(float t)
{
   return t * t * t * (t * (t * 6 - 15) + 10);
}

float sampleLowValue()
{
	ivec2 inSize = imageSize(lowImg); ;
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	return imageLoad(lowImg, inputPos).r;
}

float sampleHighValue()
{
	ivec2 inSize = imageSize(highImg); ;
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	return imageLoad(highImg, inputPos).r;
}

float sampleControlValue()
{
	ivec2 inSize = imageSize(controlImg); ;
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	return imageLoad(controlImg, inputPos).r;
}


void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
	outSize = imageSize(outputImg);
	ratio = vec2(outPos) / vec2(outSize);

   float l = sampleLowValue();
   float h = sampleHighValue();
   float c = sampleControlValue();
   float val = 0;

	if (falloff > 0.0)
	{
		if (c < (threshold - falloff))
		{
			// Lies outside of falloff area below threshold, return first source
			val = l;
		}
		else if (c > (threshold + falloff))
		{
			// Lies outside of falloff area above threshold, return second source
			val = h;
		}
		else
		{
			// Lies within falloff area.
			float lower = threshold - falloff;
			float upper = threshold + falloff;
			float blend = quinticBlend((c - lower) / (upper - lower));
			val = mix(l, h, blend);
		}
   }
   else
   {
    val = c < threshold ? l : h;
   }

   imageStore(outputImg, outPos, vec4(val));
}