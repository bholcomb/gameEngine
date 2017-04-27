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

/*  Forward declare block for other shaders to use
vec3 mod289(vec3 x);
vec2 mod289(vec2 x);
vec3 permute(vec3 x);
vec4 taylorInvSqrt(vec4 r);
*/

#define NOISE_SIMPLEX_1_DIV_289 0.00346020761245674740484429065744f

vec4 fade(vec4 t) {
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}

vec3 mod7(vec3 x) {
  return x - floor(x * (1.0 / 7.0)) * 7.0;
}

float mod289(float x) 
{
  return x - floor(x * (1.0 / 289.0)) * 289.0; 
}

vec2 mod289(vec2 x) 
{
  return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
}

vec3 mod289(vec3 x) 
{
  return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
}

vec4 mod289(vec4 x) 
{
  return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0; 
}




float permute(float x) 
{
     return mod289(((x*34.0)+1.0)*x);
}

vec3 permute(vec3 x) 
{
  return mod289(((x*34.0)+1.0)*x);
}

vec4 permute(vec4 x) 
{
     return mod289(((x*34.0)+1.0)*x);
}



float taylorInvSqrt(float r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}

vec4 taylorInvSqrt(vec4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}

