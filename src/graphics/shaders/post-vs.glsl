#version 430

layout(location = 0) uniform float time;
layout(location = 1) uniform float width;
layout(location = 2) uniform float height;

smooth out vec2 texCoord;
smooth out vec3 fragVec;

//used to calc depth of fragment in post fragment shader
flat out float p33;
flat out float p34;

const vec2 data[4] = vec2[]
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

const vec3 corners[4] = vec3[]
(
   vec3(-1, 1, 1),
   vec3(-1, -1, 1),
   vec3(1, 1, 1),
   vec3(1, -1, 1)
);

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
 
void main()
{
   //calculate terms for depth reconstruction
   p33 = (zFar + zNear) / (zNear - zFar);
   p34 = (-2.0 * zFar * zNear) / (zFar - zNear);

   //generate a vector to the corner position used for the projection frustrum
   fragVec = normalize(corners[gl_VertexID] * vec3(right, top, zNear));

   texCoord=uvs[gl_VertexID];
   gl_Position = vec4( data[ gl_VertexID ], 0.0, 1.0);
}
