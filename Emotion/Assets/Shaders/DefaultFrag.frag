#version v
 
uniform sampler2D textures[TEXTURE_COUNT];
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor; 
flat in int Tid;
 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

void main() { 
    fragColor = getTextureColor(Tid, UV) * vertColor;
    
    if (fragColor.a < 0.01)discard;
}