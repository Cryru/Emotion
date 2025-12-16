
uniform sampler2D mainTexture;
 
#if FRAG_SHADER
#define getTextureSize(sampler) textureSize(sampler, 0)
#define getTextureColor(sampler, uv) texture(sampler, uv)

// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;

uniform vec4 EffectColor;
uniform float EffectValue;
uniform int EffectType; // TextEffectType enum

//#define IQ_DEBUG 1
#if IQ_DEBUG
vec4 GetSDFDebug(float d)
{
    // Base inside/outside colors
    vec3 col = (d > 0.5)
        ? vec3(0.9, 0.6, 0.3)     // outside
        : vec3(0.65, 0.85, 1.0);  // inside

    // Distance falloff (IQ style)
    col *= 1.0 - exp(-6.0 * abs(d));
    col *= 0.8 + 0.2 * cos(150.0 * d);

    if (d > 0.48 && d < 0.52)
    {
        float edge = 1.0 - smoothstep(0.49, 0.51, d);
        col = mix(col, vec3(1.0), edge);
    }

    return vec4(col, 1.0);
}
#endif

vec4 FragmentShaderMain()
{
    float shapeEdge = 0.5;
    float sdfSpread = (EffectValue * 2) + 4; // TextEffect.GetSDFSpread in pixels (8, 16 etc.)
    float distSample = getTextureColor(mainTexture, UV).a;

#if IQ_DEBUG
    return GetSDFDebug(distSample);
#endif

    float valid = 1.0 - smoothstep(0.999, 1.0, distSample);

    float d = (distSample - shapeEdge) * sdfSpread;
    float aa = fwidth(distSample) * sdfSpread;

    // Fill alpha (inside the glyph)
    float fillAlpha = valid * (1.0 - smoothstep(-aa, +aa, d));
    fillAlpha = clamp(fillAlpha, 0.0, 1.0);
    float premultFillAlpha = fillAlpha * vertColor.a;
    vec3 premultFill = vertColor.rgb * premultFillAlpha;

    // Outline branch
    if (EffectType == 1) // Outline
    {
        vec4 outlineColor = EffectColor; // RGBA
        float outlineSize = EffectValue;  // in pixels

        float halfOutline = outlineSize * 0.5;
        float outlineAlpha = valid * (1.0 - smoothstep(halfOutline - aa, halfOutline + aa, abs(d)));

        outlineAlpha *= outlineColor.a;

        // Premultiply outline color
        vec3 premultOutline = outlineColor.rgb * outlineAlpha;

        // Composite: outline behind fill (premultiplied alpha)
        float finalAlpha = premultFillAlpha + outlineAlpha * (1.0 - premultFillAlpha);
        vec3 finalRGB = premultFill + premultOutline * (1.0 - premultFillAlpha);

        return vec4(finalRGB, finalAlpha);
    }
    else if (EffectType == 2)
    {
        vec4 shadowColor = EffectColor; // RGBA
        float shadowOffset = EffectValue - 1.0;  // in pixels

        float shadowA = 0.0;
        vec3 premultShadow = vec3(0.0);

        vec2 texSize = vec2(getTextureSize(mainTexture));

        vec2 shadowOffsetPx = vec2(shadowOffset, -shadowOffset * 0.75);
        vec2 shadowUV = UV + shadowOffsetPx / texSize;

        float shadowDist = getTextureColor(mainTexture, shadowUV).a;
       
        float shadowD    = (shadowDist - shapeEdge) * sdfSpread;

        float shadowSoftness = max(1.0, shadowOffset * 0.80); // pixels
        float shadowFill = valid * (1.0 - smoothstep(-shadowSoftness, +shadowSoftness, shadowD));
        shadowFill = clamp(shadowFill, 0.0, 1.0);

        shadowA = shadowFill * shadowColor.a;
        premultShadow = shadowColor.rgb * shadowA;

        // Composite: outline behind fill (premultiplied alpha)
        float finalAlpha = premultFillAlpha + shadowA * (1.0 - premultFillAlpha);
        vec3 finalRGB = premultFill + premultShadow * (1.0 - premultFillAlpha);

        return vec4(finalRGB, finalAlpha);
    }
    else if (EffectType == 3)
    {
        vec4 glowColor = EffectColor; // RGBA
        float glowRadius = EffectValue; // pixels

        float distFromEdge = max(0.0, abs(d));
        float glowSigma = glowRadius * 0.5;
        float glowAlpha = exp(-0.5 * (distFromEdge * distFromEdge) / (glowSigma * glowSigma));
        glowAlpha *= valid;
        glowAlpha *= glowColor.a;
        glowAlpha = pow(glowAlpha, 1.2);

        // Premultiply
        vec3 premultGlow = glowColor.rgb * glowAlpha;

        // Composite: glow behind fill (premultiplied)
        float finalAlpha = premultFillAlpha + glowAlpha * (1.0 - premultFillAlpha);
        vec3 finalRGB = premultFill + premultGlow * (1.0 - premultFillAlpha);

        return vec4(finalRGB, finalAlpha);
    }

    return vec4(premultFill, premultFillAlpha);
}
#endif

#if VERT_SHADER
uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 
 
layout(location = 0)in vec3 vertPos; 
layout(location = 1)in vec2 uv; 
layout(location = 2)in vec4 color; 
 
// Goes to the frag shader.  
out vec2 UV; 
out vec4 vertColor; 

void VertexShaderMain() { 
    // Pass to frag.
    UV = uv;
    vertColor = color;
    
    // Multiply by projection.
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertPos, 1.0);
}
#endif