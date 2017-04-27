#version 430
 
layout (points) in;
layout (invocations = 6) in;
layout (triangle_strip, max_vertices=4) out;

flat in vec4 gs_verts[][8];
flat in mat4 gs_MVP[];
flat in mat4 gs_MV[];
flat in mat3 gs_normalMatrix[];
flat in int gs_matId[][6];

smooth out vec2 texCoord;
smooth out vec3 eyenormal;
//smooth out vec4 eyevert;
smooth out vec4 color;
flat out int matId;
flat out mat3 normalMatrix;

const vec3 faceNormals[6]=vec3[6](
   vec3( 1, 0, 0),   //+X  RIGHT
   vec3(-1, 0, 0),   //-X  LEFT
   vec3( 0, 1, 0),   //+Y  TOP
   vec3( 0,-1, 0),   //-Y  BOTTOM
   vec3( 0, 0, 1),   //+Z  BACK
   vec3( 0, 0,-1)    //-Z  FRONT
);

const ivec4 faceIndex[6]=ivec4[6](
   ivec4(5,1,7,3),    //+X  RIGHT
   ivec4(0,4,2,6),    //-X  LEFT
   ivec4(6,7,2,3),    //+Y  TOP
   ivec4(0,1,4,5),    //-Y  BOTTOM
   ivec4(4,5,6,7),    //+Z  BACK
   ivec4(1,0,3,2)     //-Z  FRONT
);

vec2 localUv(vec4 localVert, int face)
{
   switch(face)
   {
      case 0:  //+X
      case 1:  //-X
         return localVert.zy;
      case 2:  //+Y
      case 3:  //-Y
         return localVert.xz;
      case 4:  //+Z
      case 5:  //-Z
         return localVert.xy;
   }
}

void main()
{
   normalMatrix = gs_normalMatrix[0];
   eyenormal = normalMatrix * faceNormals[gl_InvocationID];

   matId=gs_matId[0][gl_InvocationID];
   ivec4 vertIndex=faceIndex[gl_InvocationID];
   
   vec4[] verts = gs_verts[0];

   //should we cull the triangle. This should cull out
   //3 of the 6 faces.  Uses the first vertex in the triangle
   if(dot(eyenormal, (gs_MV[0] * verts[vertIndex.x]).xyz) > 0)
     return;

   texCoord=localUv(verts[vertIndex.x], gl_InvocationID);
   gl_Position = gs_MVP[0] * verts[vertIndex.x];
   //eyevert = gs_MV[0] * verts[vertIndex.x];
   EmitVertex(); 

   texCoord=localUv(verts[vertIndex.y], gl_InvocationID);
   gl_Position = gs_MVP[0] * verts[vertIndex.y];
   //eyevert = gs_MV[0] * verts[vertIndex.y];
   EmitVertex(); 

   texCoord=localUv(verts[vertIndex.z], gl_InvocationID);
   gl_Position = gs_MVP[0] * verts[vertIndex.z];
   //eyevert = gs_MV[0] * verts[vertIndex.z];
   EmitVertex(); 
    
   texCoord=localUv(verts[vertIndex.w], gl_InvocationID);
   gl_Position = gs_MVP[0] * verts[vertIndex.w];
   //eyevert = gs_MV[0] * verts[vertIndex.w];
   EmitVertex(); 

   EndPrimitive();
}