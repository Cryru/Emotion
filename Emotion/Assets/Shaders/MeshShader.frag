#version v
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)
 
uniform vec4 sunColor;
uniform vec3 sunDirection;
uniform vec4 ambientColor;

// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor;
 
in vec3 fragNormal;
in vec3 fragLightDir;

out vec4 fragColor; 
 
#using "Shaders/getTextureColor.c"

void main() {
    vec4 objectColor = getTextureColor(mainTexture, UV) * vertColor;

    vec4 finalColor = objectColor;

    // Lambertian
    vec3 diffuse = (max(dot(fragNormal, fragLightDir), 0.0) * sunColor.rgb);
    diffuse = min(diffuse + ambientColor.rgb, vec3(1.0));
    finalColor *= vec4(diffuse, 1.0);

    fragColor = finalColor;
    if (fragColor.a < 0.01)discard;
}