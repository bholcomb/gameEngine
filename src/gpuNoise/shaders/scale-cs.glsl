#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform readonly image2D scaleImg;
layout(r32f, binding = 1) uniform readonly image2D scaleVal;
layout(r32f, binding = 2) uniform image2D outputImg;

float sampleScaleImage(ivec2 outTexPos)
{
	ivec2 outSize = imageSize(outputImg);

	ivec2 inSize = imageSize(scaleImg); ;
	vec2 ratio = vec2(outTexPos) / vec2(outSize);
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	float val = imageLoad(scaleImg, inputPos).r;
	return val;
}

float sampleScaleValue(ivec2 outTexPos)
{
	ivec2 outSize = imageSize(outputImg);

	ivec2 inSize = imageSize(scaleVal); ;
	vec2 ratio = vec2(outTexPos) / vec2(outSize);
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	float val = imageLoad(scaleVal, inputPos).r;
	return val;
}



void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   
	float s = sampleScaleValue(outPos);
	float i = sampleScaleImage(outPos);
      
   i *= s;

   imageStore(outputImg, outPos, vec4(i));
}