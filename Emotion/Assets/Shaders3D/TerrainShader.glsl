// 1. uniform reflection
// 2. vertex shader main deep
// 3. vertex attribute passing and reflection
// 4. shader derivatives
// 5. material def reflection
// 6. includes
// ----------- Common.h

#if VERT_SHADER

#define VERTEX_ATTRIBUTE(loc, typ, name) layout(location = loc) in typ name
#define VERTEX_ATTRIBUTE_LINE_TWO(loc, typ, name) out typ pass_##name
#define VERTEX_ATTRIBUTE_WORK(loc, typ, name) pass_##name = name

#endif

#if FRAG_SHADER

#define VERTEX_ATTRIBUTE(loc, typ, name) in typ pass_##name;
#define VERTEX_ATTRIBUTE_LINE_TWO(loc, typ, name) typ name = pass_##name;

#endif

#define RENDER_STATE
#define RENDER_STATE_END

#define TRUE
#define FALSE
#define CULLING(x)
#define ALPHA_BLEND(x)

#define Vector4 vec4
#define Vector3 vec3
#define Vector2 vec2

#define WHITE vec4(1.0, 1.0, 1.0, 1.0)
#define RED vec4(1.0, 0.0, 0.0, 1.0)
#define GREEN vec4(0.0, 1.0, 0.0, 1.0)
#define BLUE vec4(0.0, 0.0, 1.0, 1.0)
#define COLOR_OPAQUE(col) vec4(col, col, col, 1.0)

// -----------------------------
RENDER_STATE

CULLING(FALSE)
ALPHA_BLEND(FALSE)

RENDER_STATE_END
//------------------------------

#define EDITOR_BRUSH 1

VERTEX_ATTRIBUTE(0, Vector3, vertPos);
VERTEX_ATTRIBUTE_LINE_TWO(0, Vector3, vertPos);

VERTEX_ATTRIBUTE(1, Vector2, uv);
VERTEX_ATTRIBUTE_LINE_TWO(1, Vector2, uv);

VERTEX_ATTRIBUTE(2, Vector4, vertColor);
VERTEX_ATTRIBUTE_LINE_TWO(2, Vector4, vertColor);

VERTEX_ATTRIBUTE(3, Vector3, normal);
VERTEX_ATTRIBUTE_LINE_TWO(3, Vector3, normal);

#if VERT_SHADER

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

// Skip this multiply with a shader variation if no model matrix or CPU mode model matrix
uniform mat4 modelMatrix;

vec4 VertexShaderMain_DEEP()
{
    return projectionMatrix * viewMatrix * modelMatrix * vec4(vertPos, 1.0);
}

void VertexShaderMain()
{
    VERTEX_ATTRIBUTE_WORK(0, Vector3, vertPos);
    VERTEX_ATTRIBUTE_WORK(1, Vector2, uv);
    VERTEX_ATTRIBUTE_WORK(2, Vector4, vertColor);
    VERTEX_ATTRIBUTE_WORK(3, Vector3, normal);

    gl_Position = VertexShaderMain_DEEP();
}

#endif

#if FRAG_SHADER

#if EDITOR_BRUSH
    uniform vec2 brushWorldSpace;
    uniform float brushRadius = 100.0;
    uniform vec4 brushColor = RED;
#endif

uniform sampler2D diffuseTexture;

vec4 FragmentShaderMain()
{
    vec4 col = vertColor;

#if EDITOR_BRUSH
    float ringThickness = 1.5;

    float d = length(brushWorldSpace - vertPos.xy);
    float innerRadius = brushRadius - ringThickness;
    float outerRadius = brushRadius;

    if (d >= innerRadius && d <= outerRadius)
    {
        float aaStrength = 0.1;

        float aaInner = smoothstep(innerRadius, innerRadius + aaStrength, d);
        float aaOuter = 1.0 - smoothstep(outerRadius - aaStrength, outerRadius, d);
        float alpha = min(aaInner, aaOuter);
        col = mix(col, brushColor, alpha);
    }
#endif

    return col;
}

#endif