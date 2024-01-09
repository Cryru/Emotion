#version v

uniform vec3 iResolution; // viewport resolution (in pixels)

uniform vec3 cameraPosition; // world pos
uniform mat4 viewMatrix;

// Shadow
#define CASCADE_COUNT 4
uniform sampler2D shadowMapTextureC1;
uniform sampler2D shadowMapTextureC2;
uniform sampler2D shadowMapTextureC3;
uniform sampler2D shadowMapTextureC4;

uniform int renderingShadowMap;
uniform float cascadePlaneFarZ[CASCADE_COUNT];
uniform mat4 cascadeLightProj[CASCADE_COUNT];

// LightModel
uniform vec3 sunDirection;
uniform float ambientLightStrength;
uniform float diffuseStrength;
uniform float shadowOpacity;
uniform vec4 ambientColor;

// 0 - default
// 1 - no ambient+diffuse
// 2 - no receive shadow
// 3 - 2 and 3
uniform int lightMode;

// Material
uniform sampler2D diffuseTexture;
uniform vec4 diffuseColor;
uniform vec4 objectTint;

// Comes in from the vertex shader.
in vec3 vertPos;
in vec2 UV; 
in vec4 vertColor;
 
in vec3 fragPosition; // world pos
in vec3 fragNormal; // multiplied by normal matrix
in vec3 fragLightDir;

out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"
#using "Shaders/ColorHelpers.c"

// todo: array texture
vec4 sampleShadowMapAtCascade(int cascade, vec2 uv)
{
    vec4 col = vec4(0.0);
    if (cascade == 0)
    {
        col = getTextureColor(shadowMapTextureC1, uv);
    }
    else if (cascade == 1)
    {
        col = getTextureColor(shadowMapTextureC2, uv);
    }
    else if (cascade == 2)
    {
        col = getTextureColor(shadowMapTextureC3, uv);
    }
    else if (cascade == 3)
    {
        col = getTextureColor(shadowMapTextureC4, uv);
    }

    return col;
}

vec4 GetCascadeDebugColor(int cascade)
{
    if (cascade == 0)
        return vec4(1.0, 0.0, 0.0, 1.0);
    else if (cascade == 1)
        return vec4(0.0, 1.0, 0.0, 1.0);
    else if (cascade == 2)
        return vec4(0.0, 0.0, 1.0, 1.0);
    else if (cascade == 3)
        return vec4(1.0, 1.0, 0.0, 1.0);

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

vec4 pepega() {

 // Determine which cascade this fragment is in
    int cascade = GetCurrentShadowCascade();

    // Get the fragment position in light space.
    vec4 fragPosLightSpace = cascadeLightProj[cascade] * vec4(fragPosition, 1.0);
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    float currentDepth = projCoords.z;
    if (currentDepth > 1.0)
        return vec4(1.0, 0.0, 0.0, 1.0);

        vec2 sampleCoord = vec2(projCoords.xy + vec2(0, 0));
        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
        return (currentDepth) > pcfDepth ? vec4(0.0, 1.0, 0.0, 1.0) : vec4(0.0, 0.0, 1.0, 1.0);

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
    {
        if (cascade == CASCADE_COUNT) return 0.0;
        cascade = cascade + 1;

        fragPosLightSpace = cascadeLightProj[cascade] * vec4(fragPosition, 1.0);
        projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
        projCoords = projCoords * 0.5 + 0.5;
        currentDepth = projCoords.z;
    }
    
    float minDepth = cascade == 0 ? 10.0f : cascadePlaneFarZ[cascade - 1];
	float maxDepth = cascadePlaneFarZ[cascade];

    float slopeBias = 0.02 * 4.0;
    float minBias = 0.002 * 4.0;

    float bias = max(slopeBias * (1.0 - dot(fragNormal, fragLightDir)), minBias);
    bias *= 1.0 / ((maxDepth - minDepth) * 0.5);
    

    // Percentage Closer Filtering
    float shadow = 0.0;
    vec2 texelSize = 1.0 / vec2(textureSize(shadowMapTextureC1, 0));

    // Unrolled PCF loop for ANGLE compat.
    {
        vec2 sampleCoord = vec2(projCoords.xy + vec2(0, 0) * texelSize);
        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
    }

//    {
//        vec2 sampleCoord = vec2(projCoords.xy + vec2(1, 0) * texelSize);
//        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
//        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
//    }
//
//    {
//        vec2 sampleCoord = vec2(projCoords.xy + vec2(2, 0) * texelSize);
//        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
//        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
//    }
//
//    {
//        vec2 sampleCoord = vec2(projCoords.xy + vec2(0, 1) * texelSize);
//        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
//        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
//    }
//
//    {
//        vec2 sampleCoord = vec2(projCoords.xy + vec2(1, 1) * texelSize);
//        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
//        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
//    }
//
//    {
//        vec2 sampleCoord = vec2(projCoords.xy + vec2(2, 1) * texelSize);
//        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
//        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
//    }
//
//    {
//        vec2 sampleCoord = vec2(projCoords.xy + vec2(0, 2) * texelSize);
//        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
//        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
//    }
//
//    {
//        vec2 sampleCoord = vec2(projCoords.xy + vec2(1, 2) * texelSize);
//        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
//        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
//    }
//
//    {
//        vec2 sampleCoord = vec2(projCoords.xy + vec2(2, 2) * texelSize);
//        float pcfDepth = sampleShadowMapAtCascade(cascade, sampleCoord).r; 
//        shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
//    }
//
//    shadow /= 9.0;

    return shadow;
}

void main()
{
    if (renderingShadowMap != -1)
    {
        fragColor = vec4(1.0);
        return;
    }

    // Calculate the color of the object.
    vec4 objectColor = getTextureColor(diffuseTexture, UV) * diffuseColor * vertColor;
    objectColor = ApplyColorTint(objectColor, objectTint);

    // Cascade/tinting debug
    //fragColor = objectColor;
    //return;

    // Cascade debug
    //int cascade = GetCurrentShadowCascade();
    //objectColor = GetCascadeDebugColor(cascade);

    vec3 finalColor = objectColor.rgb;

    // Diffuse+Ambient Light
    if (lightMode != 1 && lightMode != 3) // Flat shading flags
    {
        // Normal diffuse factor
        //float diffuseFactor = max(dot(fragNormal, fragLightDir), 0.0);

        // Valve Half Lambert diffuse factor
        // https://developer.valvesoftware.com/wiki/Half_Lambert
        float diffuseFactor = dot(fragNormal, fragLightDir) * 0.5 + 0.5;

        // Combine ambient and diffuse
        vec3 ambient = ambientColor.rgb * ambientLightStrength;
        vec3 diffuse = mix(objectColor.rgb, objectColor.rgb * diffuseFactor, diffuseStrength);
        finalColor = diffuse * ambient;
    }

    // Shadow
    if (lightMode != 2 && lightMode != 3) // Dont receive shadow flags
    {
        float shadow = GetShadowAmount() * shadowOpacity;
        finalColor *= 1.0 - shadow;
    }

    fragColor = vec4(finalColor.rgb, objectColor.a);
    if (fragColor.a < 0.01)discard;
}