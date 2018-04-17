#version 430

layout(location = 0) in vec3 position;
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

out VertexStage
{
   flat float depth;
} vs_out;


void main()
{
   mat4 MVP = viewProj;
   
   vs_out.depth = depth;
   gl_Position = MVP * vec4(position, 1);
}
