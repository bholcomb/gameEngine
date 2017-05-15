#version 450

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform readonly image2D input0;
layout(r32f, binding = 1) uniform readonly image2D input1;
layout(r32f, binding = 2) uniform readonly image2D input2;
layout(r32f, binding = 3) uniform readonly image2D input3;

layout(r32f, binding = 4) uniform writeonly image2D outTex;

layout(location = 0) uniform int numInputs;
layout(location = 1) uniform int action;

float sampledImageValue(ivec2 outTexPos, int image)
{
	ivec2 outSize = imageSize(outTex);

	ivec2 inSize;
	switch (image)
	{
		case 0: inSize = imageSize(input0); break;
		case 1: inSize = imageSize(input1); break;
		case 2: inSize = imageSize(input2); break;
		case 3: inSize = imageSize(input3); break;
	}

	vec2 ratio = vec2(outTexPos) / vec2(outSize);
	vec2 sampledLoc = vec2(inSize) * ratio;
	ivec2 inputPos = ivec2(floor(sampledLoc.x), floor(sampledLoc.y));

	float val = 0;
	switch (image)
	{
	case 0: val = imageLoad(input0, inputPos).r; break;
	case 1: val = imageLoad(input1, inputPos).r; break;
	case 2: val = imageLoad(input2, inputPos).r; break;
	case 3: val = imageLoad(input3, inputPos).r; break;
	}

	return val;
}


float add(ivec2 pixelLoc)
{
   float ret = 0.0;
   switch (numInputs)
   {
		case 4: ret += sampledImageValue(pixelLoc, 3); //fallthrough
		case 3: ret += sampledImageValue(pixelLoc, 2); //fallthrough
		case 2: ret += sampledImageValue(pixelLoc, 1); //fallthrough
		case 1: ret += sampledImageValue(pixelLoc, 0);
   }

   return ret;
}

float mul(ivec2 pixelLoc)
{
   float ret = 1.0;
   switch (numInputs)
   {
	case 4: ret *= sampledImageValue(pixelLoc, 3); //fallthrough
	case 3: ret *= sampledImageValue(pixelLoc, 2); //fallthrough
	case 2: ret *= sampledImageValue(pixelLoc, 1); //fallthrough
	case 1: ret *= sampledImageValue(pixelLoc, 0);
   }

   return ret;
}

float maximum(ivec2 pixelLoc)
{
   float ret = -1000.0;
   switch (numInputs)
   {
	case 4: ret = max(ret, sampledImageValue(pixelLoc, 3)); //fallthrough
	case 3: ret = max(ret, sampledImageValue(pixelLoc, 2)); //fallthrough
	case 2: ret = max(ret, sampledImageValue(pixelLoc, 1)); //fallthrough
	case 1: ret = max(ret, sampledImageValue(pixelLoc, 0));
   }

   return ret;
}

float minimum(ivec2 pixelLoc)
{
   float ret = 1000.0;
   switch (numInputs)
   {
	case 4: ret = min(ret, sampledImageValue(pixelLoc, 3)); //fallthrough
	case 3: ret = min(ret, sampledImageValue(pixelLoc, 2)); //fallthrough
	case 2: ret = min(ret, sampledImageValue(pixelLoc, 1)); //fallthrough
	case 1: ret = min(ret, sampledImageValue(pixelLoc, 0));
   }

   return ret;
}

float avg(ivec2 pixelLoc)
{
   float ret = add(pixelLoc);
   return ret / numInputs;
}

void main()
{
   ivec2 pixelLoc = ivec2(gl_GlobalInvocationID.xy);
   
   float val = 0;

   /*
   public enum CombinerType
      {
         Add,
         Multiply,
         Max,
         Min,
         Average
      }
   */

   switch (action)
   {
      case 0: val = add(pixelLoc); break;
      case 1: val = mul(pixelLoc); break;
      case 2: val = maximum(pixelLoc); break;
      case 3: val = minimum(pixelLoc); break;
      case 4: val = avg(pixelLoc); break;
   }

   imageStore(outTex, pixelLoc, vec4(val));
}