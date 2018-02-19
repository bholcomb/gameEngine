#version 330

uniform sampler2DArray diffuseTexture;

in GeometryStage
{
   smooth vec2 texCoord;
   smooth vec3 worldNormal;
   smooth vec3 worldVert;
   flat mat3 normalMatrix;
   flat int texLayer;
} ps_in;

out vec4 FragColor;

void main()
{   
   if(ps_in.texLayer == 0x3ff)
      discard;

   vec4 color = texture(diffuseTexture, vec3(ps_in.texCoord, ps_in.texLayer));
   
   //send to buffer
   FragColor=color;
}