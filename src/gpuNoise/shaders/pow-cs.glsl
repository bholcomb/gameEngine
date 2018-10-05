#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform image2D inTex;
layout(r32f, binding = 1) uniform image2D outTex;

layout(location = 0) uniform float pow;

//---------------------------Image coordinate location functions--------------------------------
float sampledImageValue(ivec2 outputPos, image2D outputTex, image2D inputTex)
{
	ivec2 outSize = imageSize(outputTex);
	ivec2 inSize = imageSize(inputTex);
	vec2 ratio = vec2(outputPos) / vec2(outSize);
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	return imageLoad(inputTex, inputPos).r;
}

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
	
   float val = sampledImageValue(outPos, outTex, inTex);

   val = pow(val, pow);

   imageStore(outTex, outPos, vec4(val));
}