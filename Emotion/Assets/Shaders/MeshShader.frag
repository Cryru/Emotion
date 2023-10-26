#version v


uniform vec3 cameraPosition;
uniform vec3 iResolution; // viewport resolution (in pixels)

#define CASCADE_COUNT 3

uniform sampler2D diffuseTexture;
uniform sampler2D shadowMapTextureC1;
uniform sampler2D shadowMapTextureC2;
uniform sampler2D shadowMapTextureC3;
uniform float cascadePlaneFarZ[CASCADE_COUNT];
uniform mat4 cascadeLightProj[CASCADE_COUNT];

// LightModel
uniform vec3 sunDirection;
uniform float ambientLightStrength;
uniform float diffuseStrength;
uniform vec4 ambientColor;

uniform vec4 diffuseColor;
uniform vec4 objectTint;

uniform mat4 viewMatrix;

// Comes in from the vertex shader.
in vec3 vertPos;
in vec2 UV; 
in vec4 vertColor;
 
in vec3 fragPosition;
in vec3 fragNormal;
in vec3 fragLightDir;

out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

#ifdef SHADOW_MAP
void main()
{
}
#else

// todo: array texture
vec4 sampleShadowMapAtCascade(int cascade, vec2 uv)
{
	if (cascade == 0)
	{
		return getTextureColor(shadowMapTextureC1, uv);
	}
	else if (cascade == 1)
	{
		return getTextureColor(shadowMapTextureC2, uv);
	}
	else if (cascade == 2)
	{
		return getTextureColor(shadowMapTextureC3, uv);
	}

	return vec4(0.0);
}

vec4 GetCascadeDebugColor(int cascade)
{
	if (cascade == 0)
	{
		return vec4(1.0, 0.0, 0.0, 1.0);
	}
	else if (cascade == 1)
	{
		return vec4(0.0, 1.0, 0.0, 1.0);
	}
	else if (cascade == 2)
	{
		return vec4(0.0, 0.0, 1.0, 1.0);
	}

	return vec4(0.0);
}

int GetCurrentShadowCascade()
{
	// Determine which cascade this fragment is in
	int cascade = CASCADE_COUNT;
	vec4 fragPosViewSpace = viewMatrix * vec4(fragPosition, 1.0);
	float depthValue = abs(fragPosViewSpace.z);
    
	for (int i = 0; i < CASCADE_COUNT; i++)
	{
		if (depthValue < cascadePlaneFarZ[i])
		{
			cascade = i;
			break;
		}
	}

	return cascade;
}

// todo: figure out how to do this properly, this is driving me crazy
float GetBiasScaleForCascade(int cascade)
{
	if (cascade == 0)
	{
		return 1.0;
	}
	else if (cascade == 1)
	{
		return 4.0;
	}
	else if (cascade == 2)
	{
		return 8.0;
	}

	return 0.0;
}

float GetShadowAmount()
{
	// Determine which cascade this fragment is in
	int cascade = GetCurrentShadowCascade();

	// Get the fragment position in light space.
	vec4 fragPosLightSpace = cascadeLightProj[cascade] * vec4(fragPosition, 1.0);
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	projCoords = projCoords * 0.5 + 0.5;

	float currentDepth = projCoords.z;
	if (currentDepth > 1.0)
		return 0.0;
	
	float biasScaleForCascade = GetBiasScaleForCascade(cascade);
	float minDepth = cascade == 0 ? 10.0f : cascadePlaneFarZ[cascade - 1];
	float maxDepth = cascadePlaneFarZ[cascade];

	float slopeBias = 0.02 * biasScaleForCascade;
	float minBias = 0.002 * biasScaleForCascade;

	float bias = max(slopeBias * (1.0 - dot(fragNormal, fragLightDir)), minBias);
	bias *= 1.0 / ((maxDepth - minDepth) * 0.5);

	// Percentage Closer Filtering
	float shadow = 0.0;
	vec2 texelSize = 1.0 / vec2(textureSize(shadowMapTextureC1, 0)); // assume all the same size, this is how it will behave as a texture array
	for(int x = 0; x <= 1; ++x)
	{
		for(int y = 0; y <= 1; ++y)
		{
			vec2 sampleCoord = vec2(projCoords.xy + vec2(x, y) * texelSize);
			float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
			shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;        
		}    
	}
	shadow /= 4.0;

	return shadow;
}

