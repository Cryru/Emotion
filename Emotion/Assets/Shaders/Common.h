#ifdef VERT_SHADER
    #define VERT_TO_FRAGMENT out
#endif

#ifdef FRAG_SHADER
    #define VERT_TO_FRAGMENT in
#endif

#define Vector4 vec4
#define Vector3 vec3
#define Vector2 vec2
#define new 
#define Texture sampler2D

#define WHITE vec4(1.0, 1.0, 1.0, 1.0)
#define RED vec4(1.0, 0.0, 0.0, 1.0)
#define GREEN vec4(0.0, 1.0, 0.0, 1.0)
#define BLUE vec4(0.0, 0.0, 1.0, 1.0)
#define COLOR_OPAQUE(col) vec4(col, col, col, 1.0)

float saturate(in float value)
{
    return clamp(value, 0.0, 1.0);
}

layout(std140, binding = 0) uniform BaseUniforms {
    mat4 projectionMatrix;
    mat4 viewMatrix;
    mat4 modelMatrix;
    vec4 screenResolution;
};

#define time screenResolution.w

uniform vec3 cameraPosition;

#define ALPHA_DISCARD 0.01