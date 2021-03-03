#version v
 
uniform sampler2D mainTexture;

// Comes in from the vertex shader. 
in vec2 UV; 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

void main() { 
    fragColor = getTextureColor(mainTexture, UV);
}