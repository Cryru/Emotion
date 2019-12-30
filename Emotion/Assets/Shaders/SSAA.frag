#version v 
 
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

//GetTextureColor
//GetTextureSize

uniform vec2 drawSize;

void main() {
  vec4 accum = vec4(0.0, 0.0, 0.0, 0.0);
  
  ivec2 textureSize = getTextureSize(Tid);
  float dx = 1.0/textureSize.x;
  float dy = 1.0/textureSize.y;

  int kernelSize = int(textureSize.x / drawSize.x) / 2;
  int sampleSize = 0;
  for (int y=-kernelSize; y<=kernelSize; y++)
  {
      for (int x=-kernelSize; x<=kernelSize; x++)
      {   
          vec2 dpos = vec2(float(x)*dx, float(y)*dy);
		      vec2 samplePos = UV + dpos;
          vec4 sampleColor = getTextureColor(Tid, samplePos);
		      accum += sampleColor;
		      sampleSize++;
      }
  }

  fragColor = (accum / float(sampleSize)) * vertColor;
  if (fragColor.a < 0.01)discard;
}