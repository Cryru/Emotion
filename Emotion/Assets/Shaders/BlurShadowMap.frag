#version v

uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)

// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;

out vec4 fragColor; 

#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

uniform vec2 direction; // Direction of the blur

void main() {
    // Gaussian blur - two passes, horizontal and vertical
    vec2 textureSize = vec2(getTextureSize(mainTexture));

    vec2 tex_offset = 1.0 / textureSize; // Texture coordinate offset
    float result = 0.0;
    float kernel[5];
    vec2 offset[5];
    
    // Initialize the kernel and offset arrays
    kernel[0] = 0.2270270270;
    kernel[1] = 0.1945945946;
    kernel[2] = 0.1216216216;
    kernel[3] = kernel[1];
    kernel[4] = kernel[0];

    offset[0] = tex_offset * direction * -2.0;
    offset[1] = tex_offset * direction * -1.0;
    offset[2] = vec2(0.0);
    offset[3] = tex_offset * direction;
    offset[4] = tex_offset * direction * 2.0;
    
    // Apply the kernel
    for (int i = 0; i < 5; ++i) {
        result += getTextureColor(mainTexture, UV + offset[i]).r;
    }
    
    gl_FragDepth = getTextureColor(mainTexture, UV).r;
}