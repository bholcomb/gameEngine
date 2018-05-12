#version 440
#extension GL_ARB_gpu_shader_fp64 : enable

layout(location = 0) in dvec3 position;
layout(location = 1) in float depth;


layout(location = 0) uniform float maxHeight;
layout(location = 1) uniform samplerCube cubemap;


layout(std140, binding = 0) uniform camera{
   mat4 view; //aligned 4N
   mat4 projection; //aligned 4N
   mat4 viewProj; //aligned 4N
   mat4 ortho; //aligned 4N
   vec4 viewVector; //aligned 4N
   vec4 eyeLocation; //aligned 4N
   float left, right, top, bottom; //aligned 4N
   float zNear, zFar, aspect, fov; //aligned 4N
   int frame;
   float dt;
};

out VertexStage
{
   flat float depth;
   smooth float height;
} vs_out;


void main()
{
   vec3 nor = vec3(normalize(position));
   float val = texture(cubemap, nor).r; //elevation sample from cubemap
   val = (val * 2.0) - 1.0; //convert from 0..1 to -1..1
   float terrainHeight = maxHeight * val;  //scaled from -1..1 to -maxheight..maxheight
   nor = nor * terrainHeight; //get vector size in height

   dvec4 outPos = dmat4(viewProj) * dvec4(position + nor, 1);
   gl_Position = vec4(outPos);
   vs_out.depth = depth;
   vs_out.height = val;
}
