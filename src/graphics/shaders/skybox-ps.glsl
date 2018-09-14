#version 430

layout(location = 20) uniform samplerCube skymap;

smooth in vec3 texCoord;

out vec4 FragColor;

void main()
{
   vec4 color = texture(skymap, texCoord);
   color.rgb = color.rgb;

   FragColor = color;
}