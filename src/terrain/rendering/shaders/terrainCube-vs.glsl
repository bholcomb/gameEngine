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

flat out float size;
flat out mat4 MVP;
flat out mat4 model;
flat out mat3 gs_normalMatrix;
flat out int gs_matId[6];
flat out float gs_edgeStart[12];
flat out float gs_edgeStop[12];

float maxSize=102.4; //this is for a cube with depth 10 (1024 cubes/edge) and a smallest cube size of 0.1

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

   gs_edgeStart[0]=(edge1.x >> 4) * edgeStep;
   gs_edgeStart[1]=(edge1.y >> 4) * edgeStep;   
   gs_edgeStart[2]=(edge1.z >> 4) * edgeStep;
   gs_edgeStart[3]=(edge1.w >> 4) * edgeStep;
   gs_edgeStart[4]=(edge2.x >> 4) * edgeStep;
   gs_edgeStart[5]=(edge2.y >> 4) * edgeStep;
   gs_edgeStart[6]=(edge2.z >> 4) * edgeStep;
   gs_edgeStart[7]=(edge2.w >> 4) * edgeStep;
   gs_edgeStart[8]=(edge3.x >> 4) * edgeStep;
   gs_edgeStart[9]=(edge3.y >> 4) * edgeStep;
   gs_edgeStart[10]=(edge3.z >> 4) * edgeStep;
   gs_edgeStart[11]=(edge3.w >> 4) * edgeStep;

   gs_edgeStop[0]=(edge1.x & 0xf) * edgeStep;
   gs_edgeStop[1]=(edge1.y & 0xf) * edgeStep;
   gs_edgeStop[2]=(edge1.z & 0xf) * edgeStep;
   gs_edgeStop[3]=(edge1.w & 0xf) * edgeStep;
   gs_edgeStop[4]=(edge2.x & 0xf) * edgeStep;
   gs_edgeStop[5]=(edge2.y & 0xf) * edgeStep;
   gs_edgeStop[6]=(edge2.z & 0xf) * edgeStep;
   gs_edgeStop[7]=(edge2.w & 0xf) * edgeStep;
   gs_edgeStop[8]=(edge3.x & 0xf) * edgeStep;
   gs_edgeStop[9]=(edge3.y & 0xf) * edgeStep;
   gs_edgeStop[10]=(edge3.z & 0xf) * edgeStep;
   gs_edgeStop[11]=(edge3.w & 0xf) * edgeStep;
}

void main()
{
   vec4 pos = decodeMorton(morton);
   decodeMaterial(material);
   decodeEdges();

   model = modelMatrixes[gl_DrawIDARB];
   
   //this should be the inverse transpose, but since we're not doing any non-uniform scaling, we can just 
   //skip the inverse/transpose steps since they cancel each other out when we have no scaling (or just uniform scaling).
   //http://www.lighthouse3d.com/tutorials/glsl-tutorial/the-normal-matrix/
   gs_normalMatrix = mat3(view * model);

   MVP = viewProj * model;
   gl_Position = pos;
}
