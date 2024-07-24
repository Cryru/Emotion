#version v

#define ALLOW_TEXTURE_BATCHING

uniform LOWP sampler2D mainTexture;
 
// Comes in from the vertex shader. 
in LOWP vec2 UV;
in LOWP vec4 vertColor;

out LOWP vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

void main() { 
    fragColor = vertColor * getTextureColor(mainTexture, UV).a;
    if (fragColor.a < 0.01)discard;
}