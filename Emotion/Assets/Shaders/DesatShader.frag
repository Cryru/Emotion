#version v
 
#define ALLOW_TEXTURE_BATCHING

uniform LOWP sampler2D mainTexture;
 
// Comes in from the vertex shader. 
in LOWP vec2 UV; 
in LOWP vec4 vertColor; 
 
out LOWP vec4 fragColor;

uniform LOWP float amount = 1.0;
 
#using "Shaders/getTextureColor.c"

void main() { 
    fragColor = getTextureColor(mainTexture, UV) * vertColor;
    LOWP float grey = (fragColor.r + fragColor.g + fragColor.b) / 3.0;
    fragColor.rgb = mix(fragColor.rgb, vec3(grey, grey, grey), amount);

    if (fragColor.a < 0.01)discard;
}