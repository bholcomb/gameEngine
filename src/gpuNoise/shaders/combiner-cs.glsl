#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(binding = 0) uniform readonly image2D input0;
layout(binding = 1) uniform readonly image2D input1;
layout(binding = 2) uniform readonly image2D input2;
layout(binding = 3) uniform readonly image2D input3;
layout(binding = 4) uniform readonly image2D input4;
layout(binding = 5) uniform readonly image2D input5;
layout(binding = 6) uniform readonly image2D input6;
layout(binding = 7) uniform readonly image2D input7;

layout(rgba32f, binding = 8) uniform writeonly image2D output;

layout(location = 0) uniform int numInputs;
layout(location = 1) uniform int action;


float add(ivec2 pixelLoc)
{
   float ret = 0.0;
   switch (numInputs)
   {
      case 8: ret += imageLoad(input7, pixelLoc).r; //fallthrough
      case 7: ret += imageLoad(input6, pixelLoc).r; //fallthrough
      case 6: ret += imageLoad(input5, pixelLoc).r; //fallthrough
      case 5: ret += imageLoad(input4, pixelLoc).r; //fallthrough
      case 4: ret += imageLoad(input3, pixelLoc).r; //fallthrough
      case 3: ret += imageLoad(input2, pixelLoc).r; //fallthrough
      case 2: ret += imageLoad(input1, pixelLoc).r; //fallthrough
      case 1: ret += imageLoad(input0, pixelLoc).r;
   }

   return ret;
}

float mul(ivec2 pixelLoc)
{
   float ret = 1.0;
   switch (numInputs)
   {
   case 8: ret *= imageLoad(input7, pixelLoc).r; //fallthrough
   case 7: ret *= imageLoad(input6, pixelLoc).r; //fallthrough
   case 6: ret *= imageLoad(input5, pixelLoc).r; //fallthrough
   case 5: ret *= imageLoad(input4, pixelLoc).r; //fallthrough
   case 4: ret *= imageLoad(input3, pixelLoc).r; //fallthrough
   case 3: ret *= imageLoad(input2, pixelLoc).r; //fallthrough
   case 2: ret *= imageLoad(input1, pixelLoc).r; //fallthrough
   case 1: ret *= imageLoad(input0, pixelLoc).r;
   }

   return ret;
}

float max(ivec2 pixelLoc)
{
   float ret = -1000.0;
   switch (numInputs)
   {
   case 8: ret = max(ret, imageLoad(input7, pixelLoc).r); //fallthrough
   case 7: ret = max(ret, imageLoad(input6, pixelLoc).r); //fallthrough
   case 6: ret = max(ret, imageLoad(input5, pixelLoc).r); //fallthrough
   case 5: ret = max(ret, imageLoad(input4, pixelLoc).r); //fallthrough
   case 4: ret = max(ret, imageLoad(input3, pixelLoc).r); //fallthrough
   case 3: ret = max(ret, imageLoad(input2, pixelLoc).r); //fallthrough
   case 2: ret = max(ret, imageLoad(input1, pixelLoc).r); //fallthrough
   case 1: ret = max(ret, imageLoad(input0, pixelLoc).r);
   }

   return ret;
}

float min(ivec2 pixelLoc)
{
   float ret = 1000.0;
   switch (numInputs)
   {
   case 8: ret = min(ret, imageLoad(input7, pixelLoc).r); //fallthrough
   case 7: ret = min(ret, imageLoad(input6, pixelLoc).r); //fallthrough
   case 6: ret = min(ret, imageLoad(input5, pixelLoc).r); //fallthrough
   case 5: ret = min(ret, imageLoad(input4, pixelLoc).r); //fallthrough
   case 4: ret = min(ret, imageLoad(input3, pixelLoc).r); //fallthrough
   case 3: ret = min(ret, imageLoad(input2, pixelLoc).r); //fallthrough
   case 2: ret = min(ret, imageLoad(input1, pixelLoc).r); //fallthrough
   case 1: ret = min(ret, imageLoad(input0, pixelLoc).r);
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
      case 2: val = max(pixelLoc); break;
      case 3: val = min(pixelLoc); break;
      case 4: val = avg(pixelLoc); break;
   }

   imageStore(tex, outPos, vec4(dp));
}