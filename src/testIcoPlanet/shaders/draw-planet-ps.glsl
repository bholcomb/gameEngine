#version 430

layout(location = 20) uniform sampler2D tex;
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

in GeometryStage
{
   flat float depth;
} gs_out;

out vec4 FragColor;

void main()
{
   vec4 texColor = texture(tex, vec2(gs_out.depth / 22.0f, 0.5));

   if (useOverride)
      texColor = overrideColor;

   FragColor = texColor;
}