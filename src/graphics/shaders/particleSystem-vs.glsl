#version 430

layout(location = 0) in vec3 position;
layout(location = 1) in vec4 color;
layout(location = 2) in vec3 size;
layout(location = 3) in float rotation;

layout(location = 0) uniform mat4 model;

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

out float pRot;
out vec3 pSize;
out vec4 pColor;

void main()
{
	pColor = color;
   pSize = size;
	pRot = rotation;

	gl_Position = model * vec4(position,1);
}
