#version v 
 
#ifdef GL_ES 
precision highp float; 
#endif 
 
uniform sampler2D textures[16]; 
 
// Comes in from the vertex shader.   
in vec2 UV; 
in vec4 vertColor; 
in float Tid; 
 
out vec4 fragColor; 
 
vec4 getTextureColor() { 
  vec4 sampledColor = vec4(0.0, 0.0, 0.0, 0.0);
  
  // Check if a texture is in use.
  if (Tid >= 0.0) {
    // Sample for the texture's color at the specified vertex UV and multiply it by the tint.
    if (Tid == 0.0) {
      sampledColor = texture(textures[0], UV) * vertColor;
    } else if (Tid == 1.0) {
      sampledColor = texture(textures[1], UV) * vertColor;
    } else if (Tid == 2.0) {
      sampledColor = texture(textures[2], UV) * vertColor;
    } else if (Tid == 3.0) {
      sampledColor = texture(textures[3], UV) * vertColor;
    } else if (Tid == 4.0) {
      sampledColor = texture(textures[4], UV) * vertColor;
    } else if (Tid == 5.0) {
      sampledColor = texture(textures[5], UV) * vertColor;
    } else if (Tid == 6.0) {
      sampledColor = texture(textures[6], UV) * vertColor;
    } else if (Tid == 7.0) {
      sampledColor = texture(textures[7], UV) * vertColor;
    } else if (Tid == 8.0) {
      sampledColor = texture(textures[8], UV) * vertColor;
    } else if (Tid == 9.0) {
      sampledColor = texture(textures[9], UV) * vertColor;
    } else if (Tid == 10.0) {
      sampledColor = texture(textures[10], UV) * vertColor;
    } else if (Tid == 11.0) {
      sampledColor = texture(textures[11], UV) * vertColor;
    } else if (Tid == 12.0) {
      sampledColor = texture(textures[12], UV) * vertColor;
    } else if (Tid == 13.0) {
      sampledColor = texture(textures[13], UV) * vertColor;
    } else if (Tid == 14.0) {
      sampledColor = texture(textures[14], UV) * vertColor;
    } else if (Tid == 15.0) {
      sampledColor = texture(textures[15], UV) * vertColor;
    } else {
      sampledColor = texture(textures[15], UV) * vertColor;
    }
  } else {
    // If no texture then just use the color.
    sampledColor = vertColor;
  }
  
  return sampledColor;
} 
 
void main() { 
  fragColor = getTextureColor();
  
  if (fragColor.a < 0.01)discard;
}