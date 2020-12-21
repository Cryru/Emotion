#version v
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV;
in vec4 vertColor;

out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

void main() { 
    fragColor = vertColor * getTextureColor(mainTexture, UV).a;
    if (fragColor.a < 0.01)discard;
}