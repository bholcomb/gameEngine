#version 430

layout(location = 20) uniform sampler2D tex;
layout(location = 21) uniform samplerCube cube;
layout(location = 22) uniform sampler2DArray array;

layout(location = 23) uniform int textureSource;
layout(location = 24) uniform int layer;
layout(location = 25) uniform bool linearDepth;
layout(location = 26) uniform bool singleChannel;

layout(std140, binding = 0) uniform camera{
	mat4 view; //aligned 4N
	mat4 projection; //aligned 4N
	mat4 viewProj; //aligned 4N
	mat4 ortho; //aligned 4N
	vec4 viewVector; //aligned 4N
	vec4 eyeLocation; //aligned 4N
	float left, right, top, bottom; //aligned 4N
	float zNear, zFar, aspect, fov; //aligned 4N
	int frame;
	float dt;
};

smooth in vec2 texCoord;
smooth in vec4 vecColor;

out vec4 FragColor;

vec4 calcLinearDepth(vec4 texInput)
{
	float z = texInput.x;
	float linearZ = (2.0 * zNear) / (zFar + zNear - z * (zFar - zNear));
	return vec4(linearZ, linearZ, linearZ, 1);
}

void main()
{
	vec4 texColor = vec4(1);

	switch (textureSource)
	{
	case 0:
		texColor = texture(tex, texCoord);
		break;
	case 1:
		texColor = vec4(1, 0, 0, 1);// texture(cube, vec3(texCoord, layer));
		break;
	case 2:
		texColor = texture(array, vec3(texCoord, layer));
		break;
	}
	
	if (linearDepth)
	{
		texColor = calcLinearDepth(texColor);
	}

   if(singleChannel)
   {
      texColor = vec4(texColor.r, texColor.r, texColor.r, 1);
   }

   FragColor = texColor *  vecColor;
}