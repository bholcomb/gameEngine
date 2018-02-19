#version 430

layout(location = 0) uniform float time;
layout(location = 1) uniform float width;
layout(location = 2) uniform float height;


layout(location = 20) uniform sampler2D input;
layout(location = 21)uniform float aperture;

smooth in vec2 texCoord;

out vec4 FragColor;


const float PI = 3.1415926535;

void main(void) 
{    
   float apertureHalf = 0.5 * aperture * (PI / 180.0);
  
   // This factor ajusts the coordinates in the case that
   // the aperture angle is less than 180 degrees, in which
   // case the area displayed is not the entire half-sphere.
   float maxFactor = sin(apertureHalf);
  
   vec2 pos = 2.0 * texCoord.st - 1.0;
  
   float l = length(pos);
   if (l > 1.0) 
   {
      FragColor = vec4(0, 0, 0, 1);  
   }
   else 
   {
      float x = maxFactor * pos.x;
      float y = maxFactor * pos.y;
    
      float n = length(vec2(x, y));
    
      float z = sqrt(1.0 - n * n);
  
      float r = atan(n, z) / PI; 
  
      float phi = atan(y, x);

      float u = r * cos(phi) + 0.5;
      float v = r * sin(phi) + 0.5;

      FragColor = texture2D(input, vec2(u, v));
   }
}
