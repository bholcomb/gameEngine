#version 430

out vec4 FragColor;

layout(location = 20)uniform sampler2D textMap;

//uniform for color
layout(location = 21) uniform vec4 baseColor;
layout(location = 22) uniform vec4 outlineColor;
layout(location = 23) uniform vec4 glowColor;

//uniform for antiailiasing factor
layout(location = 24) uniform float ailasingFactor;

smooth in vec2 texCoord;

//uniform for texture data

//uniforms for effects
layout(location = 25) uniform int activeEffects;
//layout(location = 26)  uniform float outlineMin_0;
//layout(location = 27) uniform float outlineMin_1;
//layout(location = 28) uniform float outlineMax_0;
//layout(location = 29) uniform float outlineMax_1;
//layout(location = 30) uniform vec2 glowOffset;

void main()
{
	vec4 color=baseColor;

	color.a = texture(textMap, texCoord).a;
	float distanceFactor = color.a;
	
	float width = fwidth(texCoord.x) * ailasingFactor; 
	color.a = smoothstep(0.5-width, 0.5+width, color.a);
	
	if((activeEffects & 0x01) != 0)
	{
		// Outline constants
		float outlineMin_0 = 0.4;
		float outlineMin_1 = outlineMin_0 + width * 2;
		float outlineMax_0 = 0.5;
		float outlineMax_1 = outlineMax_0 + width * 2;
	
		// Outline calculation
		if (distanceFactor > outlineMin_0 && distanceFactor < outlineMax_1)
		{
			float outlineAlpha;
			if (distanceFactor < outlineMin_1)
				outlineAlpha = smoothstep(outlineMin_0, outlineMin_1, distanceFactor);
			else
				outlineAlpha = smoothstep(outlineMax_1, outlineMax_0, distanceFactor);
			
			color = mix(baseColor, outlineColor, outlineAlpha);
		}
	}
	
	
	// Shadow / glow constants
	if((activeEffects & 0x02 ) != 0)
	{
		vec2 glowOffset = vec2(-0.004, -0.004);
		vec4 glowColour = vec4(0, 0, 0, 1);
	
		// Shadow / glow calculation
		float glowDistance = texture(textMap, texCoord + glowOffset).a;
		float glowFactor = smoothstep(0.3, 0.5, glowDistance);
	
		color = mix(vec4(glowColor.rgb, glowFactor), baseColor, baseColor.a);
	}
	
	FragColor = color;
}