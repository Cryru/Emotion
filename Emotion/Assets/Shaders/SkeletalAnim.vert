#version v 
 
uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 
 
// Shader toy API uniforms. 
uniform float iTime; // shader playback time (in seconds) 
uniform vec3 iResolution; // viewport resolution (in pixels) 
 
layout(location = 0)in vec3 vertPos; 
layout(location = 1)in vec2 uv; 
layout(location = 2)in vec4 boneIds;
layout(location = 3)in vec4 boneWeights;
 
const int MAX_BONES = 126;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 finalBonesMatrices[MAX_BONES];

// Goes to the frag shader.  
out vec2 UV; 
out vec4 vertColor; 

void main() { 
    // Pass to frag.
    UV = uv;
    vertColor = vec4(1.0);

    mat4 totalTransform = finalBonesMatrices[int(boneIds[0])] * boneWeights[0];
    for (int i = 1; i < MAX_BONE_INFLUENCE; i++)
    {
        totalTransform += finalBonesMatrices[int(boneIds[i])] * boneWeights[i];
    }
    
    vec4 totalPosition = totalTransform * vec4(vertPos, 1.0);

    // Multiply by projection.
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * totalPosition;
}