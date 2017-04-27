#version 330

uniform sampler2D input;
uniform sampler2D depth;
uniform vec3 fogColor;
uniform float maxDistance;
uniform float time;
uniform float width;
uniform float height;

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
smooth in vec3 fragVec;
flat in float p33;
flat in float p34;

out vec4 FragColor;

float depthReconstruction()
{
  float z = texture2D(depth, texCoord).x;
  z = p34 / (z + p33);
  return z;
}

vec3 eyeFragReconstruction2()
{ 
   return fragVec * depthReconstruction();
}

vec3 eyeFragReconstruction()
{
   float zpix = texture2D(depth, texCoord).x;
   float zndc = zpix * 2.0 - 1.0;
   float zeye = 2 * zFar * zNear / (zndc * (zFar - zNear) - (zFar + zNear));

   float xndc = gl_FragCoord.x / width * 2.0 - 1.0;
   float yndc = gl_FragCoord.y / height * 2.0 - 1.0;

   float xeye = -zeye * (xndc * (right -left) + (right + left)) / (2.0 * zNear);
   float yeye = -zeye * (yndc * (top -bottom) + (top + bottom)) / (2.0 * zNear);
   
   return vec3(xeye, yeye, zeye);
}

void main()
{
  vec4 color = texture2D(input, texCoord);

  float d = depthReconstruction();
  float fogAmount = 1.0 - exp( -depthReconstruction() * (1/maxDistance));
  //float fogAmount = d / maxDistance;
  
  if(fogAmount > 0.9)
   fogAmount = 0.9;
    
  FragColor = vec4(mix(color.rgb, fogColor.rgb, fogAmount), 1);
}
