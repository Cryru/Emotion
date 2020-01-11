#version v 
 
#ifdef GL_ES 
precision highp float; 
#endif 
 
uniform sampler2D textures[16]; 
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor; 
flat in int Tid;
 
out vec4 fragColor; 

//GetTextureColor
//GetTextureSize

// https://github.com/Jam3/glsl-fast-gaussian-blur
vec4 blur9(int image, vec2 uv, vec2 resolution, vec2 direction) {
  vec4 color = vec4(0.0);
  vec2 off1 = vec2(1.3846153846) * direction;
  vec2 off2 = vec2(3.2307692308) * direction;
  color += getTextureColor(image, uv) * 0.2270270270;
  color += getTextureColor(image, uv + (off1 / resolution)) * 0.3162162162;
  color += getTextureColor(image, uv - (off1 / resolution)) * 0.3162162162;
  color += getTextureColor(image, uv + (off2 / resolution)) * 0.0702702703;
  color += getTextureColor(image, uv - (off2 / resolution)) * 0.0702702703;
  return color;
}
 
void main() {
    // Gaussian blur - two passes, horizontal and vertical
    fragColor = blur9(Tid, UV, getTextureSize(Tid), vec2(1, 0));
    fragColor += blur9(Tid, UV, getTextureSize(Tid), vec2(0, 1));
    fragColor /= 2.0;

    fragColor *= vertColor;
    if (fragColor.a < 0.01)discard;
}