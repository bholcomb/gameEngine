#version 330

uniform sampler2DArray diffuseTexture;

smooth in vec2 texCoord;
smooth in vec3 eyenormal;
//smooth in vec3 eyevert;
smooth in vec4 color;
flat in mat3 normalMatrix;
flat in int matId;

out vec4 FragColor;

struct LightData{
   vec4 color;	
   vec4 position;
   vec4 direction;
   float constantAttenuation;
   float linearAttenuation;
   float quadraticAttenuation;
   float spotAngle;
   float spotExponential;
   float pad1;
   float pad2;
   float pad3;
};

layout(std140) uniform LightBlock{
   LightData[100] lights;
};

layout(std140) uniform environment {
   int isUnderWater;
};

vec4 ambientTerm()
{
   return lights[0].color;
}

vec4 directionalTerm(vec3 normal)
{
   vec3 lightDirection=normalize(normalMatrix * lights[0].position.xyz);
   float intensity = max(0.0, dot(normal, lightDirection));
   return lights[0].color * intensity;
}

void main()
{   
   if(gl_FrontFacing==false)
      discard;

   if(matId==0x3ff)
      discard;

   vec4 color=vec4(0,0,0,1);

   vec4 diffuseColor=texture(diffuseTexture, vec3(texCoord, matId));
   
   if(diffuseColor.a < 0.2)
      discard;	

   //this is the ambient term
   color += ambientTerm() * diffuseColor; 
   
   //directional light term
   color +=  directionalTerm(normalize(eyenormal)) * diffuseColor;

   //restore the alpha
   color.a=diffuseColor.a;

   //send to buffer
   FragColor=color;
}