#version 430

layout(location = 20) uniform sampler2DArray diffuseTexture;

in GeometryStage
{
   smooth vec2 texCoord;
   smooth vec3 normal;
   smooth vec3 eyevert;
   flat mat3 normalMatrix;
   flat int texLayer;
} ps_in;

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

layout(std140, binding = 1) uniform LightBlock{
   LightData[100] lights;
};

layout(std140, binding = 3) uniform environment {
   int isUnderWater;
};

vec4 ambientTerm()
{
   return lights[0].color * 0.2;
}

vec4 directionalTerm(vec3 normal)
{
   vec3 lightDirection = normalize(ps_in.normalMatrix * lights[0].direction.xyz);
   float intensity = max(0.0, dot(normal, lightDirection));
   return lights[0].color * intensity;
}

void main()
{   
   if(ps_in.texLayer == 0x3ff)
      discard;

   vec4 color=vec4(0,0,0,1);

   vec4 diffuseColor=texture(diffuseTexture, vec3(ps_in.texCoord, ps_in.texLayer));
   
   if(diffuseColor.a < 0.1)
      discard;	

   //this is the ambient term
   color += ambientTerm() * diffuseColor; 
   
   //directional light term
   color +=  directionalTerm(normalize(ps_in.normal)) * diffuseColor;

   //restore the alpha
   color.a=diffuseColor.a;

   //send to buffer
   FragColor=color;
}