#version 330

uniform sampler2D input;
uniform float time;
smooth in vec2 texCoord;

out vec4 FragColor;

void main()
{   
   vec2 tex = texCoord;
   tex.x += sin(tex.y * 4 * 2 * 3.14159 + time) /  100;
   FragColor = texture2D(input, tex);
}