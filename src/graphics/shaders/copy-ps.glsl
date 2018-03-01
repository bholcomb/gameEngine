#version 430

layout(location = 0) uniform float time;
layout(location = 1) uniform float width;
layout(location = 2) uniform float height;

layout(location = 20) uniform sampler2D source;

smooth in vec2 texCoord;

out vec4 FragColor;

void main()
{   
   FragColor = texture(source, texCoord);
}