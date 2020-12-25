#pragma warning disable 649, 1572, 1573

// ReSharper disable RedundantUsingDirective

#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Emotion.Platform;
using Emotion.Utility;
using Khronos;

#endregion

// ReSharper disable StringLiteralTypo
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InvalidXmlDocComment
// ReSharper disable CommentTypo
namespace OpenGL
{
    public partial class Gl
    {
        /// <summary>
        /// [GL4|GLES3.2] Gl.Clear: Indicates the depth buffer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const uint DEPTH_BUFFER_BIT = 0x00000100;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Clear: Indicates the stencil buffer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const uint STENCIL_BUFFER_BIT = 0x00000400;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Clear: Indicates the buffers currently enabled for color writing.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const uint COLOR_BUFFER_BIT = 0x00004000;

        /// <summary>
        /// [GL] Value of GL_FALSE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int FALSE = 0;

        /// <summary>
        /// [GL] Value of GL_TRUE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int TRUE = 1;

        /// <summary>
        /// [GL2.1] Gl.Begin: Treats each vertex as a single point. Vertex n defines point n. N points are drawn.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int POINTS = 0x0000;

        /// <summary>
        /// [GL2.1] Gl.Begin: Treats each pair of vertices as an independent line segment. Vertices 2⁢n-1 and 2⁢n define line n. N2
        /// lines are drawn.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LINES = 0x0001;

        /// <summary>
        /// [GL2.1] Gl.Begin: Draws a connected group of line segments from the first vertex to the last, then back to the first.
        /// Vertices n and n+1 define line n. The last line, however, is defined by vertices N and 1. N lines are drawn.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LINE_LOOP = 0x0002;

        /// <summary>
        /// [GL2.1] Gl.Begin: Draws a connected group of line segments from the first vertex to the last. Vertices n and n+1 define
        /// line n. N-1 lines are drawn.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LINE_STRIP = 0x0003;

        /// <summary>
        /// [GL2.1] Gl.Begin: Treats each triplet of vertices as an independent triangle. Vertices 3⁢n-2, 3⁢n-1, and 3⁢n define
        /// triangle n. N3 triangles are drawn.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        public const int TRIANGLES = 0x0004;

        /// <summary>
        /// [GL2.1] Gl.Begin: Draws a connected group of triangles. One triangle is defined for each vertex presented after the
        /// first two vertices. For odd n, vertices n, n+1, and n+2 define triangle n. For even n, vertices n+1, n, and n+2 define
        /// triangle n. N-2 triangles are drawn.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int TRIANGLE_STRIP = 0x0005;

        /// <summary>
        /// [GL2.1] Gl.Begin: Draws a connected group of triangles. One triangle is defined for each vertex presented after the
        /// first two vertices. Vertices 1, n+1, and n+2 define triangle n. N-2 triangles are drawn.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int TRIANGLE_FAN = 0x0006;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DepthFunc: Never passes.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFunc: Always fails.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFuncSeparate: Always fails.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DepthFunc: Never passes.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFunc: Always fails.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFuncSeparate: Always fails.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int NEVER = 0x0200;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DepthFunc: Passes if the incoming depth value is less than the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFunc: Passes if ( ref &amp; mask ) &lt; ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) &lt; ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DepthFunc: Passes if the incoming depth value is less than the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFunc: Passes if ( ref &amp; mask ) &lt; ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) &lt; ( stencil &amp; mask ).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LESS = 0x0201;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DepthFunc: Passes if the incoming depth value is equal to the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFunc: Passes if ( ref &amp; mask ) = ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) = ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DepthFunc: Passes if the incoming depth value is equal to the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFunc: Passes if ( ref &amp; mask ) = ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) = ( stencil &amp; mask ).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        public const int EQUAL = 0x0202;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DepthFunc: Passes if the incoming depth value is less than or equal to the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFunc: Passes if ( ref &amp; mask ) &lt;= ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) &lt;= ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DepthFunc: Passes if the incoming depth value is less than or equal to the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFunc: Passes if ( ref &amp; mask ) &lt;= ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) &lt;= ( stencil &amp; mask ).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LEQUAL = 0x0203;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DepthFunc: Passes if the incoming depth value is greater than the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFunc: Passes if ( ref &amp; mask ) &gt; ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) &gt; ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DepthFunc: Passes if the incoming depth value is greater than the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFunc: Passes if ( ref &amp; mask ) &gt; ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) &gt; ( stencil &amp; mask ).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int GREATER = 0x0204;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DepthFunc: Passes if the incoming depth value is not equal to the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFunc: Passes if ( ref &amp; mask ) != ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) != ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DepthFunc: Passes if the incoming depth value is not equal to the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFunc: Passes if ( ref &amp; mask ) != ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) != ( stencil &amp; mask ).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int NOTEQUAL = 0x0205;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DepthFunc: Passes if the incoming depth value is greater than or equal to the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFunc: Passes if ( ref &amp; mask ) &gt;= ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) &gt;= ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DepthFunc: Passes if the incoming depth value is greater than or equal to the stored depth value.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFunc: Passes if ( ref &amp; mask ) &gt;= ( stencil &amp; mask ).
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFuncSeparate: Passes if ( ref &amp; mask ) &gt;= ( stencil &amp; mask ).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int GEQUAL = 0x0206;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DepthFunc: Always passes.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFunc: Always passes.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.StencilFuncSeparate: Always passes.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DepthFunc: Always passes.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFunc: Always passes.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.StencilFuncSeparate: Always passes.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int ALWAYS = 0x0207;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOp: Sets the stencil buffer value to 0.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOpSeparate: Sets the stencil buffer value to 0.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int ZERO = 0;

        /// <summary>
        /// [GL] Value of GL_ONE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int ONE = 1;

        /// <summary>
        /// [GL] Value of GL_SRC_COLOR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int SRC_COLOR = 0x0300;

        /// <summary>
        /// [GL] Value of GL_ONE_MINUS_SRC_COLOR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int ONE_MINUS_SRC_COLOR = 0x0301;

        /// <summary>
        /// [GL] Value of GL_SRC_ALPHA symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int SRC_ALPHA = 0x0302;

        /// <summary>
        /// [GL] Value of GL_ONE_MINUS_SRC_ALPHA symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int ONE_MINUS_SRC_ALPHA = 0x0303;

        /// <summary>
        /// [GL] Value of GL_DST_ALPHA symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int DST_ALPHA = 0x0304;

        /// <summary>
        /// [GL] Value of GL_ONE_MINUS_DST_ALPHA symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int ONE_MINUS_DST_ALPHA = 0x0305;

        /// <summary>
        /// [GL] Value of GL_DST_COLOR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int DST_COLOR = 0x0306;

        /// <summary>
        /// [GL] Value of GL_ONE_MINUS_DST_COLOR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int ONE_MINUS_DST_COLOR = 0x0307;

        /// <summary>
        /// [GL] Value of GL_SRC_ALPHA_SATURATE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_func_extended", Api = "gles2")]
        public const int SRC_ALPHA_SATURATE = 0x0308;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DrawBuffer: No color buffers are written.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.DrawBuffers: The fragment shader output value is not written into any color buffer.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DrawBuffers: The fragment shader output value is not written into any color buffer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_4_6")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        public const int NONE = 0;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DrawBuffer: Only the front left color buffer is written.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.DrawBuffers: The fragment shader output value is written into the front left color buffer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int FRONT_LEFT = 0x0400;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DrawBuffer: Only the front right color buffer is written.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.DrawBuffers: The fragment shader output value is written into the front right color buffer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int FRONT_RIGHT = 0x0401;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DrawBuffer: Only the back left color buffer is written.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.DrawBuffers: The fragment shader output value is written into the back left color buffer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int BACK_LEFT = 0x0402;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DrawBuffer: Only the back right color buffer is written.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.DrawBuffers: The fragment shader output value is written into the back right color buffer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int BACK_RIGHT = 0x0403;

        /// <summary>
        /// [GL4] Gl.DrawBuffer: Only the front left and front right color buffers are written. If there is no front right color
        /// buffer, only the front left color buffer is written.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int FRONT = 0x0404;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.DrawBuffer: Only the back left and back right color buffers are written. If there is no back right color
        ///     buffer, only the back left color buffer is written.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.DrawBuffers: The fragment shader output value is written into the back color buffer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        public const int BACK = 0x0405;

        /// <summary>
        /// [GL4] Gl.DrawBuffer: Only the front left and back left color buffers are written. If there is no back left color
        /// buffer,
        /// only the front left color buffer is written.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int LEFT = 0x0406;

        /// <summary>
        /// [GL4] Gl.DrawBuffer: Only the front right and back right color buffers are written. If there is no back right color
        /// buffer, only the front right color buffer is written.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int RIGHT = 0x0407;

        /// <summary>
        /// [GL4] Gl.DrawBuffer: All the front and back color buffers (front left, front right, back left, back right) are written.
        /// If there are no back color buffers, only the front left and front right color buffers are written. If there are no
        /// right
        /// color buffers, only the front left and back left color buffers are written. If there are no right or back color
        /// buffers,
        /// only the front left color buffer is written.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int FRONT_AND_BACK = 0x0408;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetError: No error has been recorded. The value of this symbolic constant is guaranteed to be 0.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetGraphicsResetStatus: Indicates that the GL context has not been in a reset state since the last
        ///     call.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        public const int NO_ERROR = 0;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetError: An unacceptable value is specified for an enumerated argument. The offending command is
        /// ignored and has no other side effect than to set the error flag.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int INVALID_ENUM = 0x0500;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetError: A numeric argument is out of range. The offending command is ignored and has no other side
        /// effect than to set the error flag.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int INVALID_VALUE = 0x0501;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetError: The specified operation is not allowed in the current state. The offending command is
        /// ignored
        /// and has no other side effect than to set the error flag.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int INVALID_OPERATION = 0x0502;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetError: There is not enough memory left to execute the command. The state of the GL is undefined,
        /// except for the state of the error flags, after this error is recorded.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int OUT_OF_MEMORY = 0x0505;

        /// <summary>
        /// [GL] Value of GL_CW symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        public const int CW = 0x0900;

        /// <summary>
        /// [GL] Value of GL_CCW symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        public const int CCW = 0x0901;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the point size as specified by Gl.PointSize. The initial value is 1.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, the point size as specified by Gl.PointSize.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int POINT_SIZE = 0x0B11;

        /// <summary>
        /// [GL4] Gl.Get: data returns two values: the smallest and largest supported sizes for antialiased points. The smallest
        /// size must be at most 1, and the largest size must be at least 1. See Gl.PointSize.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int POINT_SIZE_RANGE = 0x0B12;

        /// <summary>
        /// [GL4] Gl.Get: data returns one value, the size difference between adjacent supported sizes for antialiased points. See
        /// Gl.PointSize.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int POINT_SIZE_GRANULARITY = 0x0B13;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns a single boolean value indicating whether antialiasing of lines is enabled. The initial
        ///     value
        ///     is Gl.FALSE. See Gl.LineWidth.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Enable: If enabled, draw lines with correct filtering. Otherwise, draw aliased lines. See
        ///     Gl.LineWidth.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns a single boolean value indicating whether line antialiasing is enabled. The
        ///     initial
        ///     value is Gl.FALSE. See Gl.LineWidth.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int LINE_SMOOTH = 0x0B20;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the line width as specified with Gl.LineWidth. The initial value is 1.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LINE_WIDTH = 0x0B21;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns two values: the smallest and largest supported widths for antialiased lines. See
        /// Gl.LineWidth.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int LINE_WIDTH_RANGE = 0x0B22;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns one value, the width difference between adjacent supported widths for antialiased lines.
        /// See Gl.LineWidth.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int LINE_WIDTH_GRANULARITY = 0x0B23;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns two values: symbolic constants indicating whether front-facing and back-facing polygons
        /// are rasterized as points, lines, or filled polygons. The initial value is Gl.FILL. See Gl.PolygonMode.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        public const int POLYGON_MODE = 0x0B40;

