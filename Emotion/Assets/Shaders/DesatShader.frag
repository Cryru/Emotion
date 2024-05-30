#version v
 
uniform sampler2D mainTexture;
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor; 
 
out vec4 fragColor;

uniform float amount = 1.0;
 
#using "Shaders/getTextureColor.c"

void main() { 
    fragColor = getTextureColor(mainTexture, UV) * vertColor;
    float grey = (fragColor.r + fragColor.g + fragColor.b) / 3.0;
    fragColor.rgb = mix(fragColor.rgb, vec3(grey, grey, grey), amount);

    if (fragColor.a < 0.01)discard;
}