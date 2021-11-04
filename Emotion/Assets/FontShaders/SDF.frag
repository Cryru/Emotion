#version v
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

#define SDFMethod 0

#if SDFMethod == 0

uniform float thickness = 0.5;

void main()
{
    float dist = (thickness - getTextureColor(mainTexture, UV).r);

    // Sdf distance per pixel (gradient vector)
    vec2 ddist = vec2(dFdx(dist), dFdy(dist));

    // Distance to edge in pixels (scalar)
    float pixelDist = dist / length(ddist);

    float opacity = clamp(0.5 - pixelDist, 0.0, 1.0);
    fragColor = vec4(vertColor.rgb, vertColor.a * opacity);
}

#elif SDFMethod == 2

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