        /// <summary>
        /// [GL4] Gl.Get: data returns a single boolean value indicating whether antialiasing of polygons is enabled. The initial
        /// value is Gl.FALSE. See Gl.PolygonMode.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int POLYGON_SMOOTH = 0x0B41;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns a single boolean value indicating whether polygon culling is enabled. The initial
        /// value is Gl.FALSE. See Gl.CullFace.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int CULL_FACE = 0x0B44;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns a single value indicating the mode of polygon culling. The initial value is Gl.BACK.
        /// See Gl.CullFace.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int CULL_FACE_MODE = 0x0B45;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, a symbolic constant indicating whether clockwise or counterclockwise
        ///     polygon
        ///     winding is treated as front-facing. The initial value is Gl.CCW. See Gl.FrontFace.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns a single value indicating the winding order of polygon front faces. The initial
        ///     value is
        ///     Gl.CCW. See Gl.FrontFace.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int FRONT_FACE = 0x0B46;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns two values: the near and far mapping limits for the depth buffer. Integer values, if
        ///     requested, are linearly mapped from the internal floating-point representation such that 1.0 returns the most
        ///     positive
        ///     representable integer value, and -1.0 returns the most negative representable integer value. The initial value is
        ///     (0,
        ///     1). See Gl.DepthRange.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns two values: the near and far mapping limits for the depth buffer. Integer values, if
        ///     requested, are linearly mapped from the internal floating-point representation such that 1.0 returns the most
        ///     positive
        ///     representable integer value, and -1.0 returns the most negative representable integer value. The initial value is
        ///     (0,
        ///     1). See Gl.DepthRangef.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        public const int DEPTH_RANGE = 0x0B70;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns a single boolean value indicating whether depth testing of fragments is enabled. The
        ///     initial
        ///     value is Gl.FALSE. See Gl.DepthFunc and Gl.DepthRange.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns a single boolean value indicating whether depth testing of fragments is enabled. The
        ///     initial value is Gl.FALSE. See Gl.DepthFunc and Gl.DepthRangef.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int DEPTH_TEST = 0x0B71;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns a single boolean value indicating if the depth buffer is enabled for writing. The
        /// initial value is Gl.TRUE. See Gl.DepthMask.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int DEPTH_WRITEMASK = 0x0B72;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the value that is used to clear the depth buffer. Integer values, if
        ///     requested,
        ///     are linearly mapped from the internal floating-point representation such that 1.0 returns the most positive
        ///     representable integer value, and -1.0 returns the most negative representable integer value. The initial value is
        ///     1. See
        ///     Gl.ClearDepth.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the value that is used to clear the depth buffer. Integer values, if
        ///     requested, are linearly mapped from the internal floating-point representation such that 1.0 returns the most
        ///     positive
        ///     representable integer value, and -1.0 returns the most negative representable integer value. The initial value is
        ///     1. See
        ///     Gl.ClearDepthf.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int DEPTH_CLEAR_VALUE = 0x0B73;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the symbolic constant that indicates the depth comparison function. The
        /// initial value is Gl.LESS. See Gl.DepthFunc.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int DEPTH_FUNC = 0x0B74;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns a single boolean value indicating whether stencil testing of fragments is enabled.
        /// The initial value is Gl.FALSE. See Gl.StencilFunc and Gl.StencilOp.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_TEST = 0x0B90;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the index to which the stencil bitplanes are cleared. The initial value
        /// is
        /// 0. See Gl.ClearStencil.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_CLEAR_VALUE = 0x0B91;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating what function is used to compare the
        /// stencil reference value with the stencil buffer value. The initial value is Gl.ALWAYS. See Gl.StencilFunc. This stencil
        /// state only affects non-polygons and front-facing polygons. Back-facing polygons use separate stencil state. See
        /// Gl.StencilFuncSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_FUNC = 0x0B92;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the mask that is used to mask both the stencil reference value and the
        /// stencil buffer value before they are compared. The initial value is all 1's. See Gl.StencilFunc. This stencil state
        /// only
        /// affects non-polygons and front-facing polygons. Back-facing polygons use separate stencil state. See
        /// Gl.StencilFuncSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_VALUE_MASK = 0x0B93;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating what action is taken when the stencil test
        /// fails. The initial value is Gl.KEEP. See Gl.StencilOp. This stencil state only affects non-polygons and front-facing
        /// polygons. Back-facing polygons use separate stencil state. See Gl.StencilOpSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_FAIL = 0x0B94;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating what action is taken when the stencil test
        /// passes, but the depth test fails. The initial value is Gl.KEEP. See Gl.StencilOp. This stencil state only affects
        /// non-polygons and front-facing polygons. Back-facing polygons use separate stencil state. See Gl.StencilOpSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_PASS_DEPTH_FAIL = 0x0B95;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating what action is taken when the stencil test
        /// passes and the depth test passes. The initial value is Gl.KEEP. See Gl.StencilOp. This stencil state only affects
        /// non-polygons and front-facing polygons. Back-facing polygons use separate stencil state. See Gl.StencilOpSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_PASS_DEPTH_PASS = 0x0B96;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the reference value that is compared with the contents of the stencil
        /// buffer. The initial value is 0. See Gl.StencilFunc. This stencil state only affects non-polygons and front-facing
        /// polygons. Back-facing polygons use separate stencil state. See Gl.StencilFuncSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_REF = 0x0B97;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the mask that controls writing of the stencil bitplanes. The initial
        /// value
        /// is all 1's. See Gl.StencilMask. This stencil state only affects non-polygons and front-facing polygons. Back-facing
        /// polygons use separate stencil state. See Gl.StencilMaskSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_WRITEMASK = 0x0B98;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: When used with non-indexed variants of glGet (such as glGetIntegerv), data returns four values: the x
        ///     and
        ///     y window coordinates of the viewport, followed by its width and height. Initially the x and y window coordinates
        ///     are
        ///     both set to 0, and the width and height are set to the width and height of the window into which the GL will do its
        ///     rendering. See Gl.Viewport. When used with indexed variants of glGet (such as glGetIntegeri_v), data returns four
        ///     values: the x and y window coordinates of the indexed viewport, followed by its width and height. Initially the x
        ///     and y
        ///     window coordinates are both set to 0, and the width and height are set to the width and height of the window into
        ///     which
        ///     the GL will do its rendering. See glViewportIndexedf.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns four values: the x and y window coordinates of the viewport, followed by its width
        ///     and
        ///     height. Initially the x and y window coordinates are both set to 0, and the width and height are set to the width
        ///     and
        ///     height of the window into which the GL will do its rendering. See Gl.Viewport.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        public const int VIEWPORT = 0x0BA2;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns a single boolean value indicating whether dithering of fragment colors and indices
        /// is
        /// enabled. The initial value is Gl.TRUE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int DITHER = 0x0BD0;

        /// <summary>
        /// [GLES1.1] Gl.Get: params returns one value, the symbolic constant identifying the destination blend function. See
        /// Gl.BlendFunc.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int BLEND_DST = 0x0BE0;

        /// <summary>
        /// [GLES1.1] Gl.Get: params returns one value, the symbolic constant identifying the source blend function. See
        /// Gl.BlendFunc.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int BLEND_SRC = 0x0BE1;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns a single boolean value indicating whether blending is enabled. The initial value is
        /// Gl.FALSE. See Gl.BlendFunc.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int BLEND = 0x0BE2;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, a symbolic constant indicating the selected logic operation mode. The initial
        ///     value is Gl.COPY. See Gl.LogicOp.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, a symbolic constant indicating the selected logic operation mode. See
        ///     Gl.LogicOp.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int LOGIC_OP_MODE = 0x0BF0;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, a symbolic constant indicating which buffers are being drawn to. See
        ///     Gl.DrawBuffer. The initial value is Gl.BACK if there are back buffers, otherwise it is Gl.FRONT.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating which buffers are being drawn to by the
        ///     corresponding output color. See Gl.DrawBuffers. The initial value of Gl.DRAW_BUFFER0 is Gl.BACK. The initial values
        ///     of
        ///     draw buffers for all other output colors is Gl.NONE.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_EXT_multiview_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER = 0x0C01;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, a symbolic constant indicating which color buffer is selected for reading.
        ///     The
        ///     initial value is Gl.BACK if there is a back buffer, otherwise it is Gl.FRONT. See Gl.ReadPixels.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating which color buffer is selected for
        ///     reading. The
        ///     initial value is Gl.BACK. See Gl.ReadPixels.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_multiview_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_read_buffer", Api = "gles2")]
        public const int READ_BUFFER = 0x0C02;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns four values: the x and y window coordinates of the scissor box, followed by its
        /// width
        /// and height. Initially the x and y window coordinates are both 0 and the width and height are set to the size of the
        /// window. See Gl.Scissor.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        public const int SCISSOR_BOX = 0x0C10;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns a single boolean value indicating whether scissoring is enabled. The initial value
        /// is
        /// Gl.FALSE. See Gl.Scissor.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        public const int SCISSOR_TEST = 0x0C11;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns four values: the red, green, blue, and alpha values used to clear the color buffers.
        /// Integer values, if requested, are linearly mapped from the internal floating-point representation such that 1.0 returns
        /// the most positive representable integer value, and -1.0 returns the most negative representable integer value. The
        /// initial value is (0, 0, 0, 0). See Gl.ClearColor.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int COLOR_CLEAR_VALUE = 0x0C22;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns four boolean values: the red, green, blue, and alpha write enables for the color
        /// buffers. The initial value is (Gl.TRUE, Gl.TRUE, Gl.TRUE, Gl.TRUE). See Gl.ColorMask.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        public const int COLOR_WRITEMASK = 0x0C23;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns a single boolean value indicating whether double buffering is supported.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.GetFramebufferParameter: param returns a boolean value indicating whether double buffering is supported
        ///     for the
        ///     framebuffer object.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int DOUBLEBUFFER = 0x0C32;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns a single boolean value indicating whether stereo buffers (left and right) are supported.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.GetFramebufferParameter: param returns a boolean value indicating whether stereo buffers (left and right)
        ///     are
        ///     supported for the framebuffer object.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int STEREO = 0x0C33;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, a symbolic constant indicating the mode of the line antialiasing hint. The
        ///     initial
        ///     value is Gl.DONT_CARE. See Gl.Hint.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.Hint: Indicates the sampling quality of antialiased lines. If a larger filter function is applied, hinting
        ///     Gl.NICEST can result in more pixel fragments being generated during rasterization.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, a symbolic constant indicating the mode of the line antialiasing hint.
        ///     See
        ///     Gl.Hint.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Hint: Indicates the sampling quality of antialiased lines. If a larger filter function is applied,
        ///     hinting
        ///     Gl.NICEST can result in more pixel fragments being generated during rasterization.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int LINE_SMOOTH_HINT = 0x0C52;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, a symbolic constant indicating the mode of the polygon antialiasing hint. The
        ///     initial value is Gl.DONT_CARE. See Gl.Hint.
        ///     </para>
        ///     <para>
        ///     [GL4] Gl.Hint: Indicates the sampling quality of antialiased polygons. Hinting Gl.NICEST can result in more pixel
        ///     fragments being generated during rasterization, if a larger filter function is applied.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int POLYGON_SMOOTH_HINT = 0x0C53;

        /// <summary>
        /// [GL4] Gl.Get: data returns a single boolean value indicating whether the bytes of two-byte and four-byte pixel indices
        /// and components are swapped after being read from memory. The initial value is Gl.FALSE. See Gl.PixelStore.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int UNPACK_SWAP_BYTES = 0x0CF0;

        /// <summary>
        /// [GL4] Gl.Get: data returns a single boolean value indicating whether single-bit pixels being read from memory are read
        /// first from the least significant bit of each unsigned byte. The initial value is Gl.FALSE. See Gl.PixelStore.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int UNPACK_LSB_FIRST = 0x0CF1;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the row length used for reading pixel data from memory. The initial value is
        ///     0.
        ///     See Gl.PixelStore.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the row length used for reading pixel data from memory. The initial value
        ///     is
        ///     0. See Gl.PixelStorei.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        public const int UNPACK_ROW_LENGTH = 0x0CF2;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the number of rows of pixel locations skipped before the first pixel is read
        ///     from
        ///     memory. The initial value is 0. See Gl.PixelStore.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the number of rows of pixel locations skipped before the first pixel is
        ///     read
        ///     from memory. The initial value is 0. See Gl.PixelStorei.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        public const int UNPACK_SKIP_ROWS = 0x0CF3;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the number of pixel locations skipped before the first pixel is read from
        ///     memory.
        ///     The initial value is 0. See Gl.PixelStore.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the number of pixel locations skipped before the first pixel is read from
        ///     memory. The initial value is 0. See Gl.PixelStorei.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        public const int UNPACK_SKIP_PIXELS = 0x0CF4;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the byte alignment used for reading pixel data from memory. The initial value
        ///     is
        ///     4. See Gl.PixelStore.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the byte alignment used for reading pixel data from memory. The initial
        ///     value
        ///     is 4. See Gl.PixelStorei.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int UNPACK_ALIGNMENT = 0x0CF5;

        /// <summary>
        /// [GL4] Gl.Get: data returns a single boolean value indicating whether the bytes of two-byte and four-byte pixel indices
        /// and components are swapped before being written to memory. The initial value is Gl.FALSE. See Gl.PixelStore.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int PACK_SWAP_BYTES = 0x0D00;

        /// <summary>
        /// [GL4] Gl.Get: data returns a single boolean value indicating whether single-bit pixels being written to memory are
        /// written first to the least significant bit of each unsigned byte. The initial value is Gl.FALSE. See Gl.PixelStore.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] public const int PACK_LSB_FIRST = 0x0D01;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the row length used for writing pixel data to memory. The initial value is 0.
        ///     See
        ///     Gl.PixelStore.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the row length used for writing pixel data to memory. The initial value
        ///     is 0.
        ///     See Gl.PixelStorei.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        public const int PACK_ROW_LENGTH = 0x0D02;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the number of rows of pixel locations skipped before the first pixel is
        ///     written
        ///     into memory. The initial value is 0. See Gl.PixelStore.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the number of rows of pixel locations skipped before the first pixel is
        ///     written into memory. The initial value is 0. See Gl.PixelStorei.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        public const int PACK_SKIP_ROWS = 0x0D03;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the number of pixel locations skipped before the first pixel is written into
        ///     memory. The initial value is 0. See Gl.PixelStore.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the number of pixel locations skipped before the first pixel is written
        ///     into
        ///     memory. The initial value is 0. See Gl.PixelStorei.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        public const int PACK_SKIP_PIXELS = 0x0D04;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the byte alignment used for writing pixel data to memory. The initial value
        ///     is 4.
        ///     See Gl.PixelStore.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the byte alignment used for writing pixel data to memory. The initial
        ///     value is
        ///     4. See Gl.PixelStorei.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int PACK_ALIGNMENT = 0x0D05;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value. The value gives a rough estimate of the largest texture that the GL can
        ///     handle.
        ///     The value must be at least 1024. Use a proxy texture target such as Gl.PROXY_TEXTURE_1D or Gl.PROXY_TEXTURE_2D to
        ///     determine if a texture is too large. See Gl.TexImage1D and Gl.TexImage2D.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value. The value gives a rough estimate of the largest texture that the GL can
        ///     handle. The value must be at least 2048. See Gl.TexImage2D.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int MAX_TEXTURE_SIZE = 0x0D33;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns two values: the maximum supported width and height of the viewport. These must be at
        /// least as large as the visible dimensions of the display being rendered to. See Gl.Viewport.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int MAX_VIEWPORT_DIMS = 0x0D3A;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, an estimate of the number of bits of subpixel resolution that are used to
        /// position rasterized geometry in window coordinates. The value must be at least 4.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int SUBPIXEL_BITS = 0x0D50;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Enable: If enabled and no fragment shader is active, one-dimensional texturing is performed (unless two-
        ///     or
        ///     three-dimensional or cube-mapped texturing is also enabled). See Gl.TexImage1D.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether 1D texture mapping is enabled. The initial
        ///     value is Gl.FALSE. See Gl.TexImage1D.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        public const int TEXTURE_1D = 0x0DE0;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Enable: If enabled and no fragment shader is active, two-dimensional texturing is performed (unless
        ///     three-dimensional or cube-mapped texturing is also enabled). See Gl.TexImage2D.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether 2D texture mapping is enabled. The initial
        ///     value is Gl.FALSE. See Gl.TexImage2D.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Enable: If enabled, two-dimensional texturing is performed for the active texture unit. See
        ///     Gl.ActiveTexture, Gl.TexImage2D, Gl.CompressedTexImage2D, and Gl.CopyTexImage2D.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns a single boolean value indicating whether 2D texturing is enabled. The initial
        ///     value is
        ///     Gl.FALSE. See Gl.TexImage2D.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        public const int TEXTURE_2D = 0x0DE1;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetTexLevelParameter: params returns a single value, the width of the texture image. The initial value
        /// is 0.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public const int TEXTURE_WIDTH = 0x1000;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetTexLevelParameter: params returns a single value, the height of the texture image. The initial
        /// value
        /// is 0.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public const int TEXTURE_HEIGHT = 0x1001;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetSamplerParameter: Returns four integer or floating-point numbers that comprise the RGBA color
        ///     of the
        ///     texture border. Floating-point values are returned in the range 01. Integer values are returned as a linear mapping
        ///     of
        ///     the internal floating-point representation such that 1.0 maps to the most positive representable integer and -1.0
        ///     maps
        ///     to the most negative representable integer. The initial value is (0, 0, 0, 0).
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetTexParameter: Returns four integer or floating-point numbers that comprise the RGBA color of
        ///     the
        ///     texture border. Floating-point values are returned in the range 01. Integer values are returned as a linear mapping
        ///     of
        ///     the internal floating-point representation such that 1.0 maps to the most positive representable integer and -1.0
        ///     maps
        ///     to the most negative representable integer. The initial value is (0, 0, 0, 0).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_NV_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_OES_texture_border_clamp", Api = "gles2")]
        public const int TEXTURE_BORDER_COLOR = 0x1004;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Hint: No preference.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int DONT_CARE = 0x1100;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Hint: The most efficient option should be chosen.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int FASTEST = 0x1101;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Hint: The most correct, or highest quality, option should be chosen.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int NICEST = 0x1102;

