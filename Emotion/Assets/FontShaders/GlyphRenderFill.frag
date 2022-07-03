#version v

in vec2 vpar;

out vec4 fragColor;

void main() {
    float val = float( vpar.x * vpar.x < vpar.y );
    if ( val == 0.0 ) discard;
    fragColor = vec4( 1.0 );
}