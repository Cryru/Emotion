#version v

#ifdef GL_ES
   #define LOW lowp
   #define MID mediump
   #define HIGH highp
#else
   #define LOW
   #define MID
   #define HIGH
#endif

uniform LOW sampler2D mainTexture;
 
// Comes in from the vertex shader. 
in LOW vec2 UV; 
in LOW vec4 vertColor;
 
out LOW vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

void main() { 
    fragColor = getTextureColor(mainTexture, UV) * vertColor;
    if (fragColor.a < 0.01)discard;
}