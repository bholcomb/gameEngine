#version 430

layout(location = 20) uniform samplerCube skymap;
layout(location = 21) uniform float intensity;

smooth in vec3 texCoord;

out vec4 FragColor;

void main()
{
   vec4 color = texture(skymap, texCoord);
   color.rgb = color.rgb * intensity;

   FragColor = color;
}