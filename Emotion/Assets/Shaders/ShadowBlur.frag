#version v

uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)

// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;

out vec4 fragColor; 

#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

// The sigma value for the gaussian function: higher value means more blur
uniform float sigma = 5.0;
const float pi = 3.14159265;

uniform vec2 blurDirection;

#define VSM 1

#if VSM
out float gl_FragDepth;

vec4 blur9(sampler2D image, vec2 uv, vec2 resolution, vec2 direction)
{
  vec4 color = vec4(0.0);
  vec2 off1 = vec2(1.3846153846) * direction;
  vec2 off2 = vec2(3.2307692308) * direction;
  color += texture2D(image, uv + vec2(0.0)          ) * 0.2270270270;
  color += texture2D(image, uv + (off1 / resolution)) * 0.3162162162;
  color += texture2D(image, uv - (off1 / resolution)) * 0.3162162162;
  color += texture2D(image, uv + (off2 / resolution)) * 0.0702702703;
  color += texture2D(image, uv - (off2 / resolution)) * 0.0702702703;
  return color;
}

void main() {
    vec2 textureSize = vec2(getTextureSize(mainTexture));
    fragColor = blur9(mainTexture, UV, textureSize, blurDirection);
}
#else
out float gl_FragDepth;

float blur9(sampler2D image, vec2 uv, vec2 resolution, vec2 direction)
{
  float color = 0.0;
  vec2 off1 = vec2(1.3846153846) * direction;
  vec2 off2 = vec2(3.2307692308) * direction;
  color += texture2D(image, uv + vec2(0.0)          ).r * 0.2270270270;
  color += texture2D(image, uv + (off1 / resolution)).r * 0.3162162162;
  color += texture2D(image, uv - (off1 / resolution)).r * 0.3162162162;
  color += texture2D(image, uv + (off2 / resolution)).r * 0.0702702703;
  color += texture2D(image, uv - (off2 / resolution)).r * 0.0702702703;
  return color;
}

void main() {
    vec2 textureSize = vec2(getTextureSize(mainTexture));
    gl_FragDepth = blur9(mainTexture, UV, textureSize, blurDirection);
}
#endif