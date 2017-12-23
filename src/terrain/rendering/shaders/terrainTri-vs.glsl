#version 430
#extension GL_ARB_shader_draw_parameters : enable

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 uv;
layout(location = 2) in int texIndex;

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

layout(std140, binding = 2) uniform instanceData{
   mat4[1024] modelMatrixes;
};

out VertexStage
{
   smooth vec2 texCoord;
   smooth vec3 worldVert;
   flat mat3 normalMatrix;
   flat int texLayer;
} vs_out;

const float MAX_SIZE=1024;

vec4 calcPosition(vec4 p)
{
   vec4 pos;
   switch(int(p.w))
   {
      case 0: 
         {
            pos = vec4(p.x, p.y, p.z, 10); 
         }
         break;
      case 1: 
         {
            switch(int(p.z)) //which element is max
            {
               case 0: pos = vec4(MAX_SIZE, p.x, p.y, 10); break;
               case 1: pos = vec4(p.x, MAX_SIZE, p.y, 10); break;
               case 2: pos = vec4(p.x, p.y, MAX_SIZE, 10); break;
            }
         }
         break;
      case 2:
         {
            switch(int(p.z)) //which element is not max
            {
               case 0: pos = vec4(p.x, MAX_SIZE, MAX_SIZE, 10); break;
               case 1: pos = vec4(MAX_SIZE, p.x, MAX_SIZE, 10); break;
               case 2: pos = vec4(MAX_SIZE, MAX_SIZE, p.x, 10); break;
            }
         }
         break;
      case 3: 
         {
            pos = vec4(MAX_SIZE, MAX_SIZE, MAX_SIZE, 10);
         }
         break;
   }

   return pos / 10.0f;
}

void main()
{
   //get the model matrix for this chunk
   mat4 model = modelMatrixes[gl_DrawIDARB];

   vec4 pos = calcPosition(position);

   //this should be the inverse transpose, but since we're not doing any non-uniform scaling, we can just 
   //skip the inverse/transpose steps since they cancel each other out when we have no scaling (or just uniform scaling).
   //http://www.lighthouse3d.com/tutorials/glsl-tutorial/the-normal-matrix/
   vs_out.normalMatrix = mat3(model);
   vs_out.worldVert = (model * vec4(pos.xyz, 1)).xyz;
   vs_out.texCoord = uv * 102.4;
   vs_out.texLayer =  texIndex;

   mat4 MVP = viewProj * model;
   gl_Position = MVP * pos;
}
