#version 330

uniform float width;
uniform float height;
 
smooth out vec2 texCoord;

const vec2 verts[4] = vec2[]
(
  vec2(-1.0,  1.0),
  vec2(-1.0, -1.0),
  vec2( 1.0,  1.0),
  vec2( 1.0, -1.0)
);

const vec2 uvs[4] = vec2[]
(
  vec2( 0, 1),
  vec2( 0, 0),
  vec2( 1, 1),
  vec2( 1, 0)
);

void main()
{
   texCoord = uvs[gl_VertexID];
   gl_Position = vec4( verts[ gl_VertexID ], 0.0, 1.0);
}
