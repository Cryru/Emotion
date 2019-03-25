#version 110 

uniform sampler2D textures[16]; 
 
// Comes in from the vertex shader.    
varying vec2 UV; 
varying vec4 vertColor; 
varying float Tid; 
 
vec4 getTextureColor() { 
  vec4 sampledColor = vec4(0.0, 0.0, 0.0, 0.0);
  
  // Check if a texture is in use.
  if (Tid >= 0.0) {
    // Sample for the texture's color at the specified vertex UV and multiply it by the tint.
    if (Tid == 0.0) {
      sampledColor = texture2D(textures[0], UV) * vertColor;
    } else if (Tid == 1.0) {
      sampledColor = texture2D(textures[1], UV) * vertColor;
    } else if (Tid == 2.0) {
      sampledColor = texture2D(textures[2], UV) * vertColor;
    } else if (Tid == 3.0) {
      sampledColor = texture2D(textures[3], UV) * vertColor;
    } else if (Tid == 4.0) {
      sampledColor = texture2D(textures[4], UV) * vertColor;
    } else if (Tid == 5.0) {
      sampledColor = texture2D(textures[5], UV) * vertColor;
    } else if (Tid == 6.0) {
      sampledColor = texture2D(textures[6], UV) * vertColor;
    } else if (Tid == 7.0) {
      sampledColor = texture2D(textures[7], UV) * vertColor;
    } else if (Tid == 8.0) {
      sampledColor = texture2D(textures[8], UV) * vertColor;
    } else if (Tid == 9.0) {
      sampledColor = texture2D(textures[9], UV) * vertColor;
    } else if (Tid == 10.0) {
      sampledColor = texture2D(textures[10], UV) * vertColor;
    } else if (Tid == 11.0) {
      sampledColor = texture2D(textures[11], UV) * vertColor;
    } else if (Tid == 12.0) {
      sampledColor = texture2D(textures[12], UV) * vertColor;
    } else if (Tid == 13.0) {
      sampledColor = texture2D(textures[13], UV) * vertColor;
    } else if (Tid == 14.0) {
      sampledColor = texture2D(textures[14], UV) * vertColor;
    } else if (Tid == 15.0) {
      sampledColor = texture2D(textures[15], UV) * vertColor;
    } else {
      sampledColor = texture2D(textures[15], UV) * vertColor;
    }
  } else {
    // If no texture then just use the color.
    sampledColor = vertColor;
  }
  
  return sampledColor;
} 
 
void main() { 
  gl_FragColor = getTextureColor();
  
  if (gl_FragColor.a < 0.01) discard;
}