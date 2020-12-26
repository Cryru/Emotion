#version v 

uniform sampler2D mainTexture;
uniform sampler2D depthTexture;
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;

out vec4 fragColor; 

#using "Shaders/getTextureColor.c"
 
void main() { 
    fragColor = getTextureColor(mainTexture, UV) * vertColor;

    float depth = getTextureColor(depthTexture, UV).r;
	if (gl_FragCoord.z > depth)
		gl_FragDepth = depth;
	else
		gl_FragDepth = gl_FragCoord.z;

    if (fragColor.a < 0.01)discard;
}