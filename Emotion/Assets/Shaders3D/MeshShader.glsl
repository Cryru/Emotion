// INCLUDE_FILE <Shaders/Common.h>
// INCLUDE_FILE <Shaders/ColorHelpers.c>

// DEFINE_VERTEX_ATTRIBUTE Position V_Pos
// DEFINE_VERTEX_ATTRIBUTE Normal V_Normal?
// DEFINE_VERTEX_ATTRIBUTE UV V_UV
// DEFINE_VERTEX_ATTRIBUTE BoneIds V_BoneIds?
// DEFINE_VERTEX_ATTRIBUTE BoneWeights V_BoneWeights?

VERT_TO_FRAGMENT vec3 F_Pos;

#ifdef HAS_Normal
VERT_TO_FRAGMENT vec3 F_Normal;
#endif

#ifdef VERT_SHADER

#ifdef HAS_BONES
const int MAX_BONES = 200;
const int MAX_BONE_INFLUENCE = 4;
uniform mat4 boneMatrices[MAX_BONES];
#endif

vec4 VertexShaderMain()
{
    vec4 totalPosition = vec4(V_Pos, 1.0);

#ifdef HAS_BONES
    mat4 totalTransform = boneMatrices[int(V_BoneIds[0])] * V_BoneWeights[0];
    for (int i = 1; i < MAX_BONE_INFLUENCE; i++)
    {
        totalTransform += boneMatrices[int(V_BoneIds[i])] * V_BoneWeights[i];
    }
    totalPosition = totalTransform * totalPosition;
#ifdef HAS_Normal
    F_Normal = normalize(mat3(transpose(inverse(modelMatrix * totalTransform))) * V_Normal);
#endif
#else
#ifdef HAS_Normal
    F_Normal = normalize(mat3(transpose(inverse(modelMatrix))) * V_Normal);
#endif
#endif

    vec4 localPos = modelMatrix * totalPosition;
    F_Pos = vec3(localPos);
    return projectionMatrix * viewMatrix * localPos;
}

#endif

#ifdef FRAG_SHADER

#define ALPHA_DISCARD (128.0 / 255.0)
uniform Texture diffuseTexture;
uniform vec4 diffuseColor;

#ifdef LIGHT_ENABLED
uniform vec3 sunDirection;
uniform vec4 ambientColor;
uniform float ambientLightStrength;
uniform float diffuseStrength;
#endif

vec4 FragmentShaderMain()
{
    vec4 textureColor = texture(diffuseTexture, V_UV);

    vec4 objectTint = vec4(1.0);
    vec4 objectColor = textureColor * diffuseColor;
    objectColor = ApplyColorTint(objectColor, objectTint);

#ifdef LIGHT_ENABLED
#ifdef HAS_Normal
    vec3 fragLightDir = normalize(sunDirection);

    // Valve Half Lambert diffuse factor
    // https://developer.valvesoftware.com/wiki/Half_Lambert
    float diffuseFactor = dot(F_Normal, fragLightDir) * 0.5 + 0.5;

    // Combine ambient and diffuse
    vec3 ambient = ambientColor.rgb * ambientLightStrength;
    vec3 diffuse = mix(objectColor.rgb, objectColor.rgb * diffuseFactor, diffuseStrength);
    vec3 litColor = diffuse * ambient;

    vec4 finalColor = vec4(litColor.rgb, objectColor.a);
#else
    vec4 finalColor = objectColor;
#endif
#else
    vec4 finalColor = objectColor;
#endif

    if (finalColor.a < ALPHA_DISCARD) discard;
    return finalColor;
}

#endif