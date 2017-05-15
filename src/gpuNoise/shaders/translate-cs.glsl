#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform readonly image2D transXImg;
layout(r32f, binding = 1) uniform readonly image2D transYImg;
layout(r32f, binding = 2) uniform readonly image2D inputImg;
layout(r32f, binding = 3) uniform writeonly image2D outputImg;

ivec2 outSize;
vec2 ratio;


float sampleXVal()
{
	ivec2 inSize = imageSize(transXImg); ;
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	return imageLoad(transXImg, inputPos).r;
}

float sampleYVal()
{
	ivec2 inSize = imageSize(transYImg); ;
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	return imageLoad(transYImg, inputPos).r;
}

float sampleInputVal()
{
	ivec2 inSize = imageSize(inputImg); ;
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	return imageLoad(inputImg, inputPos).r;
}

void main()
{
	ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
	outSize = imageSize(outputImg);
	ratio = vec2(outPos) / vec2(outSize);

   float x = sampleXVal();
   float y = sampleYVal();

   ivec2 newPos = outPos + ivec2(x,y);
	ratio = vec2(newPos) / vec2(outSize);
   
   float val = sampleInputVal();
   
   imageStore(outputImg, outPos, vec4(val));
}