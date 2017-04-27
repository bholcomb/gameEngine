#version 430

layout(location = 20) uniform samplerCube cubemap;

smooth in vec3 texCoord;

out vec4 FragColor;

void main()
{
   float color = texture(cubemap, texCoord).r;

   if (color > 0.4)
      FragColor = vec4(color, color, color, 1.0);
   else
      FragColor = vec4(0.0, 0.0, 1.0 + 2*(color-0.4), 1.0);
}