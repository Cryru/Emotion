#version v
 
uniform sampler2D diffuseTexture;
uniform sampler2D shadowMapTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)

// LightModel
uniform vec3 sunDirection;
uniform float ambientLightStrength;
uniform float diffuseStrength;
uniform vec4 ambientColor;

uniform vec4 diffuseColor;
uniform vec4 objectTint;

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
	float biasMax = 0.0003;
	float biasMin = 0.0001;
	float bias = max(biasMax * (1.0 - dot(normalize(fragPosLightSpace.xyz), fragLightDir)), biasMin);
	//float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	float shadow = 0.0;
	vec2 texelSize = (1.0 / vec2(textureSize(shadowMapTexture, 0))) * 0.5f;
	for(int x = 0; x <= 1; ++x)
	{
		for(int y = 0; y <= 1; ++y)
		{
			float pcfDepth = texture(shadowMapTexture, projCoords.xy + vec2(x, y) * texelSize).r;
			shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
		}
	}
	shadow /= 4.0;

	return shadow;
}

vec3 toGrayscale(in vec3 color)
{
    // Perceptual greyscale
    float grey = color.r * 0.3 + color.g * 0.7 + color.b * 0.1;
    return vec3(grey, grey, grey);
}

void main()
{
	// Calculate the color of the object.
	vec4 objectColor = getTextureColor(diffuseTexture, UV) * diffuseColor * vertColor;

	// Tint
	{
		vec3 tintColor = objectTint.rgb;

		// Calculate the brightness of the tint color
		float brightness = (tintColor.r + tintColor.g + tintColor.b) / 3.0;

		// Calculate a saturation adjustment factor based on the brightness
		// The point is to avoid desaturation when the tint color is bright.
		float saturationFactor = 1.0 - brightness;
		//saturationFactor *= saturationFactor;

		// Centre the colour values using the RGB average and
		// Increase the perceptual saturation (this is the downside of simple RGB calculations - you easily lose hue/sat accuracy)
		vec3 power = brightness-tintColor;
		power *= 2.0;
		
		vec3 objectColorGray = toGrayscale(objectColor.rgb);
		vec3 objectGrayscaleTinted = pow(objectColorGray, 1.0 + power);

		vec3 objectColorTinted = objectGrayscaleTinted;//mix(objectColor.rgb * tintColor, objectGrayscaleTinted, saturationFactor);
		objectColor = vec4(objectColorTinted.rgb, objectColor.a * objectTint.a);
	}

	// Lighting
	vec3 ambient = ambientLightStrength * ambientColor.rgb;

	float diffuseFactor = max(dot(fragNormal, fragLightDir), 0.0);
	vec3 diffuse = diffuseStrength * diffuseFactor * ambientColor.rgb;

	// todo: specular

	// Shadow
	float shadow = ShadowCalculation(fragPositionLightSpace);

	// Combine
	vec4 finalColor = vec4(ambient + (1.0 - shadow) * diffuse, 1.0) * objectColor;

    fragColor = finalColor;
    if (fragColor.a < 0.01)discard;
}
#endif