// DEFINE_VERTEX_ATTRIBUTE Position V_Pos
// DEFINE_VERTEX_ATTRIBUTE UV V_UV
// DEFINE_VERTEX_ATTRIBUTE VertexColor V_Color

#ifdef VERT_SHADER

vec4 VertexShaderMain()
{
    return projectionMatrix * viewMatrix * modelMatrix * vec4(V_Pos, 1.0);
}

#endif

#ifdef FRAG_SHADER

uniform Texture mainTexture;

vec4 FragmentShaderMain()
{
    vec4 color = texture(mainTexture, V_UV) * V_Color;
    if (color.a < 0.01) discard;
    return color;
}

#endif