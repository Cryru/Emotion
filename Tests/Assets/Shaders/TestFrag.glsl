#version v

#ifdef GL_ES
precision highp float;
#endif

uniform sampler2D textures[16];

// Comes in from the vertex shader.
in vec2 UV;
in vec4 vertColor;
flat in int Tid;

out vec4 fragColor;

//GetTextureColor

void main() {
    vec4 vertCol = vec4(0.4, 0.3, 0.4, 1.0);
	fragColor = getTextureColor(Tid, UV) * vertCol;

    if (fragColor.a < 0.01) discard;
}