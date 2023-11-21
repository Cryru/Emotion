#version v


uniform vec3 cameraPosition;
uniform vec3 iResolution; // viewport resolution (in pixels)

#define CASCADE_COUNT 3

struct Material {
	float metallic;
	float roughness;
}; 
  
uniform Material material;

struct LightData {
	float shadowOpacity;
};

uniform LightData LightModel;

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
		return 1.25;
	}
	else if (cascade == 1)
	{
		return 4.25;
	}
	else if (cascade == 2)
	{
		return 8.25;
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
			shadow += (currentDepth - bias) > pcfDepth ? LightModel.shadowOpacity : 0.0;        
		}    
	}
	shadow /= 4.0;

	return shadow;
}

const float PI = 3.14159265359;

vec3 FresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float DistributionGGX(float NdotH, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

// float GeometrySchlickGGX(float NdotV, float roughness)
// {
//     float r = (roughness + 1.0);
//     float k = (r*r) / 8.0;
// 
//     float num   = NdotV;
//     float denom = NdotV * (1.0 - k) + k;
// 	
//     return num / denom;
// }

// Disney GGX
float GeometrySchlickGGX( float hdotN, float alphaG )
{
	float a2 = alphaG * alphaG;
	float tmp = ( hdotN * hdotN ) * ( a2 - 1.0 ) + 1.0;
	//tmp *= tmp;

	return ( a2 / ( PI * tmp ) );
}

vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}   

void main()
{
	// Cascade debug
	//int cascade = GetCurrentShadowCascade();
	//fragColor = GetCascadeDebugColor(cascade);
	//return;

	// Material props
	vec4 objectColor = getTextureColor(diffuseTexture, UV) * diffuseColor * vertColor;
	objectColor = ApplyColorTint(objectColor, objectTint);
	vec3 albedo = pow(objectColor.rgb, vec3(2.2));

	float metallic = material.metallic;
	float roughness = material.roughness;

	vec3 F0 = vec3(0.0); //vec3(0.04); // non metallic metallicness
	F0 = mix(F0, albedo, metallic);

	// Ambient light (PBR)
	vec3 finalColor = vec3(0.0);
	{
		vec3 ambientIntensity = ambientColor.rgb * ambientLightStrength;

		vec3 n = normalize(fragNormal);
		vec3 v = normalize(cameraPosition - fragPosition);
		vec3 l = normalize(fragLightDir);
		vec3 h = normalize(v + l); // half vector

		float nDotH = max(dot(n, h), 0.0);
		float vDotH = max(dot(v, h), 0.0);
		float nDotL = max(dot(n, l), 0.0);
		float nDotV = max(dot(n, v), 0.0);

		// Specular BRDF
		// ---
		// Normal distribution function (microfacets reflected)
		float NDF = DistributionGGX(nDotH, roughness);

		// Geometry function
		float ggx2  = GeometrySchlickGGX(nDotV, roughness);
		float ggx1  = GeometrySchlickGGX(nDotL, roughness);
		float G = ggx1 * ggx2;

		// Fresnel function
		vec3 F = FresnelSchlick(nDotH, F0);

		// kS + kD == 1.0
		vec3 kS = F0;//fresnelSchlickRoughness(nDotV, F0, roughness); // specular
		vec3 kD = vec3(1.0) - kS; // diffuse
		kD *= 1.0 - metallic;

		vec3 numerator = NDF * G * F;
		float denominator = 4.0 * nDotV * nDotL;
		denominator = max(denominator, 0.0001); // prevent division by zero
		vec3 specular = numerator / denominator;
	
		// Diffuse BRDF
		// ---
		
		// Valve Half Lambert
		// https://developer.valvesoftware.com/wiki/Half_Lambert
		vec3 fHalfLambert = vec3(0.5) * (nDotL + vec3(1.0));
		vec3 fLambert = albedo; // color of the surface

		vec3 diffuse = kD * fLambert;

		nDotL = max(nDotL, diffuseStrength);
		finalColor += (diffuse + specular) * nDotL * ambientIntensity;
	}

	float shadow = GetShadowAmount();
	finalColor *= 1.0 - shadow;
	
	vec3 color = finalColor;
	//color = color / (color + vec3(1.0)); // tone mapping
	color = pow(color, vec3(1.0/2.2)); // gamma correction

    fragColor = vec4(color, objectColor.a);
    if (fragColor.a < 0.01)discard;
}
#endif