#version 430

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 uv;
layout(location = 3) in vec4 boneId;
layout(location = 4) in vec4 boneWeight;

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

struct Model {
	mat4 model;
	mat4 normal;
	mat4 invNormal;
	vec4 activeLights;
	int boneCount;
	int currentFrame;
	int nextFrame;
	float interpolation;
};

layout(binding = 0, std430) buffer modelData
{
	Model models[];
};
layout(binding = 1, std430) buffer boneData
{
	mat4 bones[];
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


mat4 boneMatrix(int frame)
{
   mat4 ret=mat4(1); //identity

   for(int i=0; i<4; i++)
   {
      int bone = int(boneId[i]);
      float weight = boneWeight[i];
      if(bone != -1 && weight > 0)
      {
         int offset = (frame * models[modelDataIndex].boneCount) + bone;

         //get the current bone matrix
			mat4 boneMatrix = bones[offset];
         ret += boneMatrix * weight;
      }
   }
   
   return ret;	
}

void main()
{	
   //get the bone matrixes
   mat4 b1matrix = boneMatrix(models[modelDataIndex].currentFrame);
   mat4 b2matrix = boneMatrix(models[modelDataIndex].nextFrame);

   vec4 p1 = b1matrix * vec4(position, 1);
   vec4 p2 = b2matrix * vec4(position, 1);
   vec4 pos = mix(p1, p2, models[modelDataIndex].interpolation);
   pos.w = 1;

	//this should be the inverse transpose, but since we're not doing any non-uniform scaling, we can just 
	//skip the inverse/transpose steps since they cancel each other out when we have no scaling (or just uniform scaling).
	//http://www.lighthouse3d.com/tutorials/glsl-tutorial/the-normal-matrix/
	mat4 modelMat = models[modelDataIndex].model;
	normalMatrix = mat3(modelMat);
	worldNormal = normalize(normalMatrix * normal);
	worldVert = (modelMat * vec4(position, 1)).xyz;
   
   texCoord = uv;

	activeLights = models[modelDataIndex].activeLights;

   mat4 MVP = viewProj * modelMat;
   gl_Position = MVP * pos;
}
