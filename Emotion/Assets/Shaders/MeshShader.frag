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
in vec3 lightDir;

out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"
#using "Shaders/ColorHelpers.c"

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

float random(vec2 co)
{
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

// Define a function to generate a random sample within a disk
vec2 randomSampleInDisk(vec2 center, vec2 radius)
{
	return center;

    float randAngle = 2.0 * 3.14159265359 * random(center);
    vec2 randRadius = radius;
    return center + randRadius * vec2(cos(randAngle), sin(randAngle));
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

	float slopeBias = 0.001 * biasScaleForCascade;
	float minBias = 0.0001 * biasScaleForCascade;

	float bias = max(slopeBias * (1.0 - dot(fragNormal, lightDir)), minBias);
	bias *= 1.0 / ((maxDepth - minDepth) * 0.5);
	bias = 0.0;

	// Percentage Closer Filtering
	//float shadow = 0.0;
	//vec2 texelSize = 1.0 / vec2(2048); // assume all the same size, this is how it will behave as a texture array
	//for(int i = 0; i < 16; ++i) // You can adjust the number of samples as needed
	//{
	//	vec2 randomSample = randomSampleInDisk(projCoords.xy, 0.5 * texelSize);
	//	float pcfDepth = sampleShadowMapAtCascade(cascade, randomSample).r;
	//	shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
	//}
	//shadow /= 16.0;

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

void main()
{


	// Cascade debug
	int cascade = GetCurrentShadowCascade();
	//
	//fragColor = ;
	//if(cascade != 0) return;

	// Calculate the color of the object.
	vec4 objectColor = getTextureColor(diffuseTexture, UV) * diffuseColor * vertColor;
	objectColor = ApplyColorTint(objectColor, objectTint);

	// Lighting
	vec3 ambient = ambientLightStrength * ambientColor.rgb;

	float diffuseFactor = max(dot(fragNormal, lightDir), 0.0);
	vec3 diffuse = diffuseStrength * diffuseFactor * vec3(1.0);

	// Shadow
	float shadow = GetShadowAmount();

	// Combine
	vec4 finalColor = vec4(ambient + max((1.0 - shadow), 0.1) * diffuse, 1.0) * objectColor;

    fragColor = finalColor;
    if (fragColor.a < 0.01)discard;
}
#endif