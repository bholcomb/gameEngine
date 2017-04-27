#version 430

layout(location = 20) uniform bool hasDiffuseMap;
layout(location = 21) uniform sampler2D diffuseMap;
layout(location = 22) uniform vec4 color;
layout(location = 23) uniform float alpha;

in VertexStage
{
	smooth vec2 texCoord;
	smooth vec3 worldNormal;
	smooth vec3 worldVert;
	mat3 normalMatrix;
};


out vec4 FragColor;

void main()
{   
	vec4 outColor = color;
	float outAlpha = min(color.a, alpha);
	
	if(hasDiffuseMap)
		outColor = texture(diffuseMap, texCoord) * color;

	outColor.a = outAlpha;

	FragColor = outColor;
}