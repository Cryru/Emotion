#version 300 es

uniform mat4 projectionMatrix;

layout (location = 0) in vec2 vertPos;
layout (location = 1) in vec2 vertTex;

// Goes to the frag shader.
out vec2 UV;

void main() {
    gl_Position = projectionMatrix * vec4(vertPos, 0, 1);
    UV = vertTex;
}