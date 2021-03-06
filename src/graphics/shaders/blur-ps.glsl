#version 430

layout(location = 0) uniform float time;
layout(location = 1) uniform float width;
layout(location = 2) uniform float height;

layout(location = 20) uniform sampler2D source;
layout(location = 21) uniform sampler2D depth;
layout(location = 22) uniform int blurDirection;

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



float linearDepth()
{
  float n = zNear; // camera z near
  float f = zFar; // camera z far
  float z = texture(depth, texCoord).x;
  return (2.0 * n) / (f + n - z * (f - n));	
}

vec4 horizontalBlur(float blurSize)
{
   // blur in x (horizontal)
   // take nine samples, with the distance blurSize between them
   vec4 sum = vec4(0.0);
   sum += texture(source, vec2(texCoord.x - 4.0*blurSize, texCoord.y)) * 0.05;
   sum += texture(source, vec2(texCoord.x - 3.0*blurSize, texCoord.y)) * 0.09;
   sum += texture(source, vec2(texCoord.x - 2.0*blurSize, texCoord.y)) * 0.12;
   sum += texture(source, vec2(texCoord.x - blurSize, texCoord.y)) * 0.15;
   sum += texture(source, vec2(texCoord.x, texCoord.y)) * 0.16;
   sum += texture(source, vec2(texCoord.x + blurSize, texCoord.y)) * 0.15;
   sum += texture(source, vec2(texCoord.x + 2.0*blurSize, texCoord.y)) * 0.12;
   sum += texture(source, vec2(texCoord.x + 3.0*blurSize, texCoord.y)) * 0.09;
   sum += texture(source, vec2(texCoord.x + 4.0*blurSize, texCoord.y)) * 0.05;
   return sum;
}

vec4 verticalBlur(float blurSize)
{
   // blur in x (horizontal)
   // take nine samples, with the distance blurSize between them
   vec4 sum = vec4(0.0);
   sum += texture(source, vec2(texCoord.x, texCoord.y + 4.0*blurSize)) * 0.05;
   sum += texture(source, vec2(texCoord.x, texCoord.y - 4.0*blurSize)) * 0.05;
   sum += texture(source, vec2(texCoord.x, texCoord.y - 3.0*blurSize)) * 0.09;
   sum += texture(source, vec2(texCoord.x, texCoord.y - 2.0*blurSize)) * 0.12;
   sum += texture(source, vec2(texCoord.x, texCoord.y - blurSize)) * 0.15;
   sum += texture(source, vec2(texCoord.x, texCoord.y)) * 0.16;
   sum += texture(source, vec2(texCoord.x, texCoord.y + blurSize)) * 0.15;
   sum += texture(source, vec2(texCoord.x, texCoord.y + 2.0*blurSize)) * 0.12;
   sum += texture(source, vec2(texCoord.x, texCoord.y + 3.0*blurSize)) * 0.09;
   return sum;
}

void main()
{  
   vec4 color;
   float depth = 1.0 / zFar;

   if(blurDirection == 0)
	   color = horizontalBlur(depth);
   else
      color = verticalBlur(depth);

   FragColor = vec4(color.rgb, 1);
}