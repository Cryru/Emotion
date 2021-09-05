#version v 
 
uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;
 
layout(location = 0)in vec3 vertPos; 
layout(location = 1)in vec2 uv; 
layout(location = 2)in vec4 color;
 
// Goes to the frag shader.  
out vec2 UV; 
out vec4 vertColor; 
 
uniform vec2 drawSize;

out vec2 left_coord;
out vec2 right_coord;
out vec2 above_coord;
out vec2 below_coord;

out vec2 lefta_coord;
out vec2 righta_coord;
out vec2 leftb_coord;
out vec2 rightb_coord;

void main() { 
    // Pass to frag.
    UV = uv;
    vertColor = color;
    
	vec2 d = 1.0/drawSize;
	left_coord = clamp(vec2(uv.xy + vec2(-d.x , 0)),0.0,1.0);
	right_coord = clamp(vec2(uv.xy + vec2(d.x , 0)),0.0,1.0);
	above_coord = clamp(vec2(uv.xy + vec2(0,d.y)),0.0,1.0);
	below_coord = clamp(vec2(uv.xy + vec2(0,-d.y)),0.0,1.0);
	lefta_coord = clamp(vec2(uv.xy + vec2(-d.x , d.x)),0.0,1.0);
	righta_coord = clamp(vec2(uv.xy + vec2(d.x , d.x)),0.0,1.0);
	leftb_coord = clamp(vec2(uv.xy + vec2(-d.x , -d.x)),0.0,1.0);
	rightb_coord = clamp(vec2(uv.xy + vec2(d.x , -d.x)),0.0,1.0);

    // Multiply by projection.
    gl_Position = projectionMatrix * modelMatrix * vec4(vertPos, 1.0);
}