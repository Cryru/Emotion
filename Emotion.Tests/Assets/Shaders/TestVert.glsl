#version 300 es

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform float time;

layout (location = 0) in vec3 vertPos;
layout (location = 1) in vec2 uv;
layout (location = 2) in float tid;
layout (location = 3) in vec4 color;

// Goes to the frag shader.
out vec2 UV;
out vec4 vertColor;
out float Tid;

void main() {
    // Pass to frag.
    UV = uv;
    vertColor = color;
    Tid = tid;

    vec3 vertTest = vertPos;
    vertTest.x += 30.0;

    // Multiply by projection.
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertTest, 1.0);
}