namespace Emotion.Graphics.Shading
{
    public static class ShaderData
    {
        public static string GetTextureColor = @"
         #if !defined(CompatTextureColor)
vec4 getTextureColor(vec2 uvInput) { 
    vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);
    
    // Check if a texture is in use.
    if (Tid >= 0) {
        // Sample for the texture's color at the specified vertex UV and multiply it by the tint.
        sampledColor = texture(textures[Tid], uvInput);
    }
    
    return sampledColor;
}
#else

vec4 getTextureColor(vec2 uvInput) { 
  vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);
  
  // Check if a texture is in use.
  if (Tid >= 0) {
    // Sample for the texture's color at the specified vertex uvInput.
    if (Tid == 0) {
      sampledColor = texture(textures[0], uvInput);
    } else if (Tid == 1) {
      sampledColor = texture(textures[1], uvInput);
    } else if (Tid == 2) {
      sampledColor = texture(textures[2], uvInput);
    } else if (Tid == 3) {
      sampledColor = texture(textures[3], uvInput);
    } else if (Tid == 4) {
      sampledColor = texture(textures[4], uvInput);
    } else if (Tid == 5) {
      sampledColor = texture(textures[5], uvInput);
    } else if (Tid == 6) {
      sampledColor = texture(textures[6], uvInput);
    } else if (Tid == 7) {
      sampledColor = texture(textures[7], uvInput);
    } else if (Tid == 8) {
      sampledColor = texture(textures[8], uvInput);
    } else if (Tid == 9) {
      sampledColor = texture(textures[9], uvInput);
    } else if (Tid == 10) {
      sampledColor = texture(textures[10], uvInput);
    } else if (Tid == 11) {
      sampledColor = texture(textures[11], uvInput);
    } else if (Tid == 12) {
      sampledColor = texture(textures[12], uvInput);
    } else if (Tid == 13) {
      sampledColor = texture(textures[13], uvInput);
    } else if (Tid == 14) {
      sampledColor = texture(textures[14], uvInput);
    } else if (Tid == 15) {
      sampledColor = texture(textures[15], uvInput);
    } else {
      sampledColor = texture(textures[15], uvInput);
    }
  }
  
  return sampledColor;
}

#endif
";

        public static string DefaultVertShader = @"#version v 
 
uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 
 
// Shader toy API uniforms. 
uniform float iTime; // shader playback time (in seconds) 
uniform vec3 iResolution; // viewport resolution (in pixels) 
uniform vec4 iMouse; // mouse pixel coords. xy: current, zw: click 
 
layout(location = 0)in vec3 vertPos; 
layout(location = 1)in vec2 uv; 
layout(location = 2)in float tid; 
layout(location = 3)in vec4 color; 
 
// Goes to the frag shader.  
out vec2 UV; 
out vec4 vertColor; 
flat out int Tid; 
 
void main() { 
    // Pass to frag.
    UV = uv;
    vertColor = color;
    Tid = int(tid);
    
    // Multiply by projection.
    gl_Position = projectionMatrix * vec4(vertPos, 1.0);
}";

        public static string DefaultFragShader = @"#version v 
 
#ifdef GL_ES 
precision highp float; 
#endif 
 
uniform sampler2D textures[16]; 
uniform vec3 iResolution; // viewport resolution (in pixels)
 
// Comes in from the vertex shader. 
in vec2 UV; 
in vec4 vertColor; 
flat in int Tid;
 
out vec4 fragColor; 
 
//GetTextureColor()
 
void main() { 
    fragColor = getTextureColor(UV) * vertColor;
    
    if (fragColor.a < 0.01)discard;
}";
    }
}