vec3 PerceptualGrayscale(in vec3 color)
{
    // Perceptual greyscale
    float grey = color.r * 0.3 + color.g * 0.7 + color.b * 0.1;
    return vec3(grey, grey, grey);
}

float cbrt(float x)
{
    float y = uintBitsToFloat(709973695u+floatBitsToUint(x)/3u);
    y = y*(2.0/3.0) + (1.0/3.0)*x/(y*y);
    y = y*(2.0/3.0) + (1.0/3.0)*x/(y*y);
    return y;
}

vec3 RGBToOklab(vec3 rgb)
{

  float r = rgb.x;
  float g = rgb.g;
  float b = rgb.b;

  // This is the Oklab math:
  float l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b;
  float m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b;
  float s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b;

  l = cbrt(l);
  m = cbrt(m);
  s = cbrt(s);

  return vec3(
	l * +0.2104542553 + m * +0.7936177850 + s * -0.0040720468,
	l * +1.9779984951 + m * -2.4285922050 + s * +0.4505937099,
	l * +0.0259040371 + m * +0.7827717662 + s * -0.8086757660
  );
}

vec3 OklabToRGB(vec3 lab) {
  float L = lab.x;
  float a = lab.y;
  float b = lab.z;

  float l = L + a * +0.3963377774 + b * +0.2158037573;
  float m = L + a * -0.1055613458 + b * -0.0638541728;
  float s = L + a * -0.0894841775 + b * -1.2914855480;

  l = pow(l, 3);
  m = pow(m, 3);
  s = pow(s, 3);

  float R = l * +4.0767416621 + m * -3.3077115913 + s * +0.2309699292;
  float G = l * -1.2684380046 + m * +2.6097574011 + s * -0.3413193965;
  float B = l * -0.0041960863 + m * -0.7034186147 + s * +1.7076147010;
  
  return vec3(R, G, B);
}

void main()
{
	// Cascade debug
	//int cascade = GetCurrentShadowCascade();
	//fragColor = GetCascadeDebugColor(cascade);
	//return;

	// Calculate the color of the object.
	vec4 objectColor = getTextureColor(diffuseTexture, UV) * diffuseColor * vertColor;

	// Tint
	{
		vec3 tintColor = objectTint.rgb;

		// Convert both the object and tint colors to OKLab space
		vec3 oklabColor = RGBToOklab(objectColor.rgb);
		vec3 oklabTint = RGBToOklab(tintColor);

		// Calculate the hue difference between the original color and the tint color
		// y holds green-red
		// z holds blue-yellow
		float hueDifference = atan(oklabTint.z, oklabTint.y) - atan(oklabColor.z, oklabColor.y);

		// Modify the hue by rotating it by the difference in hue
		oklabColor.yz = mat2(
			cos(hueDifference), sin(hueDifference),
			-sin(hueDifference), cos(hueDifference)
		) * oklabColor.yz;

		// Convert the modified color back to RGB
		vec3 modifiedRGB = OklabToRGB(oklabColor);

		// Take whichever color is brighter between the source and tint color,
		// and use that to adjust the saturation to avoid desaturation when
		// either the object or tint is too bright.
		float brightness = (tintColor.r + tintColor.g + tintColor.b) / 3.0;
		float brightnessSource = (objectColor.r + objectColor.g + objectColor.b) / 3.0;
		brightness = max(brightness, brightnessSource);

		float saturationFactor = 1.0 - brightness;
		vec3 objectColorTinted = mix(objectColor.rgb * tintColor, modifiedRGB, saturationFactor);

		objectColor = vec4(objectColorTinted.rgb, objectColor.a * objectTint.a);
	}

	// Lighting
	vec3 ambient = ambientLightStrength * ambientColor.rgb;

	float diffuseFactor = max(dot(fragNormal, fragLightDir), 0.0);
	vec3 diffuse = diffuseStrength * diffuseFactor * vec3(1.0);

	// Shadow
	float shadow = GetShadowAmount();

	// Combine
	vec4 finalColor = vec4(ambient + max((1.0 - shadow), 0.1) * diffuse, 1.0) * objectColor;

    fragColor = finalColor;
    if (fragColor.a < 0.01)discard;
}
#endif