#define V_POSITION vertPos
#define V_NORMAL normal
#define V_COLOR vertColor
#define V_UV uv

#define F_POSITION fragPosition
#define F_NORMAL fragNormal

#ifdef VERT_SHADER

#define VERTEX_ATTRIBUTE(loc, typ, name) layout(location = loc) in typ name
#define VERTEX_ATTRIBUTE_LINE_TWO(loc, typ, name) out typ pass_##name
#define VERTEX_ATTRIBUTE_WORK(loc, typ, name) pass_##name = name

#endif

#ifdef FRAG_SHADER

#define VERTEX_ATTRIBUTE(loc, typ, name) in typ pass_##name
#define VERTEX_ATTRIBUTE_LINE_TWO(loc, typ, name) typ name = pass_##name
#define RETURN_DEBUG_NORMAL return vec4((F_NORMAL + vec3(1.0)) / 2.0, 1.0);

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

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

// Skip this multiply with a shader variation if no model matrix or CPU mode model matrix
uniform mat4 modelMatrix; 
 
uniform float time;
uniform vec3 screenResolution;
uniform vec3 cameraPosition;