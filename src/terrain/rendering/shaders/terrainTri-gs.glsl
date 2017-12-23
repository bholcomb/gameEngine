#version 430
 
layout (triangles) in;
layout (triangle_strip, max_vertices=3) out;

in VertexStage
{
   smooth vec2 texCoord;
   smooth vec3 worldVert;
   flat mat3 normalMatrix;
   flat int texLayer;
} gs_in[];

out GeometryStage
{
   smooth vec2 texCoord;
   smooth vec3 worldNormal;
   smooth vec3 worldVert;
   flat mat3 normalMatrix;
   flat int texLayer;
} gs_out;

vec3 calcNormal()
{
   vec3 a = vec3(gl_in[0].gl_Position) - vec3(gl_in[1].gl_Position);
   vec3 b = vec3(gl_in[2].gl_Position) - vec3(gl_in[1].gl_Position);
   vec3 n= normalize(cross(a, b));
   return gs_in[0].normalMatrix * n;
}  

void main()
{
   gs_out.worldNormal = calcNormal();

   //discard if the normal is not facing the camera

   for(int i=0; i< 3; i++)
   {
      gl_Position = gl_in[i].gl_Position;
      gs_out.normalMatrix = gs_in[i].normalMatrix;
      gs_out.worldVert = gs_in[i].worldVert;
      gs_out.texCoord = gs_in[i].texCoord;
      gs_out.texLayer = gs_in[i].texLayer;
      EmitVertex();
   }
   
   EndPrimitive();
}