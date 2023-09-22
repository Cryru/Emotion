#version v
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

void main() { 
    fragColor = getTextureColor(mainTexture, UV) * vertColor;
    if (fragColor.a < 0.01)discard;
}