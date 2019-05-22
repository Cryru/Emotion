#version 110 

uniform sampler2D textures[16]; 
 
// Comes in from the vertex shader.    
varying vec2 UV; 
varying vec4 vertColor; 
varying float Tid; 

vec4 getTextureColor(vec2 uvInput) { 
  vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);
  
  // Check if a texture is in use.
  if (Tid >= 0.0) {
    // Sample for the texture's color at the specified vertex uvInput.
    if (Tid == 0.0) {
      sampledColor = texture2D(textures[0], uvInput);
    } else if (Tid == 1.0) {
      sampledColor = texture2D(textures[1], uvInput);
    } else if (Tid == 2.0) {
      sampledColor = texture2D(textures[2], uvInput);
    } else if (Tid == 3.0) {
      sampledColor = texture2D(textures[3], uvInput);
    } else if (Tid == 4.0) {
      sampledColor = texture2D(textures[4], uvInput);
    } else if (Tid == 5.0) {
      sampledColor = texture2D(textures[5], uvInput);
    } else if (Tid == 6.0) {
      sampledColor = texture2D(textures[6], uvInput);
    } else if (Tid == 7.0) {
      sampledColor = texture2D(textures[7], uvInput);
    } else if (Tid == 8.0) {
      sampledColor = texture2D(textures[8], uvInput);
    } else if (Tid == 9.0) {
      sampledColor = texture2D(textures[9], uvInput);
    } else if (Tid == 10.0) {
      sampledColor = texture2D(textures[10], uvInput);
    } else if (Tid == 11.0) {
      sampledColor = texture2D(textures[11], uvInput);
    } else if (Tid == 12.0) {
      sampledColor = texture2D(textures[12], uvInput);
    } else if (Tid == 13.0) {
      sampledColor = texture2D(textures[13], uvInput);
    } else if (Tid == 14.0) {
      sampledColor = texture2D(textures[14], uvInput);
    } else if (Tid == 15.0) {
      sampledColor = texture2D(textures[15], uvInput);
    } else {
      sampledColor = texture2D(textures[15], uvInput);
    }
  }
  
  return sampledColor;
}
 
void main() { 
  gl_FragColor = getTextureColor(UV) * vertColor;
  
  if (gl_FragColor.a < 0.01) discard;
}
