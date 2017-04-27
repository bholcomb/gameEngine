#version 330

precision highp float;
 
layout (points) in;
layout (triangle_strip, max_vertices=4) out;

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

in float pRot[];
in vec3 pSize[];
in vec4 pColor[];

smooth out vec2 uv;
smooth out vec4 color;

void emit(in vec2 texcoord, in vec4 point) 
{
  uv = texcoord;
  gl_Position = viewProj * point;
  EmitVertex();
}

void main()
{ 
	color=pColor[0];
	
	mat4 rotate = mat4( cos( pRot[0] ),	   -sin( pRot[0] ),	 0, 0,
						sin( pRot[0] ),    cos( pRot[0] ),	 0, 0,
						0,             0,            1, 0,
						0,             0,            0, 1);

	mat4 scale = mat4(pSize[0].x, 0,          0, 0,
	                  0,          pSize[0].y, 0, 0,
					  0,          0,          1, 0,
					  0,          0,          0, 1);

	mat4 rs= scale * rotate;

	//these are the 4 points rotated about the z axis in th x-y plane
	vec4 p1 = rs * vec4(-1, -1, 0, 1);
	vec4 p2 = rs * vec4(-1,  1, 0, 1);
	vec4 p3 = rs * vec4( 1, -1, 0, 1);
	vec4 p4 = rs * vec4( 1,  1, 0, 1);

	//get the position of the particle
	vec4 pos = gl_in[0].gl_Position;

	//get the basis vectors for the orientation to the camera
	vec3 toCamera = normalize(location.xyz - pos.xyz);
	vec3 up = vec3(0.0, 1.0, 0.0); //already normalized
	vec3 right = normalize(cross(up, toCamera));
	up=normalize(cross(toCamera, right)); //redo the up vector to get it correct

	//create a matrix from the basis angles to rotate to the camera
	mat4 basis=mat4(right, 0,
	                up, 0,
					toCamera, 0,
					pos.xyz, 1);

	emit(vec2(0.0, 0.0), basis * p1);

	emit(vec2(0.0, 1.0), basis * p2);

	emit(vec2(1.0, 0.0), basis * p3);

	emit(vec2(1.0, 1.0), basis * p4);

	EndPrimitive();
}