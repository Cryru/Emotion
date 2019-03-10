#version 330

uniform sampler2D textures[16];

// Comes in from the vertex shader.
in vec2 UV;
in vec4 vertColor;
in float Tid;

out vec4 fragColor;

void main() {
  fragColor = vec4(0.0, 0.0, 0.0, 0.0);

  // Check if a texture is in use.
  if (Tid >= 0.0) {
    // Sample for the texture's color at the specified vertex UV and multiply it by the tint.
    if (Tid == 0.0) {
      fragColor = texture(textures[0], UV) * vertColor;
    } else if (Tid == 1.0) {
      fragColor = texture(textures[1], UV) * vertColor;
    } else if (Tid == 2.0) {
      fragColor = texture(textures[2], UV) * vertColor;
    } else if (Tid == 3.0) {
      fragColor = texture(textures[3], UV) * vertColor;
    } else if (Tid == 4.0) {
      fragColor = texture(textures[4], UV) * vertColor;
    } else if (Tid == 5.0) {
      fragColor = texture(textures[5], UV) * vertColor;
    } else if (Tid == 6.0) {
      fragColor = texture(textures[6], UV) * vertColor;
    } else if (Tid == 7.0) {
      fragColor = texture(textures[7], UV) * vertColor;
    } else if (Tid == 8.0) {
      fragColor = texture(textures[8], UV) * vertColor;
    } else if (Tid == 9.0) {
      fragColor = texture(textures[9], UV) * vertColor;
    } else if (Tid == 10.0) {
      fragColor = texture(textures[10], UV) * vertColor;
    } else if (Tid == 11.0) {
      fragColor = texture(textures[11], UV) * vertColor;
    } else if (Tid == 12.0) {
      fragColor = texture(textures[12], UV) * vertColor;
    } else if (Tid == 13.0) {
      fragColor = texture(textures[13], UV) * vertColor;
    } else if (Tid == 14.0) {
      fragColor = texture(textures[14], UV) * vertColor;
    } else if (Tid == 15.0) {
      fragColor = texture(textures[15], UV) * vertColor;
    }
  } else {
    // If no texture then just use the color.
    fragColor = vertColor;
  }

  if (fragColor.a < 0.01) discard;
}