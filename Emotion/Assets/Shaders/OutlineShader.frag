#version v 

uniform sampler2D mainTexture;
uniform float outlineThickness = 1.;

in vec2 UV;
in vec4 vertColor;
out vec4 fragColor;

#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"
#define SAMPLE_TEXTURE(loc) getTextureColor(mainTexture, UV + loc).a

float map(float value, float leftMin, float leftMax, float rightMin, float rightMax)
{
    return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
}

void main() {
	ivec2 tSize = getTextureSize(mainTexture);
	vec2 pixelSize = vec2(1.0) / vec2(tSize);

	vec4 texCol = getTextureColor(mainTexture, UV);

	float sum = texCol.a;
	for (float n = 1.0; n <= outlineThickness; n++)
	{
		sum += SAMPLE_TEXTURE(vec2(pixelSize.x * -n, 0.0));
		sum += SAMPLE_TEXTURE(vec2(pixelSize.x * n, 0.0));
		sum += SAMPLE_TEXTURE(vec2(0.0, pixelSize.y * -n));
		sum += SAMPLE_TEXTURE(vec2(0.0, pixelSize.y * n));
		sum += SAMPLE_TEXTURE(vec2(pixelSize.x * -n, pixelSize.y * -n));
		sum += SAMPLE_TEXTURE(vec2(pixelSize.x * -n, pixelSize.y * n));
		sum += SAMPLE_TEXTURE(vec2(pixelSize.x * n, pixelSize.y * -n));
		sum += SAMPLE_TEXTURE(vec2(pixelSize.x * n, pixelSize.y * n));
	}
	sum = sum / ((8.0 * outlineThickness) + 1.0);

	const float threshold = 0.2;
	const float smoothness = 0.05;
	if(sum > threshold + smoothness)
	{
		float t = 1.0 - map(texCol.a, threshold + smoothness, 1.0, 0.0, 1.0);
		t = t * t;
		fragColor = vec4(mix(texCol.rgb, vertColor.rgb, t), vertColor.a);
	}
	else if(sum >= smoothness)
	{
		float m = 1.0 - map(sum, smoothness, threshold + smoothness, 0.0, 1.0);
		fragColor = mix(vertColor.rgba, vec4(0.0), m);
	}
	else
	{
		fragColor = texCol;
	}

	if (fragColor.a < 0.01) discard;
}