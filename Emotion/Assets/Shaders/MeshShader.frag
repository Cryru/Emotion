#version v
 
uniform sampler2D diffuseTexture;
uniform sampler2D shadowMapTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
uniform vec4 sunColor;
uniform vec3 sunDirection;
uniform vec4 ambientColor;

// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
in vec3 fragNormal;
in vec3 fragLightDir;

in vec3 fragPosition;
in vec4 fragPositionLightSpace;

out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

#ifdef SHADOW_MAP
void main()
{
}
#else
float ShadowCalculation(vec4 fragPosLightSpace)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5; 

    if (projCoords.z > 1.0)
        return 0.0;

    float closestDepth = getTextureColor(shadowMapTexture, projCoords.xy).r;
    float currentDepth = projCoords.z;

    // Fight shadow acne
    float bias = 0.0001;
    //float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;  

	float shadow = 0.0;
	vec2 texelSize = (1.0 / textureSize(shadowMapTexture, 0)) * 0.5f;
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			float pcfDepth = texture(shadowMapTexture, projCoords.xy + vec2(x, y) * texelSize).r; 
			shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;        
		}    
	}
	shadow /= 9.0;

    return shadow;
}

void main()
{
    // Diffuse
    vec3 diffuseCalc = max(dot(fragNormal, fragLightDir), 0.0) * sunColor.rgb;
	vec3 diffuse = sunColor.rgb == vec3(0.0) ? vec3(1.0) : diffuseCalc;

    // Shadow
    float shadow = ShadowCalculation(fragPositionLightSpace);       

    vec4 objectColor = getTextureColor(diffuseTexture, UV) * vertColor;
    vec4 finalColor = vec4((ambientColor.rgb + (1.0 - shadow) * diffuse), 1.0) * objectColor;

    fragColor = finalColor;
    if (fragColor.a < 0.01)discard;
}
#endif