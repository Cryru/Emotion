#version v 
 
#ifdef GL_ES 
precision highp float; 
#endif 
 
uniform sampler2D textures[16]; 
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor; 
flat in int Tid;
 
out vec4 fragColor; 
in vec4 gl_FragCoord;
out float gl_FragDepth;

//GetTextureColor
 
void main() { 
    fragColor = getTextureColor(Tid, UV) * vertColor;

    float depth = getTextureColor(1, UV).r;
	if(gl_FragCoord.z > depth) 
	{
		gl_FragDepth = depth;
	}
	else
	{
		gl_FragDepth = gl_FragCoord.z;
	}

    if (fragColor.a < 0.01)discard;
}