        /// <summary>
        /// [GL2.1] Gl.CallLists: lists is treated as an array of signed bytes, each in the range -128 through 127.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_OES_byte_coordinates", Api = "gl|gles1")]
        public const int BYTE = 0x1400;

        /// <summary>
        /// [GL2.1] Gl.CallLists: lists is treated as an array of unsigned bytes, each in the range 0 through 255.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int UNSIGNED_BYTE = 0x1401;

        /// <summary>
        /// [GL2.1] Gl.CallLists: lists is treated as an array of signed two-byte integers, each in the range -32768 through 32767.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        public const int SHORT = 0x1402;

        /// <summary>
        /// [GL2.1] Gl.CallLists: lists is treated as an array of unsigned two-byte integers, each in the range 0 through 65535.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        public const int UNSIGNED_SHORT = 0x1403;

        /// <summary>
        /// [GL2.1] Gl.CallLists: lists is treated as an array of signed four-byte integers.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int INT = 0x1404;

        /// <summary>
        /// [GL2.1] Gl.CallLists: lists is treated as an array of unsigned four-byte integers.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_element_index_uint", Api = "gles1|gles2")]
        public const int UNSIGNED_INT = 0x1405;

        /// <summary>
        /// [GL2.1] Gl.CallLists: lists is treated as an array of four-byte floating-point values.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        public const int FLOAT = 0x1406;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.GetError: An attempt has been made to perform an operation that would cause an internal stack to overflow.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.GetError: This command would cause a stack overflow. The offending command is ignored, and has no
        ///     other
        ///     side effect than to set the error flag.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        public const int STACK_OVERFLOW = 0x0503;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.GetError: An attempt has been made to perform an operation that would cause an internal stack to
        ///     underflow.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.GetError: This command would cause a stack underflow. The offending command is ignored, and has no
        ///     other
        ///     side effect than to set the error flag.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        public const int STACK_UNDERFLOW = 0x0504;

        /// <summary>
        /// [GL] Value of GL_CLEAR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int CLEAR = 0x1500;

        /// <summary>
        /// [GL] Value of GL_AND symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int AND = 0x1501;

        /// <summary>
        /// [GL] Value of GL_AND_REVERSE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int AND_REVERSE = 0x1502;

        /// <summary>
        /// [GL] Value of GL_COPY symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int COPY = 0x1503;

        /// <summary>
        /// [GL] Value of GL_AND_INVERTED symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int AND_INVERTED = 0x1504;

        /// <summary>
        /// [GL] Value of GL_NOOP symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int NOOP = 0x1505;

        /// <summary>
        /// [GL] Value of GL_XOR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int XOR = 0x1506;

        /// <summary>
        /// [GL] Value of GL_OR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int OR = 0x1507;

        /// <summary>
        /// [GL] Value of GL_NOR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int NOR = 0x1508;

        /// <summary>
        /// [GL] Value of GL_EQUIV symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int EQUIV = 0x1509;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOp: Bitwise inverts the current stencil buffer value.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOpSeparate: Bitwise inverts the current stencil buffer value.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int INVERT = 0x150A;

        /// <summary>
        /// [GL] Value of GL_OR_REVERSE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int OR_REVERSE = 0x150B;

        /// <summary>
        /// [GL] Value of GL_COPY_INVERTED symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int COPY_INVERTED = 0x150C;

        /// <summary>
        /// [GL] Value of GL_OR_INVERTED symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int OR_INVERTED = 0x150D;

        /// <summary>
        /// [GL] Value of GL_NAND symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int NAND = 0x150E;

        /// <summary>
        /// [GL] Value of GL_SET symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int SET = 0x150F;

        /// <summary>
        /// [GL2.1|GLES1.1] Gl.MatrixMode: Applies subsequent matrix operations to the texture matrix stack.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int TEXTURE = 0x1702;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.CopyPixels: Indices or RGBA colors are read from the buffer currently specified as the read source
        ///     buffer
        ///     (see Gl.ReadBuffer). If the GL is in color index mode, each index that is read from this buffer is converted to a
        ///     fixed-point format with an unspecified number of bits to the right of the binary point. Each index is then shifted
        ///     left
        ///     by Gl.INDEX_SHIFT bits, and added to Gl.INDEX_OFFSET. If Gl.INDEX_SHIFT is negative, the shift is to the right. In
        ///     either case, zero bits fill otherwise unspecified bit locations in the result. If Gl.MAP_COLOR is true, the index
        ///     is
        ///     replaced with the value that it references in lookup table Gl.PIXEL_MAP_I_TO_I. Whether the lookup replacement of
        ///     the
        ///     index is done or not, the integer part of the index is then ANDed with 2b-1, where b is the number of bits in a
        ///     color
        ///     index buffer. If the GL is in RGBA mode, the red, green, blue, and alpha components of each pixel that is read are
        ///     converted to an internal floating-point format with unspecified precision. The conversion maps the largest
        ///     representable
        ///     component value to 1.0, and component value 0 to 0.0. The resulting floating-point color values are then multiplied
        ///     by
        ///     Gl.c_SCALE and added to Gl.c_BIAS, where c is RED, GREEN, BLUE, and ALPHA for the respective color components. The
        ///     results are clamped to the range [0,1]. If Gl.MAP_COLOR is true, each color component is scaled by the size of
        ///     lookup
        ///     table Gl.PIXEL_MAP_c_TO_c, then replaced by the value that it references in that table. c is R, G, B, or A. If the
        ///     ARB_imaging extension is supported, the color values may be additionally processed by color-table lookups,
        ///     color-matrix
        ///     transformations, and convolution filters. The GL then converts the resulting indices or RGBA colors to fragments by
        ///     attaching the current raster position z coordinate and texture coordinates to each pixel, then assigning window
        ///     coordinates xr+iyr+j, where xryr is the current raster position, and the pixel was the ith pixel in the jth row.
        ///     These
        ///     pixel fragments are then treated just like the fragments generated by rasterizing points, lines, or polygons.
        ///     Texture
        ///     mapping, fog, and all the fragment operations are applied before the fragments are written to the frame buffer.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.MatrixMode: Applies subsequent matrix operations to the color matrix stack.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        public const int COLOR = 0x1800;

        /// <summary>
        /// [GL2.1] Gl.CopyPixels: Depth values are read from the depth buffer and converted directly to an internal floating-point
        /// format with unspecified precision. The resulting floating-point depth value is then multiplied by Gl.DEPTH_SCALE and
        /// added to Gl.DEPTH_BIAS. The result is clamped to the range [0,1]. The GL then converts the resulting depth components
        /// to
        /// fragments by attaching the current raster position color or color index and texture coordinates to each pixel, then
        /// assigning window coordinates xr+iyr+j, where xryr is the current raster position, and the pixel was the ith pixel in
        /// the
        /// jth row. These pixel fragments are then treated just like the fragments generated by rasterizing points, lines, or
        /// polygons. Texture mapping, fog, and all the fragment operations are applied before the fragments are written to the
        /// frame buffer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        public const int DEPTH = 0x1801;

        /// <summary>
        /// [GL2.1] Gl.CopyPixels: Stencil indices are read from the stencil buffer and converted to an internal fixed-point format
        /// with an unspecified number of bits to the right of the binary point. Each fixed-point index is then shifted left by
        /// Gl.INDEX_SHIFT bits, and added to Gl.INDEX_OFFSET. If Gl.INDEX_SHIFT is negative, the shift is to the right. In either
        /// case, zero bits fill otherwise unspecified bit locations in the result. If Gl.MAP_STENCIL is true, the index is
        /// replaced
        /// with the value that it references in lookup table Gl.PIXEL_MAP_S_TO_S. Whether the lookup replacement of the index is
        /// done or not, the integer part of the index is then ANDed with 2b-1, where b is the number of bits in the stencil
        /// buffer.
        /// The resulting stencil indices are then written to the stencil buffer such that the index read from the ith location of
        /// the jth row is written to location xr+iyr+j, where xryr is the current raster position. Only the pixel ownership test,
        /// the scissor test, and the stencil writemask affect these write operations.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        public const int STENCIL = 0x1802;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.DrawPixels: Each pixel is a single value, a stencil index. It is converted to fixed-point format, with
        ///     an
        ///     unspecified number of bits to the right of the binary point, regardless of the memory data type. Floating-point
        ///     values
        ///     convert to true fixed-point values. Signed and unsigned integer data is converted with all fraction bits set to 0.
        ///     Bitmap data convert to either 0 or 1. Each fixed-point index is then shifted left by Gl.INDEX_SHIFT bits, and added
        ///     to
        ///     Gl.INDEX_OFFSET. If Gl.INDEX_SHIFT is negative, the shift is to the right. In either case, zero bits fill otherwise
        ///     unspecified bit locations in the result. If Gl.MAP_STENCIL is true, the index is replaced with the value that it
        ///     references in lookup table Gl.PIXEL_MAP_S_TO_S. Whether the lookup replacement of the index is done or not, the
        ///     integer
        ///     part of the index is then ANDed with 2b-1, where b is the number of bits in the stencil buffer. The resulting
        ///     stencil
        ///     indices are then written to the stencil buffer such that the nth index is written to location
        ///     xn=xr+n%widthyn=yr+nwidth
        ///     where xryr is the current raster position. Only the pixel ownership test, the scissor test, and the stencil
        ///     writemask
        ///     affect these write operations.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.ReadPixels: Stencil values are read from the stencil buffer. Each index is converted to fixed point,
        ///     shifted
        ///     left or right depending on the value and sign of Gl.INDEX_SHIFT, and added to Gl.INDEX_OFFSET. If Gl.MAP_STENCIL is
        ///     Gl.TRUE, indices are replaced by their mappings in the table Gl.PIXEL_MAP_S_TO_S.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_stencil8", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_texture_stencil8", Api = "gles2")]
        public const int STENCIL_INDEX = 0x1901;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.DrawPixels: Each pixel is a single-depth component. Floating-point data is converted directly to an
        ///     internal
        ///     floating-point format with unspecified precision. Signed integer data is mapped linearly to the internal
        ///     floating-point
        ///     format such that the most positive representable integer value maps to 1.0, and the most negative representable
        ///     value
        ///     maps to -1.0. Unsigned integer data is mapped similarly: the largest integer value maps to 1.0, and 0 maps to 0.0.
        ///     The
        ///     resulting floating-point depth value is then multiplied by Gl.DEPTH_SCALE and added to Gl.DEPTH_BIAS. The result is
        ///     clamped to the range 01. The GL then converts the resulting depth components to fragments by attaching the current
        ///     raster position color or color index and texture coordinates to each pixel, then assigning x and y window
        ///     coordinates to
        ///     the nth fragment such that xn=xr+n%widthyn=yr+nwidth where xryr is the current raster position. These pixel
        ///     fragments
        ///     are then treated just like the fragments generated by rasterizing points, lines, or polygons. Texture mapping, fog,
        ///     and
        ///     all the fragment operations are applied before the fragments are written to the frame buffer.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.ReadPixels: Depth values are read from the depth buffer. Each component is converted to floating point
        ///     such
        ///     that the minimum depth value maps to 0 and the maximum value maps to 1. Each component is then multiplied by
        ///     Gl.DEPTH_SCALE, added to Gl.DEPTH_BIAS, and finally clamped to the range 01.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage1D: Each element is a single depth value. The GL converts it to floating point, multiplies by
        ///     the
        ///     signed scale factor Gl.DEPTH_SCALE, adds the signed bias Gl.DEPTH_BIAS, and clamps to the range [0,1] (see
        ///     Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage2D: Each element is a single depth value. The GL converts it to floating point, multiplies by
        ///     the
        ///     signed scale factor Gl.DEPTH_SCALE, adds the signed bias Gl.DEPTH_BIAS, and clamps to the range [0,1] (see
        ///     Gl.PixelTransfer).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        public const int DEPTH_COMPONENT = 0x1902;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.DrawPixels: Each pixel is a single red component. This component is converted to the internal
        ///     floating-point
        ///     format in the same way the red component of an RGBA pixel is. It is then converted to an RGBA pixel with green and
        ///     blue
        ///     set to 0, and alpha set to 1. After this conversion, the pixel is treated as if it had been read as an RGBA pixel.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage1D: Each element is a single red component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for green and blue, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage2D: Each element is a single red component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for green and blue, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage3D: Each element is a single red component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for green and blue, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_EXT_texture_rg", Api = "gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int RED = 0x1903;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.DrawPixels: Each pixel is a single green component. This component is converted to the internal
        ///     floating-point format in the same way the green component of an RGBA pixel is. It is then converted to an RGBA
        ///     pixel
        ///     with red and blue set to 0, and alpha set to 1. After this conversion, the pixel is treated as if it had been read
        ///     as an
        ///     RGBA pixel.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage1D: Each element is a single green component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red and blue, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage2D: Each element is a single green component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red and blue, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage3D: Each element is a single green component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red and blue, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int GREEN = 0x1904;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.DrawPixels: Each pixel is a single blue component. This component is converted to the internal
        ///     floating-point
        ///     format in the same way the blue component of an RGBA pixel is. It is then converted to an RGBA pixel with red and
        ///     green
        ///     set to 0, and alpha set to 1. After this conversion, the pixel is treated as if it had been read as an RGBA pixel.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage1D: Each element is a single blue component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red and green, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage2D: Each element is a single blue component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red and green, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage3D: Each element is a single blue component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red and green, and 1 for alpha. Each component is then multiplied by the
        ///     signed
        ///     scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int BLUE = 0x1905;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.DrawPixels: Each pixel is a single alpha component. This component is converted to the internal
        ///     floating-point format in the same way the alpha component of an RGBA pixel is. It is then converted to an RGBA
        ///     pixel
        ///     with red, green, and blue set to 0. After this conversion, the pixel is treated as if it had been read as an RGBA
        ///     pixel.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage1D: Each element is a single alpha component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red, green, and blue. Each component is then multiplied by the signed scale
        ///     factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage2D: Each element is a single alpha component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red, green, and blue. Each component is then multiplied by the signed scale
        ///     factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage3D: Each element is a single alpha component. The GL converts it to floating point and assembles
        ///     it
        ///     into an RGBA element by attaching 0 for red, green, and blue. Each component is then multiplied by the signed scale
        ///     factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.TexImage2D: Each element is a single alpha component. The GL converts it to floating point and
        ///     assembles it
        ///     into an RGBA element by attaching 0 for red, green, and blue.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        public const int ALPHA = 0x1906;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.DrawPixels:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage1D:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage2D:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage3D:
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.TexImage2D: Each element is an RGB triple. The GL converts it to fixed-point or floating-point and
        ///     assembles it into an RGBA element by attaching 1 for alpha.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int RGB = 0x1907;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.DrawPixels:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage1D:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage2D:
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage3D:
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.ReadPixels:
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.TexImage2D: Each element contains all four components. The GL converts it to fixed-point or
        ///     floating-point.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int RGBA = 0x1908;

