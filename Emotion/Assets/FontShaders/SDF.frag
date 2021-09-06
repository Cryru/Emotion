#version v
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

#define MSDF 0

#if MSDF
    // Based on generated atlas.
    #define PXRANGE 2
    float median(float r, float g, float b) {
        return max(min(r, g), min(max(r, g), b));
    }
#else
    // Based on range in GenerateSDF.frag
    uniform float scaleFactor;
    #define PXRANGE scaleFactor
    float median(float r, float g, float b) {
        return r;
    }
#endif

float screenPxRange() {
    vec2 unitRange = vec2(PXRANGE)/vec2(getTextureSize(mainTexture));
    vec2 screenTexSize = vec2(1.0)/fwidth(UV);
    return max(0.5*dot(unitRange, screenTexSize), 1.0);
}

void main() {
    vec3 msd = getTextureColor(mainTexture, UV).rgb;
    float sd = median(msd.r, msd.g, msd.b);
    float screenPxDistance = screenPxRange()*(sd - 0.5);
    float opacity = clamp(screenPxDistance + 0.5, 0.0, 1.0);
    fragColor = vec4(vertColor.rgb, vertColor.a * opacity);
}