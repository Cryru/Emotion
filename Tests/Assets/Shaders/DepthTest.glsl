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
uniform Texture depthTexture;

vec4 FragmentShaderMain()
{
    float depth = texture(depthTexture, V_UV).r;
    if (gl_FragCoord.z > depth)
        gl_FragDepth = depth;
    else
        gl_FragDepth = gl_FragCoord.z;
        
    vec4 color = texture(mainTexture, V_UV) * V_Color;
    if (color.a < ALPHA_DISCARD) discard;
    return color;
}

#endif