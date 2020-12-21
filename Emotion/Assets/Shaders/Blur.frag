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

vec4 gaussianBlur(sampler2D image, vec2 uv, vec2 imageSize, vec2 direction, float directionDimensionSize)
{
    float blurSize = 1.0f / directionDimensionSize;
    const float numBlurPixelsPerSide = 4.0;

    // Incremental Gaussian Coefficent Calculation (See GPU Gems 3 pp. 877 - 889)
    vec3 incrementalGaussian = vec3(0.0);
    incrementalGaussian.x = 1.0f / (sqrt(2.0f * pi) * sigma);
    incrementalGaussian.y = exp(-0.5f / (sigma * sigma));
    incrementalGaussian.z = incrementalGaussian.y * incrementalGaussian.y;

    // Take the central sample first...
    vec4 val = getTextureColor(image, uv);
    vec4 avgValue = val * incrementalGaussian.x;
    float coefficientSum = incrementalGaussian.x;
    incrementalGaussian.xy *= incrementalGaussian.yz;

    // Go through the remaining samples (based on the kernel size)
    vec2 blurSizeDirection = blurSize * direction;
    for (float i = 1.0f; i <= numBlurPixelsPerSide; i++)
    {
        vec2 sampleCoordLeftUp = uv - i * blurSizeDirection;
        vec2 sampleCoordRightDown = uv + i * blurSizeDirection;
        // Clamp samples to within texture.
        if(sampleCoordLeftUp.y < 0.0 || sampleCoordLeftUp.x < 0.0 || sampleCoordRightDown.y > 1.0 || sampleCoordRightDown.x > 1.0) continue;

        avgValue += getTextureColor(image, sampleCoordLeftUp) * incrementalGaussian.x;
        avgValue += getTextureColor(image, sampleCoordRightDown) * incrementalGaussian.x;
        coefficientSum += 2.0 * incrementalGaussian.x;
        incrementalGaussian.xy *= incrementalGaussian.yz;
    }

    return avgValue / coefficientSum;
}

void main() {
    // Gaussian blur - two passes, horizontal and vertical
    vec2 textureSize = vec2(getTextureSize(mainTexture));
    fragColor = gaussianBlur(mainTexture, UV, textureSize, vec2(1.0, 0), textureSize.x);
    fragColor += gaussianBlur(mainTexture, UV, textureSize, vec2(0, 1.0), textureSize.y);
    fragColor /= 2.0;
    fragColor *= vertColor;
    if (fragColor.a < 0.01) discard;
}