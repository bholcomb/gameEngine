#version 430

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec4 color;

layout(std140, binding = 0) uniform camera {
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

layout(location = 0) uniform mat4 model;

smooth out vec2 texCoord;
smooth out vec4 vecColor;

void main()
{
   texCoord=uv;
   vecColor = color;
   gl_Position= ortho * model * vec4(position, 0, 1);
}