        /// <summary>
        /// [GL4] Gl.PolygonMode: Polygon vertices that are marked as the start of a boundary edge are drawn as points. Point
        /// attributes such as Gl.POINT_SIZE and Gl.POINT_SMOOTH control the rasterization of the points. Polygon rasterization
        /// attributes other than Gl.POLYGON_MODE have no effect.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        public const int POINT = 0x1B00;

        /// <summary>
        /// [GL4] Gl.PolygonMode: Boundary edges of the polygon are drawn as line segments. Line attributes such as Gl.LINE_WIDTH
        /// and Gl.LINE_SMOOTH control the rasterization of the lines. Polygon rasterization attributes other than Gl.POLYGON_MODE
        /// have no effect.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        public const int LINE = 0x1B01;

        /// <summary>
        /// [GL4] Gl.PolygonMode: The interior of the polygon is filled. Polygon attributes such as Gl.POLYGON_SMOOTH control the
        /// rasterization of the polygon.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        public const int FILL = 0x1B02;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOp: Keeps the current value.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOpSeparate: Keeps the current value.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int KEEP = 0x1E00;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOp: Sets the stencil buffer value to ref, as specified by Gl.StencilFunc.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOpSeparate: Sets the stencil buffer value to ref, as specified by Gl.StencilFunc.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int REPLACE = 0x1E01;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOp: Increments the current stencil buffer value. Clamps to the maximum representable
        ///     unsigned
        ///     value.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOpSeparate: Increments the current stencil buffer value. Clamps to the maximum
        ///     representable
        ///     unsigned value.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int INCR = 0x1E02;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOp: Decrements the current stencil buffer value. Clamps to 0.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.StencilOpSeparate: Decrements the current stencil buffer value. Clamps to 0.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int DECR = 0x1E03;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetString: Returns the company responsible for this GL implementation. This name does not change from
        /// release to release.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int VENDOR = 0x1F00;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetString: Returns the name of the renderer. This name is typically specific to a particular
        /// configuration of a hardware platform. It does not change from release to release.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int RENDERER = 0x1F01;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetString: Returns a version or release number.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int VERSION = 0x1F02;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.GetString: For glGetStringi only, returns the extension string supported by the implementation at index.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.GetString: Returns the extension string supported by the implementation.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int EXTENSIONS = 0x1F03;

        /// <summary>
        /// [GL] Value of GL_NEAREST symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int NEAREST = 0x2600;

        /// <summary>
        /// [GL] Value of GL_LINEAR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LINEAR = 0x2601;

        /// <summary>
        /// [GL] Value of GL_NEAREST_MIPMAP_NEAREST symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int NEAREST_MIPMAP_NEAREST = 0x2700;

        /// <summary>
        /// [GL] Value of GL_LINEAR_MIPMAP_NEAREST symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LINEAR_MIPMAP_NEAREST = 0x2701;

        /// <summary>
        /// [GL] Value of GL_NEAREST_MIPMAP_LINEAR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int NEAREST_MIPMAP_LINEAR = 0x2702;

        /// <summary>
        /// [GL] Value of GL_LINEAR_MIPMAP_LINEAR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LINEAR_MIPMAP_LINEAR = 0x2703;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetSamplerParameter: Returns the single-valued texture magnification filter, a symbolic constant.
        ///     The
        ///     initial value is Gl.LINEAR.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetTexParameter: Returns the single-valued texture magnification filter, a symbolic constant. The
        ///     initial value is Gl.LINEAR.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int TEXTURE_MAG_FILTER = 0x2800;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetSamplerParameter: Returns the single-valued texture minification filter, a symbolic constant.
        ///     The
        ///     initial value is Gl.NEAREST_MIPMAP_LINEAR.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetTexParameter: Returns the single-valued texture minification filter, a symbolic constant. The
        ///     initial value is Gl.NEAREST_MIPMAP_LINEAR.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int TEXTURE_MIN_FILTER = 0x2801;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetSamplerParameter: Returns the single-valued wrapping function for texture coordinate s, a
        ///     symbolic
        ///     constant. The initial value is Gl.REPEAT.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetTexParameter: Returns the single-valued wrapping function for texture coordinate s, a symbolic
        ///     constant. The initial value is Gl.REPEAT.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int TEXTURE_WRAP_S = 0x2802;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetSamplerParameter: Returns the single-valued wrapping function for texture coordinate t, a
        ///     symbolic
        ///     constant. The initial value is Gl.REPEAT.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetTexParameter: Returns the single-valued wrapping function for texture coordinate t, a symbolic
        ///     constant. The initial value is Gl.REPEAT.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int TEXTURE_WRAP_T = 0x2803;

        /// <summary>
        /// [GL] Value of GL_REPEAT symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int REPEAT = 0x2901;

