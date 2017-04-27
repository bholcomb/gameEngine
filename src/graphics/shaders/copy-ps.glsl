#version 330

uniform sampler2D input;
uniform float time;
smooth in vec2 texCoord;

out vec4 FragColor;

void main()
{   
   FragColor = texture2D(input, texCoord);
}