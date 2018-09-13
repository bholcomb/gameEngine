#version 430   

//based on http://blog.simonrodriguez.fr/articles/28-06-2015_un_ciel_dans_le_shader.html

//---------IN------------
layout(location = 0 ) in vec3 position;
layout(location = 1 ) in vec3 normal;

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


//---------UNIFORM------------
layout(location = 0) uniform vec3 sun_pos; //sun position in world space
layout(location = 1) uniform mat4 rot_stars;//rotation matrix for the stars


//---------OUT------------
out vec3 pos;
out vec3 sun_norm;
out vec3 star_pos;

//---------MAIN------------
void main()
{
   mat4 viewOri = mat4(mat3(view));
   mat4 MVP = projection * viewOri;
   gl_Position = (MVP * vec4(position, 1.0));
   
   pos = position;

   //Sun pos being a constant vector, we can normalize it in the vshader
   //and pass it to the fshader without having to re-normalize it
   sun_norm = normalize(sun_pos);

   //And we compute an approximate star position using the special rotation matrix
   mat3 starRot = mat3(rot_stars);
   star_pos = starRot * normalize(pos);
}
