#version 300 es

uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;

layout (location = 0) in vec3 vertPos;
layout (location = 1) in vec4 color;

// Goes to the frag shader.
out vec2 UV;
out vec4 vertColor;

void main() {
    gl_Position = projectionMatrix * vec4(vertPos, 1);
    //UV = vertTex;
    vertColor = color;
}