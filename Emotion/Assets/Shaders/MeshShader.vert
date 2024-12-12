#version v 

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

#ifdef SKINNED
layout(location = 4)in vec4 boneIds;
layout(location = 5)in vec4 boneWeights;
#endif

uniform vec4 sunColor;
uniform vec3 sunDirection;

#define CASCADE_COUNT 4
uniform mat4 cascadeLightProj[CASCADE_COUNT];
uniform int renderingShadowMap;

#ifdef SKINNED
const int MAX_BONES = 200;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 boneMatrices[MAX_BONES];
#endif

// Goes to the frag shader.  
out vec2 UV; 
out vec4 vertColor;

out vec3 fragNormal;
out vec3 fragLightDir;

out vec3 fragPosition;

//#ifdef SKINNED
//#define DEBUG_WEIGHT_BONE 3
//#endif

#ifdef DEBUG_WEIGHT_BONE
out float debugBoneWeight;
#endif

void main() { 
    // Pass to frag.
    UV = uv;
    vertColor = color;
    
    fragLightDir = normalize(sunDirection);

    vec4 totalPosition = vec4(vertPos, 1.0);

#ifdef SKINNED

#ifdef DEBUG_WEIGHT_BONE
    debugBoneWeight = 0.0;
    for (int i = 0; i < MAX_BONE_INFLUENCE; i++)
    {
        int id = int(boneIds[0]);
        if (id == DEBUG_WEIGHT_BONE)
        {
            debugBoneWeight = boneWeights[i];
            break;
        }
    }
#endif

    mat4 totalTransform = boneMatrices[int(boneIds[0])] * boneWeights[0];
    for (int i = 1; i < MAX_BONE_INFLUENCE; i++)
    {
        totalTransform += boneMatrices[int(boneIds[i])] * boneWeights[i];
    }
    totalPosition = totalTransform * totalPosition;
    fragNormal = normalize(mat3(transpose(inverse(modelMatrix * totalTransform))) * normal);
#else
    fragNormal = normalize(mat3(transpose(inverse(modelMatrix))) * normal);
#endif

    fragPosition = vec3(modelMatrix * totalPosition);

    // Multiply by projection.
    if (renderingShadowMap == -1)
        gl_Position = projectionMatrix * viewMatrix * modelMatrix * totalPosition;
    else
        gl_Position = cascadeLightProj[renderingShadowMap] * modelMatrix * totalPosition;
}