#version 430

layout(location = 0) in vec3 position;

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

smooth out vec3 texCoord;

void main()
{
   mat4 viewOri = mat4(mat3(view));
   vec4 pos = projection * viewOri * vec4(position,1);
   gl_Position = pos.xyww;
   texCoord = position.xyz;
}
