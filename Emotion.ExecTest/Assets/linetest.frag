#version v
 

in vec2 vpar;
in vec2 vlimits;
in float dist_scale;

out vec4 fragColor; 

// Updated root finding algorithm that copes better with degenerate cases (straight lines)
// From "The Low-Rank LDL^T Quartic Solver" by Peter Strobach, 2015

float solve_par_dist( vec2 pcoord, int iter )
{
    float sigx = pcoord.x > 0.0 ? 1.0 : -1.0;  
    float px = abs( pcoord.x );
    float py = pcoord.y;
    float h = 0.5 * px;
    float g = 0.5 - py;
    float xr = sqrt( 0.5 * px );
    float x0 = g < -h ? sqrt( abs( g ) ) :
               g > xr ? h / abs( g ) :
               xr;

    for ( int i = 0; i < iter; ++i ) {
        float rcx0 = 1.0 / x0;
        float pb = h * rcx0 * rcx0;
        float pc = -px * rcx0 + g;
        x0 = 2.0 * pc / ( -pb - sqrt( abs( pb*pb - 4.0*pc ) ) );
    }

    x0 = sigx * x0;
    float dx = sigx * sqrt( -0.75 * x0*x0 - g );
    float x1 = -0.5 * x0 - dx;
    
    x0 = clamp( x0, vlimits.x, vlimits.y );        
    x1 = clamp( x1, vlimits.x, vlimits.y );

    float d0 = length( vec2( x0, x0*x0 ) - pcoord );
    float d1 = length( vec2( x1, x1*x1 ) - pcoord );

    float dist = min( d0, d1 );
    return dist;
}


void main() {
    float dist = solve_par_dist( vpar, 3 );
    float pdist = min( dist * dist_scale, 1.0 );
    
    float color = 0.5 - 0.5 * pdist;

    if ( color == 0.0 ) discard;

    fragColor = vec4( color );
    gl_FragDepth = pdist;        
}