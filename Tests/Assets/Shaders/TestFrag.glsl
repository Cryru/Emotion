#version v

uniform sampler2D mainTexture;

// Comes in from the vertex shader.
in vec2 UV;
in vec4 vertColor;

out vec4 fragColor;

#using "Shaders/getTextureColor.c"

void main() {
    vec4 vertCol = vec4(0.4, 0.3, 0.4, 1.0);
	fragColor = getTextureColor(mainTexture, UV) * vertCol;

    if (fragColor.a < 0.01) discard;
}