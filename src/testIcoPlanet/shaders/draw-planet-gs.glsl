#version 430

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in VertexStage
{
   flat float depth;
} vs_in[];

out GeometryStage
{   
   flat float depth;
} gs_out;

void main()
{
   gs_out.depth = 100;
   for (int i = 0; i< 3; i++)
   {
      if (vs_in[i].depth < gs_out.depth)
         gs_out.depth = vs_in[i].depth;

      gl_Position = gl_in[i].gl_Position;

      EmitVertex();
   }

   EndPrimitive();
}