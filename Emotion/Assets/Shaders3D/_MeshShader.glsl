// ----------- ColorHelpers.h

float cbrt(float x)
{
    float y = uintBitsToFloat(709973695u+floatBitsToUint(x)/3u);
    y = y*(2.0/3.0) + (1.0/3.0)*x/(y*y);
    y = y*(2.0/3.0) + (1.0/3.0)*x/(y*y);
    return y;
}

vec3 RGBToOklab(vec3 rgb)
{

  float r = rgb.x;
  float g = rgb.g;
  float b = rgb.b;

  // This is the Oklab math:
  float l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b;
  float m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b;
  float s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b;

  l = cbrt(l);
  m = cbrt(m);
  s = cbrt(s);

  return vec3(
	l * +0.2104542553 + m * +0.7936177850 + s * -0.0040720468,
	l * +1.9779984951 + m * -2.4285922050 + s * +0.4505937099,
	l * +0.0259040371 + m * +0.7827717662 + s * -0.8086757660
  );
}

vec3 OklabToRGB(vec3 lab) {
  float L = lab.x;
  float a = lab.y;
  float b = lab.z;

  float l = L + a * +0.3963377774 + b * +0.2158037573;
  float m = L + a * -0.1055613458 + b * -0.0638541728;
  float s = L + a * -0.0894841775 + b * -1.2914855480;

  l = pow(l, 3.);
  m = pow(m, 3.);
  s = pow(s, 3.);

  float R = l * +4.0767416621 + m * -3.3077115913 + s * +0.2309699292;
  float G = l * -1.2684380046 + m * +2.6097574011 + s * -0.3413193965;
  float B = l * -0.0041960863 + m * -0.7034186147 + s * +1.7076147010;
  
  return vec3(R, G, B);
}

vec4 ApplyColorTint(vec4 originalColor, vec4 tintColor)
{
	// Convert both the origin and tint colors to OKLab space
	vec3 oklabColor = RGBToOklab(originalColor.rgb);
	vec3 oklabTint = RGBToOklab(tintColor.rgb);

	// Calculate the hue difference between the original color and the tint color
	// y holds green-red
	// z holds blue-yellow
	float hueDifference = atan(oklabTint.z, oklabTint.y) - atan(oklabColor.z, oklabColor.y);

	// Modify the hue by rotating it by the difference in hue
	oklabColor.yz = mat2(
		cos(hueDifference), sin(hueDifference),
		-sin(hueDifference), cos(hueDifference)
	) * oklabColor.yz;

	// Convert the modified color back to RGB
	vec3 modifiedRGB = OklabToRGB(oklabColor);

	// Take whichever color is brighter between the source and tint color,
	// and use that to adjust the saturation to avoid desaturation when
	// either the object or tint is too bright.
	float brightness = (tintColor.r + tintColor.g + tintColor.b) / 3.0;
	float brightnessSource = (originalColor.r + originalColor.g + originalColor.b) / 3.0;
	brightness = max(brightness, brightnessSource);

	float saturationFactor = 1.0 - brightness;
	vec3 originColorTinted = mix(originalColor.rgb * tintColor.rgb, modifiedRGB, saturationFactor);

	return vec4(originColorTinted.rgb, originalColor.a * tintColor.a);
}

// ----------- Common.h

#if VERT_SHADER

#define VERTEX_ATTRIBUTE(loc, typ, name) layout(location = loc) in typ name
#define VERTEX_ATTRIBUTE_LINE_TWO(loc, typ, name) out typ pass_##name
#define VERTEX_ATTRIBUTE_WORK(loc, typ, name) pass_##name = name

#endif

#if FRAG_SHADER

#define VERTEX_ATTRIBUTE(loc, typ, name) in typ pass_##name;
#define VERTEX_ATTRIBUTE_LINE_TWO(loc, typ, name) typ name = pass_##name;
#define RETURN_DEBUG_NORMAL return vec4((fragNormal + vec3(1.0)) / 2.0, 1.0);

#endif

#define WHITE vec4(1.0, 1.0, 1.0, 1.0)
#define RED vec4(1.0, 0.0, 0.0, 1.0)
#define GREEN vec4(0.0, 1.0, 0.0, 1.0)
#define BLUE vec4(0.0, 0.0, 1.0, 1.0)
#define COLOR_OPAQUE(col) vec4(col, col, col, 1.0)

float saturate(in float value)
{
    return clamp(value, 0.0, 1.0);
}

#define EDITOR_BRUSH 1

VERTEX_ATTRIBUTE(0, vec3, vertPos);
VERTEX_ATTRIBUTE_LINE_TWO(0, vec3, vertPos);

VERTEX_ATTRIBUTE(1, vec2, uv);
VERTEX_ATTRIBUTE_LINE_TWO(1, vec2, uv);

VERTEX_ATTRIBUTE(2, vec3, normal);
VERTEX_ATTRIBUTE_LINE_TWO(2, vec3, normal);

VERTEX_ATTRIBUTE(3, vec4, vertColor);
VERTEX_ATTRIBUTE_LINE_TWO(3, vec4, vertColor);

#if VERT_SHADER

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

// Skip this multiply with a shader variation if no model matrix or CPU mode model matrix
uniform mat4 modelMatrix;

vec4 VertexShaderMain_DEEP()
{
    return projectionMatrix * viewMatrix * modelMatrix * vec4(vertPos, 1.0);
}

out vec3 fragPosition;
out vec3 fragNormal;

void VertexShaderMain()
{
    VERTEX_ATTRIBUTE_WORK(0, vec3, vertPos);
    VERTEX_ATTRIBUTE_WORK(1, vec2, uv);
    VERTEX_ATTRIBUTE_WORK(2, vec3, normal);
    VERTEX_ATTRIBUTE_WORK(3, vec4, vertColor);

    fragPosition = vec3(modelMatrix * vec4(vertPos, 1.0));
    fragNormal = normalize(mat3(transpose(inverse(modelMatrix))) * normal);

    gl_Position = VertexShaderMain_DEEP();
}

#endif

#if FRAG_SHADER

uniform sampler2D diffuseTexture;
uniform vec3 cameraPosition;

in vec3 fragPosition;
in vec3 fragNormal;

// Material
uniform vec4 diffuseColor;

vec4 FragmentShaderMain()
{
    vec4 col = vertColor;

    // Calculate the color of the object.
    vec4 textureColor = getTextureColor(diffuseTexture, UV);

    vec4 objectColor = textureColor * diffuseColor * vertColor;
    objectColor = ApplyColorTint(objectColor, objectTint);

    if (objectColor.a < ALPHA_DISCARD) discard;
    return vec4(finalColor.rgb, objectColor.a);
}

#endif