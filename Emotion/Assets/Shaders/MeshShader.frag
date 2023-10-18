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