        /// <summary>
        ///     <para>
        ///     [GL4] glCullFace: specify whether front- or back-facing facets can be culled
        ///     </para>
        ///     <para>
        ///     [GLES3.2] glCullFace: specify whether front- or back-facing polygons can be culled
        ///     </para>
        /// </summary>
        /// <param name="mode">
        /// Specifies whether front- or back-facing facets are candidates for culling. Symbolic constants Gl.FRONT, Gl.BACK, and
        /// Gl.FRONT_AND_BACK are accepted. The initial value is Gl.BACK.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void CullFace(CullFaceMode mode)
        {
            Debug.Assert(Delegates.pglCullFace != null, "pglCullFace not implemented");
            Delegates.pglCullFace((int) mode);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glFrontFace: define front- and back-facing polygons
        ///     </para>
        /// </summary>
        /// <param name="mode">
        /// Specifies the orientation of front-facing polygons. Gl.CW and Gl.CCW are accepted. The initial value is Gl.CCW.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void FrontFace(FrontFaceDirection mode)
        {
            Debug.Assert(Delegates.pglFrontFace != null, "pglFrontFace not implemented");
            Delegates.pglFrontFace((int) mode);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glHint: specify implementation-specific hints
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies a symbolic constant indicating the behavior to be controlled. Gl.LINE_SMOOTH_HINT, Gl.POLYGON_SMOOTH_HINT,
        /// Gl.TEXTURE_COMPRESSION_HINT, and Gl.FRAGMENT_SHADER_DERIVATIVE_HINT are accepted.
        /// </param>
        /// <param name="mode">
        /// Specifies a symbolic constant indicating the desired behavior. Gl.FASTEST, Gl.NICEST, and Gl.DONT_CARE are accepted.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Hint(HintTarget target, HintMode mode)
        {
            Debug.Assert(Delegates.pglHint != null, "pglHint not implemented");
            Delegates.pglHint((int) target, (int) mode);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES1.1] glLineWidth: specify the width of rasterized lines
        ///     </para>
        /// </summary>
        /// <param name="width">
        /// Specifies the width of rasterized lines. The initial value is 1.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void LineWidth(float width)
        {
            Debug.Assert(Delegates.pglLineWidth != null, "pglLineWidth not implemented");
            Delegates.pglLineWidth(width);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES1.1] glPointSize: specify the diameter of rasterized points
        ///     </para>
        /// </summary>
        /// <param name="size">
        /// Specifies the diameter of rasterized points. The initial value is 1.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        public static void PointSize(float size)
        {
            Debug.Assert(Delegates.pglPointSize != null, "pglPointSize not implemented");
            Delegates.pglPointSize(size);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glPolygonMode: select a polygon rasterization mode
        /// </summary>
        /// <param name="face">
        /// Specifies the polygons that <paramref name="mode" /> applies to. Must be Gl.FRONT_AND_BACK for front- and back-facing
        /// polygons.
        /// </param>
        /// <param name="mode">
        /// Specifies how polygons will be rasterized. Accepted values are Gl.POINT, Gl.LINE, and Gl.FILL. The initial value is
        /// Gl.FILL for both front- and back-facing polygons.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        public static void PolygonMode(MaterialFace face, PolygonMode mode)
        {
            Debug.Assert(Delegates.pglPolygonMode != null, "pglPolygonMode not implemented");
            Delegates.pglPolygonMode((int) face, (int) mode);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glScissor: define the scissor box
        ///     </para>
        /// </summary>
        /// <param name="x">
        /// Specify the lower left corner of the scissor box. Initially (0, 0).
        /// </param>
        /// <param name="y">
        /// Specify the lower left corner of the scissor box. Initially (0, 0).
        /// </param>
        /// <param name="width">
        /// Specify the width and height of the scissor box. When a GL context is first attached to a window,
        /// <paramref
        ///     name="width" />
        /// and <paramref name="height" /> are set to the dimensions of that window.
        /// </param>
        /// <param name="height">
        /// Specify the width and height of the scissor box. When a GL context is first attached to a window,
        /// <paramref
        ///     name="width" />
        /// and <paramref name="height" /> are set to the dimensions of that window.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Scissor(int x, int y, int width, int height)
        {
            Debug.Assert(Delegates.pglScissor != null, "pglScissor not implemented");
            Delegates.pglScissor(x, y, width, height);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexParameterf: set texture parameters
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture, which must be either Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, or Gl.TEXTURE_CUBE_MAP.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a single-valued texture parameter. <paramref name="pname" /> can be one of the
        /// following:
        /// Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MAG_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_BASE_LEVEL,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, Gl.TEXTURE_WRAP_R, Gl.TEXTURE_PRIORITY,
        /// Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC, Gl.DEPTH_TEXTURE_MODE, or Gl.GENERATE_MIPMAP.
        /// </param>
        /// <param name="param">
        /// Specifies the value of <paramref name="pname" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void TexParameter(TextureTarget target, TextureParameterName pname, float param)
        {
            Debug.Assert(Delegates.pglTexParameterf != null, "pglTexParameterf not implemented");
            Delegates.pglTexParameterf((int) target, (int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexParameterfv: set texture parameters
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture, which must be either Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, or Gl.TEXTURE_CUBE_MAP.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a single-valued texture parameter. <paramref name="pname" /> can be one of the
        /// following:
        /// Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MAG_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_BASE_LEVEL,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, Gl.TEXTURE_WRAP_R, Gl.TEXTURE_PRIORITY,
        /// Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC, Gl.DEPTH_TEXTURE_MODE, or Gl.GENERATE_MIPMAP.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:float[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void TexParameter(TextureTarget target, TextureParameterName pname, float[] @params)
        {
            unsafe
            {
                fixed (float* p_params = @params)
                {
                    Debug.Assert(Delegates.pglTexParameterfv != null, "pglTexParameterfv not implemented");
                    Delegates.pglTexParameterfv((int) target, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexParameterfv: set texture parameters
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture, which must be either Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, or Gl.TEXTURE_CUBE_MAP.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a single-valued texture parameter. <paramref name="pname" /> can be one of the
        /// following:
        /// Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MAG_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_BASE_LEVEL,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, Gl.TEXTURE_WRAP_R, Gl.TEXTURE_PRIORITY,
        /// Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC, Gl.DEPTH_TEXTURE_MODE, or Gl.GENERATE_MIPMAP.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:float*" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static unsafe void TexParameter(TextureTarget target, TextureParameterName pname, float* @params)
        {
            Debug.Assert(Delegates.pglTexParameterfv != null, "pglTexParameterfv not implemented");
            Delegates.pglTexParameterfv((int) target, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexParameteri: set texture parameters
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture, which must be either Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, or Gl.TEXTURE_CUBE_MAP.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a single-valued texture parameter. <paramref name="pname" /> can be one of the
        /// following:
        /// Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MAG_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_BASE_LEVEL,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, Gl.TEXTURE_WRAP_R, Gl.TEXTURE_PRIORITY,
        /// Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC, Gl.DEPTH_TEXTURE_MODE, or Gl.GENERATE_MIPMAP.
        /// </param>
        /// <param name="param">
        /// Specifies the value of <paramref name="pname" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void TexParameter(TextureTarget target, TextureParameterName pname, int param)
        {
            Debug.Assert(Delegates.pglTexParameteri != null, "pglTexParameteri not implemented");
            Delegates.pglTexParameteri((int) target, (int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexParameteriv: set texture parameters
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture, which must be either Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, or Gl.TEXTURE_CUBE_MAP.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a single-valued texture parameter. <paramref name="pname" /> can be one of the
        /// following:
        /// Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MAG_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_BASE_LEVEL,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, Gl.TEXTURE_WRAP_R, Gl.TEXTURE_PRIORITY,
        /// Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC, Gl.DEPTH_TEXTURE_MODE, or Gl.GENERATE_MIPMAP.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:int[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void TexParameter(TextureTarget target, TextureParameterName pname, int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglTexParameteriv != null, "pglTexParameteriv not implemented");
                    Delegates.pglTexParameteriv((int) target, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexParameteriv: set texture parameters
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture, which must be either Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, or Gl.TEXTURE_CUBE_MAP.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a single-valued texture parameter. <paramref name="pname" /> can be one of the
        /// following:
        /// Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MAG_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_BASE_LEVEL,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, Gl.TEXTURE_WRAP_R, Gl.TEXTURE_PRIORITY,
        /// Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC, Gl.DEPTH_TEXTURE_MODE, or Gl.GENERATE_MIPMAP.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:int*" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static unsafe void TexParameter(TextureTarget target, TextureParameterName pname, int* @params)
        {
            Debug.Assert(Delegates.pglTexParameteriv != null, "pglTexParameteriv not implemented");
            Delegates.pglTexParameteriv((int) target, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glTexImage1D: specify a one-dimensional texture image
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture. Must be Gl.TEXTURE_1D or Gl.PROXY_TEXTURE_1D.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="internalFormat">
        /// Specifies the number of color components in the texture. Must be 1, 2, 3, or 4, or one of the following symbolic
        /// constants: Gl.ALPHA, Gl.ALPHA4, Gl.ALPHA8, Gl.ALPHA12, Gl.ALPHA16, Gl.COMPRESSED_ALPHA, Gl.COMPRESSED_LUMINANCE,
        /// Gl.COMPRESSED_LUMINANCE_ALPHA, Gl.COMPRESSED_INTENSITY, Gl.COMPRESSED_RGB, Gl.COMPRESSED_RGBA, Gl.DEPTH_COMPONENT,
        /// Gl.DEPTH_COMPONENT16, Gl.DEPTH_COMPONENT24, Gl.DEPTH_COMPONENT32, Gl.LUMINANCE, Gl.LUMINANCE4, Gl.LUMINANCE8,
        /// Gl.LUMINANCE12, Gl.LUMINANCE16, Gl.LUMINANCE_ALPHA, Gl.LUMINANCE4_ALPHA4, Gl.LUMINANCE6_ALPHA2, Gl.LUMINANCE8_ALPHA8,
        /// Gl.LUMINANCE12_ALPHA4, Gl.LUMINANCE12_ALPHA12, Gl.LUMINANCE16_ALPHA16, Gl.INTENSITY, Gl.INTENSITY4, Gl.INTENSITY8,
        /// Gl.INTENSITY12, Gl.INTENSITY16, Gl.R3_G3_B2, Gl.RGB, Gl.RGB4, Gl.RGB5, Gl.RGB8, Gl.RGB10, Gl.RGB12, Gl.RGB16, Gl.RGBA,
        /// Gl.RGBA2, Gl.RGBA4, Gl.RGB5_A1, Gl.RGBA8, Gl.RGB10_A2, Gl.RGBA12, Gl.RGBA16, Gl.SLUMINANCE, Gl.SLUMINANCE8,
        /// Gl.SLUMINANCE_ALPHA, Gl.SLUMINANCE8_ALPHA8, Gl.SRGB, Gl.SRGB8, Gl.SRGB_ALPHA, or Gl.SRGB8_ALPHA8.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture image including the border if any. If the GL version does not support
        /// non-power-of-two sizes, this value must be 2n+2⁡border for some integer n. All implementations support texture images
        /// that are at least 64 texels wide. The height of the 1D texture image is 1.
        /// </param>
        /// <param name="border">
        /// Specifies the width of the border. Must be either 0 or 1.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.COLOR_INDEX, Gl.RED, Gl.GREEN,
        /// Gl.BLUE, Gl.ALPHA, Gl.RGB, Gl.BGR, Gl.RGBA, Gl.BGRA, Gl.LUMINANCE, and Gl.LUMINANCE_ALPHA.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.BITMAP, Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2,
        /// Gl.UNSIGNED_BYTE_2_3_3_REV, Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4,
        /// Gl.UNSIGNED_SHORT_4_4_4_4_REV, Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8,
        /// Gl.UNSIGNED_INT_8_8_8_8_REV, Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void TexImage1D(TextureTarget target, int level, InternalFormat internalFormat, int width, int border, PixelFormat format, PixelType type, IntPtr data)
        {
            Debug.Assert(Delegates.pglTexImage1D != null, "pglTexImage1D not implemented");
            Delegates.pglTexImage1D((int) target, level, (int) internalFormat, width, border, (int) format, (int) type, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glTexImage1D: specify a one-dimensional texture image
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture. Must be Gl.TEXTURE_1D or Gl.PROXY_TEXTURE_1D.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="internalFormat">
        /// Specifies the number of color components in the texture. Must be 1, 2, 3, or 4, or one of the following symbolic
        /// constants: Gl.ALPHA, Gl.ALPHA4, Gl.ALPHA8, Gl.ALPHA12, Gl.ALPHA16, Gl.COMPRESSED_ALPHA, Gl.COMPRESSED_LUMINANCE,
        /// Gl.COMPRESSED_LUMINANCE_ALPHA, Gl.COMPRESSED_INTENSITY, Gl.COMPRESSED_RGB, Gl.COMPRESSED_RGBA, Gl.DEPTH_COMPONENT,
        /// Gl.DEPTH_COMPONENT16, Gl.DEPTH_COMPONENT24, Gl.DEPTH_COMPONENT32, Gl.LUMINANCE, Gl.LUMINANCE4, Gl.LUMINANCE8,
        /// Gl.LUMINANCE12, Gl.LUMINANCE16, Gl.LUMINANCE_ALPHA, Gl.LUMINANCE4_ALPHA4, Gl.LUMINANCE6_ALPHA2, Gl.LUMINANCE8_ALPHA8,
        /// Gl.LUMINANCE12_ALPHA4, Gl.LUMINANCE12_ALPHA12, Gl.LUMINANCE16_ALPHA16, Gl.INTENSITY, Gl.INTENSITY4, Gl.INTENSITY8,
        /// Gl.INTENSITY12, Gl.INTENSITY16, Gl.R3_G3_B2, Gl.RGB, Gl.RGB4, Gl.RGB5, Gl.RGB8, Gl.RGB10, Gl.RGB12, Gl.RGB16, Gl.RGBA,
        /// Gl.RGBA2, Gl.RGBA4, Gl.RGB5_A1, Gl.RGBA8, Gl.RGB10_A2, Gl.RGBA12, Gl.RGBA16, Gl.SLUMINANCE, Gl.SLUMINANCE8,
        /// Gl.SLUMINANCE_ALPHA, Gl.SLUMINANCE8_ALPHA8, Gl.SRGB, Gl.SRGB8, Gl.SRGB_ALPHA, or Gl.SRGB8_ALPHA8.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture image including the border if any. If the GL version does not support
        /// non-power-of-two sizes, this value must be 2n+2⁡border for some integer n. All implementations support texture images
        /// that are at least 64 texels wide. The height of the 1D texture image is 1.
        /// </param>
        /// <param name="border">
        /// Specifies the width of the border. Must be either 0 or 1.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.COLOR_INDEX, Gl.RED, Gl.GREEN,
        /// Gl.BLUE, Gl.ALPHA, Gl.RGB, Gl.BGR, Gl.RGBA, Gl.BGRA, Gl.LUMINANCE, and Gl.LUMINANCE_ALPHA.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.BITMAP, Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2,
        /// Gl.UNSIGNED_BYTE_2_3_3_REV, Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4,
        /// Gl.UNSIGNED_SHORT_4_4_4_4_REV, Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8,
        /// Gl.UNSIGNED_INT_8_8_8_8_REV, Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void TexImage1D(TextureTarget target, int level, InternalFormat internalFormat, int width, int border, PixelFormat format, PixelType type, object data)
        {
            GCHandle pin_pixels = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                TexImage1D(target, level, internalFormat, width, border, format, type, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexImage2D: specify a two-dimensional texture image
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture. Must be Gl.TEXTURE_2D, Gl.PROXY_TEXTURE_2D, Gl.TEXTURE_CUBE_MAP_POSITIVE_X,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, or Gl.PROXY_TEXTURE_CUBE_MAP.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="internalFormat">
        /// Specifies the number of color components in the texture. Must be 1, 2, 3, or 4, or one of the following symbolic
        /// constants: Gl.ALPHA, Gl.ALPHA4, Gl.ALPHA8, Gl.ALPHA12, Gl.ALPHA16, Gl.COMPRESSED_ALPHA, Gl.COMPRESSED_LUMINANCE,
        /// Gl.COMPRESSED_LUMINANCE_ALPHA, Gl.COMPRESSED_INTENSITY, Gl.COMPRESSED_RGB, Gl.COMPRESSED_RGBA, Gl.DEPTH_COMPONENT,
        /// Gl.DEPTH_COMPONENT16, Gl.DEPTH_COMPONENT24, Gl.DEPTH_COMPONENT32, Gl.LUMINANCE, Gl.LUMINANCE4, Gl.LUMINANCE8,
        /// Gl.LUMINANCE12, Gl.LUMINANCE16, Gl.LUMINANCE_ALPHA, Gl.LUMINANCE4_ALPHA4, Gl.LUMINANCE6_ALPHA2, Gl.LUMINANCE8_ALPHA8,
        /// Gl.LUMINANCE12_ALPHA4, Gl.LUMINANCE12_ALPHA12, Gl.LUMINANCE16_ALPHA16, Gl.INTENSITY, Gl.INTENSITY4, Gl.INTENSITY8,
        /// Gl.INTENSITY12, Gl.INTENSITY16, Gl.R3_G3_B2, Gl.RGB, Gl.RGB4, Gl.RGB5, Gl.RGB8, Gl.RGB10, Gl.RGB12, Gl.RGB16, Gl.RGBA,
        /// Gl.RGBA2, Gl.RGBA4, Gl.RGB5_A1, Gl.RGBA8, Gl.RGB10_A2, Gl.RGBA12, Gl.RGBA16, Gl.SLUMINANCE, Gl.SLUMINANCE8,
        /// Gl.SLUMINANCE_ALPHA, Gl.SLUMINANCE8_ALPHA8, Gl.SRGB, Gl.SRGB8, Gl.SRGB_ALPHA, or Gl.SRGB8_ALPHA8.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture image including the border if any. If the GL version does not support
        /// non-power-of-two sizes, this value must be 2n+2⁡border for some integer n. All implementations support texture images
        /// that are at least 64 texels wide.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture image including the border if any. If the GL version does not support
        /// non-power-of-two sizes, this value must be 2m+2⁡border for some integer m. All implementations support texture images
        /// that are at least 64 texels high.
        /// </param>
        /// <param name="border">
        /// Specifies the width of the border. Must be either 0 or 1.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.COLOR_INDEX, Gl.RED, Gl.GREEN,
        /// Gl.BLUE, Gl.ALPHA, Gl.RGB, Gl.BGR, Gl.RGBA, Gl.BGRA, Gl.LUMINANCE, and Gl.LUMINANCE_ALPHA.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.BITMAP, Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2,
        /// Gl.UNSIGNED_BYTE_2_3_3_REV, Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4,
        /// Gl.UNSIGNED_SHORT_4_4_4_4_REV, Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8,
        /// Gl.UNSIGNED_INT_8_8_8_8_REV, Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void TexImage2D(TextureTarget target, int level, InternalFormat internalFormat, int width, int height, int border, PixelFormat format, PixelType type, IntPtr data)
        {
            Debug.Assert(Delegates.pglTexImage2D != null, "pglTexImage2D not implemented");
            Delegates.pglTexImage2D((int) target, level, (int) internalFormat, width, height, border, (int) format, (int) type, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexImage2D: specify a two-dimensional texture image
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture. Must be Gl.TEXTURE_2D, Gl.PROXY_TEXTURE_2D, Gl.TEXTURE_CUBE_MAP_POSITIVE_X,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, or Gl.PROXY_TEXTURE_CUBE_MAP.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="internalFormat">
        /// Specifies the number of color components in the texture. Must be 1, 2, 3, or 4, or one of the following symbolic
        /// constants: Gl.ALPHA, Gl.ALPHA4, Gl.ALPHA8, Gl.ALPHA12, Gl.ALPHA16, Gl.COMPRESSED_ALPHA, Gl.COMPRESSED_LUMINANCE,
        /// Gl.COMPRESSED_LUMINANCE_ALPHA, Gl.COMPRESSED_INTENSITY, Gl.COMPRESSED_RGB, Gl.COMPRESSED_RGBA, Gl.DEPTH_COMPONENT,
        /// Gl.DEPTH_COMPONENT16, Gl.DEPTH_COMPONENT24, Gl.DEPTH_COMPONENT32, Gl.LUMINANCE, Gl.LUMINANCE4, Gl.LUMINANCE8,
        /// Gl.LUMINANCE12, Gl.LUMINANCE16, Gl.LUMINANCE_ALPHA, Gl.LUMINANCE4_ALPHA4, Gl.LUMINANCE6_ALPHA2, Gl.LUMINANCE8_ALPHA8,
        /// Gl.LUMINANCE12_ALPHA4, Gl.LUMINANCE12_ALPHA12, Gl.LUMINANCE16_ALPHA16, Gl.INTENSITY, Gl.INTENSITY4, Gl.INTENSITY8,
        /// Gl.INTENSITY12, Gl.INTENSITY16, Gl.R3_G3_B2, Gl.RGB, Gl.RGB4, Gl.RGB5, Gl.RGB8, Gl.RGB10, Gl.RGB12, Gl.RGB16, Gl.RGBA,
        /// Gl.RGBA2, Gl.RGBA4, Gl.RGB5_A1, Gl.RGBA8, Gl.RGB10_A2, Gl.RGBA12, Gl.RGBA16, Gl.SLUMINANCE, Gl.SLUMINANCE8,
        /// Gl.SLUMINANCE_ALPHA, Gl.SLUMINANCE8_ALPHA8, Gl.SRGB, Gl.SRGB8, Gl.SRGB_ALPHA, or Gl.SRGB8_ALPHA8.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture image including the border if any. If the GL version does not support
        /// non-power-of-two sizes, this value must be 2n+2⁡border for some integer n. All implementations support texture images
        /// that are at least 64 texels wide.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture image including the border if any. If the GL version does not support
        /// non-power-of-two sizes, this value must be 2m+2⁡border for some integer m. All implementations support texture images
        /// that are at least 64 texels high.
        /// </param>
        /// <param name="border">
        /// Specifies the width of the border. Must be either 0 or 1.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.COLOR_INDEX, Gl.RED, Gl.GREEN,
        /// Gl.BLUE, Gl.ALPHA, Gl.RGB, Gl.BGR, Gl.RGBA, Gl.BGRA, Gl.LUMINANCE, and Gl.LUMINANCE_ALPHA.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.BITMAP, Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2,
        /// Gl.UNSIGNED_BYTE_2_3_3_REV, Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4,
        /// Gl.UNSIGNED_SHORT_4_4_4_4_REV, Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8,
        /// Gl.UNSIGNED_INT_8_8_8_8_REV, Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void TexImage2D(TextureTarget target, int level, InternalFormat internalFormat, int width, int height, int border, PixelFormat format, PixelType type, object data)
        {
            GCHandle pin_pixels = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                TexImage2D(target, level, internalFormat, width, height, border, format, type, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glClear: clear buffers to preset values
        ///     </para>
        /// </summary>
        /// <param name="mask">
        /// Bitwise OR of masks that indicate the buffers to be cleared. The three masks are Gl.COLOR_BUFFER_BIT,
        /// Gl.DEPTH_BUFFER_BIT, and Gl.STENCIL_BUFFER_BIT.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Clear(ClearBufferMask mask)
        {
            Debug.Assert(Delegates.pglClear != null, "pglClear not implemented");
            Delegates.pglClear((uint) mask);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glClearColor: specify clear values for the color buffers
        ///     </para>
        /// </summary>
        /// <param name="red">
        /// Specify the red, green, blue, and alpha values used when the color buffers are cleared. The initial values are all 0.
        /// </param>
        /// <param name="green">
        /// Specify the red, green, blue, and alpha values used when the color buffers are cleared. The initial values are all 0.
        /// </param>
        /// <param name="blue">
        /// Specify the red, green, blue, and alpha values used when the color buffers are cleared. The initial values are all 0.
        /// </param>
        /// <param name="alpha">
        /// Specify the red, green, blue, and alpha values used when the color buffers are cleared. The initial values are all 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void ClearColor(float red, float green, float blue, float alpha)
        {
            Debug.Assert(Delegates.pglClearColor != null, "pglClearColor not implemented");
            Delegates.pglClearColor(red, green, blue, alpha);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glClearStencil: specify the clear value for the stencil buffer
        ///     </para>
        /// </summary>
        /// <param name="s">
        /// Specifies the index used when the stencil buffer is cleared. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void ClearStencil(int s)
        {
            Debug.Assert(Delegates.pglClearStencil != null, "pglClearStencil not implemented");
            Delegates.pglClearStencil(s);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glClearDepth: specify the clear value for the depth buffer
        /// </summary>
        /// <param name="depth">
        /// Specifies the depth value used when the depth buffer is cleared. The initial value is 1.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void ClearDepth(double depth)
        {
            Debug.Assert(Delegates.pglClearDepth != null, "pglClearDepth not implemented");
            Delegates.pglClearDepth(depth);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glStencilMask: control the front and back writing of individual bits in the stencil planes
        ///     </para>
        /// </summary>
        /// <param name="mask">
        /// Specifies a bit mask to enable and disable writing of individual bits in the stencil planes. Initially, the mask is all
        /// 1's.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void StencilMask(uint mask)
        {
            Debug.Assert(Delegates.pglStencilMask != null, "pglStencilMask not implemented");
            Delegates.pglStencilMask(mask);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glColorMask: enable and disable writing of frame buffer color components
        ///     </para>
        /// </summary>
        /// <param name="red">
        /// Specify whether red, green, blue, and alpha are to be written into the frame buffer. The initial values are all
        /// Gl.TRUE,
        /// indicating that the color components are written.
        /// </param>
        /// <param name="green">
        /// Specify whether red, green, blue, and alpha are to be written into the frame buffer. The initial values are all
        /// Gl.TRUE,
        /// indicating that the color components are written.
        /// </param>
        /// <param name="blue">
        /// Specify whether red, green, blue, and alpha are to be written into the frame buffer. The initial values are all
        /// Gl.TRUE,
        /// indicating that the color components are written.
        /// </param>
        /// <param name="alpha">
        /// Specify whether red, green, blue, and alpha are to be written into the frame buffer. The initial values are all
        /// Gl.TRUE,
        /// indicating that the color components are written.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void ColorMask(bool red, bool green, bool blue, bool alpha)
        {
            Debug.Assert(Delegates.pglColorMask != null, "pglColorMask not implemented");
            Delegates.pglColorMask(red, green, blue, alpha);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDepthMask: enable or disable writing into the depth buffer
        ///     </para>
        /// </summary>
        /// <param name="flag">
        /// Specifies whether the depth buffer is enabled for writing. If <paramref name="flag" /> is Gl.FALSE, depth buffer
        /// writing
        /// is disabled. Otherwise, it is enabled. Initially, depth buffer writing is enabled.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void DepthMask(bool flag)
        {
            Debug.Assert(Delegates.pglDepthMask != null, "pglDepthMask not implemented");
            Delegates.pglDepthMask(flag);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glDisable: enable or disable server-side GL capabilities
        ///     </para>
        /// </summary>
        /// <param name="cap">
        /// Specifies a symbolic constant indicating a GL capability.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Disable(EnableCap cap)
        {
            Debug.Assert(Delegates.pglDisable != null, "pglDisable not implemented");
            Delegates.pglDisable((int) cap);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glEnable: enable or disable server-side GL capabilities
        ///     </para>
        /// </summary>
        /// <param name="cap">
        /// Specifies a symbolic constant indicating a GL capability.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Enable(EnableCap cap)
        {
            Debug.Assert(Delegates.pglEnable != null, "pglEnable not implemented");
            Delegates.pglEnable((int) cap);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glFinish: block until all GL execution is complete
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Finish()
        {
            Debug.Assert(Delegates.pglFinish != null, "pglFinish not implemented");
            Delegates.pglFinish();
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glFlush: force execution of GL commands in finite time
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Flush()
        {
            Debug.Assert(Delegates.pglFlush != null, "pglFlush not implemented");
            Delegates.pglFlush();
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glBlendFunc: specify pixel arithmetic
        ///     </para>
        /// </summary>
        /// <param name="sfactor">
        /// Specifies how the red, green, blue, and alpha source blending factors are computed. The following symbolic constants
        /// are
        /// accepted: Gl.ZERO, Gl.ONE, Gl.SRC_COLOR, Gl.ONE_MINUS_SRC_COLOR, Gl.DST_COLOR, Gl.ONE_MINUS_DST_COLOR, Gl.SRC_ALPHA,
        /// Gl.ONE_MINUS_SRC_ALPHA, Gl.DST_ALPHA, Gl.ONE_MINUS_DST_ALPHA, Gl.CONSTANT_COLOR, Gl.ONE_MINUS_CONSTANT_COLOR,
        /// Gl.CONSTANT_ALPHA, Gl.ONE_MINUS_CONSTANT_ALPHA, and Gl.SRC_ALPHA_SATURATE. The initial value is Gl.ONE.
        /// </param>
        /// <param name="dfactor">
        /// Specifies how the red, green, blue, and alpha destination blending factors are computed. The following symbolic
        /// constants are accepted: Gl.ZERO, Gl.ONE, Gl.SRC_COLOR, Gl.ONE_MINUS_SRC_COLOR, Gl.DST_COLOR, Gl.ONE_MINUS_DST_COLOR,
        /// Gl.SRC_ALPHA, Gl.ONE_MINUS_SRC_ALPHA, Gl.DST_ALPHA, Gl.ONE_MINUS_DST_ALPHA. Gl.CONSTANT_COLOR,
        /// Gl.ONE_MINUS_CONSTANT_COLOR, Gl.CONSTANT_ALPHA, and Gl.ONE_MINUS_CONSTANT_ALPHA. The initial value is Gl.ZERO.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            Debug.Assert(Delegates.pglBlendFunc != null, "pglBlendFunc not implemented");
            Delegates.pglBlendFunc((int) sfactor, (int) dfactor);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4] glLogicOp: specify a logical pixel operation for rendering
        ///     </para>
        ///     <para>
        ///     [GLES1.1] glLogicOp: specify a logical pixel operation
        ///     </para>
        /// </summary>
        /// <param name="opcode">
        /// Specifies a symbolic constant that selects a logical operation. The following symbols are accepted: Gl.CLEAR, Gl.SET,
        /// Gl.COPY, Gl.COPY_INVERTED, Gl.NOOP, Gl.INVERT, Gl.AND, Gl.NAND, Gl.OR, Gl.NOR, Gl.XOR, Gl.EQUIV, Gl.AND_REVERSE,
        /// Gl.AND_INVERTED, Gl.OR_REVERSE, and Gl.OR_INVERTED. The initial value is Gl.COPY.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public static void LogicOp(LogicOp opcode)
        {
            Debug.Assert(Delegates.pglLogicOp != null, "pglLogicOp not implemented");
            Delegates.pglLogicOp((int) opcode);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glStencilFunc: set front and back function and reference value for stencil testing
        ///     </para>
        /// </summary>
        /// <param name="func">
        /// Specifies the test function. Eight symbolic constants are valid: Gl.NEVER, Gl.LESS, Gl.LEQUAL, Gl.GREATER, Gl.GEQUAL,
        /// Gl.EQUAL, Gl.NOTEQUAL, and Gl.ALWAYS. The initial value is Gl.ALWAYS.
        /// </param>
        /// <param name="ref">
        /// Specifies the reference value for the stencil test. <paramref name="ref" /> is clamped to the range 02n-1, where n is
        /// the
        /// number of bitplanes in the stencil buffer. The initial value is 0.
        /// </param>
        /// <param name="mask">
        /// Specifies a mask that is ANDed with both the reference value and the stored stencil value when the test is done. The
        /// initial value is all 1's.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void StencilFunc(StencilFunction func, int @ref, uint mask)
        {
            Debug.Assert(Delegates.pglStencilFunc != null, "pglStencilFunc not implemented");
            Delegates.pglStencilFunc((int) func, @ref, mask);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glStencilOp: set front and back stencil test actions
        ///     </para>
        /// </summary>
        /// <param name="sfail">
        /// Specifies the action to take when the stencil test fails. Eight symbolic constants are accepted: Gl.KEEP, Gl.ZERO,
        /// Gl.REPLACE, Gl.INCR, Gl.INCR_WRAP, Gl.DECR, Gl.DECR_WRAP, and Gl.INVERT. The initial value is Gl.KEEP.
        /// </param>
        /// <param name="dpfail">
        /// Specifies the stencil action when the stencil test passes, but the depth test fails. <paramref name="dpfail" /> accepts
        /// the same symbolic constants as <paramref name="sfail" />. The initial value is Gl.KEEP.
        /// </param>
        /// <param name="dppass">
        /// Specifies the stencil action when both the stencil test and the depth test pass, or when the stencil test passes and
        /// either there is no depth buffer or depth testing is not enabled. <paramref name="dppass" /> accepts the same symbolic
        /// constants as <paramref name="sfail" />. The initial value is Gl.KEEP.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void StencilOp(StencilOp sfail, StencilOp dpfail, StencilOp dppass)
        {
            Debug.Assert(Delegates.pglStencilOp != null, "pglStencilOp not implemented");
            Delegates.pglStencilOp((int) sfail, (int) dpfail, (int) dppass);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDepthFunc: specify the value used for depth buffer comparisons
        ///     </para>
        /// </summary>
        /// <param name="func">
        /// Specifies the depth comparison function. Symbolic constants Gl.NEVER, Gl.LESS, Gl.EQUAL, Gl.LEQUAL, Gl.GREATER,
        /// Gl.NOTEQUAL, Gl.GEQUAL, and Gl.ALWAYS are accepted. The initial value is Gl.LESS.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void DepthFunc(DepthFunction func)
        {
            Debug.Assert(Delegates.pglDepthFunc != null, "pglDepthFunc not implemented");
            Delegates.pglDepthFunc((int) func);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glPixelStoref: set pixel storage modes
        /// </summary>
        /// <param name="pname">
        /// Specifies the symbolic name of the parameter to be set. Six values affect the packing of pixel data into memory:
        /// Gl.PACK_SWAP_BYTES, Gl.PACK_LSB_FIRST, Gl.PACK_ROW_LENGTH, Gl.PACK_IMAGE_HEIGHT, Gl.PACK_SKIP_PIXELS,
        /// Gl.PACK_SKIP_ROWS,
        /// Gl.PACK_SKIP_IMAGES, and Gl.PACK_ALIGNMENT. Six more affect the unpacking of pixel data from memory:
        /// Gl.UNPACK_SWAP_BYTES, Gl.UNPACK_LSB_FIRST, Gl.UNPACK_ROW_LENGTH, Gl.UNPACK_IMAGE_HEIGHT, Gl.UNPACK_SKIP_PIXELS,
        /// Gl.UNPACK_SKIP_ROWS, Gl.UNPACK_SKIP_IMAGES, and Gl.UNPACK_ALIGNMENT.
        /// </param>
        /// <param name="param">
        /// Specifies the value that <paramref name="pname" /> is set to.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void PixelStore(PixelStoreParameter pname, float param)
        {
            Debug.Assert(Delegates.pglPixelStoref != null, "pglPixelStoref not implemented");
            Delegates.pglPixelStoref((int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glPixelStorei: set pixel storage modes
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the symbolic name of the parameter to be set. Six values affect the packing of pixel data into memory:
        /// Gl.PACK_SWAP_BYTES, Gl.PACK_LSB_FIRST, Gl.PACK_ROW_LENGTH, Gl.PACK_IMAGE_HEIGHT, Gl.PACK_SKIP_PIXELS,
        /// Gl.PACK_SKIP_ROWS,
        /// Gl.PACK_SKIP_IMAGES, and Gl.PACK_ALIGNMENT. Six more affect the unpacking of pixel data from memory:
        /// Gl.UNPACK_SWAP_BYTES, Gl.UNPACK_LSB_FIRST, Gl.UNPACK_ROW_LENGTH, Gl.UNPACK_IMAGE_HEIGHT, Gl.UNPACK_SKIP_PIXELS,
        /// Gl.UNPACK_SKIP_ROWS, Gl.UNPACK_SKIP_IMAGES, and Gl.UNPACK_ALIGNMENT.
        /// </param>
        /// <param name="param">
        /// Specifies the value that <paramref name="pname" /> is set to.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void PixelStore(PixelStoreParameter pname, int param)
        {
            Debug.Assert(Delegates.pglPixelStorei != null, "pglPixelStorei not implemented");
            Delegates.pglPixelStorei((int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glReadBuffer: select a color buffer source for pixels
        ///     </para>
        /// </summary>
        /// <param name="mode">
        /// Specifies a color buffer. Accepted values are Gl.FRONT_LEFT, Gl.FRONT_RIGHT, Gl.BACK_LEFT, Gl.BACK_RIGHT, Gl.FRONT,
        /// Gl.BACK, Gl.LEFT, Gl.RIGHT, and the constants Gl.COLOR_ATTACHMENTi.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        public static void ReadBuffer(ReadBufferMode mode)
        {
            Debug.Assert(Delegates.pglReadBuffer != null, "pglReadBuffer not implemented");
            Delegates.pglReadBuffer((int) mode);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1] glReadPixels: read a block of pixels from the frame buffer
        ///     </para>
        ///     <para>
        ///     [GLES1.1] glReadPixels: read a block of pixels from the color buffer
        ///     </para>
        /// </summary>
        /// <param name="x">
        /// Specify the window coordinates of the first pixel that is read from the frame buffer. This location is the lower left
        /// corner of a rectangular block of pixels.
        /// </param>
        /// <param name="y">
        /// Specify the window coordinates of the first pixel that is read from the frame buffer. This location is the lower left
        /// corner of a rectangular block of pixels.
        /// </param>
        /// <param name="width">
        /// Specify the dimensions of the pixel rectangle. <paramref name="width" /> and <paramref name="height" /> of one
        /// correspond
        /// to a single pixel.
        /// </param>
        /// <param name="height">
        /// Specify the dimensions of the pixel rectangle. <paramref name="width" /> and <paramref name="height" /> of one
        /// correspond
        /// to a single pixel.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.COLOR_INDEX, Gl.STENCIL_INDEX,
        /// Gl.DEPTH_COMPONENT, Gl.RED, Gl.GREEN, Gl.BLUE, Gl.ALPHA, Gl.RGB, Gl.BGR, Gl.RGBA, Gl.BGRA, Gl.LUMINANCE, and
        /// Gl.LUMINANCE_ALPHA.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. Must be one of Gl.UNSIGNED_BYTE, Gl.BYTE, Gl.BITMAP, Gl.UNSIGNED_SHORT,
        /// Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, or Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="data">
        /// Returns the pixel data.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void ReadPixels(int x, int y, int width, int height, PixelFormat format, PixelType type, IntPtr data)
        {
            Debug.Assert(Delegates.pglReadPixels != null, "pglReadPixels not implemented");
            Delegates.pglReadPixels(x, y, width, height, (int) format, (int) type, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetBooleanv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(int pname, [Out] byte[] data)
        {
            unsafe
            {
                fixed (byte* p_data = data)
                {
                    Debug.Assert(Delegates.pglGetBooleanv != null, "pglGetBooleanv not implemented");
                    Delegates.pglGetBooleanv(pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetBooleanv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(GetPName pname, [Out] byte[] data)
        {
            unsafe
            {
                fixed (byte* p_data = data)
                {
                    Debug.Assert(Delegates.pglGetBooleanv != null, "pglGetBooleanv not implemented");
                    Delegates.pglGetBooleanv((int) pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetBooleanv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(int pname, out byte data)
        {
            unsafe
            {
                fixed (byte* p_data = &data)
                {
                    Debug.Assert(Delegates.pglGetBooleanv != null, "pglGetBooleanv not implemented");
                    Delegates.pglGetBooleanv(pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetBooleanv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(GetPName pname, out byte data)
        {
            unsafe
            {
                fixed (byte* p_data = &data)
                {
                    Debug.Assert(Delegates.pglGetBooleanv != null, "pglGetBooleanv not implemented");
                    Delegates.pglGetBooleanv((int) pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetBooleanv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static unsafe void Get(GetPName pname, [Out] byte* data)
        {
            Debug.Assert(Delegates.pglGetBooleanv != null, "pglGetBooleanv not implemented");
            Delegates.pglGetBooleanv((int) pname, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetBooleanv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetBoolean(GetPName pname, out bool[] data)
        {
            Debug.Assert(Delegates.pglGetBooleanv != null, "pglGetBooleanv not implemented");
            data = default;
            unsafe
            {
                fixed (bool* refDataPtr = &data[0])
                {
                    Delegates.pglGetBooleanv((int) pname, (byte*) refDataPtr);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetDoublev: return the value or values of a selected parameter
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void Get(int pname, [Out] double[] data)
        {
            unsafe
            {
                fixed (double* p_data = data)
                {
                    Debug.Assert(Delegates.pglGetDoublev != null, "pglGetDoublev not implemented");
                    Delegates.pglGetDoublev(pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetDoublev: return the value or values of a selected parameter
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void Get(GetPName pname, [Out] double[] data)
        {
            unsafe
            {
                fixed (double* p_data = data)
                {
                    Debug.Assert(Delegates.pglGetDoublev != null, "pglGetDoublev not implemented");
                    Delegates.pglGetDoublev((int) pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetDoublev: return the value or values of a selected parameter
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void Get(int pname, out double data)
        {
            unsafe
            {
                fixed (double* p_data = &data)
                {
                    Debug.Assert(Delegates.pglGetDoublev != null, "pglGetDoublev not implemented");
                    Delegates.pglGetDoublev(pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetDoublev: return the value or values of a selected parameter
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void Get(GetPName pname, out double data)
        {
            unsafe
            {
                fixed (double* p_data = &data)
                {
                    Debug.Assert(Delegates.pglGetDoublev != null, "pglGetDoublev not implemented");
                    Delegates.pglGetDoublev((int) pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetDoublev: return the value or values of a selected parameter
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static unsafe void Get(GetPName pname, [Out] double* data)
        {
            Debug.Assert(Delegates.pglGetDoublev != null, "pglGetDoublev not implemented");
            Delegates.pglGetDoublev((int) pname, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetDoublev: return the value or values of a selected parameter
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void GetDouble(GetPName pname, out double[] data)
        {
            Debug.Assert(Delegates.pglGetDoublev != null, "pglGetDoublev not implemented");
            data = default;
            unsafe
            {
                fixed (double* refDataPtr = &data[0])
                {
                    Delegates.pglGetDoublev((int) pname, refDataPtr);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetError: return error information
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static ErrorCode GetError()
        {
            int retValue;

            Debug.Assert(Delegates.pglGetError != null, "pglGetError not implemented");
            retValue = Delegates.pglGetError();

            return (ErrorCode) retValue;
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetFloatv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(int pname, [Out] float[] data)
        {
            unsafe
            {
                fixed (float* p_data = data)
                {
                    Debug.Assert(Delegates.pglGetFloatv != null, "pglGetFloatv not implemented");
                    Delegates.pglGetFloatv(pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetFloatv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(GetPName pname, [Out] float[] data)
        {
            unsafe
            {
                fixed (float* p_data = data)
                {
                    Debug.Assert(Delegates.pglGetFloatv != null, "pglGetFloatv not implemented");
                    Delegates.pglGetFloatv((int) pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetFloatv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(int pname, out float data)
        {
            unsafe
            {
                fixed (float* p_data = &data)
                {
                    Debug.Assert(Delegates.pglGetFloatv != null, "pglGetFloatv not implemented");
                    Delegates.pglGetFloatv(pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetFloatv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(GetPName pname, out float data)
        {
            unsafe
            {
                fixed (float* p_data = &data)
                {
                    Debug.Assert(Delegates.pglGetFloatv != null, "pglGetFloatv not implemented");
                    Delegates.pglGetFloatv((int) pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetFloatv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static unsafe void Get(GetPName pname, [Out] float* data)
        {
            Debug.Assert(Delegates.pglGetFloatv != null, "pglGetFloatv not implemented");
            Delegates.pglGetFloatv((int) pname, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetFloatv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetFloat(GetPName pname, out float[] data)
        {
            Debug.Assert(Delegates.pglGetFloatv != null, "pglGetFloatv not implemented");
            data = default;
            unsafe
            {
                fixed (float* refDataPtr = &data[0])
                {
                    Delegates.pglGetFloatv((int) pname, refDataPtr);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetIntegerv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(int pname, [Out] int[] data)
        {
            unsafe
            {
                fixed (int* p_data = data)
                {
                    Debug.Assert(Delegates.pglGetIntegerv != null, "pglGetIntegerv not implemented");
                    Delegates.pglGetIntegerv(pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetIntegerv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(GetPName pname, [Out] int[] data)
        {
            unsafe
            {
                fixed (int* p_data = data)
                {
                    Debug.Assert(Delegates.pglGetIntegerv != null, "pglGetIntegerv not implemented");
                    Delegates.pglGetIntegerv((int) pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetIntegerv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(int pname, out int data)
        {
            unsafe
            {
                fixed (int* p_data = &data)
                {
                    Debug.Assert(Delegates.pglGetIntegerv != null, "pglGetIntegerv not implemented");
                    Delegates.pglGetIntegerv(pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetIntegerv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Get(GetPName pname, out int data)
        {
            unsafe
            {
                fixed (int* p_data = &data)
                {
                    Debug.Assert(Delegates.pglGetIntegerv != null, "pglGetIntegerv not implemented");
                    Delegates.pglGetIntegerv((int) pname, p_data);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetIntegerv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static unsafe void Get(GetPName pname, [Out] int* data)
        {
            Debug.Assert(Delegates.pglGetIntegerv != null, "pglGetIntegerv not implemented");
            Delegates.pglGetIntegerv((int) pname, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetIntegerv: return the value or values of a selected parameter
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the parameter value to be returned for non-indexed versions of Gl.Get. The symbolic constants in the list
        /// below are accepted.
        /// </param>
        /// <param name="data">
        /// Returns the value or values of the specified parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetInteger(GetPName pname, ref int[] data)
        {
            Debug.Assert(Delegates.pglGetIntegerv != null, "pglGetIntegerv not implemented");
            unsafe
            {
                fixed (int* refDataPtr = &data[0])
                {
                    Delegates.pglGetIntegerv((int) pname, refDataPtr);
                }
            }

            DebugCheckErrors(null);
        }

        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetInteger(GetPName pname, out int data)
        {
            Debug.Assert(Delegates.pglGetIntegerv != null, "pglGetIntegerv not implemented");
            data = default;
            unsafe
            {
                fixed (int* refDataPtr = &data)
                {
                    Delegates.pglGetIntegerv((int) pname, refDataPtr);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetString: return a string describing the current GL connection
        ///     </para>
        /// </summary>
        /// <param name="name">
        /// Specifies a symbolic constant, one of Gl.VENDOR, Gl.RENDERER, Gl.VERSION, or Gl.SHADING_LANGUAGE_VERSION. Additionally,
        /// Gl.GetStringi accepts the Gl.EXTENSIONS token.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static string GetString(StringName name)
        {
            IntPtr retValue;

            Debug.Assert(Delegates.pglGetString != null, "pglGetString not implemented");
            retValue = Delegates.pglGetString((int) name);
            DebugCheckErrors(retValue);

            return NativeHelpers.StringFromPtr(retValue);
        }

        /// <summary>
        /// [GL2.1] glGetTexImage: return a texture image
        /// </summary>
        /// <param name="target">
        /// Specifies which texture is to be obtained. Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP_POSITIVE_X,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, and Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z are accepted.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="format">
        /// Specifies a pixel format for the returned data. The supported formats are Gl.RED, Gl.GREEN, Gl.BLUE, Gl.ALPHA, Gl.RGB,
        /// Gl.BGR, Gl.RGBA, Gl.BGRA, Gl.LUMINANCE, and Gl.LUMINANCE_ALPHA.
        /// </param>
        /// <param name="type">
        /// Specifies a pixel type for the returned data. The supported types are Gl.UNSIGNED_BYTE, Gl.BYTE, Gl.UNSIGNED_SHORT,
        /// Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// A <see cref="T:IntPtr" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void GetTexImage(TextureTarget target, int level, PixelFormat format, PixelType type, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglGetTexImage != null, "pglGetTexImage not implemented");
            Delegates.pglGetTexImage((int) target, level, (int) format, (int) type, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glGetTexImage: return a texture image
        /// </summary>
        /// <param name="target">
        /// Specifies which texture is to be obtained. Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP_POSITIVE_X,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, and Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z are accepted.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="format">
        /// Specifies a pixel format for the returned data. The supported formats are Gl.RED, Gl.GREEN, Gl.BLUE, Gl.ALPHA, Gl.RGB,
        /// Gl.BGR, Gl.RGBA, Gl.BGRA, Gl.LUMINANCE, and Gl.LUMINANCE_ALPHA.
        /// </param>
        /// <param name="type">
        /// Specifies a pixel type for the returned data. The supported types are Gl.UNSIGNED_BYTE, Gl.BYTE, Gl.UNSIGNED_SHORT,
        /// Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// A <see cref="T:object" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void GetTexImage(TextureTarget target, int level, PixelFormat format, PixelType type, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetTexImage(target, level, format, type, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexParameterfv: return texture parameter values
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexParameterfv, Gl.GetTexParameteriv,
        /// Gl.GetTexParameterIiv, and Gl.GetTexParameterIuiv functions. Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP,
        /// Gl.TEXTURE_RECTANGLE, and Gl.TEXTURE_CUBE_MAP_ARRAY are accepted.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetTexParameter(TextureTarget target, GetTextureParameter pname, [Out] float[] @params)
        {
            unsafe
            {
                fixed (float* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTexParameterfv != null, "pglGetTexParameterfv not implemented");
                    Delegates.pglGetTexParameterfv((int) target, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexParameterfv: return texture parameter values
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexParameterfv, Gl.GetTexParameteriv,
        /// Gl.GetTexParameterIiv, and Gl.GetTexParameterIuiv functions. Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP,
        /// Gl.TEXTURE_RECTANGLE, and Gl.TEXTURE_CUBE_MAP_ARRAY are accepted.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetTexParameter(TextureTarget target, GetTextureParameter pname, out float @params)
        {
            unsafe
            {
                fixed (float* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTexParameterfv != null, "pglGetTexParameterfv not implemented");
                    Delegates.pglGetTexParameterfv((int) target, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexParameterfv: return texture parameter values
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexParameterfv, Gl.GetTexParameteriv,
        /// Gl.GetTexParameterIiv, and Gl.GetTexParameterIuiv functions. Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP,
        /// Gl.TEXTURE_RECTANGLE, and Gl.TEXTURE_CUBE_MAP_ARRAY are accepted.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static unsafe void GetTexParameter(TextureTarget target, GetTextureParameter pname, [Out] float* @params)
        {
            Debug.Assert(Delegates.pglGetTexParameterfv != null, "pglGetTexParameterfv not implemented");
            Delegates.pglGetTexParameterfv((int) target, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexParameteriv: return texture parameter values
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexParameterfv, Gl.GetTexParameteriv,
        /// Gl.GetTexParameterIiv, and Gl.GetTexParameterIuiv functions. Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP,
        /// Gl.TEXTURE_RECTANGLE, and Gl.TEXTURE_CUBE_MAP_ARRAY are accepted.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetTexParameter(TextureTarget target, GetTextureParameter pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTexParameteriv != null, "pglGetTexParameteriv not implemented");
                    Delegates.pglGetTexParameteriv((int) target, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexParameteriv: return texture parameter values
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexParameterfv, Gl.GetTexParameteriv,
        /// Gl.GetTexParameterIiv, and Gl.GetTexParameterIuiv functions. Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP,
        /// Gl.TEXTURE_RECTANGLE, and Gl.TEXTURE_CUBE_MAP_ARRAY are accepted.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetTexParameter(TextureTarget target, GetTextureParameter pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTexParameteriv != null, "pglGetTexParameteriv not implemented");
                    Delegates.pglGetTexParameteriv((int) target, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexParameteriv: return texture parameter values
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexParameterfv, Gl.GetTexParameteriv,
        /// Gl.GetTexParameterIiv, and Gl.GetTexParameterIuiv functions. Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP,
        /// Gl.TEXTURE_RECTANGLE, and Gl.TEXTURE_CUBE_MAP_ARRAY are accepted.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static unsafe void GetTexParameter(TextureTarget target, GetTextureParameter pname, [Out] int* @params)
        {
            Debug.Assert(Delegates.pglGetTexParameteriv != null, "pglGetTexParameteriv not implemented");
            Delegates.pglGetTexParameteriv((int) target, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexLevelParameterfv: return texture parameter values for a specific level of detail
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexLevelParameterfv and Gl.GetTexLevelParameteriv
        /// functions. Must be one of the following values: Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, Gl.TEXTURE_1D_ARRAY,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_RECTANGLE, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, Gl.PROXY_TEXTURE_1D,
        /// Gl.PROXY_TEXTURE_2D, Gl.PROXY_TEXTURE_3D, Gl.PROXY_TEXTURE_1D_ARRAY, Gl.PROXY_TEXTURE_2D_ARRAY,
        /// Gl.PROXY_TEXTURE_RECTANGLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.PROXY_TEXTURE_CUBE_MAP, or Gl.TEXTURE_BUFFER.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public static void GetTexLevelParameter(TextureTarget target, int level, GetTextureParameter pname, [Out] float[] @params)
        {
            unsafe
            {
                fixed (float* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTexLevelParameterfv != null, "pglGetTexLevelParameterfv not implemented");
                    Delegates.pglGetTexLevelParameterfv((int) target, level, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexLevelParameterfv: return texture parameter values for a specific level of detail
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexLevelParameterfv and Gl.GetTexLevelParameteriv
        /// functions. Must be one of the following values: Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, Gl.TEXTURE_1D_ARRAY,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_RECTANGLE, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, Gl.PROXY_TEXTURE_1D,
        /// Gl.PROXY_TEXTURE_2D, Gl.PROXY_TEXTURE_3D, Gl.PROXY_TEXTURE_1D_ARRAY, Gl.PROXY_TEXTURE_2D_ARRAY,
        /// Gl.PROXY_TEXTURE_RECTANGLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.PROXY_TEXTURE_CUBE_MAP, or Gl.TEXTURE_BUFFER.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public static void GetTexLevelParameter(TextureTarget target, int level, GetTextureParameter pname, out float @params)
        {
            unsafe
            {
                fixed (float* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTexLevelParameterfv != null, "pglGetTexLevelParameterfv not implemented");
                    Delegates.pglGetTexLevelParameterfv((int) target, level, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexLevelParameterfv: return texture parameter values for a specific level of detail
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexLevelParameterfv and Gl.GetTexLevelParameteriv
        /// functions. Must be one of the following values: Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, Gl.TEXTURE_1D_ARRAY,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_RECTANGLE, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, Gl.PROXY_TEXTURE_1D,
        /// Gl.PROXY_TEXTURE_2D, Gl.PROXY_TEXTURE_3D, Gl.PROXY_TEXTURE_1D_ARRAY, Gl.PROXY_TEXTURE_2D_ARRAY,
        /// Gl.PROXY_TEXTURE_RECTANGLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.PROXY_TEXTURE_CUBE_MAP, or Gl.TEXTURE_BUFFER.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public static unsafe void GetTexLevelParameter(TextureTarget target, int level, GetTextureParameter pname, [Out] float* @params)
        {
            Debug.Assert(Delegates.pglGetTexLevelParameterfv != null, "pglGetTexLevelParameterfv not implemented");
            Delegates.pglGetTexLevelParameterfv((int) target, level, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexLevelParameteriv: return texture parameter values for a specific level of detail
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexLevelParameterfv and Gl.GetTexLevelParameteriv
        /// functions. Must be one of the following values: Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, Gl.TEXTURE_1D_ARRAY,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_RECTANGLE, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, Gl.PROXY_TEXTURE_1D,
        /// Gl.PROXY_TEXTURE_2D, Gl.PROXY_TEXTURE_3D, Gl.PROXY_TEXTURE_1D_ARRAY, Gl.PROXY_TEXTURE_2D_ARRAY,
        /// Gl.PROXY_TEXTURE_RECTANGLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.PROXY_TEXTURE_CUBE_MAP, or Gl.TEXTURE_BUFFER.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public static void GetTexLevelParameter(TextureTarget target, int level, GetTextureParameter pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTexLevelParameteriv != null, "pglGetTexLevelParameteriv not implemented");
                    Delegates.pglGetTexLevelParameteriv((int) target, level, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexLevelParameteriv: return texture parameter values for a specific level of detail
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexLevelParameterfv and Gl.GetTexLevelParameteriv
        /// functions. Must be one of the following values: Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, Gl.TEXTURE_1D_ARRAY,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_RECTANGLE, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, Gl.PROXY_TEXTURE_1D,
        /// Gl.PROXY_TEXTURE_2D, Gl.PROXY_TEXTURE_3D, Gl.PROXY_TEXTURE_1D_ARRAY, Gl.PROXY_TEXTURE_2D_ARRAY,
        /// Gl.PROXY_TEXTURE_RECTANGLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.PROXY_TEXTURE_CUBE_MAP, or Gl.TEXTURE_BUFFER.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public static void GetTexLevelParameter(TextureTarget target, int level, GetTextureParameter pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTexLevelParameteriv != null, "pglGetTexLevelParameteriv not implemented");
                    Delegates.pglGetTexLevelParameteriv((int) target, level, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetTexLevelParameteriv: return texture parameter values for a specific level of detail
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetTexLevelParameterfv and Gl.GetTexLevelParameteriv
        /// functions. Must be one of the following values: Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D, Gl.TEXTURE_1D_ARRAY,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_RECTANGLE, Gl.TEXTURE_2D_MULTISAMPLE, Gl.TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, Gl.PROXY_TEXTURE_1D,
        /// Gl.PROXY_TEXTURE_2D, Gl.PROXY_TEXTURE_3D, Gl.PROXY_TEXTURE_1D_ARRAY, Gl.PROXY_TEXTURE_2D_ARRAY,
        /// Gl.PROXY_TEXTURE_RECTANGLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE, Gl.PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY,
        /// Gl.PROXY_TEXTURE_CUBE_MAP, or Gl.TEXTURE_BUFFER.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public static unsafe void GetTexLevelParameter(TextureTarget target, int level, GetTextureParameter pname, [Out] int* @params)
        {
            Debug.Assert(Delegates.pglGetTexLevelParameteriv != null, "pglGetTexLevelParameteriv not implemented");
            Delegates.pglGetTexLevelParameteriv((int) target, level, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glIsEnabled: test whether a capability is enabled
        ///     </para>
        /// </summary>
        /// <param name="cap">
        /// Specifies a symbolic constant indicating a GL capability.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static bool IsEnabled(EnableCap cap)
        {
            bool retValue;

            Debug.Assert(Delegates.pglIsEnabled != null, "pglIsEnabled not implemented");
            retValue = Delegates.pglIsEnabled((int) cap);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        /// [GL4] glDepthRange: specify mapping of depth values from normalized device coordinates to window coordinates
        /// </summary>
        /// <param name="nearVal">
        /// Specifies the mapping of the near clipping plane to window coordinates. The initial value is 0.
        /// </param>
        /// <param name="farVal">
        /// Specifies the mapping of the far clipping plane to window coordinates. The initial value is 1.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        public static void DepthRange(double nearVal, double farVal)
        {
            Debug.Assert(Delegates.pglDepthRange != null, "pglDepthRange not implemented");
            Delegates.pglDepthRange(nearVal, farVal);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glViewport: set the viewport
        ///     </para>
        /// </summary>
        /// <param name="x">
        /// Specify the lower left corner of the viewport rectangle, in pixels. The initial value is (0,0).
        /// </param>
        /// <param name="y">
        /// Specify the lower left corner of the viewport rectangle, in pixels. The initial value is (0,0).
        /// </param>
        /// <param name="width">
        /// Specify the width and height of the viewport. When a GL context is first attached to a window,
        /// <paramref name="width" />
        /// and <paramref name="height" /> are set to the dimensions of that window.
        /// </param>
        /// <param name="height">
        /// Specify the width and height of the viewport. When a GL context is first attached to a window,
        /// <paramref name="width" />
        /// and <paramref name="height" /> are set to the dimensions of that window.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void Viewport(int x, int y, int width, int height)
        {
            Debug.Assert(Delegates.pglViewport != null, "pglViewport not implemented");
            Delegates.pglViewport(x, y, width, height);
            DebugCheckErrors(null);
        }

        public static unsafe partial class Delegates
        {
            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCullFace(int mode);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glCullFace pglCullFace;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glFrontFace(int mode);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glFrontFace pglFrontFace;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glHint(int target, int mode);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glHint pglHint;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glLineWidth(float width);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glLineWidth pglLineWidth;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPointSize(float size);

            [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")] [ThreadStatic]
            public static glPointSize pglPointSize;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPolygonMode(int face, int mode);

            [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2", EntryPoint = "glPolygonModeNV")] [ThreadStatic]
            public static glPolygonMode pglPolygonMode;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glScissor(int x, int y, int width, int height);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glScissor pglScissor;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexParameterf(int target, int pname, float param);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glTexParameterf pglTexParameterf;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexParameterfv(int target, int pname, float* @params);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glTexParameterfv pglTexParameterfv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexParameteri(int target, int pname, int param);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glTexParameteri pglTexParameteri;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexParameteriv(int target, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glTexParameteriv pglTexParameteriv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexImage1D(int target, int level, int internalFormat, int width, int border, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_1_0")] [ThreadStatic]
            public static glTexImage1D pglTexImage1D;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexImage2D(int target, int level, int internalFormat, int width, int height, int border, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glTexImage2D pglTexImage2D;

            [RequiredByFeature("GL_VERSION_1_0")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDrawBuffer(int buf);

            [RequiredByFeature("GL_VERSION_1_0")] [ThreadStatic]
            public static glDrawBuffer pglDrawBuffer;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClear(uint mask);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glClear pglClear;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearColor(float red, float green, float blue, float alpha);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glClearColor pglClearColor;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearStencil(int s);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glClearStencil pglClearStencil;

            [RequiredByFeature("GL_VERSION_1_0")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearDepth(double depth);

            [RequiredByFeature("GL_VERSION_1_0")] [ThreadStatic]
            public static glClearDepth pglClearDepth;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glStencilMask(uint mask);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glStencilMask pglStencilMask;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glColorMask([MarshalAs(UnmanagedType.I1)] bool red, [MarshalAs(UnmanagedType.I1)] bool green, [MarshalAs(UnmanagedType.I1)] bool blue,
                [MarshalAs(UnmanagedType.I1)] bool alpha);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glColorMask pglColorMask;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDepthMask([MarshalAs(UnmanagedType.I1)] bool flag);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glDepthMask pglDepthMask;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDisable(int cap);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glDisable pglDisable;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glEnable(int cap);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glEnable pglEnable;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glFinish();

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glFinish pglFinish;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glFlush();

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glFlush pglFlush;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glBlendFunc(int sfactor, int dfactor);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glBlendFunc pglBlendFunc;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glLogicOp(int opcode);

            [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [ThreadStatic]
            public static glLogicOp pglLogicOp;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glStencilFunc(int func, int @ref, uint mask);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glStencilFunc pglStencilFunc;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glStencilOp(int fail, int zfail, int zpass);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glStencilOp pglStencilOp;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDepthFunc(int func);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glDepthFunc pglDepthFunc;

            [RequiredByFeature("GL_VERSION_1_0")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPixelStoref(int pname, float param);

            [RequiredByFeature("GL_VERSION_1_0")] [ThreadStatic]
            public static glPixelStoref pglPixelStoref;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPixelStorei(int pname, int param);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glPixelStorei pglPixelStorei;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glReadBuffer(int src);

            [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [ThreadStatic]
            public static glReadBuffer pglReadBuffer;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glReadPixels(int x, int y, int width, int height, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glReadPixels pglReadPixels;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetBooleanv(int pname, byte* data);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glGetBooleanv pglGetBooleanv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetDoublev(int pname, double* data);

            [RequiredByFeature("GL_VERSION_1_0")] [ThreadStatic]
            public static glGetDoublev pglGetDoublev;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate int glGetError();

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glGetError pglGetError;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetFloatv(int pname, float* data);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glGetFloatv pglGetFloatv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetIntegerv(int pname, int* data);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glGetIntegerv pglGetIntegerv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate IntPtr glGetString(int name);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glGetString pglGetString;

            [RequiredByFeature("GL_VERSION_1_0")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTexImage(int target, int level, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_1_0")] [ThreadStatic]
            public static glGetTexImage pglGetTexImage;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTexParameterfv(int target, int pname, float* @params);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glGetTexParameterfv pglGetTexParameterfv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTexParameteriv(int target, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glGetTexParameteriv pglGetTexParameteriv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTexLevelParameterfv(int target, int level, int pname, float* @params);

            [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [ThreadStatic]
            public static glGetTexLevelParameterfv pglGetTexLevelParameterfv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTexLevelParameteriv(int target, int level, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [ThreadStatic]
            public static glGetTexLevelParameteriv pglGetTexLevelParameteriv;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.I1)]
            public delegate bool glIsEnabled(int cap);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glIsEnabled pglIsEnabled;

            [RequiredByFeature("GL_VERSION_1_0")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDepthRange(double near, double far);

            [RequiredByFeature("GL_VERSION_1_0")] [ThreadStatic]
            public static glDepthRange pglDepthRange;

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glViewport(int x, int y, int width, int height);

            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glViewport pglViewport;
        }
    }
}