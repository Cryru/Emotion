#version v 

uniform sampler2D mainTexture;
uniform float outlineThickness = 1.;

in vec2 UV;
in vec2 originalUV;
in vec4 vertColor;
out vec4 fragColor;

#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"
#define SAMPLE_TEXTURE(loc) getTextureColor(mainTexture, UV + loc).a

void main() {
	ivec2 tSize = getTextureSize(mainTexture);
	vec2 pixelSize = vec2(1.0) / vec2((float(tSize.x)), (float(tSize.y)));

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
	sum = sum / (8.0 * outlineThickness + 1.);

	float t = 1.0 - texCol.a;
	if(sum > 0.1)
		fragColor = mix(texCol.rgba, vertColor.rgba, t);
	else
		fragColor = texCol;

	if (fragColor.a < 0.01)discard;
}