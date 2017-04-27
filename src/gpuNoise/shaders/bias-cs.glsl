#version 430

layout(local_size_x = 32, local_size_y = 32) in;

layout(rgba32f, binding = 0) uniform image2D tex;

layout(location = 0) uniform float bias;

#define LOG_0_5 -0.30102999566f // log(0.5)

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   
   float val = imageLoad(tex, outPos).r;

   val = pow(val, log(bias) / LOG_0_5);

   imageStore(tex, outPos, vec4(val));
}