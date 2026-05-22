// DEFINE_VERTEX_ATTRIBUTE Position V_Pos
// DEFINE_VERTEX_ATTRIBUTE UV V_UV
// DEFINE_VERTEX_ATTRIBUTE VertexColor V_Color

uniform float RadiusPixels;
uniform vec2 RectSize;

float sdBox(in vec2 p, in vec2 b)
{
    vec2 d = abs(p)-b;
    return length(max(d,vec2(0))) + min(max(d.x,d.y),0.0);
}

float sdCircle(vec2 p, float r)
{
    return length(p) - r;
}

float opUnion(float d1, float d2)
{
    return min(d1,d2);
}

float map(float value, float leftMin, float leftMax, float rightMin, float rightMax)
{
    return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
}

#ifdef VERT_SHADER

vec4 VertexShaderMain()
{
    return projectionMatrix * viewMatrix * modelMatrix * vec4(V_Pos, 1.0);
}

#endif

#ifdef FRAG_SHADER

vec4 FragmentShaderMain()
{
    float aspect = RectSize.x / RectSize.y;
    vec2 ratio = vec2(aspect, 1.0); // The actual resolution here.
    vec2 uv = ((2. * V_UV) - 1.) * ratio;

    float mn = min(ratio.x, ratio.y);
    float radiusInRatio = min((RadiusPixels / RectSize.x) * ratio.x, mn);
    vec2 deflatedRect = ratio - vec2(radiusInRatio); // The rectangle without the radius.
    vec2 left = deflatedRect;

    // Four rounded corners
    float d = sdCircle(uv + left, radiusInRatio);
    d = opUnion(d, sdCircle(uv - left, radiusInRatio));
    d = opUnion(d, sdCircle(uv + vec2(left.x, -left.y), radiusInRatio));
    d = opUnion(d, sdCircle(uv + vec2(-left.x, left.y), radiusInRatio));

    // Fill
    d = opUnion(d, sdBox(uv, vec2(deflatedRect.x + radiusInRatio, left.y)));
    d = opUnion(d, sdBox(uv, vec2(left.x, deflatedRect.y + radiusInRatio)));

    // AA
    d = 1.0 + d;
    vec4 col = V_Color;
    const float aaPixels = 1.0;
    float aaStrength = (aaPixels / RectSize.x) * ratio.x;
    float aaThreshold = ratio.y - aaStrength;
    if (d > aaThreshold)
    {
        float d10 = map(d, 1.0 - aaStrength, 1.0, 0.0, 1.0);
        col.a *= smoothstep(1.0, 0.0, d10);
    }

    // Discard invisible
    if (col.a < ALPHA_DISCARD) discard;
    return col;
}

#endif