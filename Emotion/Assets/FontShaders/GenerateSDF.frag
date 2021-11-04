#version v
 
uniform sampler2D mainTexture;

in vec2 UV; 
in vec4 vertColor;
out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

float squaredDistanceBetween(vec2 uv1, vec2 uv2)
{
    vec2 delta = uv1 - uv2;
    float dist = (delta.x * delta.x) + (delta.y * delta.y);
    return dist;
}

#define SPREAD 64.0

void main()
{
    vec2 pixel = UV;
    vec2 pixelSize = (1.0/vec2(getTextureSize(mainTexture)));
    float spreadInUnits = SPREAD * pixelSize.x;
    bool inside = getTextureColor(mainTexture, pixel).a != 0.0;

    // Find the shortest squared distance to a pixel what is in the opposite state of this one.
    float test = 0.0;
    float totes = 0.0;
    float minDistance = spreadInUnits * spreadInUnits;

    for (float y = -spreadInUnits; y < spreadInUnits; y+=pixelSize.y)
    {
        for (float x = -spreadInUnits; x < spreadInUnits; x+=pixelSize.x)
        {
            vec2 coord = vec2(pixel.x + x, pixel.y + y);
            bool thisInside = getTextureColor(mainTexture, coord).a != 0.0;
            if (inside != thisInside)
            {
                minDistance = min(minDistance, squaredDistanceBetween(coord, pixel));
            }
        }
    }

    minDistance = sqrt(minDistance);
    float sdfValue = minDistance / spreadInUnits; // 0-1
    if (!inside)
    {
        sdfValue = -sdfValue; // Inside values are negative.
    }

    // -1;1 to 0;1
    sdfValue = (sdfValue + 1.0) * 0.5f;
    fragColor = vec4(sdfValue, sdfValue, sdfValue, 1.0);
}