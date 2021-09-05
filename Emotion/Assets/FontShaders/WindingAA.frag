#version v
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

in vec2 left_coord;
in vec2 right_coord;
in vec2 above_coord;
in vec2 below_coord;

in vec2 lefta_coord;
in vec2 righta_coord;
in vec2 leftb_coord;
in vec2 rightb_coord;

vec4 GET_WINDING_COLOR(sampler2D tex, vec2 uv)
{
	vec4 str = vec4(0.0);
	vec4 textureColor = getTextureColor(tex, uv);
	if (mod(textureColor.r * 255, 2) != 0)
	{
		str.r = 1.0f;
	}

	if (mod(textureColor.g * 255, 2) != 0)
	{
		str.g = 1.0f;
	}

	if (mod(textureColor.b * 255, 2) != 0)
	{
		str.b = 1.0f;
	}

	if (mod(textureColor.a * 255, 2) != 0)
	{
		str.a = 1.0f;
	}

	return str;
}

void main()
{
	//float convFilter[9] = float[9](
	//	0.11111, 0.11111, 0.11111,
	//	0.11111, 0.11111, 0.11111,
	//	0.11111, 0.11111, 0.11111
	//);

	//float convFilter[9] = float[9](
	//	0f, 0f, 0f,
	//	0f, 1f, 0f,
	//	0f, 0f, 0f
	//);

	//float convFilter[9] = float[9](
	//	0.025f, 0.10f, 0.025f,
	//	0.10, 0.50, 0.10,
	//	0.025f, 0.10f, 0.025f
	//);

	float convFilter[9] = float[9](
		1f, 1f, 1f,
		1f, 1f, 1f,
		1f, 1f, 1f
	);

	//vec4 colorLA = GET_WINDING_COLOR(mainTexture, lefta_coord) * convFilter[0];
	//vec4 colorA = GET_WINDING_COLOR(mainTexture, above_coord) * convFilter[1];
	//vec4 colorRA = GET_WINDING_COLOR(mainTexture, righta_coord) * convFilter[2];
	
	vec4 colorL = GET_WINDING_COLOR(mainTexture, left_coord) * convFilter[3];
	vec4 color = GET_WINDING_COLOR(mainTexture, UV) * convFilter[4];
	vec4 colorR = GET_WINDING_COLOR(mainTexture, right_coord) * convFilter[5];
	
	//vec4 colorLB = GET_WINDING_COLOR(mainTexture, leftb_coord) * convFilter[6];
	//vec4 colorB = GET_WINDING_COLOR(mainTexture, below_coord) * convFilter[7];
	//vec4 colorRB = GET_WINDING_COLOR(mainTexture, rightb_coord) * convFilter[8];
	
	//vec4 combinedSample = colorLA + colorA + colorRA + colorL + color + colorR + colorLB + colorB + colorRB;

	float subpixelFilter[9] = float[9](
		0f, 0f, 0f,
		0f, 1f, 0f,
		0f, 0f, 0f
	);

	float r = colorL.a * subpixelFilter[3] + color.r * subpixelFilter[4] + color.g * subpixelFilter[5];
	float g = color.r * subpixelFilter[3] + color.g * subpixelFilter[4] + color.b * subpixelFilter[5];
	float b = color.g * subpixelFilter[3] + color.b * subpixelFilter[4] + color.a * subpixelFilter[5];
	float a = color.b * subpixelFilter[3] + color.a * subpixelFilter[4] + colorR.r * subpixelFilter[5];
	fragColor = vec4(vertColor.rgb, (r + g + b + a) / 4.);
}