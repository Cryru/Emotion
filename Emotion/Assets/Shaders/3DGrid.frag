#version v
 
in vec2 UV; 

out vec4 fragColor;

uniform vec2 cameraPos;
uniform vec2 totalSize;

float sdBox( in vec2 p, in vec2 b )
{
    vec2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}

void main() {
    vec2 squareSize = vec2(100, 100);
    vec2 squares = totalSize / squareSize;
    vec2 uv = UV + cameraPos;//vec2(2.0);
    vec2 p = fract(uv * squares) - 0.5;

    float dist = abs(sdBox(p, vec2(0.5))) - 0.025;

    float fwidthValue = fwidth(dist);
    float alpha = (1.0 - smoothstep(0.0, fwidthValue, dist));

    // Fade away from center
    float distanceFromCenter = length(UV);
    float alphaDist = mix(alpha, 0.0, clamp(distanceFromCenter / 0.6, 0.0, 1.0));

    fragColor = vec4(1.0, 1.0, 1.0, alphaDist);
    if(fragColor.a < 0.1) discard;
}