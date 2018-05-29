#version 440
#extension GL_ARB_gpu_shader_fp64 : enable

layout(location = 0) in dvec3 position;
layout(location = 1) in float depth;


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

void main()
{
   dvec4 outPos = dmat4(viewProj) * dvec4(position, 1);
   gl_Position = vec4(outPos);
}
