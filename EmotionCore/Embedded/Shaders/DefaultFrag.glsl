#version 300 es

#ifdef GL_ES
precision highp float;
#endif

uniform sampler2D textures[16];

// Comes in from the vertex shader.
in vec2 UV;
in vec4 vertColor;
in float Tid;

out vec4 fragColor;

void main() {
    // Check if a texture is in use.
    if (Tid >= 0.0)
    {
        // Sample for the texture's color at the specified vertex UV and multiply it by the tint.
        fragColor = texture(textures[int(Tid)], UV) * vertColor;
    } else {
        // If no texture then just use the color.
        fragColor = vertColor;
    }
}