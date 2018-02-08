#version 430

layout(std140, binding = 0) uniform camera{
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

struct LightData{
   vec4 color;
   vec4 position;
   vec4 direction;
   int lightType;
   float constantAttenuation;
   float linearAttenuation;
   float quadraticAttenuation;
   float spotAngle;
   float spotExponential;
   float pad1;
   float pad2;
};

layout(std140, binding = 1) uniform LightBlock{
   LightData lights[255];
};

//uniform block 2 is used in vertex shader for terrain block model instances

layout(std140, binding = 3) uniform material{
   vec4 matAmbientReflectivity;
   vec4 matDiffuseReflectivity;
   vec4 matSpecularReflectivity;
   vec4 emmission;
   float shininess;
   float alpha;
   bool hasSpecularMap;
   bool hasNormalMap;
   bool hasParallaxMap;
   float parallaxScale;
};

layout(std140, binding = 4) uniform environment {
   int isUnderWater;
};


layout(location = 20) uniform sampler2DArray diffuseTexture;

vec4 specularReflectivity;
vec4 normal;
//flat in vec4 activeLights;

in GeometryStage
{
   smooth vec2 texCoord;
   smooth vec3 worldNormal;
   smooth vec3 worldVert;
   flat mat3 normalMatrix;
   flat int texLayer;
} ps_in;

//outputs
out vec4 FragColor;

vec3 lightIntensity(LightData light, vec3 pos)
{
   float dist = distance(light.position.xyz, pos);
   float a = light.linearAttenuation;
   float b = light.quadraticAttenuation;

   float intensity = 1.0 / (1.0 + a*dist + b*dist*dist);

   if (intensity < 0.01)
      return vec3(0);
   else
      return intensity * light.color.rgb;
}

vec3 directionalLight(LightData light, vec3 pos, vec3 norm)
{
   vec3 lightDir = normalize(light.position.xyz);
   vec3 viewDir = normalize(eyeLocation.xyz - pos);
   vec3 halfVec = normalize(viewDir + lightDir);

   vec3 amb = light.color.rgb *  matAmbientReflectivity.rgb;
   vec3 diff = light.color.rgb * matDiffuseReflectivity.rgb * max(dot(lightDir, norm), 0.0);
   vec3 spec = light.color.rgb * specularReflectivity.rgb * pow(max(dot(norm, halfVec), 0.0001), shininess);

   return amb + diff + spec;
}

vec3 pointLight(LightData light, vec3 pos, vec3 norm)
{
   vec3 lightDir = normalize(light.position.xyz - pos);
   vec3 viewDir = normalize(eyeLocation.xyz - pos);
   vec3 halfVec = normalize(viewDir + lightDir);

   //attenuate the light 
   vec3 intensity = lightIntensity(light, pos);

   vec3 amb = intensity *  matAmbientReflectivity.rgb;
   vec3 diff = intensity * matDiffuseReflectivity.rgb * max(dot(lightDir, norm), 0.0);
   vec3 spec = intensity * specularReflectivity.rgb * pow(max(dot(norm, halfVec), 0.0001), shininess);

   return amb + diff + spec;
}

vec3 spotLight(LightData light, vec3 pos, vec3 norm)
{
   vec3 lightDir = normalize(light.position.xyz - pos);
   float dotSDir = dot(-lightDir, light.direction.xyz);
   float angle = acos(dotSDir);
   float cutoff = radians(clamp(light.spotAngle, 0.0, 90.0));
   vec3 intensity = lightIntensity(light, pos);
   vec3 ambient = intensity * matAmbientReflectivity.rgb;

   if (angle < cutoff)
   {
      float spotFactor = pow(dotSDir, light.spotExponential);
      vec3 viewDir = normalize(eyeLocation.xyz - pos);
      vec3 halfVec = normalize(viewDir + lightDir);

      return ambient + spotFactor * intensity * (
         matDiffuseReflectivity.rgb * max(dot(lightDir, norm), 0.0) +
         specularReflectivity.rgb * pow(max(dot(norm, halfVec), 0.0001), shininess)
         );
   }
   else
   {
      return ambient;
   }
}

vec4 diffuseColor(in vec4 color)
{
   color = texture(diffuseTexture, vec3(ps_in.texCoord, ps_in.texLayer));

   if (color.a < 0.05)
      discard;

   return color;
}

void main()
{   
   if(ps_in.texLayer == 0x3ff)
      discard;

   vec4 outputFrag = vec4(0, 0, 0, 1);
   vec3 n = normalize(ps_in.worldNormal);
   vec3 dirToEye = normalize(eyeLocation.xyz - ps_in.worldVert);

   vec4 albedo = diffuseColor(outputFrag);
   
   if(albedo.a < 0.1)
      discard;	

   vec4 activeLights = vec4(0, 1, 2, -1);

   //lighting
   for (int i = 0; i < 4; i++)
   {
      vec3 lightContribution = vec3(0);
      int lightId = int(activeLights[i]);
      LightData light = lights[lightId];

      if (lightId == -1) {
         continue;
      }
      else if (light.lightType == 0) {
         lightContribution += directionalLight(light, dirToEye, n);
      }
      else if (light.lightType == 1) {
         lightContribution += pointLight(light, dirToEye, n);
      }
      else if (light.lightType == 2) {
         lightContribution += spotLight(light, dirToEye, n);
      }

      outputFrag += vec4(lightContribution, 1) * albedo;
   }

   //restore the alpha
   outputFrag.a = albedo.a;

   //send to buffer
   FragColor = outputFrag;
}