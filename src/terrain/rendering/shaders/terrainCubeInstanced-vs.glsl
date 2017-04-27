#version 430
#extension GL_ARB_shader_draw_parameters : enable

layout(location = 0) in uint morton;
layout(location = 1) in uint material;
layout(location = 2) in uvec4 edge1;
layout(location = 3) in uvec4 edge2;
layout(location = 4) in uvec4 edge3;

layout(std140) uniform camera {
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

layout(std140) uniform instanceData{
   mat4[1024] modelMatrixes;
};

flat out vec4 gs_verts[8];
flat out mat4 gs_MV;
flat out mat4 gs_MVP;
flat out mat3 gs_normalMatrix;
flat out int gs_matId[6];

float size=102.4;
float edgeStart[12];
float edgeStop[12];

const float maxSize=102.4; //this is for a cube with depth 10 (1024 cubes/edge) and a smallest cube size of 0.1

const vec4 vertexOffsets[8]=vec4[8](
   vec4(0, 0, 0, 0),
   vec4(1, 0, 0, 0),
   vec4(0, 1, 0, 0),
   vec4(1, 1, 0, 0),
   vec4(0, 0, 1, 0),
   vec4(1, 0, 1, 0),
   vec4(0, 1, 1, 0),
   vec4(1, 1, 1, 0)
);

//These are edge ids in order of z,y,x axis of influence
//if vertex id's binary rep has a 1 set for that axis (also read ZYX)
//then that an edge end, if it is a 0, then that is an edge start
//vertex 0 has all edges going away from it (it's the start of all its connected edges)
//vertex 7 has all edges coming into it (it's the end of all its connected edges)
const ivec3 vertexEdges[8] = ivec3[8](       
   ivec3(11, 6, 3),
   ivec3(8, 7, 3),
   ivec3(10, 6, 2),
   ivec3(9, 7, 2),
   ivec3(11, 5, 0),
   ivec3(8, 4, 0),
   ivec3(10, 5, 1),
   ivec3(9, 4, 1)
);

void adjustEdges(inout vec4 pos, int vertIndex)
{
   //the index of the vert is also a binary 3 digit key for 
   //which end of the edge influences the vert
   //a 0 means the beginning and a 1 means the end of edge
   //pushes/pulls the vert
   ivec3 edges = vertexEdges[vertIndex];

   if ((vertIndex & 0x1) != 0)
      pos.x -= edgeStop[edges.z];
   else
      pos.x += edgeStart[edges.z];

   if ((vertIndex & 0x2) != 0)
      pos.y -= edgeStop[edges.y];
   else
      pos.y += edgeStart[edges.y];

   if ((vertIndex & 0x4) != 0)
      pos.z -= edgeStop[edges.x];
   else
      pos.z += edgeStart[edges.x];
}

//Calculate the verts once, then reuse for each face
void calcVerts(vec4 cubePos, float size, mat4 m)
{
   for(int i=0; i<8; i++)
   {
      vec4 p;
      p = cubePos + (vertexOffsets[i] * size); //cube positon + specific vertex offset * cube size
      adjustEdges(p, i);
      gs_verts[i] = p;      
   }
}

vec4 decodeMorton(in uint mort)
{
   uint key=mort;
   int depth=0;
   size=maxSize;
   vec4 pos=vec4(0,0,0,1);

   //determine the size
   while(key > 1u)
   {
      depth++;
      size = size/2.0;
      key = key >> 3;
   }

   key=mort;
   for(int i=depth; i > 0; i--)
   {
      float depthSize = maxSize / (1 << i);

      pos.x += (key & 0x1) != 0 ? depthSize : 0;
      pos.y += (key & 0x2) != 0 ? depthSize : 0;
      pos.z += (key & 0x4) != 0 ? depthSize : 0;

      key = key >> 3;
   }

   return pos;
}

void decodeMaterial(in uint material)
{
   uint numTextures=material >> 30;
   switch(numTextures)
   {
      case 1: //same texture on on 6 faces
         gs_matId[0]=int(material & 0x3fffffffu);
         gs_matId[1]=gs_matId[0];
         gs_matId[2]=gs_matId[0];
         gs_matId[3]=gs_matId[0];
         gs_matId[4]=gs_matId[0];
         gs_matId[5]=gs_matId[0];
         break;
      case 2: //different texture on top vs sides
         gs_matId[0]=int((material & 0x7fffu));
         gs_matId[1]=gs_matId[0];
         gs_matId[2]=int((material & 0x3fff8000u) >> 15u);
         gs_matId[3]=gs_matId[0];
         gs_matId[4]=gs_matId[0];
         gs_matId[5]=gs_matId[0];
         break;
      case 3: //different texture on top and on bottom
         gs_matId[0]=int((material & 0xffc00u) >> 10u);
         gs_matId[1]=gs_matId[0];
         gs_matId[2]=int((material & 0x3ff00000u) >> 20u);
         gs_matId[3]=int((material & 0x3ffu));
         gs_matId[4]=gs_matId[0];
         gs_matId[5]=gs_matId[0];
         break;
   }
}

void decodeEdges()
{
   float edgeStep=size/15;

   edgeStart[0]=(edge1.x >> 4) * edgeStep;
   edgeStart[1]=(edge1.y >> 4) * edgeStep;   
   edgeStart[2]=(edge1.z >> 4) * edgeStep;
   edgeStart[3]=(edge1.w >> 4) * edgeStep;
   edgeStart[4]=(edge2.x >> 4) * edgeStep;
   edgeStart[5]=(edge2.y >> 4) * edgeStep;
   edgeStart[6]=(edge2.z >> 4) * edgeStep;
   edgeStart[7]=(edge2.w >> 4) * edgeStep;
   edgeStart[8]=(edge3.x >> 4) * edgeStep;
   edgeStart[9]=(edge3.y >> 4) * edgeStep;
   edgeStart[10]=(edge3.z >> 4) * edgeStep;
   edgeStart[11]=(edge3.w >> 4) * edgeStep;

   edgeStop[0]=(edge1.x & 0xf) * edgeStep;
   edgeStop[1]=(edge1.y & 0xf) * edgeStep;
   edgeStop[2]=(edge1.z & 0xf) * edgeStep;
   edgeStop[3]=(edge1.w & 0xf) * edgeStep;
   edgeStop[4]=(edge2.x & 0xf) * edgeStep;
   edgeStop[5]=(edge2.y & 0xf) * edgeStep;
   edgeStop[6]=(edge2.z & 0xf) * edgeStep;
   edgeStop[7]=(edge2.w & 0xf) * edgeStep;
   edgeStop[8]=(edge3.x & 0xf) * edgeStep;
   edgeStop[9]=(edge3.y & 0xf) * edgeStep;
   edgeStop[10]=(edge3.z & 0xf) * edgeStep;
   edgeStop[11]=(edge3.w & 0xf) * edgeStep;
}

void main()
{
   vec4 pos = decodeMorton(morton);
   decodeMaterial(material);
   decodeEdges();

   mat4 model = modelMatrixes[gl_DrawIDARB];
   
   //this should be the inverse transpose, but since we're not doing any non-uniform scaling, we can just 
   //skip the inverse/transpose steps since they cancel each other out when we have no scaling (or just uniform scaling).
   //http://www.lighthouse3d.com/tutorials/glsl-tutorial/the-normal-matrix/
   gs_normalMatrix = mat3(view * model);

   gs_MVP = viewProj * model;
   gs_MV = view * model;
      
   calcVerts(pos, size, model);
   
   gl_Position = pos;
}
