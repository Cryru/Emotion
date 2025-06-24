// 1. uniform reflection
// 2. vertex shader main deep
// 3. includes
// 4. vertex attribute passing and reflection
// 5. shader derivatives
// 6. material def reflection
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

float saturate(in float value)
{
    return clamp(value, 0.0, 1.0);
}

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

VERTEX_ATTRIBUTE(2, Vector3, normal);
VERTEX_ATTRIBUTE_LINE_TWO(2, Vector3, normal);

VERTEX_ATTRIBUTE(3, Vector4, vertColor);
VERTEX_ATTRIBUTE_LINE_TWO(3, Vector4, vertColor);

#if VERT_SHADER

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

// Skip this multiply with a shader variation if no model matrix or CPU mode model matrix
uniform mat4 modelMatrix;

vec4 VertexShaderMain_DEEP()
{
    return projectionMatrix * viewMatrix * modelMatrix * vec4(vertPos, 1.0);
}

out vec3 fragPosition;
out vec3 fragNormal;

void VertexShaderMain()
{
    VERTEX_ATTRIBUTE_WORK(0, Vector3, vertPos);
    VERTEX_ATTRIBUTE_WORK(1, Vector2, uv);
    VERTEX_ATTRIBUTE_WORK(2, Vector3, normal);
    VERTEX_ATTRIBUTE_WORK(3, Vector4, vertColor);

    fragPosition = vec3(modelMatrix * vec4(vertPos, 1.0));
    fragNormal = normalize(mat3(transpose(inverse(modelMatrix))) * normal);

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
uniform vec3 cameraPosition;

in vec3 fragPosition;
in vec3 fragNormal;

vec4 FragmentShaderMain()
{
    vec4 col = vertColor;

    vec4 textureColor = texture2D(diffuseTexture, uv);
    vec3 albedo = textureColor.rgb * col.rgb;

    vec3 lightDirection = normalize(vec3(0.0, 0.5, 1.0));
    vec3 lightColor = vec3(0.3);

    float ambient = 0.0;

    // Calculate the diffuse light factor
    vec3 flattenedNormal = mix(fragNormal, vec3(0.0, 0.0, 1.0), 0.2);

    float NdotL = dot(flattenedNormal, lightDirection);
    float diffuse = max(NdotL, 0.01);
    diffuse = saturate(diffuse + ambient);

    // Combine light and terrain color
    vec3 finalColor = albedo * diffuse;
    // finalColor = gammaCorrect(finalColor);

#if EDITOR_BRUSH
    float ringThickness = 0.5;

    float d = length(brushWorldSpace - vertPos.xy);
    float innerRadius = brushRadius - ringThickness;
    float outerRadius = brushRadius;

    if (d >= innerRadius && d <= outerRadius)
    {
        float aaStrength = 0.1;

        float aaInner = smoothstep(innerRadius, innerRadius + aaStrength, d);
        float aaOuter = 1.0 - smoothstep(outerRadius - aaStrength, outerRadius, d);
        float alpha = min(aaInner, aaOuter);
        finalColor = mix(finalColor, brushColor.rgb, alpha);
    }
#endif

    // RETURN_DEBUG_NORMAL

    return vec4(finalColor, 1.0);
}

#endif