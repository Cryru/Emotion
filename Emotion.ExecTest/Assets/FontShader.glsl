

#if VERT_SHADER
uniform mat4 projectionMatrix; 
uniform mat4 viewMatrix; 
uniform mat4 modelMatrix; 
 
layout(location = 0)in vec3 vertPos; 
layout(location = 1)in vec2 uv; 
layout(location = 2)in vec4 color; 
 
// Goes to the frag shader.  
out vec2 UV; 
out vec4 vertColor; 

void VertexShaderMain() { 
    // Pass to frag.
    UV = uv;
    vertColor = color;
    
    // Multiply by projection.
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vertPos, 1.0);
}
#endif

#if FRAG_SHADER
vec4 FragmentShaderMain()
{
    return vec4(1.0, 0.0, 0.0, 1.0);
}
#endif