#version v
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
out vec4 fragColor; 
 
uniform float zNear;
uniform float zFar;

#using "Shaders/getTextureColor.c"

float LinearizeDepth(in vec2 uv)
{
    float depth = getTextureColor(mainTexture, uv).r;
    return (2.0 * zNear) / (zFar + zNear - depth * (zFar - zNear));
}

void main() {
    float c = LinearizeDepth(UV);
    fragColor = vec4(c, c, c, 1.0);
}