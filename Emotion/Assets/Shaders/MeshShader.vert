#version v 

#if SKINNED_SHADOW_MAP
#define SHADOW_MAP 1
#define SKINNED 1
#endif

uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 
 
// Shader toy API uniforms. 
uniform float iTime; // shader playback time (in seconds) 
uniform vec3 iResolution; // viewport resolution (in pixels) 
 
layout(location = 0)in vec3 vertPos; 
layout(location = 1)in vec2 uv; 
layout(location = 2)in vec4 color;

layout(location = 3)in vec3 normal; 

#if SKINNED
layout(location = 4)in vec4 boneIds;
layout(location = 5)in vec4 boneWeights;
#endif

uniform vec4 diffuseColor;
uniform vec4 objectTint;

uniform vec4 sunColor;
uniform vec3 sunDirection;

#if SHADOW_MAP

#endif
uniform mat4 lightViewProj;

#if SKINNED
const int MAX_BONES = 126;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 boneMatrices[MAX_BONES];
#endif

// Goes to the frag shader.  
out vec2 UV; 
out vec4 vertColor;

out vec3 fragNormal;
out vec3 fragLightDir;

out vec3 fragPosition;
out vec4 fragPositionLightSpace;

void main() { 
    // Pass to frag.
    UV = uv;
    vertColor = color * diffuseColor * objectTint;

    fragPosition = vec3(modelMatrix * vec4(vertPos, 1.0));
    fragPositionLightSpace = lightViewProj * vec4(fragPosition, 1.0);

    fragNormal = mat3(transpose(inverse(modelMatrix))) * normal;
    fragLightDir = normalize(sunDirection);

    vec4 totalPosition = vec4(vertPos, 1.0);

    #if SKINNED

    mat4 totalTransform = boneMatrices[int(boneIds[0])] * boneWeights[0];
    for (int i = 1; i < MAX_BONE_INFLUENCE; i++)
    {
        totalTransform += boneMatrices[int(boneIds[i])] * boneWeights[i];
    }
    totalPosition = totalTransform * totalPosition;

    #endif

    // Multiply by projection.
    #if SHADOW_MAP
        gl_Position = lightViewProj * modelMatrix * totalPosition;
    #else
        gl_Position = projectionMatrix * viewMatrix * modelMatrix * totalPosition;
    #endif
}