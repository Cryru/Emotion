#version 110
// ADFECTUS_VERSION_FORCE

uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 
 
uniform float time; // shader playback time (in seconds) 
 
attribute vec3 vertPos; 
attribute vec2 uv; 
attribute float tid; 
attribute vec4 color; 
 
// Goes to the frag shader.  
varying vec2 UV; 
varying vec4 vertColor; 
varying float Tid; 
 
void main() { 
    // Pass to frag.
    UV = uv;
    vertColor = color;
    Tid = tid;
    
    // Multiply by projection.
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertPos, 1.0);
}