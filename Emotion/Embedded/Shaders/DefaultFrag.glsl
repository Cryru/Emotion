#version 300 es

#ifdef GL_ES
precision highp float;
#endif

uniform vec4 color;
uniform mat4 textureMatrix;
uniform sampler2D drawTexture;

out vec4 fragColor;

// Comes in from the vertex shader.
in vec2 UV;

void main() {
    vec4 uvTransformed = textureMatrix * vec4(UV, 0, 1);

    // Sample for the texture's color at the specified vertex UV and multiply it by the tint.
    fragColor = texture(drawTexture, uvTransformed.xy) * color;
}