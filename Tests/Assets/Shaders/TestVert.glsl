#version v

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform float time;

layout (location = 0) in vec3 vertPos;
layout (location = 1) in vec2 uv;
layout (location = 2) in vec4 color;

// Goes to the frag shader.
out vec2 UV;
out vec4 vertColor;

void main() {
    // Pass to frag.
    UV = uv;
    vertColor = color;

    // Prevent culling of time uniform.
    float t = time;

    vec3 vertTest = vertPos;
    vertTest.x += 30.0;

    // Multiply by projection.
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertTest, 1.0);
}