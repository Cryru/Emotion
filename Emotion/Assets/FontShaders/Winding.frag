#version v
 
uniform sampler2D mainTexture;
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;

out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

float GET_WINDING_COLOR(sampler2D tex, vec2 uv)
{
	vec4 textureColor = getTextureColor(tex, uv);
	if (mod(textureColor.r * 255., 2.) != 0.)
		return 1.0f;
	return 0.0f;
}

void main()
{
	float s = GET_WINDING_COLOR(mainTexture, UV);
	fragColor = vec4(vertColor.rgb, s);
}