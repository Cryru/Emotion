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
	if (mod(textureColor.r * 255., 2.) != 0.)
	{
		str.r = 1.0f;
	}

	if (mod(textureColor.g * 255., 2.) != 0.)
	{
		str.g = 1.0f;
	}

	if (mod(textureColor.b * 255., 2.) != 0.)
	{
		str.b = 1.0f;
	}

	if (mod(textureColor.a * 255., 2.) != 0.)
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
	//	0.f, 0.f, 0.f,
	//	0.f, 1.f, 0.f,
	//	0.f, 0.f, 0.f
	//);

	//float convFilter[9] = float[9](
	//	0.025f, 0.10.f, 0.025f,
	//	0.10, 0.50, 0.10,
	//	0.025f, 0.10.f, 0.025f
	//);

	float convFilter[9] = float[9](
		1.f, 1.f, 1.f,
		1.f, 1.f, 1.f,
		1.f, 1.f, 1.f
	);

	vec4 colorLA = GET_WINDING_COLOR(mainTexture, lefta_coord) * convFilter[0];
	vec4 colorA = GET_WINDING_COLOR(mainTexture, above_coord) * convFilter[1];
	vec4 colorRA = GET_WINDING_COLOR(mainTexture, righta_coord) * convFilter[2];
	
	vec4 colorL = GET_WINDING_COLOR(mainTexture, left_coord) * convFilter[3];
	vec4 color = GET_WINDING_COLOR(mainTexture, UV) * convFilter[4];
	vec4 colorR = GET_WINDING_COLOR(mainTexture, right_coord) * convFilter[5];
	
	vec4 colorLB = GET_WINDING_COLOR(mainTexture, leftb_coord) * convFilter[6];
	vec4 colorB = GET_WINDING_COLOR(mainTexture, below_coord) * convFilter[7];
	vec4 colorRB = GET_WINDING_COLOR(mainTexture, rightb_coord) * convFilter[8];
	
	//vec4 combinedSample = colorLA + colorA + colorRA + colorL + color + colorR + colorLB + colorB + colorRB;

	float subpixelFilter[9] = float[9](
		0.f, 0.2f, 0.f,
		0.2f, 0.3f, 0.2f,
		0.f, 0.2f, 0.f
	);

	float subpixelFilterUp[9] = float[9](
		0.f, 0.2f, 0.f,
		0.333f, 0.333f, 0.333f,
		0.f, 0.2f, 0.f
	);

	float r = colorL.a * subpixelFilter[3] + color.r * subpixelFilter[4] + color.g * subpixelFilter[5];

	float pAboveR = colorLA.a * subpixelFilterUp[3] + colorA.r * subpixelFilterUp[4] + colorA.g * subpixelFilterUp[5];
	float pBelowR = colorLB.a * subpixelFilterUp[3] + colorB.r * subpixelFilterUp[4] + colorB.g * subpixelFilterUp[5];
	r = r + pAboveR * subpixelFilter[1] + pBelowR * subpixelFilter[7];

	float g = color.r * subpixelFilter[3] + color.g * subpixelFilter[4] + color.b * subpixelFilter[5];

	float pAboveG = colorA.r * subpixelFilterUp[3] + colorA.g * subpixelFilterUp[4] + colorA.b * subpixelFilterUp[5];
	float pBelowG = colorB.r * subpixelFilterUp[3] + colorB.g * subpixelFilterUp[4] + colorB.b * subpixelFilterUp[5];
	g = g + pAboveG * subpixelFilter[1] + pBelowG * subpixelFilter[7];

	float b = color.g * subpixelFilter[3] + color.b * subpixelFilter[4] + color.a * subpixelFilter[5];

	float pAboveB = colorA.g * subpixelFilterUp[3] + colorA.b * subpixelFilterUp[4] + colorA.a * subpixelFilterUp[5];
	float pBelowB = colorB.g * subpixelFilterUp[3] + colorB.b * subpixelFilterUp[4] + colorB.a * subpixelFilterUp[5];
	b = b + pAboveB * subpixelFilter[1] + pBelowB * subpixelFilter[7];

	float a = color.b * subpixelFilter[3] + color.a * subpixelFilter[4] + colorR.r * subpixelFilter[5];

	float pAboveA = colorA.b * subpixelFilterUp[3] + colorA.a * subpixelFilterUp[4] + colorRB.r * subpixelFilterUp[5];
	float pBelowA = colorB.b * subpixelFilterUp[3] + colorB.a * subpixelFilterUp[4] + colorRB.r * subpixelFilterUp[5];
	a = a + pAboveA * subpixelFilter[1] + pBelowA * subpixelFilter[7];

	fragColor = vec4(vertColor.rgb, (r + g + b + a) / 4.);
}