#version 430

layout(location = 20) uniform sampler2D diffuseTexture;

smooth in vec2 uv;
smooth in vec4 color;

out vec4 FragColor;

void main()
{   
	vec4 c=texture(diffuseTexture, uv);
	c.w *=color.w;

	if(c.w < 0.2)
		discard;

	FragColor=c;
}