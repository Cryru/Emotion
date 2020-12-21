#version v 
 
uniform sampler2D mainTexture;
uniform vec3 iResolution; // viewport resolution (in pixels)

// Comes in from the vertex shader.           
in vec2 UV; 
in vec4 vertColor; 
 
out vec4 fragColor;

#using "Shaders/getTextureColor.c"
#using "Shaders/getTextureSize.c"

uniform vec2 drawSize;

void main() {
  vec4 accum = vec4(0.0, 0.0, 0.0, 0.0);
  
  ivec2 textureSize = getTextureSize(mainTexture);
  float dx = 1.0/float(textureSize.x);
  float dy = 1.0/float(textureSize.y);

  int kernelSize = int((float(textureSize.x) / drawSize.x) / 2.0);
  kernelSize = max(kernelSize, 1);
  int sampleSize = 0;
  for (int y=-kernelSize; y<=kernelSize; y++)
  {
      for (int x=-kernelSize; x<=kernelSize; x++)
      {   
          vec2 dpos = vec2(float(x)*dx, float(y)*dy);
		  vec2 samplePos = UV + dpos;
          vec4 sampleColor = getTextureColor(mainTexture, samplePos);
		  accum += sampleColor;
		  sampleSize++;
      }
  }

  fragColor = (accum / float(sampleSize)) * vertColor;
  if (fragColor.a < 0.01)discard;
}