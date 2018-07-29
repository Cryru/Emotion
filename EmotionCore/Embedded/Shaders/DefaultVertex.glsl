#version 300 es

uniform mat4 projectionMatrix;

layout (location = 0) in vec3 vertPos;
layout (location = 1) in vec2 uv;
layout (location = 3) in vec4 color;

// Goes to the frag shader.
out vec2 UV;
out vec4 vertColor;

void main() {
    // Pass to frag.
    UV = uv;
    vertColor = color;

    // Multiply by projection.
    gl_Position = projectionMatrix * vec4(vertPos, 1);
}