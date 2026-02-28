// 1. uniform reflection
// 2. vertex shader main deep
// 4. vertex attribute passing and reflection
// 5. shader derivatives
// 6. material def reflection

// INCLUDE_FILE <Shaders/Common.h>

#define EDITOR_BRUSH 1

VERTEX_ATTRIBUTE(0, Vector3, V_POSITION);
VERTEX_ATTRIBUTE_LINE_TWO(0, Vector3, V_POSITION);

VERTEX_ATTRIBUTE(1, Vector3, V_NORMAL);
VERTEX_ATTRIBUTE_LINE_TWO(1, Vector3, V_NORMAL);

VERTEX_ATTRIBUTE(2, Vector4, V_COLOR);
VERTEX_ATTRIBUTE_LINE_TWO(2, Vector4, V_COLOR);

VERTEX_ATTRIBUTE(3, Vector4, weights);
VERTEX_ATTRIBUTE_LINE_TWO(3, Vector4, weights);

#ifdef VERT_SHADER

vec4 VertexShaderMain_DEEP()
{
    return projectionMatrix * viewMatrix * modelMatrix * vec4(V_POSITION, 1.0);
}

out vec3 F_POSITION;
out vec3 F_NORMAL;

void VertexShaderMain()
{
    VERTEX_ATTRIBUTE_WORK(0, Vector3, V_POSITION);
    VERTEX_ATTRIBUTE_WORK(1, Vector3, V_NORMAL);
    VERTEX_ATTRIBUTE_WORK(2, Vector4, V_COLOR);
    VERTEX_ATTRIBUTE_WORK(3, Vector4, weights);

    F_POSITION = vec3(modelMatrix * vec4(V_POSITION, 1.0));
    F_NORMAL = normalize(mat3(transpose(inverse(modelMatrix))) * V_NORMAL);

    gl_Position = VertexShaderMain_DEEP();
}

#endif

#ifdef FRAG_SHADER

#if EDITOR_BRUSH
    uniform vec2 brushWorldSpace;
    uniform float brushRadius = 100.0;
    uniform vec4 brushColor = RED;
#endif

uniform Texture diffuseTexture;

in vec3 F_POSITION;
in vec3 F_NORMAL;

vec4 SampleTexture(const int textureIndex)
{  
    if (textureIndex >= 4)
    {
        // Divide by 2 to ensure that each square in the default grid texture is 1 big!
        return texture2D(diffuseTexture, F_POSITION.xy / 2.0);
    }

    vec4 colorsForWeights[4] = vec4[](
        RED,
        GREEN,
        BLUE,
        vec4(253.0 / 255.0, 141.0 / 255.0, 20.0 / 255.0, 1.0)
    );
    return colorsForWeights[textureIndex];
}

vec4 FragmentShaderMain()
{
    // Splat
    float sum = weights[0] + weights[1] + weights[2] + weights[3];
    float w4 = clamp(1.0 - sum, 0.0, 1.0);
    vec4 colorsForWeights[5] = vec4[](
        RED,
        GREEN,
        BLUE,
        vec4(253.0 / 255.0, 141.0 / 255.0, 20.0 / 255.0, 1.0),
        vec4(0.0, 0.0, 0.0, 1.0)
    );
    vec4 col = SampleTexture(0) * weights[0] +
           SampleTexture(1) * weights[1] +
           SampleTexture(2) * weights[2] +
           SampleTexture(3) * weights[3] +
           SampleTexture(4) * w4;
    col *= V_COLOR;
    vec3 albedo = col.rgb;

    vec3 lightDirection = normalize(vec3(0.0, 0.5, 1.0));
    vec3 lightColor = vec3(0.3);

    float ambient = 0.0;

    // Calculate the diffuse light factor
    vec3 flattenedNormal = mix(F_NORMAL, vec3(0.0, 0.0, 1.0), 0.2);

    float NdotL = dot(flattenedNormal, lightDirection);
    float diffuse = max(NdotL, 0.01);
    diffuse = saturate(diffuse + ambient);

    // Combine light and terrain color
    vec3 finalColor = albedo * diffuse;
    // finalColor = gammaCorrect(finalColor);

#ifdef EDITOR_BRUSH
    float ringThickness = 0.3;

    float d = length(brushWorldSpace - V_POSITION.xy);
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