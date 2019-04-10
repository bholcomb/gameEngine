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
   vec3 a = vec3(gs_in[1].worldVert) - vec3(gs_in[0].worldVert);
   vec3 b = vec3(gs_in[2].worldVert) - vec3(gs_in[0].worldVert);
   vec3 n = normalize(cross(a, b));
   return gs_in[0].normalMatrix * n;
}  

void main()
{
    vec3 n = calcNormal();
   //discard if the normal is not facing the camera

   for(int i=0; i< 3; i++)
   {
      gl_Position = gl_in[i].gl_Position;
      gs_out.normalMatrix = gs_in[i].normalMatrix;
      gs_out.worldVert = gs_in[i].worldVert;
      gs_out.texCoord = gs_in[i].texCoord;
      gs_out.texLayer = gs_in[i].texLayer;
      gs_out.worldNormal = n;
      EmitVertex();
   }
   
   EndPrimitive();
}