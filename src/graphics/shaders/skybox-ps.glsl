#version 430

layout(location = 20) uniform samplerCube skymap;

smooth in vec3 texCoord;

out vec4 FragColor;

void main()
{
   vec4 color = texture(skymap, texCoord);

	//FragColor = vec4(1, 0, color.b, 1);     
	FragColor = color;
}