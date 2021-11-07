#version v
 
uniform sampler2D mainTexture;
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

#define SDFMethod 0 // currently in for comparison reasons.

#if SDFMethod == 0

#define OUTLINE 1

uniform float thickness = 0.5;

#if OUTLINE
uniform float outlineWidthDist = 0.0;
uniform vec4 outlineColor = vec4(0.0);
#endif

void main()
{
    float distSample = getTextureColor(mainTexture, UV).r;
    float dist = thickness - distSample;
    
    vec4 fill = vertColor.rgba;
    vec4 color = fill;

#ifdef OUTLINE

    // Apply outline color in the outline region (if any).
    float test = length(fwidth(UV) * vec2(getTextureSize(mainTexture)));
    float distOutline = (thickness - outlineWidthDist) - distSample;
    if (outlineWidthDist + dist > abs(distOutline) - outlineWidthDist)
    {
        // Interpolate between colors.
        float t = outlineWidthDist + (dist / outlineWidthDist);
        color = mix(fill, vec4(outlineColor.rgb, fill.a), t);

        // Override distance with outline distance, to grow the distance out.
        dist = distOutline;
    }

#endif

    // Sdf distance per pixel (gradient vector)
    vec2 ddist = vec2(dFdx(dist), dFdy(dist));

    // Distance to edge in pixels (scalar)
    float pixelDist = dist / length(ddist);
    float opacity = clamp(0.5 - pixelDist, 0.0, 1.0);
    fragColor = vec4(color.rgb, color.a * opacity);
}

#elif SDFMethod == 1

// FWidth AA

void main() {
    vec2 pos = UV.xy;
    vec3 dist = getTextureColor(mainTexture, UV).rgb;
    ivec2 sz = getTextureSize(mainTexture).xy;
    float sigDist = dist.r; // Other components are the same.
    float w = fwidth(sigDist);
    float opacity = smoothstep(0.5 - w, 0.5 + w, sigDist);
    fragColor = vec4(vertColor.rgb, vertColor.a * opacity);
}

#else

// MSDF Documentation

uniform float scaleFactor;

float screenPxRange() {
    vec2 unitRange = vec2(scaleFactor)/vec2(getTextureSize(mainTexture));
    vec2 screenTexSize = vec2(1.0)/fwidth(UV);
    return max(0.5*dot(unitRange, screenTexSize), 1.0);
}

void main() {
    float sd = getTextureColor(mainTexture, UV).r;
    float screenPxDistance = screenPxRange()*(sd - 0.5);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    fragColor = vec4(vertColor.rgb, vertColor.a * opacity);
}

#endif