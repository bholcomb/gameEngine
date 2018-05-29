#version 430

layout(location = 20) uniform sampler2D tex;

smooth in vec2 texCoord;
smooth in vec4 vecColor;

out vec4 FragColor;

void main()
{  
   FragColor = texture(tex, texCoord) * vecColor;
}