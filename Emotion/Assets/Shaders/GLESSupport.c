#ifdef GL_ES
    // No default for fragment shader
    precision highp float;
#endif

#ifdef GL_ES
   #define LOWP lowp
   #define MIDP mediump
   #define HIGHP highp
#else
   #define LOWP
   #define MIDP
   #define HIGHP
#endif