#version 430

layout(location = 0) uniform float time;
layout(location = 1) uniform float width;
layout(location = 2) uniform float height;

layout(location = 20) uniform sampler2D input;

smooth in vec2 texCoord;

out vec4 FragColor;

void main()
{   
   vec2 tex = texCoord;
   tex.x += sin(tex.y * 4 * 2 * 3.14159 + time) /  100;
   FragColor = texture2D(input, tex);
}