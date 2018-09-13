#version 430
#extension GL_ARB_bindless_texture : require


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

struct LightData {
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

layout(std140, binding = 2) uniform material{
	vec4 matAmbientReflectivity;
	vec4 matDiffuseReflectivity;
	vec4 matSpecularReflectivity;
	vec4 emmission;
	float shininess;
	float alpha;
   float alphaTest;
   float parallaxScale;
   sampler2D diffuseMap;
   sampler2D specularMap;
   sampler2D normalMap;
	bool hasSpecularMap;
	bool hasNormalMap;
   bool hasParallaxMap;
};


//layout(location = 20) uniform sampler2D diffuseMap;
//layout(location = 21) uniform sampler2D specularMap;
//layout(location = 22) uniform sampler2D normalMap;

vec4 specularReflectivity;
vec4 normal;

flat in vec4 activeLights;

in VertexStage
{
   smooth vec2 texCoord;
   smooth vec3 worldNormal;
   smooth vec3 worldVert;
   mat3 normalMatrix;
};

//outputs
out vec4 FragColor;

//tangent space calculation from http://www.thetenthplanet.de/archives/1180
mat3 cotangent_frame(vec3 N, vec3 p, vec2 uv)
{
	// get edge vectors of the pixel triangle
	vec3 dp1 = dFdx(p);
	vec3 dp2 = dFdy(p);
	vec2 duv1 = dFdx(uv);
	vec2 duv2 = dFdy(uv);

	// solve the linear system
	vec3 dp2perp = cross(dp2, N);
	vec3 dp1perp = cross(N, dp1);
	vec3 T = dp2perp * duv1.x + dp1perp * duv2.x;
	vec3 B = dp2perp * duv1.y + dp1perp * duv2.y;

	// construct a scale-invariant frame 
	float invmax = inversesqrt(max(dot(T, T), dot(B, B)));
	return mat3(T * invmax, B * invmax, N);
}

//#define WITH_NORMALMAP_GREEN_UP 1

vec3 perturb_normal(mat3 TBN, vec2 texcoord)
{
	// assume N, the interpolated vertex normal and 
	// V, the view vector (vertex to eye)
	vec3 map = texture(normalMap, texcoord).xyz;
   map = map * 2.0 - 1.0;

#ifdef WITH_NORMALMAP_UNSIGNED
	map = map * 255. / 127. - 128. / 127.;
#endif
#ifdef WITH_NORMALMAP_2CHANNEL
	map.z = sqrt(1. - dot(map.xy, map.xy));
#endif
#ifdef WITH_NORMALMAP_GREEN_UP
	map.y = -map.y;
#endif
	return normalize(TBN * map);
}

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
	vec3 diff = light.color.rgb * matDiffuseReflectivity.rgb * max(dot(lightDir, norm ), 0.0);
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

vec4 diffuseColor(in vec4 color, vec2 T)
{
	color = texture(diffuseMap, T);

	if (color.a < alphaTest)
		discard;

	return color;
}

//steep parallax mapping from http://sunandblackcat.com/tipFullView.php?topicid=28
vec2 parallax(vec3 V, vec2 T, out float parallaxHeight)
{

   // get depth for this fragment
   float initialHeight = 1.0 - texture(normalMap, T).a;

   // calculate amount of offset for Parallax Mapping
   vec2 texCoordOffset = parallaxScale * -V.xy * initialHeight;

   parallaxHeight = initialHeight;

   // retunr modified texture coordinates
   return T + texCoordOffset;
}

void main()
{  
	vec4 outputFrag = vec4(0, 0, 0, 1);
   vec2 uv = texCoord;
   vec3 n = normalize(worldNormal);
   vec3 dirToEye = normalize(eyeLocation.xyz - worldVert);
   mat3 TBN = cotangent_frame(n, worldVert, texCoord);

   if(hasParallaxMap)
   {
      float height = 0;
      vec3 tangentViewVector = normalize(transpose(TBN) * dirToEye); //move view vector from world to tangent space
      uv = parallax(tangentViewVector, uv, height);
   }

	vec4 albedo = diffuseColor(outputFrag, uv);
	
	if (albedo.a < 0.1) 
		discard;

	specularReflectivity = matSpecularReflectivity;
   if (hasSpecularMap)
   {
      specularReflectivity = texture(specularMap, uv);
   }

   if (hasNormalMap)
   {
     n = perturb_normal(TBN, uv);
   }

	//FragColor = vec4(transpose(TBN) * n, 1);
	//return;

	for (int i = 0; i < 4; i++)
	{
		vec3 lightContribution = vec3(0);
		int lightId = int(activeLights[i]);
		LightData light = lights[lightId];

		if (lightId == -1) {
			continue;
		}
		else if (light.lightType == 0) {
			lightContribution += directionalLight(light, worldVert, n);
		}
		else if (light.lightType == 1) {
			lightContribution += pointLight(light, worldVert, n);
		}
		else if (light.lightType == 2) {
			lightContribution += spotLight(light, worldVert, n);
		}

		outputFrag += vec4(lightContribution, 1) * albedo;
	}

	//restore the alpha
	outputFrag.a = albedo.a;
	
	FragColor = outputFrag;
}
