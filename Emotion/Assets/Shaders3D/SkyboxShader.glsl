// ----------- Common.h

#if VERT_SHADER

#define VERTEX_ATTRIBUTE(loc, typ, name) layout(location = loc) in typ name
#define VERTEX_ATTRIBUTE_LINE_TWO(loc, typ, name) out typ pass_##name
#define VERTEX_ATTRIBUTE_WORK(loc, typ, name) pass_##name = name

#endif

#if FRAG_SHADER

#define VERTEX_ATTRIBUTE(loc, typ, name) in typ pass_##name;
#define VERTEX_ATTRIBUTE_LINE_TWO(loc, typ, name) typ name = pass_##name;
#define RETURN_DEBUG_NORMAL return vec4((fragNormal + vec3(1.0)) / 2.0, 1.0);

#endif

#define Vector4 vec4
#define Vector3 vec3
#define Vector2 vec2

VERTEX_ATTRIBUTE(0, Vector3, vertPos);
VERTEX_ATTRIBUTE_LINE_TWO(0, Vector3, vertPos);

#if VERT_SHADER

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

out vec3 texCoords;

vec4 VertexShaderMain_DEEP()
{
    texCoords = vertPos;

    mat4 viewMatrixNoTranslate = viewMatrix;
    viewMatrixNoTranslate[3] = vec4(0.0, 0.0, 0.0, 1.0);

    vec4 result = projectionMatrix * viewMatrixNoTranslate * modelMatrix * vec4(vertPos, 1.0);
    return result.xyww; // Set depth to 1.0
}

void VertexShaderMain()
{
    VERTEX_ATTRIBUTE_WORK(0, Vector3, vertPos);

    gl_Position = VertexShaderMain_DEEP();
}

#endif

#if FRAG_SHADER

uniform samplerCube diffuseTexture;

in vec3 texCoords;

vec4 FragmentShaderMain()
{
    return texture(diffuseTexture, texCoords);
}

#endif