#version v
 
in vec2 UV;
in vec4 vertColor;

out vec4 fragColor;

uniform vec2 squareSize;
uniform vec2 cameraPos;
uniform vec2 totalSize;

float sdBox( in vec2 p, in vec2 b )
{
    vec2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}

float filteredGrid( in vec2 p, in vec2 dpdx, in vec2 dpdy )
{
    const float N = 30.0;
    vec2 w = max(abs(dpdx), abs(dpdy));
    vec2 a = p + 0.5*w;                        
    vec2 b = p - 0.5*w;           
    vec2 i = (floor(a)+min(fract(a)*N,1.0)-
              floor(b)-min(fract(b)*N,1.0))/(N*w);
    return (1.0-i.x)*(1.0-i.y);
}

void main() {
    vec2 squares = totalSize / squareSize;
    vec2 uv = UV + cameraPos;
    vec2 p = fract(uv * squares) - 0.5;

    vec2 ddx = dFdx( p ); 
    vec2 ddy = dFdy( p );
    float col = filteredGrid(p, ddx, ddy);
    float alphaDist = 1.0 - col;
    fragColor = vec4(vertColor.rgb, vertColor.a * alphaDist);
    if(fragColor.a < 0.1) discard;
}