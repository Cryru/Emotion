#version v

uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)

// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;

out vec4 fragColor; 

#using "Shaders/getTextureColor.c"

// The sigma value for the gaussian function: higher value means more blur
uniform float sigma = 5.0;
const float pi = 3.14159265;

uniform vec2 blurDirection;
uniform float textureSizeInDirection;

out float gl_FragDepth;

void main() {
    float blurSize = 1.0f / textureSizeInDirection;
    const float numBlurPixelsPerSide = 16.0;

    // Incremental Gaussian Coefficent Calculation (See GPU Gems 3 pp. 877 - 889)
    vec3 incrementalGaussian = vec3(0.0);
    incrementalGaussian.x = 1.0f / (sqrt(2.0f * pi) * sigma);
    incrementalGaussian.y = exp(-0.5f / (sigma * sigma));
    incrementalGaussian.z = incrementalGaussian.y * incrementalGaussian.y;

    // Take the central sample first...
    float val = getTextureColor(mainTexture, UV).r;
    float avgValue = val * incrementalGaussian.x;
    float coefficientSum = incrementalGaussian.x;
    incrementalGaussian.xy *= incrementalGaussian.yz;

    // Go through the remaining samples (based on the kernel size)
    vec2 blurSizeDirection = blurSize * blurDirection;
    for (float i = 1.0f; i <= numBlurPixelsPerSide; i++)
    {
        vec2 sampleCoordLeftUp = UV - i * blurSizeDirection;
        vec2 sampleCoordRightDown = UV + i * blurSizeDirection;
        // Clamp samples to within texture.
        if (sampleCoordLeftUp.y < 0.0 || sampleCoordLeftUp.x < 0.0 || sampleCoordRightDown.y > 1.0 || sampleCoordRightDown.x > 1.0) continue;

        avgValue += getTextureColor(mainTexture, sampleCoordLeftUp).r * incrementalGaussian.x;
        avgValue += getTextureColor(mainTexture, sampleCoordRightDown).r * incrementalGaussian.x;
        coefficientSum += 2.0 * incrementalGaussian.x;
        incrementalGaussian.xy *= incrementalGaussian.yz;
    }

    gl_FragDepth = avgValue / coefficientSum;
}