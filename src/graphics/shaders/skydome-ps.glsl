#version 430 core

//based on http://blog.simonrodriguez.fr/articles/28-06-2015_un_ciel_dans_le_shader.html

//---------IN------------
in vec3 pos;
in vec3 sun_norm;
in vec3 star_pos;

//---------UNIFORM------------
layout(location = 20) uniform sampler2D tint;//the color of the sky on the half-sphere where the sun is. (time x height)
layout(location = 21) uniform sampler2D tint2;//the color of the sky on the opposite half-sphere. (time x height)
layout(location = 22) uniform sampler2D sun;//sun texture (radius x time)
layout(location = 23) uniform sampler2D moon;//moon texture (circular)
layout(location = 24) uniform sampler2D clouds1;//light clouds texture (spherical UV projection)
layout(location = 25) uniform sampler2D clouds2;//heavy clouds texture (spherical UV projection)
layout(location = 26) uniform float weatherIntensity;//mixing factor (0.5 to 1.0)
layout(location = 27) uniform float weatherSpeed;
layout(location = 28) uniform float sunSize;
layout(location = 29) uniform float moonSize;

//---------OUT------------
out vec3 FragColor;


//---------NOISE GENERATION------------
//Noise generation based on a simple hash, to ensure that if a given point on the dome
//(after taking into account the rotation of the sky) is a star, it remains a star all night long
float Hash( float n )
{
   return fract( (1.0 + sin(n)) * 415.92653);
}

float Noise3d( vec3 x )
{
    float xhash = Hash(round(400*x.x) * 37.0);
    float yhash = Hash(round(400*x.y) * 57.0);
    float zhash = Hash(round(400*x.z) * 67.0);
    return fract(xhash + yhash + zhash);
}

//---------MAIN------------
void main()
{
    vec3 pos_norm = normalize(pos);
    float dist = dot(sun_norm,pos_norm);

    //We read the tint texture according to the position of the sun and the weather factor
    vec3 color_wo_sun = texture(tint2, vec2((sun_norm.y + 1.0) / 2.0,max(0.01,pos_norm.y))).rgb;
    vec3 color_w_sun = texture(tint, vec2((sun_norm.y + 1.0) / 2.0,max(0.01,pos_norm.y))).rgb;
    FragColor = weatherIntensity * mix(color_wo_sun ,color_w_sun, dist * 0.5 + 0.5);

    //Computing u and v for the clouds textures (spherical projection)
    float u = 0.5 + atan(pos_norm.z, pos_norm.x) / (2 * 3.14159265);
    float v = - 0.5 + asin(pos_norm.y) / 3.14159265;

    //Cloud color
    //color depending on the weather (shade of grey) *  (day or night) ?
    vec3 cloud_color = vec3(min(weatherIntensity * 3.0 / 2.0, 1.0)) * (sun_norm.y > 0 ? 0.95 : 0.95 + sun_norm.y * 1.8);

    //Reading from the clouds maps
    //mixing according to the weather (1.0 -> clouds1 (sunny), 0.5 -> clouds2 (rainy))
    //+ time translation along the u-axis (horizontal) for the clouds movement
    float transparency = mix(texture(clouds2,vec2(u + weatherSpeed, v)).r, texture(clouds1,vec2(u + weatherSpeed, v)).r, (weatherIntensity-0.5)*2.0);

    // Stars
    if(sun_norm.y<0.1) //Night or dawn
    {
        float threshold = 0.99;
        //We generate a random value between 0 and 1
        float star_intensity = Noise3d(normalize(star_pos));
        //And we apply a threshold to keep only the brightest areas
        if (star_intensity >= threshold)
        {
            //We compute the star intensity
            star_intensity = pow((star_intensity - threshold)/(1.0 - threshold), 6.0)*(-sun_norm.y+0.1);
            FragColor += vec3(star_intensity);
        }
    }

    //Sun
    float radius = length(pos_norm - sun_norm);
    if(radius < sunSize)  //We are in the area of the sky which is covered by the sun
    {
        float sunHeight = clamp(sun_norm.y, 0.01f, 1);
        radius = radius / sunSize;
        if(radius < 0.99) //< we need a small bias to avoid flickering on the border of the texture
        {
            //We read the alpha value from a texture where x = radius and y=height in the sky (~sunHeight)
            vec4 sun_color = texture(sun, vec2(radius, sunHeight));
            FragColor = mix(FragColor, sun_color.rgb, sun_color.a);
        }
    }

    //Moon
    float radius_moon = length(pos_norm + sun_norm);//the moon is at position -sun_pos
    if(radius_moon < moonSize) //We are in the area of the sky which is covered by the moon
    {
        //We define a local plane tangent to the skydome at -sun_norm
        //We work in model space (everything normalized)
        vec3 n1 = normalize(cross(-sun_norm, vec3(0,1,0)));
        vec3 n2 = normalize(cross(-sun_norm, n1));
        //We project pos_norm on this plane
        float x = dot(pos_norm, n1);
        float y = dot(pos_norm, n2);
        //x,y are two sine, ranging approx from 0 to sqrt(2)*0.03. We scale them to [-1,1], then we will translate to [0,1]
        float scale = (1.0 / (sqrt(2.0) * moonSize)) * 0.5;
        //we need a compensation term because we made projection on the plane and not on the real sphere + other approximations.
        float compensation = 1.4;
        //And we read in the texture of the moon. The projection we did previously allows us to have an undeformed moon
        //(for the sun we didn't care as there are no details on it)
        FragColor = mix(FragColor, texture(moon, vec2(x,y) * scale * compensation + vec2(0.5)).rgb, clamp(-sun_norm.y * 3, 0.0f, 1));
    }

    //Final mix
    //mixing with the cloud color allows us to hide things behind clouds (sun, stars, moon)
    FragColor = mix(FragColor, cloud_color, clamp((2 - weatherIntensity) * transparency, 0.0f, 1));
}
