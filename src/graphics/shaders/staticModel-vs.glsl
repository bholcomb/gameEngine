#version 430

layout(location = 0) in vec3 position;
layout(location = 1 ) in vec3 normal;
layout(location = 2) in vec2 uv;

layout(std140, binding = 0) uniform camera {
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

struct Model{
	mat4 model;
	mat4 normal;
	mat4 invNormal;
	vec4 activeLights;
};

layout(std430, binding = 2) buffer modelData
{
	Model models[];
};

layout(location = 0) uniform int modelDataIndex;

out VertexStage
{
	smooth vec2 texCoord;
	smooth vec3 worldNormal;
	smooth vec3 worldVert;
	mat3 normalMatrix;
};
	
flat out vec4 activeLights;

void main()
{
	//this should be the inverse transpose, but since we're not doing any non-uniform scaling, we can just 
	//skip the inverse/transpose steps since they cancel each other out when we have no scaling (or just uniform scaling).
	//http://www.lighthouse3d.com/tutorials/glsl-tutorial/the-normal-matrix/
	mat4 modelMat = models[modelDataIndex].model;
	normalMatrix = mat3(modelMat);
	worldNormal = normalize(normalMatrix * normal);
	worldVert = (modelMat * vec4(position, 1)).xyz;

	texCoord=uv;
	activeLights = models[modelDataIndex].activeLights;

	mat4 MVP = viewProj * modelMat;
	gl_Position = MVP * vec4(position,1);
}
