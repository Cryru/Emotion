// INCLUDE_FILE <Shaders/Common.h>

// DEFINE_VERTEX_ATTRIBUTE Position V_Pos
// DEFINE_VERTEX_ATTRIBUTE Normal V_Normal
// DEFINE_VERTEX_ATTRIBUTE VertexColor V_Color
// DEFINE_VERTEX_ATTRIBUTE Custom0 V_Weights

VERT_TO_FRAGMENT vec3 F_Pos;
VERT_TO_FRAGMENT vec3 F_Normal;

#define EDITOR_BRUSH 1

#ifdef VERT_SHADER

vec4 VertexShaderMain()
{
    vec4 localPos = modelMatrix * vec4(V_Pos, 1.0);

    F_Pos = vec3(modelMatrix * localPos);
    F_Normal = normalize(mat3(transpose(inverse(modelMatrix))) * V_Normal);

    return projectionMatrix * viewMatrix * localPos;
}

#endif

#ifdef FRAG_SHADER

#if EDITOR_BRUSH
    uniform vec2 brushWorldSpace;
    uniform float brushRadius = 100.0;
    uniform vec4 brushColor = RED;
#endif

uniform Texture diffuseTexture;

vec4 SampleTexture(const int textureIndex) // temp until texture splatting
{  
    if (textureIndex >= 4)
    {
        // Divide by 2 to ensure that each square in the default grid texture is 1 big!
        return texture2D(diffuseTexture, F_Pos.xy / 2.0);
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
    float sum = V_Weights[0] + V_Weights[1] + V_Weights[2] + V_Weights[3];
    float w4 = clamp(1.0 - sum, 0.0, 1.0);
    vec4 colorsForWeights[5] = vec4[](
        RED,
        GREEN,
        BLUE,
        vec4(253.0 / 255.0, 141.0 / 255.0, 20.0 / 255.0, 1.0),
        vec4(0.0, 0.0, 0.0, 1.0)
    );
    vec4 col = SampleTexture(0) * V_Weights[0] +
           SampleTexture(1) * V_Weights[1] +
           SampleTexture(2) * V_Weights[2] +
           SampleTexture(3) * V_Weights[3] +
           SampleTexture(4) * w4;
    col *= V_Color;
    vec3 albedo = col.rgb;

    vec3 lightDirection = normalize(vec3(0.0, 0.5, 1.0));
    vec3 lightColor = vec3(0.3);

    float ambient = 0.0;

    // Calculate the diffuse light factor
    vec3 flattenedNormal = mix(F_Normal, vec3(0.0, 0.0, 1.0), 0.2);

    float NdotL = dot(flattenedNormal, lightDirection);
    float diffuse = max(NdotL, 0.01);
    diffuse = saturate(diffuse + ambient);

    // Combine light and terrain color
    vec3 finalColor = albedo * diffuse;

#ifdef EDITOR_BRUSH
    float ringThickness = 0.3;

    float d = length(brushWorldSpace - V_Pos.xy);
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