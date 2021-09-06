#version v
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

bool isIn(vec2 startPos, vec2 uv, vec2 pixelSize)
{
    vec2 sampleAt = startPos + uv * pixelSize;
	vec4 texColor = getTextureColor(mainTexture, sampleAt);
    return texColor.a == 1.0;
}

bool isIn(vec2 sampleAt)
{
	vec4 texColor = getTextureColor(mainTexture, sampleAt);
    return texColor.a == 1.0;
}

float squaredDistanceBetween(vec2 uv1, vec2 uv2)
{
    vec2 delta = uv1 - uv2;
    float dist = (delta.x * delta.x) + (delta.y * delta.y);
    return dist;
}

#define RANGE 64.0

void main() {
    const int iRange = int(RANGE);
    float halfRange = RANGE / 2.0;
    int iHalfRange = int(halfRange);

    ivec2 textureSize = getTextureSize(mainTexture);
    float tSx = 1.0/float(textureSize.x);
    float tSy = 1.0/float(textureSize.y);
    vec2 pixelSize = vec2(tSx, tSy);

    vec2 startPosition = UV;
    bool fragIsIn = isIn(UV);
    float squaredDistanceToEdge = (halfRange*halfRange)*2.0;
    
    for(int dx=-iHalfRange; dx < iHalfRange; dx++)
    {
        for(int dy=-iHalfRange; dy < iHalfRange; dy++)
        {
            vec2 scanPositionUV = vec2(dx, dy);
            
            bool scanIsIn = isIn(startPosition, scanPositionUV, pixelSize);
            if (scanIsIn != fragIsIn)
            {
                float scanDistance = squaredDistanceBetween(UV, scanPositionUV);
                if (scanDistance < squaredDistanceToEdge)
                    squaredDistanceToEdge = scanDistance;
            }
        }
    }
    
    float normalised = squaredDistanceToEdge / ((halfRange*halfRange)*2.0);
    float distanceToEdge = sqrt(normalised);
    if (fragIsIn)
        distanceToEdge = -distanceToEdge ;
    normalised = 0.5 - distanceToEdge;

    fragColor = vec4(normalised, normalised, normalised, 1.0);
}