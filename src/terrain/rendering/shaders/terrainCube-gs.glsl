#version 430
 
layout (points) in;
layout (triangle_strip, max_vertices=24) out;

flat in mat4 MVP[];
flat in mat4 model[];
flat in float size[];
flat in mat3 gs_normalMatrix[];
flat in int gs_matId[][6];
flat in float gs_edgeStart[][12];
flat in float gs_edgeStop[][12];

smooth out vec2 texCoord;
smooth out vec3 eyenormal;
smooth out vec3 eyevert;
smooth out vec4 color;
flat out int matId;
flat out mat3 normalMatrix;

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

//These are edge ids in order of z,y,x axis of influence
//if vertex id's binary rep has a 1 set for that axis (also read ZYX)
//then that an edge end, if it is a 0, then that is an edge start
//vertex 0 has all edges going away from it (it's the start of all its connected edges)
//vertex 7 has all edges coming into it (it's the end of all its connected edges)
ivec3 vertexEdges[8] = ivec3[8](       
   ivec3(11, 6, 3),
   ivec3(8, 7, 3),
   ivec3(10, 6, 2),
   ivec3(9, 7, 2),
   ivec3(11, 5, 0),
   ivec3(8, 4, 0),
   ivec3(10, 5, 1),
   ivec3(9, 4, 1)
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

void adjustEdges(inout vec4 pos, int vertIndex)
{
   //the index of the vert is also a binary 3 digit key for 
   //which end of the edge influences the vert
   //a 0 means the beginning and a 1 means the end of edge
   //pushes/pulls the vert
   ivec3 edges = vertexEdges[vertIndex];

   if ((vertIndex & 0x1) != 0)
      pos.x -= gs_edgeStop[0][edges.z];
   else
      pos.x += gs_edgeStart[0][edges.z];

   if ((vertIndex & 0x2) != 0)
      pos.y -= gs_edgeStop[0][edges.y];
   else
      pos.y += gs_edgeStart[0][edges.y];

   if ((vertIndex & 0x4) != 0)
      pos.z -= gs_edgeStop[0][edges.x];
   else
      pos.z += gs_edgeStart[0][edges.x];
}

//cube verts.
vec4 verts[8];
vec3 normals[6];
vec4 localVerts[8];
vec3 eyeVerts[8];
mat4 MV;

//Calculate the verts once, then reuse for each face
void calcVerts()
{
   for(int i=0; i<8; i++)
   {
      vec4 pos;
      pos=gl_in[0].gl_Position + (vertexOffsets[i] * size[0]); //cube positon + specific vertex offset * cube size
      adjustEdges(pos, i);
      localVerts[i]=pos;
      verts[i] = MVP[0] * pos;
      eyeVerts[i] = (MV * pos).xyz;
   }
}

void calcNormals()
{
   for(int face=0; face<6; face++)
   {
      ivec4 vertIndex=faceIndex[face];
      vec3 a = eyeVerts[vertIndex.x] - eyeVerts[vertIndex.y];
      vec3 b = eyeVerts[vertIndex.x] - eyeVerts[vertIndex.z];;
      vec3 n= normalize(cross(a, b));
      normals[face] = normalMatrix * n;
   }
}  

bool shouldCull()
{
   vec3 p= (MV * gl_in[0].gl_Position).xyz;

   float angle=dot(p, viewVector.xyz);
   return angle < 0; 
}

void main()
{
   MV = (view * model[0]);
   
//   if(shouldCull() == true)
//      return;

   normalMatrix = gs_normalMatrix[0];
   float s=size[0];
   calcVerts();
   calcNormals();

   for(int face=0; face<6; face++)
   {
      eyenormal = normals[face];

      matId=gs_matId[0][face];
      ivec4 vertIndex=faceIndex[face];

      //should we cull the triangle. This should cull out
      //3 of the 6 faces.  Uses the first vertex in the triangle
      //if(dot(eyenormal, eyeVerts[vertIndex.x]) > 0)
      //  continue;

      texCoord=localUv(localVerts[vertIndex.x], face);
      gl_Position = verts[vertIndex.x];
      eyevert=eyeVerts[vertIndex.x];
      EmitVertex(); 

      texCoord=localUv(localVerts[vertIndex.y], face);
      gl_Position = verts[vertIndex.y];
      eyevert=eyeVerts[vertIndex.y];
      EmitVertex(); 

      texCoord=localUv(localVerts[vertIndex.z], face);
      gl_Position = verts[vertIndex.z];
      eyevert=eyeVerts[vertIndex.z];
      EmitVertex(); 
       
      texCoord=localUv(localVerts[vertIndex.w], face);
      gl_Position = verts[vertIndex.w];
      eyevert=eyeVerts[vertIndex.w];
      EmitVertex(); 

      EndPrimitive();
   }
}