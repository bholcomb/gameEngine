//
// Description : Array and textureless GLSL 2D simplex noise function.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : stegu
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//               https://github.com/stegu/webgl-noise
// 

#version 430

//forward declared from noise3d-cs.glsl
float snoise(vec3 v, float seed);

layout(local_size_x = 32, local_size_y = 32) in;
layout(r32f, binding = 0) uniform image2D tex;

layout(location = 0 ) uniform float seed;
layout(location = 1 ) uniform int function;
layout(location = 2 ) uniform int octaves;
layout(location = 3 ) uniform float frequency;
layout(location = 4 ) uniform float lacunarity;
layout(location = 5 ) uniform float H;
layout(location = 6 ) uniform float gain;
layout(location = 7 ) uniform float offset;
layout(location = 8 ) uniform int face;

float fBm(vec3 p)
{
   float value = 0.0;
   
   p *= frequency;

	for(int i = 0; i < octaves; ++i) 
   {
		value += snoise(p, seed) * pow(lacunarity, -H * i);
		p *= lacunarity;
	}
	return value;
}

float hybridmultifractal(vec3 p)
{
   p *= frequency;

   float result = snoise(p, seed) + offset;
   float weight = gain * result;
   
   p *= lacunarity;

    for(int i=1; i<octaves; ++i)
    {
        //prevent divergence
        weight = min(weight, 1.0f);
        float signal = snoise(p, seed) + offset * pow(lacunarity, -H * i);
        result += weight * signal;
        weight *= gain * signal;
        
        p *= lacunarity;
    }

    return result;
}

float ridgedmultifractal(vec3 p)
{
   float value = 0.0;

   p *= frequency;

   for (int i = 0; i< octaves; i++)
   {
      float signal = snoise(p, seed);
      signal = offset - abs(signal);
      signal *= signal;
      value += signal * pow(lacunarity, -i * H);

      p *= lacunarity;
   }

   return value;
}

float multifractal(vec3 p)
{
   float value = 1.0;

   p *= frequency;

   for(int i=0; i< octaves; i++)
   {
      value *= (snoise(p, seed) + offset) * pow(lacunarity, -i * H) + 1;
      p *= lacunarity;
   }

   return value;
}

vec3 generateSampleVec(int f, vec2 uv)
{
   //map input coord from 0..1 to -1..1
   vec2 t = uv * 2 - 1.0;

   switch(f)
   {
      case 0: // +x
         return vec3(1, t.y, -t.x);
      case 1: // -x
         return vec3(-1, t.y, t.x);
      case 2: // +y
         return vec3(t.x, -1, t.y);
      case 3: // -y
         return vec3(t.x, 1, -t.y);
      case 4: // +z
         return vec3(t.x, t.y, 1);
      case 5: // -z
         return vec3(-t.x, t.y, -1);
   }
}

void main()
{
   ivec2 outPos = ivec2(gl_GlobalInvocationID.xy);
   vec2 texCoord = vec2(outPos) / vec2(imageSize(tex));
   vec3 sampleVec = generateSampleVec(face, texCoord);

   sampleVec = normalize(sampleVec);
   
   float n = 0;
   switch (function)
   {
      case 0: n = fBm(sampleVec); break;
      case 1: n = multifractal(sampleVec); break;
      case 2: n = ridgedmultifractal(sampleVec); break;
      case 3: n = hybridmultifractal(sampleVec); break;
   }
   
   //remap to the range 0..1 vs -1..1
   n = 0.5 + 0.5 * n;

   //write it back out
   imageStore(tex, outPos, vec4(n));
}