#version v

layout(location = 0)in vec3 vertPos;
layout(location = 1)in vec2 uv;
layout(location = 2)in vec4 color;

uniform vec2 sdf_tex_size; // Size of font texture in pixels
uniform float vertexZ;
uniform float sdf_border_size;

uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 

out vec2  UV;
out float doffset;
out vec2  sdf_texel;
out float subpixel_offset;

out vec4 vertColor; 

void main()
{
    float scale = vertPos.z;
    float sdf_size = 2.0 * scale * sdf_border_size;
    UV = uv;
    doffset = 1.0 / sdf_size;         // Distance field delta in screen pixels
    sdf_texel = 1.0 / sdf_tex_size;
    subpixel_offset = 0.3333 / scale; // 1/3 of screen pixel to texels
    vertColor = color;

    // z is passed as a uniform because the vert z is used for keeping the glyph scale
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4( vertPos.xy, vertexZ, 1.0 );    
}