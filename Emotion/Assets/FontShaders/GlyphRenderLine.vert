#version v 
 
uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 

layout(location = 0)in vec2 pos;
layout(location = 1)in vec2 par;
layout(location = 2)in vec2 limits;
layout(location = 3)in float scale;
layout(location = 4)in float line_width;

out vec2 vpar;
out vec2 vlimits;
out float dist_scale;

void main() {
    vpar = par;
    vlimits = limits;
    dist_scale = scale / line_width;
    
    vec2 tpos = ( projectionMatrix * viewMatrix * modelMatrix * vec4( pos, 1.0, 1.0 ) ).xy;
    gl_Position = vec4( tpos, 0.0, 1.0 );
}