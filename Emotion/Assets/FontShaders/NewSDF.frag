#version v

#define GLYPH_HINTING 1.
//#define PIXEL_FONT
//#define SUBPIXEL_HINTING

uniform sampler2D mainTexture;
uniform vec2 sdf_tex_size; // Size of font texture in pixels
uniform float sdf_border_size;
uniform vec3 iResolution; // viewport resolution (in pixels)
uniform float scale; // viewport resolution (in pixels)

in vec2 UV;
in vec4 vertColor;

#ifdef SUBPIXEL_HINTING
layout(location = 0, index = 0) out vec4 fragColor;
layout(location = 0, index = 1) out vec4 fragColorBlend;
#else
out vec4 fragColor;
#endif

vec3 sdf_triplet_alpha(float doffset, vec3 sdf, float horz_scale, float vert_scale, float vgrad )
{
    float hdoffset = mix( doffset * horz_scale, doffset * vert_scale, vgrad );
    float rdoffset = mix( doffset, hdoffset, GLYPH_HINTING );

#ifdef PIXEL_FONT
    hdoffset = 0.;
    rdoffset = 0.;
#endif

    vec3 alpha = smoothstep( vec3( 0.5 - rdoffset ), vec3( 0.5 + rdoffset ), sdf );
    alpha = pow( alpha, vec3( 1.0 + 0.2 * vgrad * GLYPH_HINTING ) );
    return alpha;
}

float sdf_alpha(float doffset, float sdf, float horz_scale, float vert_scale, float vgrad )
{
    float hdoffset = mix( doffset * horz_scale, doffset * vert_scale, vgrad );
    float rdoffset = mix( doffset, hdoffset, GLYPH_HINTING );

#ifdef PIXEL_FONT
    hdoffset = 0.;
    rdoffset = 0.;
#endif

    float alpha = smoothstep( 0.5 - rdoffset, 0.5 + rdoffset, sdf );
    alpha = pow( alpha, 1.0 + 0.2 * vgrad * GLYPH_HINTING );
    return alpha;
}

float screenPxRange() {
    vec2 unitRange = vec2(iResolution.xy)/sdf_tex_size;
    vec2 screenTexSize = vec2(1.0)/fwidth(UV);
    return 2*dot(unitRange, screenTexSize);
}

void main()
{
    float sdf_size = 2.0 * scale * sdf_border_size;
    float doffset = 1.0 / sdf_size;
    vec2 sdf_texel = 1.0 / sdf_tex_size;
    float subpixel_offset = 0.3333 / scale;

    // Sampling the texture, L pattern
    float sdf       = texture( mainTexture, UV ).r;
    float sdf_north = texture( mainTexture, UV + vec2( 0.0, sdf_texel.y ) ).r;
    float sdf_east  = texture( mainTexture, UV + vec2( sdf_texel.x, 0.0 ) ).r;

    // Estimating stroke direction by the distance field gradient vector
    vec2  sgrad     = vec2( sdf_east - sdf, sdf_north - sdf );
    float sgrad_len = max( length( sgrad ), 1.0 / 128.0 );
    vec2  grad      = sgrad / vec2( sgrad_len );
    float vgrad = abs( grad.y ); // 0.0 - vertical stroke, 1.0 - horizontal one

    #ifdef SUBPIXEL_HINTING
        // Subpixel SDF samples
        vec2 subpixel = vec2( subpixel_offset, 0.0 );
    
        float sdf_sp_n  = texture( mainTexture, UV - subpixel ).r;
        float sdf_sp_p  = texture( mainTexture, UV + subpixel ).r;

        float horz_scale  = 0.5; // Should be 0.33333, a subpixel size, but that is too colorful
        float vert_scale  = 0.6;

        vec3 triplet_alpha = sdf_triplet_alpha(doffset, vec3( sdf_sp_n, sdf, sdf_sp_p ), horz_scale, vert_scale, vgrad);
    
        fragColor = vec4( triplet_alpha.rgb * vertColor.a, 1.0 );
        fragColorBlend = vertColor;
    #else
        float horz_scale  = 1.1;
        float vert_scale  = 0.6;
        
        float alpha = sdf_alpha(doffset, sdf, horz_scale, vert_scale, vgrad);
        fragColor = vec4( vertColor.rgb, vertColor.a * alpha );
    #endif
}
