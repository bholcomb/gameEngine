#version 430

layout(location = 20) uniform sampler2D diffuseTexture;
layout(location = 21) uniform vec4 color;

smooth in vec2 texCoord;

out vec4 FragColor;

void main()
{   
	FragColor = texture(diffuseTexture, texCoord)  * color;
}