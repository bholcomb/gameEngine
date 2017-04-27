#version 330

uniform sampler2D terrainDepth;
uniform sampler2D waterDepth;
uniform sampler2D waterColor;
uniform sampler2D input;

layout(std140) uniform camera {
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

smooth in vec2 texCoord;

out vec4 FragColor;

float eyeDepth(sampler2D sampler)
{
  float zBuffer = texture2D(sampler, texCoord).x;
  float zNormalized= 2.0 * zBuffer - 1.0;
  return 2.0 * zNear * zFar / (zFar + zNear - zNormalized * (zFar - zNear));
}

float linearDepth(sampler2D sampler)
{
  float z = texture2D(sampler, texCoord).x;
  return (2.0 * zNear) / (zFar + zNear - z * (zFar - zNear));	
}

void main()
{
  float maxDistance=100.0;

  float td = linearDepth(terrainDepth);
  float wd = linearDepth(waterDepth);

  if(wd >= 0.99)
   discard;

  if(wd > td) 
   discard;

  vec4 color = texture2D(input, texCoord);
  vec4 wc = texture2D(waterColor, texCoord);

  FragColor=mix(color, wc, clamp((wd * 10) + 0.25, 0, 1));
}