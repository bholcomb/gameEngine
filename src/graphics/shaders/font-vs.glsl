#version 430

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;

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
layout(location = 1) uniform bool is3d;

smooth out vec2 texCoord;

void main()
{
   texCoord=uv;

   mat4 MVP;
   if(is3d==true)
   {
      //need to billboard here or in GS
      MVP = view * model;
   }
   else
   {
      MVP = ortho * model;
   }

   gl_Position = MVP * vec4(position,1);
}
