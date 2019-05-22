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
 
vec4 getTextureColor(vec2 uvInput) { 
  vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);
  
  // Check if a texture is in use.
  if (Tid >= 0.0) {
    // Sample for the texture's color at the specified vertex uvInput.
    if (Tid == 0.0) {
      sampledColor = texture(textures[0], uvInput);
    } else if (Tid == 1.0) {
      sampledColor = texture(textures[1], uvInput);
    } else if (Tid == 2.0) {
      sampledColor = texture(textures[2], uvInput);
    } else if (Tid == 3.0) {
      sampledColor = texture(textures[3], uvInput);
    } else if (Tid == 4.0) {
      sampledColor = texture(textures[4], uvInput);
    } else if (Tid == 5.0) {
      sampledColor = texture(textures[5], uvInput);
    } else if (Tid == 6.0) {
      sampledColor = texture(textures[6], uvInput);
    } else if (Tid == 7.0) {
      sampledColor = texture(textures[7], uvInput);
    } else if (Tid == 8.0) {
      sampledColor = texture(textures[8], uvInput);
    } else if (Tid == 9.0) {
      sampledColor = texture(textures[9], uvInput);
    } else if (Tid == 10.0) {
      sampledColor = texture(textures[10], uvInput);
    } else if (Tid == 11.0) {
      sampledColor = texture(textures[11], uvInput);
    } else if (Tid == 12.0) {
      sampledColor = texture(textures[12], uvInput);
    } else if (Tid == 13.0) {
      sampledColor = texture(textures[13], uvInput);
    } else if (Tid == 14.0) {
      sampledColor = texture(textures[14], uvInput);
    } else if (Tid == 15.0) {
      sampledColor = texture(textures[15], uvInput);
    } else {
      sampledColor = texture(textures[15], uvInput);
    }
  }
  
  return sampledColor;
}
 
void main() { 
  fragColor = getTextureColor(UV) * vertColor;
  
  if (fragColor.a < 0.01)discard;
}