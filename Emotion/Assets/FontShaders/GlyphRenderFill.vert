#version v 
 
uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 

layout(location = 0)in vec2 pos;
layout(location = 1)in vec2 par;

out vec2 vpar;

void main() {
    vpar = par;

    vec2 tpos = ( projectionMatrix * viewMatrix * modelMatrix * vec4( pos, 1.0, 1.0 ) ).xy;
    gl_Position = vec4( tpos, 0.0, 1.0 );
}