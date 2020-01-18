#version v 
 
#ifdef GL_ES 
precision highp float; 
#endif 
 
uniform sampler2D textures[16]; 
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor; 
in float Tid; 
 
out vec4 fragColor; 
 
vec4 getTextureColor(vec2 uvInput) { 
    vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);
    
    // Check if a texture is in use.
    if (Tid >= 0.0) {
        // Sample for the texture's color at the specified vertex UV and multiply it by the tint.
        sampledColor = texture(textures[int(Tid)], uvInput);
    }

    // How to find the location of the pixel on the screen.
    // vec2 uv = gl_FragCoord.xy / iResolution.xy;
    // uv = vec2(uv.x, 1. - uv.y);
    
    return sampledColor;
} 
 
void main() { 
    fragColor = getTextureColor(UV) * vertColor;
    
    if (fragColor.a < 0.01)discard;
}