#version 450

layout(local_size_x = 32, local_size_y = 32) in;

layout(r32f, binding = 0) uniform image2D tex;
layout(r32f, binding = 1) uniform image2D outTex;
layout(std430, binding = 1) buffer minMax
{
   float min;
   float max;
};

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);

   float val = imageLoad(tex, outPos).x;

   //correct the value based on the minmaxd
   float ratio = 1.0 / (max - min);
   val =  ratio * (val - min);
	//val = abs(sin(val));

   //write it back out
   imageStore(outTex, outPos, vec4(val) );
}