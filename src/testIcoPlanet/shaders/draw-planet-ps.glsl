#version 440

layout(location = 20) uniform sampler2D heightColorTex;
layout(location = 21) uniform vec4 overrideColor;
layout(location = 22) uniform bool useOverride;


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

in VertexStage
{
   flat float depth;
   smooth float height;
} vs_out;

out vec4 FragColor;

void main()
{
   float val = (vs_out.height + 1.0) / 2.0;
   if (val < 0) val = 0;
   if (val > 1) val = 1;
   vec4 texColor = texture(heightColorTex, vec2(val, 0.5));

   if (useOverride)
   {
      texColor = overrideColor;
   }

   FragColor = texColor;
  
   return;
   
}