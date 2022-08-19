#version v
 
uniform sampler2D mainTexture;
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

#define OUTLINE 1
#if OUTLINE
uniform float outlineWidthDist = 0.0;
uniform vec4 outlineColor = vec4(0.0);
#endif

// AA is based on the font's render size to allow both big and small
// renderings to look crisp. This formula is taken from the MSDF documentation.
uniform vec2 scaleFactor = vec2(2);
uniform float thickness = 0.5;
float screenPxRange() {
    vec2 unitRange = scaleFactor/vec2(getTextureSize(mainTexture));
    vec2 screenTexSize = vec2(1.0)/fwidth(UV);
    return max(0.5*dot(unitRange, screenTexSize), 1.0);
}

void main()
{
    float distSample = getTextureColor(mainTexture, UV).r;
    float dist = thickness - distSample;
    
    vec4 fill = vertColor.rgba;
    vec4 color = fill;

#if OUTLINE

    // Apply outline color in the outline region (if any).
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

    // Anti aliasing
    dist = thickness - dist;
    float screenPxDistance = screenPxRange() * (thickness + dist - 1.);
    float opacity = clamp(screenPxDistance - thickness + 1., 0.0, 1.0);

    fragColor = vec4(color.rgb, color.a * opacity);
}