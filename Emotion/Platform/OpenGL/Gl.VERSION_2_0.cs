#pragma warning disable 649, 1572, 1573

// ReSharper disable RedundantUsingDirective

#region Using

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
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
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating whether the RGB blend equation is
        /// Gl.FUNC_ADD, Gl.FUNC_SUBTRACT, Gl.FUNC_REVERSE_SUBTRACT, Gl.MIN or Gl.MAX. See Gl.BlendEquationSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_equation_separate")]
        [RequiredByFeature("GL_OES_blend_equation_separate", Api = "gles1")]
        public const int BLEND_EQUATION_RGB = 0x8009;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetVertexAttrib: params returns a single value that is non-zero (true) if the vertex attribute array
        /// for index is enabled and 0 (false) if it is disabled. The initial value is Gl.FALSE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int VERTEX_ATTRIB_ARRAY_ENABLED = 0x8622;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetVertexAttrib: params returns a single value, the size of the vertex attribute array for index. The
        /// size is the number of values for each element of the vertex attribute array, and it will be 1, 2, 3, or 4. The initial
        /// value is 4.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int VERTEX_ATTRIB_ARRAY_SIZE = 0x8623;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetVertexAttrib: params returns a single value, the array stride for (number of bytes between
        /// successive elements in) the vertex attribute array for index. A value of 0 indicates that the array elements are stored
        /// sequentially in memory. The initial value is 0.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int VERTEX_ATTRIB_ARRAY_STRIDE = 0x8624;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.GetVertexAttrib: params returns a single value, a symbolic constant indicating the array type for the
        ///     vertex
        ///     attribute array for index. Possible values are Gl.BYTE, Gl.UNSIGNED_BYTE, Gl.SHORT, Gl.UNSIGNED_SHORT, Gl.INT,
        ///     Gl.UNSIGNED_INT, Gl.FLOAT, and Gl.DOUBLE. The initial value is Gl.FLOAT.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.GetVertexAttrib: params returns a single value, a symbolic constant indicating the array type for the
        ///     vertex attribute array for index. Possible values are Gl.BYTE, Gl.UNSIGNED_BYTE, Gl.SHORT, Gl.UNSIGNED_SHORT,
        ///     Gl.INT,
        ///     Gl.INT_2_10_10_10_REV, Gl.UNSIGNED_INT, Gl.FIXED, Gl.HALF_FLOAT, and Gl.FLOAT. The initial value is Gl.FLOAT.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int VERTEX_ATTRIB_ARRAY_TYPE = 0x8625;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.GetVertexAttrib: params returns four values that represent the current value for the generic vertex
        ///     attribute
        ///     specified by index. Generic vertex attribute 0 is unique in that it has no current state, so an error will be
        ///     generated
        ///     if index is 0. The initial value for all other generic vertex attributes is (0,0,0,1).glGetVertexAttribdv and
        ///     glGetVertexAttribfv return the current attribute values as four single-precision floating-point values;
        ///     glGetVertexAttribiv reads them as floating-point values and converts them to four integer values;
        ///     glGetVertexAttribIiv
        ///     and glGetVertexAttribIuiv read and return them as signed or unsigned integer values, respectively;
        ///     glGetVertexAttribLdv
        ///     reads and returns them as four double-precision floating-point values.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.GetVertexAttrib: params returns four values that represent the current value for the generic vertex
        ///     attribute specified by index. The initial value for all generic vertex attributes is (0,0,0,1).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int CURRENT_VERTEX_ATTRIB = 0x8626;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Enable: If enabled and a vertex shader is active, then the derived point size is taken from the
        ///     (potentially
        ///     clipped) shader builtin Gl.PointSize and clamped to the implementation-dependent point size range.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether vertex program point size mode is enabled.
        ///     If
        ///     enabled, and a vertex shader is active, then the point size is taken from the shader built-in gl_PointSize. If
        ///     disabled,
        ///     and a vertex shader is active, then the point size is taken from the point state as specified by Gl.PointSize. The
        ///     initial value is Gl.FALSE.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ARB_vertex_program")] [RequiredByFeature("GL_ARB_vertex_shader")] [RequiredByFeature("GL_NV_vertex_program")]
        public const int VERTEX_PROGRAM_POINT_SIZE = 0x8642;

        /// <summary>
        /// [GL] Value of GL_VERTEX_ATTRIB_ARRAY_POINTER symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int VERTEX_ATTRIB_ARRAY_POINTER = 0x8645;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating what function is used for back-facing
        /// polygons to compare the stencil reference value with the stencil buffer value. The initial value is Gl.ALWAYS. See
        /// Gl.StencilFuncSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        public const int STENCIL_BACK_FUNC = 0x8800;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating what action is taken for back-facing
        /// polygons when the stencil test fails. The initial value is Gl.KEEP. See Gl.StencilOpSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        public const int STENCIL_BACK_FAIL = 0x8801;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating what action is taken for back-facing
        /// polygons when the stencil test passes, but the depth test fails. The initial value is Gl.KEEP. See
        /// Gl.StencilOpSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        public const int STENCIL_BACK_PASS_DEPTH_FAIL = 0x8802;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating what action is taken for back-facing
        /// polygons when the stencil test passes and the depth test passes. The initial value is Gl.KEEP. See
        /// Gl.StencilOpSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        public const int STENCIL_BACK_PASS_DEPTH_PASS = 0x8803;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the maximum number of simultaneous outputs that may be written in a fragment
        ///     shader. The value must be at least 8. See Gl.DrawBuffers.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of simultaneous outputs that may be written in a
        ///     fragment
        ///     shader. The value must be at least 4. See Gl.DrawBuffers.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int MAX_DRAW_BUFFERS = 0x8824;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER0 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER0 = 0x8825;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER1 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER1 = 0x8826;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER2 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER2 = 0x8827;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER3 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER3 = 0x8828;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER4 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER4 = 0x8829;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER5 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER5 = 0x882A;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER6 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER6 = 0x882B;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER7 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER7 = 0x882C;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER8 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER8 = 0x882D;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER9 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER9 = 0x882E;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER10 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER10 = 0x882F;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER11 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER11 = 0x8830;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER12 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER12 = 0x8831;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER13 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER13 = 0x8832;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER14 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER14 = 0x8833;

        /// <summary>
        /// [GL] Value of GL_DRAW_BUFFER15 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        public const int DRAW_BUFFER15 = 0x8834;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating whether the Alpha blend equation is
        /// Gl.FUNC_ADD, Gl.FUNC_SUBTRACT, Gl.FUNC_REVERSE_SUBTRACT, Gl.MIN or Gl.MAX. See Gl.BlendEquationSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_equation_separate")]
        [RequiredByFeature("GL_OES_blend_equation_separate", Api = "gles1")]
        public const int BLEND_EQUATION_ALPHA = 0x883D;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of 4-component generic vertex attributes accessible to
        /// a vertex shader. The value must be at least 16. See Gl.VertexAttrib.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int MAX_VERTEX_ATTRIBS = 0x8869;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetVertexAttrib: params returns a single value that is non-zero (true) if fixed-point data types for
        /// the vertex attribute array indicated by index are normalized when they are converted to floating point, and 0 (false)
        /// otherwise. The initial value is Gl.FALSE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int VERTEX_ATTRIB_ARRAY_NORMALIZED = 0x886A;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access
        /// texture maps from the fragment shader. The value must be at least 16. See Gl.ActiveTexture.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_fragment_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_fragment_program")]
        public const int MAX_TEXTURE_IMAGE_UNITS = 0x8872;

        /// <summary>
        /// [GL] Value of GL_FRAGMENT_SHADER symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_fragment_shader")]
        public const int FRAGMENT_SHADER = 0x8B30;

        /// <summary>
        /// [GL] Value of GL_VERTEX_SHADER symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int VERTEX_SHADER = 0x8B31;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean values
        ///     that
        ///     can be held in uniform variable storage for a fragment shader. The value must be at least 1024. See Gl.Uniform.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean
        ///     values
        ///     that can be held in uniform variable storage for a fragment shader. The value must be at least 896. See Gl.Uniform.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_fragment_shader")]
        public const int MAX_FRAGMENT_UNIFORM_COMPONENTS = 0x8B49;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean
        /// values that can be held in uniform variable storage for a vertex shader. The value must be at least 1024. See
        /// Gl.Uniform.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int MAX_VERTEX_UNIFORM_COMPONENTS = 0x8B4A;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access
        /// texture maps from the vertex shader. The value may be at least 16. See Gl.ActiveTexture.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program3")]
        public const int MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8B4C;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access texture
        ///     maps
        ///     from the vertex shader and the fragment processor combined. If both the vertex shader and the fragment processing
        ///     stage
        ///     access the same texture image unit, then that counts as using two texture image units against this limit. The value
        ///     must
        ///     be at least 48. See Gl.ActiveTexture.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access
        ///     texture
        ///     maps from the all the shader stages combined. If both the vertex shader and the fragment processing stage access
        ///     the
        ///     same texture image unit, then that counts as using two texture image units against this limit. The value must be at
        ///     least 96. See Gl.ActiveTexture.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int MAX_COMBINED_TEXTURE_IMAGE_UNITS = 0x8B4D;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.GetShader: params returns Gl.VERTEX_SHADER if shader is a vertex shader object, Gl.GEOMETRY_SHADER if
        ///     shader is
        ///     a geometry shader object, and Gl.FRAGMENT_SHADER if shader is a fragment shader object.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.GetShader: params returns Gl.VERTEX_SHADER if shader is a vertex shader object, Gl.TESS_CONTROL_SHADER
        ///     if
        ///     shader is a tesselation control shader object,Gl.TESS_EVALUATION_SHADER if shader is a tesselation evaluation
        ///     shader
        ///     object,Gl.GEOMETRY_SHADER if shader is a geometry shader object, Gl.FRAGMENT_SHADER if shader is a fragment shader,
        ///     and
        ///     Gl.COMPUTE_SHADER if shader is a compute shader object.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int SHADER_TYPE = 0x8B4F;

        /// <summary>
        /// [GL] Value of GL_FLOAT_VEC2 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int FLOAT_VEC2 = 0x8B50;

        /// <summary>
        /// [GL] Value of GL_FLOAT_VEC3 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int FLOAT_VEC3 = 0x8B51;

        /// <summary>
        /// [GL] Value of GL_FLOAT_VEC4 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int FLOAT_VEC4 = 0x8B52;

        /// <summary>
        /// [GL] Value of GL_INT_VEC2 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int INT_VEC2 = 0x8B53;

        /// <summary>
        /// [GL] Value of GL_INT_VEC3 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int INT_VEC3 = 0x8B54;

        /// <summary>
        /// [GL] Value of GL_INT_VEC4 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int INT_VEC4 = 0x8B55;

        /// <summary>
        /// [GL] Value of GL_BOOL symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int BOOL = 0x8B56;

        /// <summary>
        /// [GL] Value of GL_BOOL_VEC2 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int BOOL_VEC2 = 0x8B57;

        /// <summary>
        /// [GL] Value of GL_BOOL_VEC3 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int BOOL_VEC3 = 0x8B58;

        /// <summary>
        /// [GL] Value of GL_BOOL_VEC4 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int BOOL_VEC4 = 0x8B59;

        /// <summary>
        /// [GL] Value of GL_FLOAT_MAT2 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int FLOAT_MAT2 = 0x8B5A;

        /// <summary>
        /// [GL] Value of GL_FLOAT_MAT3 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int FLOAT_MAT3 = 0x8B5B;

        /// <summary>
        /// [GL] Value of GL_FLOAT_MAT4 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        public const int FLOAT_MAT4 = 0x8B5C;

        /// <summary>
        /// [GL] Value of GL_SAMPLER_1D symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int SAMPLER_1D = 0x8B5D;

        /// <summary>
        /// [GL] Value of GL_SAMPLER_2D symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public const int SAMPLER_2D = 0x8B5E;

        /// <summary>
        /// [GL] Value of GL_SAMPLER_3D symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        public const int SAMPLER_3D = 0x8B5F;

        /// <summary>
        /// [GL] Value of GL_SAMPLER_CUBE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int SAMPLER_CUBE = 0x8B60;

        /// <summary>
        /// [GL] Value of GL_SAMPLER_1D_SHADOW symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ARB_shader_objects")]
        public const int SAMPLER_1D_SHADOW = 0x8B61;

        /// <summary>
        /// [GL] Value of GL_SAMPLER_2D_SHADOW symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        [RequiredByFeature("GL_EXT_shadow_samplers", Api = "gles2")]
        public const int SAMPLER_2D_SHADOW = 0x8B62;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetProgram: params returns Gl.TRUE if program is currently flagged for deletion, and Gl.FALSE
        ///     otherwise.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetShader: params returns Gl.TRUE if shader is currently flagged for deletion, and Gl.FALSE
        ///     otherwise.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int DELETE_STATUS = 0x8B80;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetShader: params returns Gl.TRUE if the last compile operation on shader was successful, and Gl.FALSE
        /// otherwise.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int COMPILE_STATUS = 0x8B81;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetProgram: params returns Gl.TRUE if the last link operation on program was successful, and Gl.FALSE
        /// otherwise.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int LINK_STATUS = 0x8B82;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetProgram: params returns Gl.TRUE or if the last validation operation on program was successful, and
        /// Gl.FALSE otherwise.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int VALIDATE_STATUS = 0x8B83;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetProgram: params returns the number of characters in the information log for program including
        ///     the
        ///     null termination character (i.e., the size of the character buffer required to store the information log). If
        ///     program
        ///     has no information log, a value of 0 is returned.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.GetShader: params returns the number of characters in the information log for shader including the
        ///     null
        ///     termination character (i.e., the size of the character buffer required to store the information log). If shader has
        ///     no
        ///     information log, a value of 0 is returned.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int INFO_LOG_LENGTH = 0x8B84;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetProgram: params returns the number of shader objects attached to program.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int ATTACHED_SHADERS = 0x8B85;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetProgram: params returns the number of active uniform variables for program.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int ACTIVE_UNIFORMS = 0x8B86;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetProgram: params returns the length of the longest active uniform variable name for program,
        /// including the null termination character (i.e., the size of the character buffer required to store the longest uniform
        /// variable name). If no active uniform variables exist, 0 is returned.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int ACTIVE_UNIFORM_MAX_LENGTH = 0x8B87;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetShader: params returns the length of the concatenation of the source strings that make up the
        /// shader
        /// source for the shader, including the null termination character. (i.e., the size of the character buffer required to
        /// store the shader source). If no source code exists, 0 is returned.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int SHADER_SOURCE_LENGTH = 0x8B88;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetProgram: params returns the number of active attribute variables for program.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int ACTIVE_ATTRIBUTES = 0x8B89;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetProgram: params returns the length of the longest active attribute name for program, including the
        /// null termination character (i.e., the size of the character buffer required to store the longest attribute name). If no
        /// active attributes exist, 0 is returned.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public const int ACTIVE_ATTRIBUTE_MAX_LENGTH = 0x8B8A;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.Get: data returns one value, a symbolic constant indicating the mode of the derivative accuracy
        ///     hint
        ///     for fragment shaders. The initial value is Gl.DONT_CARE. See Gl.Hint.
        ///     </para>
        ///     <para>
        ///     [GL4|GLES3.2] Gl.Hint: Indicates the accuracy of the derivative calculation for the GL shading language fragment
        ///     processing built-in functions: Gl.x, Gl.y, and Gl.dth.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_fragment_shader")]
        [RequiredByFeature("GL_OES_standard_derivatives", Api = "gles2|glsc2")]
        public const int FRAGMENT_SHADER_DERIVATIVE_HINT = 0x8B8B;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetString: Returns a version or release number for the shading language.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shading_language_100")]
        public const int SHADING_LANGUAGE_VERSION = 0x8B8C;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the name of the program object that is currently active, or 0 if no
        /// program object is active. See Gl.UseProgram.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int CURRENT_PROGRAM = 0x8B8D;

        /// <summary>
        /// [GL4] Gl.PointParameter: params is a single enum specifying the point sprite texture coordinate origin, either
        /// Gl.LOWER_LEFT or Gl.UPPER_LEFT. The default value is Gl.UPPER_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] public const int POINT_SPRITE_COORD_ORIGIN = 0x8CA0;

        /// <summary>
        /// [GL] Value of GL_LOWER_LEFT symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        public const int LOWER_LEFT = 0x8CA1;

        /// <summary>
        /// [GL] Value of GL_UPPER_LEFT symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        public const int UPPER_LEFT = 0x8CA2;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the reference value that is compared with the contents of the stencil
        /// buffer for back-facing polygons. The initial value is 0. See Gl.StencilFuncSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_BACK_REF = 0x8CA3;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the mask that is used for back-facing polygons to mask both the stencil
        /// reference value and the stencil buffer value before they are compared. The initial value is all 1's. See
        /// Gl.StencilFuncSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_BACK_VALUE_MASK = 0x8CA4;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the mask that controls writing of the stencil bitplanes for back-facing
        /// polygons. The initial value is all 1's. See Gl.StencilMaskSeparate.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int STENCIL_BACK_WRITEMASK = 0x8CA5;

        /// <summary>
        /// [GL2.1] glBlendEquationSeparate: set the RGB blend equation and the alpha blend equation separately
        /// </summary>
        /// <param name="modeRGB">
        /// specifies the RGB blend equation, how the red, green, and blue components of the source and destination colors are
        /// combined. It must be Gl.FUNC_ADD, Gl.FUNC_SUBTRACT, Gl.FUNC_REVERSE_SUBTRACT, Gl.MIN, Gl.MAX.
        /// </param>
        /// <param name="modeAlpha">
        /// specifies the alpha blend equation, how the alpha component of the source and destination colors are combined. It must
        /// be Gl.FUNC_ADD, Gl.FUNC_SUBTRACT, Gl.FUNC_REVERSE_SUBTRACT, Gl.MIN, Gl.MAX.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_blend_equation_separate")]
        public static void BlendEquationSeparate(BlendEquationMode modeRGB, BlendEquationMode modeAlpha)
        {
            Assert(Delegates.pglBlendEquationSeparate != null, "pglBlendEquationSeparate not implemented");
            Delegates.pglBlendEquationSeparate((int) modeRGB, (int) modeAlpha);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDrawBuffers: Specifies a list of color buffers to be drawn into
        ///     </para>
        /// </summary>
        /// <param name="bufs">
        /// Points to an array of symbolic constants specifying the buffers into which fragment colors or data values will be
        /// written.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        public static void DrawBuffers(params int[] bufs)
        {
            unsafe
            {
                fixed (int* p_bufs = bufs)
                {
                    Assert(Delegates.pglDrawBuffers != null, "pglDrawBuffers not implemented");
                    Delegates.pglDrawBuffers(bufs.Length, p_bufs);
                }
            }

            DebugCheckErrors(null);
        }

        public static unsafe void DrawBuffers(int singleBuf)
        {
            Assert(Delegates.pglDrawBuffers != null, "pglDrawBuffers not implemented");
            Delegates.pglDrawBuffers(1, &singleBuf);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glStencilOpSeparate: set front and/or back stencil test actions
        ///     </para>
        /// </summary>
        /// <param name="face">
        /// Specifies whether front and/or back stencil state is updated. Three symbolic constants are valid: Gl.FRONT, Gl.BACK,
        /// and
        /// Gl.FRONT_AND_BACK.
        /// </param>
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
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        public static void StencilOpSeparate(StencilFaceDirection face, StencilOp sfail, StencilOp dpfail, StencilOp dppass)
        {
            Assert(Delegates.pglStencilOpSeparate != null, "pglStencilOpSeparate not implemented");
            Delegates.pglStencilOpSeparate((int) face, (int) sfail, (int) dpfail, (int) dppass);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glStencilFuncSeparate: set front and/or back function and reference value for stencil testing
        ///     </para>
        /// </summary>
        /// <param name="face">
        /// Specifies whether front and/or back stencil state is updated. Three symbolic constants are valid: Gl.FRONT, Gl.BACK,
        /// and
        /// Gl.FRONT_AND_BACK.
        /// </param>
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
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void StencilFuncSeparate(StencilFaceDirection face, StencilFunction func, int @ref, uint mask)
        {
            Assert(Delegates.pglStencilFuncSeparate != null, "pglStencilFuncSeparate not implemented");
            Delegates.pglStencilFuncSeparate((int) face, (int) func, @ref, mask);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glStencilMaskSeparate: control the front and/or back writing of individual bits in the stencil planes
        ///     </para>
        /// </summary>
        /// <param name="face">
        /// Specifies whether the front and/or back stencil writemask is updated. Three symbolic constants are valid: Gl.FRONT,
        /// Gl.BACK, and Gl.FRONT_AND_BACK.
        /// </param>
        /// <param name="mask">
        /// Specifies a bit mask to enable and disable writing of individual bits in the stencil planes. Initially, the mask is all
        /// 1's.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void StencilMaskSeparate(StencilFaceDirection face, uint mask)
        {
            Assert(Delegates.pglStencilMaskSeparate != null, "pglStencilMaskSeparate not implemented");
            Delegates.pglStencilMaskSeparate((int) face, mask);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glAttachShader: Attaches a shader object to a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to which a shader object will be attached.
        /// </param>
        /// <param name="shader">
        /// Specifies the shader object that is to be attached.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void AttachShader(uint program, uint shader)
        {
            Assert(Delegates.pglAttachShader != null, "pglAttachShader not implemented");
            Delegates.pglAttachShader(program, shader);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glBindAttribLocation: Associates a generic vertex attribute index with a named attribute variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the handle of the program object in which the association is to be made.
        /// </param>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be bound.
        /// </param>
        /// <param name="name">
        /// Specifies a null terminated string containing the name of the vertex shader attribute variable to which
        /// <paramref
        ///     name="index" />
        /// is to be bound.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void BindAttribLocation(uint program, uint index, string name)
        {
            Assert(Delegates.pglBindAttribLocation != null, "pglBindAttribLocation not implemented");
            Delegates.pglBindAttribLocation(program, index, name);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glCompileShader: Compiles a shader object
        ///     </para>
        /// </summary>
        /// <param name="shader">
        /// Specifies the shader object to be compiled.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void CompileShader(uint shader)
        {
            Assert(Delegates.pglCompileShader != null, "pglCompileShader not implemented");
            Delegates.pglCompileShader(shader);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glCreateProgram: Creates a program object
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static uint CreateProgram()
        {
            uint retValue;

            Assert(Delegates.pglCreateProgram != null, "pglCreateProgram not implemented");
            retValue = Delegates.pglCreateProgram();
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glCreateShader: Creates a shader object
        ///     </para>
        /// </summary>
        /// <param name="type">
        /// A <see cref="T:ShaderType" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static uint CreateShader(ShaderType type)
        {
            uint retValue;

            Assert(Delegates.pglCreateShader != null, "pglCreateShader not implemented");
            retValue = Delegates.pglCreateShader((int) type);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDeleteProgram: Deletes a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be deleted.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void DeleteProgram(uint program)
        {
            Assert(Delegates.pglDeleteProgram != null, "pglDeleteProgram not implemented");
            Delegates.pglDeleteProgram(program);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDeleteShader: Deletes a shader object
        ///     </para>
        /// </summary>
        /// <param name="shader">
        /// Specifies the shader object to be deleted.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void DeleteShader(uint shader)
        {
            Assert(Delegates.pglDeleteShader != null, "pglDeleteShader not implemented");
            Delegates.pglDeleteShader(shader);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDetachShader: Detaches a shader object from a program object to which it is attached
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object from which to detach the shader object.
        /// </param>
        /// <param name="shader">
        /// Specifies the shader object to be detached.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void DetachShader(uint program, uint shader)
        {
            Assert(Delegates.pglDetachShader != null, "pglDetachShader not implemented");
            Delegates.pglDetachShader(program, shader);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDisableVertexAttribArray: Enable or disable a generic vertex attribute array
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be enabled or disabled.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void DisableVertexAttribArray(uint index)
        {
            Assert(Delegates.pglDisableVertexAttribArray != null, "pglDisableVertexAttribArray not implemented");
            Delegates.pglDisableVertexAttribArray(index);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glEnableVertexAttribArray: Enable or disable a generic vertex attribute array
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be enabled or disabled.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void EnableVertexAttribArray(uint index)
        {
            Assert(Delegates.pglEnableVertexAttribArray != null, "pglEnableVertexAttribArray not implemented");
            Delegates.pglEnableVertexAttribArray(index);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetActiveAttrib: Returns information about an active attribute variable for the specified program
        ///     object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="index">
        /// Specifies the index of the attribute variable to be queried.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the maximum number of characters OpenGL is allowed to write in the character buffer indicated by
        /// <paramref
        ///     name="name" />
        /// .
        /// </param>
        /// <param name="length">
        /// Returns the number of characters actually written by OpenGL in the string indicated by <paramref name="name" />
        /// (excluding the null terminator) if a value other than Gl.L is passed.
        /// </param>
        /// <param name="size">
        /// Returns the size of the attribute variable.
        /// </param>
        /// <param name="type">
        /// Returns the data type of the attribute variable.
        /// </param>
        /// <param name="name">
        /// Returns a null terminated string containing the name of the attribute variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void GetActiveAttrib(uint program, uint index, int bufSize, out int length, out int size, out int type, StringBuilder name)
        {
            unsafe
            {
                fixed (int* p_length = &length)
                fixed (int* p_size = &size)
                fixed (int* p_type = &type)
                {
                    Assert(Delegates.pglGetActiveAttrib != null, "pglGetActiveAttrib not implemented");
                    Delegates.pglGetActiveAttrib(program, index, bufSize, p_length, p_size, p_type, name);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetActiveUniform: Returns information about an active uniform variable for the specified program
        ///     object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="index">
        /// Specifies the index of the uniform variable to be queried.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the maximum number of characters OpenGL is allowed to write in the character buffer indicated by
        /// <paramref
        ///     name="name" />
        /// .
        /// </param>
        /// <param name="length">
        /// Returns the number of characters actually written by OpenGL in the string indicated by <paramref name="name" />
        /// (excluding the null terminator) if a value other than Gl.L is passed.
        /// </param>
        /// <param name="size">
        /// Returns the size of the uniform variable.
        /// </param>
        /// <param name="type">
        /// Returns the data type of the uniform variable.
        /// </param>
        /// <param name="name">
        /// Returns a null terminated string containing the name of the uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void GetActiveUniform(uint program, uint index, int bufSize, out int length, out int size, out int type, StringBuilder name)
        {
            unsafe
            {
                fixed (int* p_length = &length)
                fixed (int* p_size = &size)
                fixed (int* p_type = &type)
                {
                    Assert(Delegates.pglGetActiveUniform != null, "pglGetActiveUniform not implemented");
                    Delegates.pglGetActiveUniform(program, index, bufSize, p_length, p_size, p_type, name);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetAttachedShaders: Returns the handles of the shader objects attached to a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="count">
        /// Returns the number of names actually returned in <paramref name="shaders" />.
        /// </param>
        /// <param name="shaders">
        /// Specifies an array that is used to return the names of attached shader objects.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void GetAttachedShaders(uint program, out int count, [Out] uint[] shaders)
        {
            unsafe
            {
                fixed (int* p_count = &count)
                fixed (uint* p_shaders = shaders)
                {
                    Assert(Delegates.pglGetAttachedShaders != null, "pglGetAttachedShaders not implemented");
                    Delegates.pglGetAttachedShaders(program, shaders.Length, p_count, p_shaders);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetAttribLocation: Returns the location of an attribute variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="name">
        /// Points to a null terminated string containing the name of the attribute variable whose location is to be queried.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static int GetAttribLocation(uint program, string name)
        {
            int retValue;

            Assert(Delegates.pglGetAttribLocation != null, "pglGetAttribLocation not implemented");
            retValue = Delegates.pglGetAttribLocation(program, name);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetProgramiv: Returns a parameter from a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the object parameter. Accepted symbolic names are Gl.DELETE_STATUS, Gl.LINK_STATUS, Gl.VALIDATE_STATUS,
        /// Gl.INFO_LOG_LENGTH, Gl.ATTACHED_SHADERS, Gl.ACTIVE_ATOMIC_COUNTER_BUFFERS, Gl.ACTIVE_ATTRIBUTES,
        /// Gl.ACTIVE_ATTRIBUTE_MAX_LENGTH, Gl.ACTIVE_UNIFORMS, Gl.ACTIVE_UNIFORM_BLOCKS, Gl.ACTIVE_UNIFORM_BLOCK_MAX_NAME_LENGTH,
        /// Gl.ACTIVE_UNIFORM_MAX_LENGTH, Gl.COMPUTE_WORK_GROUP_SIZEGl.PROGRAM_BINARY_LENGTH, Gl.TRANSFORM_FEEDBACK_BUFFER_MODE,
        /// Gl.TRANSFORM_FEEDBACK_VARYINGS, Gl.TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH, Gl.GEOMETRY_VERTICES_OUT,
        /// Gl.GEOMETRY_INPUT_TYPE, and Gl.GEOMETRY_OUTPUT_TYPE.
        /// </param>
        /// <param name="params">
        /// Returns the requested object parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetProgram(uint program, ProgramProperty pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Assert(Delegates.pglGetProgramiv != null, "pglGetProgramiv not implemented");
                    Delegates.pglGetProgramiv(program, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetProgramiv: Returns a parameter from a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the object parameter. Accepted symbolic names are Gl.DELETE_STATUS, Gl.LINK_STATUS, Gl.VALIDATE_STATUS,
        /// Gl.INFO_LOG_LENGTH, Gl.ATTACHED_SHADERS, Gl.ACTIVE_ATOMIC_COUNTER_BUFFERS, Gl.ACTIVE_ATTRIBUTES,
        /// Gl.ACTIVE_ATTRIBUTE_MAX_LENGTH, Gl.ACTIVE_UNIFORMS, Gl.ACTIVE_UNIFORM_BLOCKS, Gl.ACTIVE_UNIFORM_BLOCK_MAX_NAME_LENGTH,
        /// Gl.ACTIVE_UNIFORM_MAX_LENGTH, Gl.COMPUTE_WORK_GROUP_SIZEGl.PROGRAM_BINARY_LENGTH, Gl.TRANSFORM_FEEDBACK_BUFFER_MODE,
        /// Gl.TRANSFORM_FEEDBACK_VARYINGS, Gl.TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH, Gl.GEOMETRY_VERTICES_OUT,
        /// Gl.GEOMETRY_INPUT_TYPE, and Gl.GEOMETRY_OUTPUT_TYPE.
        /// </param>
        /// <param name="params">
        /// Returns the requested object parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GetProgram(uint program, ProgramProperty pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Assert(Delegates.pglGetProgramiv != null, "pglGetProgramiv not implemented");
                    Delegates.pglGetProgramiv(program, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetProgramInfoLog: Returns the information log for a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object whose information log is to be queried.
        /// </param>
        /// <param name="maxLength">
        /// Specifies the size of the character buffer for storing the returned information log.
        /// </param>
        /// <param name="length">
        /// Returns the length of the string returned in <paramref name="infoLog" /> (excluding the null terminator).
        /// </param>
        /// <param name="infoLog">
        /// Specifies an array of characters that is used to return the information log.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void GetProgramInfoLog(uint program, int maxLength, out int length, StringBuilder infoLog)
        {
            unsafe
            {
                fixed (int* p_length = &length)
                {
                    Assert(Delegates.pglGetProgramInfoLog != null, "pglGetProgramInfoLog not implemented");
                    Delegates.pglGetProgramInfoLog(program, maxLength, p_length, infoLog);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetShaderiv: Returns a parameter from a shader object
        ///     </para>
        /// </summary>
        /// <param name="shader">
        /// Specifies the shader object to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the object parameter. Accepted symbolic names are Gl.SHADER_TYPE, Gl.DELETE_STATUS, Gl.COMPILE_STATUS,
        /// Gl.INFO_LOG_LENGTH, Gl.SHADER_SOURCE_LENGTH.
        /// </param>
        /// <param name="params">
        /// Returns the requested object parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void GetShader(uint shader, ShaderParameterName pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Assert(Delegates.pglGetShaderiv != null, "pglGetShaderiv not implemented");
                    Delegates.pglGetShaderiv(shader, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetShaderiv: Returns a parameter from a shader object
        ///     </para>
        /// </summary>
        /// <param name="shader">
        /// Specifies the shader object to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the object parameter. Accepted symbolic names are Gl.SHADER_TYPE, Gl.DELETE_STATUS, Gl.COMPILE_STATUS,
        /// Gl.INFO_LOG_LENGTH, Gl.SHADER_SOURCE_LENGTH.
        /// </param>
        /// <param name="params">
        /// Returns the requested object parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void GetShader(uint shader, ShaderParameterName pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Assert(Delegates.pglGetShaderiv != null, "pglGetShaderiv not implemented");
                    Delegates.pglGetShaderiv(shader, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetShaderInfoLog: Returns the information log for a shader object
        ///     </para>
        /// </summary>
        /// <param name="shader">
        /// Specifies the shader object whose information log is to be queried.
        /// </param>
        /// <param name="maxLength">
        /// Specifies the size of the character buffer for storing the returned information log.
        /// </param>
        /// <param name="length">
        /// Returns the length of the string returned in <paramref name="infoLog" /> (excluding the null terminator).
        /// </param>
        /// <param name="infoLog">
        /// Specifies an array of characters that is used to return the information log.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void GetShaderInfoLog(uint shader, int maxLength, out int length, StringBuilder infoLog)
        {
            unsafe
            {
                fixed (int* p_length = &length)
                {
                    Assert(Delegates.pglGetShaderInfoLog != null, "pglGetShaderInfoLog not implemented");
                    Delegates.pglGetShaderInfoLog(shader, maxLength, p_length, infoLog);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetShaderSource: Returns the source code string from a shader object
        ///     </para>
        /// </summary>
        /// <param name="shader">
        /// Specifies the shader object to be queried.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the character buffer for storing the returned source code string.
        /// </param>
        /// <param name="length">
        /// Returns the length of the string returned in <paramref name="source" /> (excluding the null terminator).
        /// </param>
        /// <param name="source">
        /// Specifies an array of characters that is used to return the source code string.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void GetShaderSource(uint shader, int bufSize, out int length, StringBuilder source)
        {
            unsafe
            {
                fixed (int* p_length = &length)
                {
                    Assert(Delegates.pglGetShaderSource != null, "pglGetShaderSource not implemented");
                    Delegates.pglGetShaderSource(shader, bufSize, p_length, source);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetUniformLocation: Returns the location of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="name">
        /// Points to a null terminated string containing the name of the uniform variable whose location is to be queried.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static int GetUniformLocation(uint program, string name)
        {
            int retValue;

            Assert(Delegates.pglGetUniformLocation != null, "pglGetUniformLocation not implemented");
            retValue = Delegates.pglGetUniformLocation(program, name);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetUniformfv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void GetUniform(uint program, int location, [Out] float[] @params)
        {
            unsafe
            {
                fixed (float* p_params = @params)
                {
                    Assert(Delegates.pglGetUniformfv != null, "pglGetUniformfv not implemented");
                    Delegates.pglGetUniformfv(program, location, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetUniformfv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void GetUniform(uint program, int location, [Out] float* @params)
        {
            Assert(Delegates.pglGetUniformfv != null, "pglGetUniformfv not implemented");
            Delegates.pglGetUniformfv(program, location, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetUniformfv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void GetUniformf<T>(uint program, int location, out T param) where T : unmanaged
        {
            Assert(Delegates.pglGetUniformfv != null, "pglGetUniformfv not implemented");

            unsafe
            {
                T p = default;
                Delegates.pglGetUniformfv(program, location, (float*) &p);
                param = p;
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetUniformiv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void GetUniform(uint program, int location, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Assert(Delegates.pglGetUniformiv != null, "pglGetUniformiv not implemented");
                    Delegates.pglGetUniformiv(program, location, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetUniformiv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void GetUniform(uint program, int location, [Out] int* @params)
        {
            Assert(Delegates.pglGetUniformiv != null, "pglGetUniformiv not implemented");
            Delegates.pglGetUniformiv(program, location, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetUniformiv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void GetUniformi<T>(uint program, int location, out T param) where T : unmanaged
        {
            Assert(Delegates.pglGetUniformiv != null, "pglGetUniformiv not implemented");
            unsafe
            {
                T p = default;
                Delegates.pglGetUniformiv(program, location, (int*) &p);
                param = p;
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetVertexAttribdv: Return a generic vertex attribute parameter
        /// </summary>
        /// <param name="index">
        /// Specifies the generic vertex attribute parameter to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of the vertex attribute parameter to be queried. Accepted values are
        /// Gl.VERTEX_ATTRIB_ARRAY_BUFFER_BINDING, Gl.VERTEX_ATTRIB_ARRAY_ENABLED, Gl.VERTEX_ATTRIB_ARRAY_SIZE,
        /// Gl.VERTEX_ATTRIB_ARRAY_STRIDE, Gl.VERTEX_ATTRIB_ARRAY_TYPE, Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED,
        /// Gl.VERTEX_ATTRIB_ARRAY_INTEGER, Gl.VERTEX_ATTRIB_ARRAY_DIVISOR, or Gl.CURRENT_VERTEX_ATTRIB.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void GetVertexAttrib(uint index, int pname, [Out] double[] @params)
        {
            Assert(@params.Length >= 4);
            unsafe
            {
                fixed (double* p_params = @params)
                {
                    Assert(Delegates.pglGetVertexAttribdv != null, "pglGetVertexAttribdv not implemented");
                    Delegates.pglGetVertexAttribdv(index, pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetVertexAttribfv: Return a generic vertex attribute parameter
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the generic vertex attribute parameter to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of the vertex attribute parameter to be queried. Accepted values are
        /// Gl.VERTEX_ATTRIB_ARRAY_BUFFER_BINDING, Gl.VERTEX_ATTRIB_ARRAY_ENABLED, Gl.VERTEX_ATTRIB_ARRAY_SIZE,
        /// Gl.VERTEX_ATTRIB_ARRAY_STRIDE, Gl.VERTEX_ATTRIB_ARRAY_TYPE, Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED,
        /// Gl.VERTEX_ATTRIB_ARRAY_INTEGER, Gl.VERTEX_ATTRIB_ARRAY_DIVISOR, or Gl.CURRENT_VERTEX_ATTRIB.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void GetVertexAttrib(uint index, int pname, [Out] float[] @params)
        {
            Assert(@params.Length >= 4);
            unsafe
            {
                fixed (float* p_params = @params)
                {
                    Assert(Delegates.pglGetVertexAttribfv != null, "pglGetVertexAttribfv not implemented");
                    Delegates.pglGetVertexAttribfv(index, pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetVertexAttribfv: Return a generic vertex attribute parameter
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the generic vertex attribute parameter to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of the vertex attribute parameter to be queried. Accepted values are
        /// Gl.VERTEX_ATTRIB_ARRAY_BUFFER_BINDING, Gl.VERTEX_ATTRIB_ARRAY_ENABLED, Gl.VERTEX_ATTRIB_ARRAY_SIZE,
        /// Gl.VERTEX_ATTRIB_ARRAY_STRIDE, Gl.VERTEX_ATTRIB_ARRAY_TYPE, Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED,
        /// Gl.VERTEX_ATTRIB_ARRAY_INTEGER, Gl.VERTEX_ATTRIB_ARRAY_DIVISOR, or Gl.CURRENT_VERTEX_ATTRIB.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void GetVertexAttrib(uint index, int pname, out float @params)
        {
            unsafe
            {
                fixed (float* p_params = &@params)
                {
                    Assert(Delegates.pglGetVertexAttribfv != null, "pglGetVertexAttribfv not implemented");
                    Delegates.pglGetVertexAttribfv(index, pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetVertexAttribiv: Return a generic vertex attribute parameter
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the generic vertex attribute parameter to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of the vertex attribute parameter to be queried. Accepted values are
        /// Gl.VERTEX_ATTRIB_ARRAY_BUFFER_BINDING, Gl.VERTEX_ATTRIB_ARRAY_ENABLED, Gl.VERTEX_ATTRIB_ARRAY_SIZE,
        /// Gl.VERTEX_ATTRIB_ARRAY_STRIDE, Gl.VERTEX_ATTRIB_ARRAY_TYPE, Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED,
        /// Gl.VERTEX_ATTRIB_ARRAY_INTEGER, Gl.VERTEX_ATTRIB_ARRAY_DIVISOR, or Gl.CURRENT_VERTEX_ATTRIB.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void GetVertexAttrib(uint index, int pname, [Out] int[] @params)
        {
            Assert(@params.Length >= 4);
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Assert(Delegates.pglGetVertexAttribiv != null, "pglGetVertexAttribiv not implemented");
                    Delegates.pglGetVertexAttribiv(index, pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetVertexAttribiv: Return a generic vertex attribute parameter
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the generic vertex attribute parameter to be queried.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of the vertex attribute parameter to be queried. Accepted values are
        /// Gl.VERTEX_ATTRIB_ARRAY_BUFFER_BINDING, Gl.VERTEX_ATTRIB_ARRAY_ENABLED, Gl.VERTEX_ATTRIB_ARRAY_SIZE,
        /// Gl.VERTEX_ATTRIB_ARRAY_STRIDE, Gl.VERTEX_ATTRIB_ARRAY_TYPE, Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED,
        /// Gl.VERTEX_ATTRIB_ARRAY_INTEGER, Gl.VERTEX_ATTRIB_ARRAY_DIVISOR, or Gl.CURRENT_VERTEX_ATTRIB.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void GetVertexAttrib(uint index, int pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Assert(Delegates.pglGetVertexAttribiv != null, "pglGetVertexAttribiv not implemented");
                    Delegates.pglGetVertexAttribiv(index, pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetVertexAttribPointerv: return the address of the specified generic vertex attribute pointer
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the generic vertex attribute parameter to be returned.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of the generic vertex attribute parameter to be returned. Must be
        /// Gl.VERTEX_ATTRIB_ARRAY_POINTER.
        /// </param>
        /// <param name="pointer">
        /// Returns the pointer value.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void GetVertexAttribPointer(uint index, int pname, out IntPtr pointer)
        {
            unsafe
            {
                fixed (IntPtr* p_pointer = &pointer)
                {
                    Assert(Delegates.pglGetVertexAttribPointerv != null, "pglGetVertexAttribPointerv not implemented");
                    Delegates.pglGetVertexAttribPointerv(index, pname, p_pointer);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetVertexAttribPointerv: return the address of the specified generic vertex attribute pointer
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the generic vertex attribute parameter to be returned.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of the generic vertex attribute parameter to be returned. Must be
        /// Gl.VERTEX_ATTRIB_ARRAY_POINTER.
        /// </param>
        /// <param name="pointer">
        /// Returns the pointer value.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void GetVertexAttribPointer(uint index, int pname, object pointer)
        {
            GCHandle pin_pointer = GCHandle.Alloc(pointer, GCHandleType.Pinned);
            try
            {
                GetVertexAttribPointer(index, pname, pin_pointer.AddrOfPinnedObject());
            }
            finally
            {
                pin_pointer.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glIsProgram: Determines if a name corresponds to a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies a potential program object.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static bool IsProgram(uint program)
        {
            bool retValue;

            Assert(Delegates.pglIsProgram != null, "pglIsProgram not implemented");
            retValue = Delegates.pglIsProgram(program);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glIsShader: Determines if a name corresponds to a shader object
        ///     </para>
        /// </summary>
        /// <param name="shader">
        /// Specifies a potential shader object.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static bool IsShader(uint shader)
        {
            bool retValue;

            Assert(Delegates.pglIsShader != null, "pglIsShader not implemented");
            retValue = Delegates.pglIsShader(shader);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glLinkProgram: Links a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the handle of the program object to be linked.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void LinkProgram(uint program)
        {
            Assert(Delegates.pglLinkProgram != null, "pglLinkProgram not implemented");
            Delegates.pglLinkProgram(program);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glShaderSource: Replaces the source code in a shader object
        ///     </para>
        /// </summary>
        /// <param name="shader">
        /// Specifies the handle of the shader object whose source code is to be replaced.
        /// </param>
        /// <param name="string">
        /// Specifies an array of pointers to strings containing the source code to be loaded into the shader.
        /// </param>
        /// <param name="length">
        /// Specifies an array of string lengths.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void ShaderSource(uint shader, string[] @string, int[] lengths)
        {
            unsafe
            {
                fixed (int* p_length = lengths)
                {
                    Assert(Delegates.pglShaderSource != null, "pglShaderSource not implemented");
                    Delegates.pglShaderSource(shader, @string.Length, @string, p_length);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUseProgram: Installs a program object as part of current rendering state
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the handle of the program object whose executables are to be used as part of current rendering state.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void UseProgram(uint program)
        {
            Assert(Delegates.pglUseProgram != null, "pglUseProgram not implemented");
            Delegates.pglUseProgram(program);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform1f: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="v0">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform1(int location, float v0)
        {
            Assert(Delegates.pglUniform1f != null, "pglUniform1f not implemented");
            Delegates.pglUniform1f(location, v0);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform2f: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="v0">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v1">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform2(int location, float v0, float v1)
        {
            Assert(Delegates.pglUniform2f != null, "pglUniform2f not implemented");
            Delegates.pglUniform2f(location, v0, v1);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform3f: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="v0">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v1">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v2">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform3(int location, float v0, float v1, float v2)
        {
            Assert(Delegates.pglUniform3f != null, "pglUniform3f not implemented");
            Delegates.pglUniform3f(location, v0, v1, v2);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform4f: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="v0">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v1">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v2">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v3">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform4(int location, float v0, float v1, float v2, float v3)
        {
            Assert(Delegates.pglUniform4f != null, "pglUniform4f not implemented");
            Delegates.pglUniform4f(location, v0, v1, v2, v3);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform1i: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="v0">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform1(int location, int v0)
        {
            Assert(Delegates.pglUniform1i != null, "pglUniform1i not implemented");
            Delegates.pglUniform1i(location, v0);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform2i: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="v0">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v1">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform2(int location, int v0, int v1)
        {
            Assert(Delegates.pglUniform2i != null, "pglUniform2i not implemented");
            Delegates.pglUniform2i(location, v0, v1);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform3i: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="v0">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v1">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v2">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform3(int location, int v0, int v1, int v2)
        {
            Assert(Delegates.pglUniform3i != null, "pglUniform3i not implemented");
            Delegates.pglUniform3i(location, v0, v1, v2);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform4i: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="v0">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v1">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v2">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        /// <param name="v3">
        /// For the scalar commands, specifies the new values to be used for the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform4(int location, int v0, int v1, int v2, int v3)
        {
            Assert(Delegates.pglUniform4i != null, "pglUniform4i not implemented");
            Delegates.pglUniform4i(location, v0, v1, v2, v3);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform1fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform1(int location, float[] value)
        {
            unsafe
            {
                fixed (float* p_value = value)
                {
                    Assert(Delegates.pglUniform1fv != null, "pglUniform1fv not implemented");
                    Delegates.pglUniform1fv(location, value.Length, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform1fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void Uniform1(int location, int count, float* value)
        {
            Assert(Delegates.pglUniform1fv != null, "pglUniform1fv not implemented");
            Delegates.pglUniform1fv(location, count, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform1fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform1f<T>(int location, int count, T value) where T : unmanaged
        {
            Assert(Delegates.pglUniform1fv != null, "pglUniform1fv not implemented");
            unsafe
            {
                Delegates.pglUniform1fv(location, count, (float*) &value);
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform2fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform2(int location, float[] value)
        {
            Assert(value.Length > 0 && value.Length % 2 == 0, "empty or not multiple of 2");
            unsafe
            {
                fixed (float* p_value = value)
                {
                    Assert(Delegates.pglUniform2fv != null, "pglUniform2fv not implemented");
                    Delegates.pglUniform2fv(location, value.Length / 2, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform2fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void Uniform2(int location, int count, float* value)
        {
            Assert(Delegates.pglUniform2fv != null, "pglUniform2fv not implemented");
            Delegates.pglUniform2fv(location, count, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform2fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform2f<T>(int location, int count, T value) where T : unmanaged
        {
            Assert(Delegates.pglUniform2fv != null, "pglUniform2fv not implemented");
            unsafe
            {
                Delegates.pglUniform2fv(location, count, (float*) &value);
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform3fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform3(int location, float[] value)
        {
            Assert(value.Length > 0 && value.Length % 3 == 0, "empty or not multiple of 3");
            unsafe
            {
                fixed (float* p_value = value)
                {
                    Assert(Delegates.pglUniform3fv != null, "pglUniform3fv not implemented");
                    Delegates.pglUniform3fv(location, value.Length / 3, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform3fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void Uniform3(int location, int count, float* value)
        {
            Assert(Delegates.pglUniform3fv != null, "pglUniform3fv not implemented");
            Delegates.pglUniform3fv(location, count, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform3fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform3f<T>(int location, int count, T value) where T : unmanaged
        {
            Assert(Delegates.pglUniform3fv != null, "pglUniform3fv not implemented");
            unsafe
            {
                float* vecPtr = (float*) &value;
                Delegates.pglUniform3fv(location, count, vecPtr);
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform4fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform4(int location, float[] value)
        {
            Assert(value.Length > 0 && value.Length % 4 == 0, "empty or not multiple of 4");
            unsafe
            {
                fixed (float* p_value = value)
                {
                    Assert(Delegates.pglUniform4fv != null, "pglUniform4fv not implemented");
                    Delegates.pglUniform4fv(location, value.Length / 4, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform4fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void Uniform4(int location, int count, float* value)
        {
            Assert(Delegates.pglUniform4fv != null, "pglUniform4fv not implemented");
            Delegates.pglUniform4fv(location, count, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform4fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public unsafe static void Uniform4f<T>(int location, int count, T value) where T : unmanaged
        {
            Assert(Delegates.pglUniform4fv != null, "pglUniform4fv not implemented");
            float* ptr = (float*)&value;
            Delegates.pglUniform4fv(location, count, ptr);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform1iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform1(int location, int[] value)
        {
            unsafe
            {
                fixed (int* p_value = value)
                {
                    Assert(Delegates.pglUniform1iv != null, "pglUniform1iv not implemented");
                    Delegates.pglUniform1iv(location, value.Length, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform1iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void Uniform1(int location, int count, int* value)
        {
            Assert(Delegates.pglUniform1iv != null, "pglUniform1iv not implemented");
            Delegates.pglUniform1iv(location, count, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform1iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform1i<T>(int location, int count, T value) where T : struct
        {
            Assert(Delegates.pglUniform1iv != null, "pglUniform1iv not implemented");
#if NETCOREAPP1_1
			GCHandle valueHandle = GCHandle.Alloc(value);
			try {
				unsafe {
					Delegates.pglUniform1iv(location, count, (int*)valueHandle.AddrOfPinnedObject().ToPointer());
				}
			} finally {
				valueHandle.Free();
			}
#else
            unsafe
            {
                TypedReference refValue = __makeref(value);
                IntPtr refValuePtr = *(IntPtr*) (&refValue);

                Delegates.pglUniform1iv(location, count, (int*) refValuePtr.ToPointer());
            }
#endif
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform2iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform2(int location, int[] value)
        {
            Assert(value.Length > 0 && value.Length % 2 == 0, "empty or not multiple of 2");
            unsafe
            {
                fixed (int* p_value = value)
                {
                    Assert(Delegates.pglUniform2iv != null, "pglUniform2iv not implemented");
                    Delegates.pglUniform2iv(location, value.Length / 2, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform2iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void Uniform2(int location, int count, int* value)
        {
            Assert(Delegates.pglUniform2iv != null, "pglUniform2iv not implemented");
            Delegates.pglUniform2iv(location, count, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform2iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform2i<T>(int location, int count, T value) where T : struct
        {
            Assert(Delegates.pglUniform2iv != null, "pglUniform2iv not implemented");
            unsafe
            {
                TypedReference refValue = __makeref(value);
                IntPtr refValuePtr = *(IntPtr*) (&refValue);

                Delegates.pglUniform2iv(location, count, (int*) refValuePtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform3iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform3(int location, int[] value)
        {
            Assert(value.Length > 0 && value.Length % 3 == 0, "empty or not multiple of 3");
            unsafe
            {
                fixed (int* p_value = value)
                {
                    Assert(Delegates.pglUniform3iv != null, "pglUniform3iv not implemented");
                    Delegates.pglUniform3iv(location, value.Length / 3, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform3iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void Uniform3(int location, int count, int* value)
        {
            Assert(Delegates.pglUniform3iv != null, "pglUniform3iv not implemented");
            Delegates.pglUniform3iv(location, count, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform3iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform3i<T>(int location, int count, T value) where T : struct
        {
            Assert(Delegates.pglUniform3iv != null, "pglUniform3iv not implemented");
            unsafe
            {
                TypedReference refValue = __makeref(value);
                IntPtr refValuePtr = *(IntPtr*) (&refValue);

                Delegates.pglUniform3iv(location, count, (int*) refValuePtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform4iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform4(int location, int[] value)
        {
            Assert(value.Length > 0 && value.Length % 4 == 0, "empty or not multiple of 4");
            unsafe
            {
                fixed (int* p_value = value)
                {
                    Assert(Delegates.pglUniform4iv != null, "pglUniform4iv not implemented");
                    Delegates.pglUniform4iv(location, value.Length / 4, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform4iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void Uniform4(int location, int count, int* value)
        {
            Assert(Delegates.pglUniform4iv != null, "pglUniform4iv not implemented");
            Delegates.pglUniform4iv(location, count, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniform4iv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void Uniform4i<T>(int location, int count, T value) where T : struct
        {
            Assert(Delegates.pglUniform4iv != null, "pglUniform4iv not implemented");
            unsafe
            {
                TypedReference refValue = __makeref(value);
                IntPtr refValuePtr = *(IntPtr*) (&refValue);

                Delegates.pglUniform4iv(location, count, (int*) refValuePtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix2fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void UniformMatrix2(int location, bool transpose, float[] value)
        {
            Assert(value.Length > 0 && value.Length % 4 == 0, "empty or not multiple of 4");
            unsafe
            {
                fixed (float* p_value = value)
                {
                    Assert(Delegates.pglUniformMatrix2fv != null, "pglUniformMatrix2fv not implemented");
                    Delegates.pglUniformMatrix2fv(location, value.Length / 4, transpose, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix2fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void UniformMatrix2(int location, int count, bool transpose, float* value)
        {
            Assert(Delegates.pglUniformMatrix2fv != null, "pglUniformMatrix2fv not implemented");
            Delegates.pglUniformMatrix2fv(location, count, transpose, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix2fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void UniformMatrix2f<T>(int location, int count, bool transpose, T value) where T : struct
        {
            Assert(Delegates.pglUniformMatrix2fv != null, "pglUniformMatrix2fv not implemented");
#if NETCOREAPP1_1
			GCHandle valueHandle = GCHandle.Alloc(value);
			try {
				unsafe {
					Delegates.pglUniformMatrix2fv(location, count, transpose, (float*)valueHandle.AddrOfPinnedObject().ToPointer());
				}
			} finally {
				valueHandle.Free();
			}
#else
            unsafe
            {
                TypedReference refValue = __makeref(value);
                IntPtr refValuePtr = *(IntPtr*) (&refValue);

                Delegates.pglUniformMatrix2fv(location, count, transpose, (float*) refValuePtr.ToPointer());
            }
#endif
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix3fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void UniformMatrix3(int location, bool transpose, float[] value)
        {
            Assert(value.Length > 0 && value.Length % 9 == 0, "empty or not multiple of 9");
            unsafe
            {
                fixed (float* p_value = value)
                {
                    Assert(Delegates.pglUniformMatrix3fv != null, "pglUniformMatrix3fv not implemented");
                    Delegates.pglUniformMatrix3fv(location, value.Length / 9, transpose, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix3fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void UniformMatrix3(int location, int count, bool transpose, float* value)
        {
            Assert(Delegates.pglUniformMatrix3fv != null, "pglUniformMatrix3fv not implemented");
            Delegates.pglUniformMatrix3fv(location, count, transpose, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix3fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void UniformMatrix3f<T>(int location, int count, bool transpose, T value) where T : struct
        {
            Assert(Delegates.pglUniformMatrix3fv != null, "pglUniformMatrix3fv not implemented");
            unsafe
            {
                TypedReference refValue = __makeref(value);
                IntPtr refValuePtr = *(IntPtr*) (&refValue);

                Delegates.pglUniformMatrix3fv(location, count, transpose, (float*) refValuePtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix4fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void UniformMatrix4(int location, bool transpose, float[] value)
        {
            Assert(value.Length > 0 && value.Length % 16 == 0, "empty or not multiple of 16");
            unsafe
            {
                fixed (float* p_value = value)
                {
                    Assert(Delegates.pglUniformMatrix4fv != null, "pglUniformMatrix4fv not implemented");
                    Delegates.pglUniformMatrix4fv(location, value.Length / 16, transpose, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix4fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static unsafe void UniformMatrix4(int location, int count, bool transpose, float* value)
        {
            Assert(Delegates.pglUniformMatrix4fv != null, "pglUniformMatrix4fv not implemented");
            Delegates.pglUniformMatrix4fv(location, count, transpose, value);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glUniformMatrix4fv: Specify the value of a uniform variable for the current program object
        ///     </para>
        /// </summary>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be modified.
        /// </param>
        /// <param name="count">
        /// For the vector (Gl.Uniform*v) commands, specifies the number of elements that are to be modified. This should be 1 if
        /// the targeted uniform variable is not an array, and 1 or more if it is an array.
        /// </param>
        /// <param name="transpose">
        /// For the matrix commands, specifies whether to transpose the matrix as the values are loaded into the uniform variable.
        /// </param>
        /// <param name="value">
        /// Values that will be used to update the specified uniform variable.
        /// used
        /// to update the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void UniformMatrix4f<T>(int location, int count, bool transpose, T value) where T : struct
        {
            Assert(Delegates.pglUniformMatrix4fv != null, "pglUniformMatrix4fv not implemented");
            unsafe
            {
                TypedReference refValue = __makeref(value);
                IntPtr refValuePtr = *(IntPtr*) (&refValue);

                Delegates.pglUniformMatrix4fv(location, count, transpose, (float*) refValuePtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glValidateProgram: Validates a program object
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the handle of the program object to be validated.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        public static void ValidateProgram(uint program)
        {
            Assert(Delegates.pglValidateProgram != null, "pglValidateProgram not implemented");
            Delegates.pglValidateProgram(program);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib1d: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:double" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1(uint index, double x)
        {
            Assert(Delegates.pglVertexAttrib1d != null, "pglVertexAttrib1d not implemented");
            Delegates.pglVertexAttrib1d(index, x);
        }

        /// <summary>
        /// [GL4] glVertexAttrib1dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1(uint index, double[] v)
        {
            Assert(v.Length >= 1);
            unsafe
            {
                fixed (double* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib1dv != null, "pglVertexAttrib1dv not implemented");
                    Delegates.pglVertexAttrib1dv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib1dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib1(uint index, double* v)
        {
            Assert(Delegates.pglVertexAttrib1dv != null, "pglVertexAttrib1dv not implemented");
            Delegates.pglVertexAttrib1dv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib1dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1d<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib1dv != null, "pglVertexAttrib1dv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib1dv(index, (double*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib1f: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:float" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1(uint index, float x)
        {
            Assert(Delegates.pglVertexAttrib1f != null, "pglVertexAttrib1f not implemented");
            Delegates.pglVertexAttrib1f(index, x);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib1fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1(uint index, float[] v)
        {
            Assert(v.Length >= 1);
            unsafe
            {
                fixed (float* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib1fv != null, "pglVertexAttrib1fv not implemented");
                    Delegates.pglVertexAttrib1fv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib1fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib1(uint index, float* v)
        {
            Assert(Delegates.pglVertexAttrib1fv != null, "pglVertexAttrib1fv not implemented");
            Delegates.pglVertexAttrib1fv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib1fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1f<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib1fv != null, "pglVertexAttrib1fv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib1fv(index, (float*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib1s: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:short" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1(uint index, short x)
        {
            Assert(Delegates.pglVertexAttrib1s != null, "pglVertexAttrib1s not implemented");
            Delegates.pglVertexAttrib1s(index, x);
        }

        /// <summary>
        /// [GL4] glVertexAttrib1sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1(uint index, short[] v)
        {
            Assert(v.Length >= 1);
            unsafe
            {
                fixed (short* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib1sv != null, "pglVertexAttrib1sv not implemented");
                    Delegates.pglVertexAttrib1sv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib1sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib1(uint index, short* v)
        {
            Assert(Delegates.pglVertexAttrib1sv != null, "pglVertexAttrib1sv not implemented");
            Delegates.pglVertexAttrib1sv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib1sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib1s<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib1sv != null, "pglVertexAttrib1sv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib1sv(index, (short*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib2d: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:double" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:double" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2(uint index, double x, double y)
        {
            Assert(Delegates.pglVertexAttrib2d != null, "pglVertexAttrib2d not implemented");
            Delegates.pglVertexAttrib2d(index, x, y);
        }

        /// <summary>
        /// [GL4] glVertexAttrib2dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2(uint index, double[] v)
        {
            Assert(v.Length >= 2);
            unsafe
            {
                fixed (double* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib2dv != null, "pglVertexAttrib2dv not implemented");
                    Delegates.pglVertexAttrib2dv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib2dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib2(uint index, double* v)
        {
            Assert(Delegates.pglVertexAttrib2dv != null, "pglVertexAttrib2dv not implemented");
            Delegates.pglVertexAttrib2dv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib2dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2d<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib2dv != null, "pglVertexAttrib2dv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib2dv(index, (double*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib2f: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:float" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:float" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2(uint index, float x, float y)
        {
            Assert(Delegates.pglVertexAttrib2f != null, "pglVertexAttrib2f not implemented");
            Delegates.pglVertexAttrib2f(index, x, y);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib2fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2(uint index, float[] v)
        {
            Assert(v.Length >= 2);
            unsafe
            {
                fixed (float* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib2fv != null, "pglVertexAttrib2fv not implemented");
                    Delegates.pglVertexAttrib2fv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib2fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib2(uint index, float* v)
        {
            Assert(Delegates.pglVertexAttrib2fv != null, "pglVertexAttrib2fv not implemented");
            Delegates.pglVertexAttrib2fv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib2fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2f<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib2fv != null, "pglVertexAttrib2fv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib2fv(index, (float*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib2s: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:short" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:short" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2(uint index, short x, short y)
        {
            Assert(Delegates.pglVertexAttrib2s != null, "pglVertexAttrib2s not implemented");
            Delegates.pglVertexAttrib2s(index, x, y);
        }

        /// <summary>
        /// [GL4] glVertexAttrib2sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2(uint index, short[] v)
        {
            Assert(v.Length >= 2);
            unsafe
            {
                fixed (short* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib2sv != null, "pglVertexAttrib2sv not implemented");
                    Delegates.pglVertexAttrib2sv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib2sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib2(uint index, short* v)
        {
            Assert(Delegates.pglVertexAttrib2sv != null, "pglVertexAttrib2sv not implemented");
            Delegates.pglVertexAttrib2sv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib2sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib2s<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib2sv != null, "pglVertexAttrib2sv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib2sv(index, (short*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib3d: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:double" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:double" />.
        /// </param>
        /// <param name="z">
        /// A <see cref="T:double" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3(uint index, double x, double y, double z)
        {
            Assert(Delegates.pglVertexAttrib3d != null, "pglVertexAttrib3d not implemented");
            Delegates.pglVertexAttrib3d(index, x, y, z);
        }

        /// <summary>
        /// [GL4] glVertexAttrib3dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3(uint index, double[] v)
        {
            Assert(v.Length >= 3);
            unsafe
            {
                fixed (double* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib3dv != null, "pglVertexAttrib3dv not implemented");
                    Delegates.pglVertexAttrib3dv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib3dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib3(uint index, double* v)
        {
            Assert(Delegates.pglVertexAttrib3dv != null, "pglVertexAttrib3dv not implemented");
            Delegates.pglVertexAttrib3dv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib3dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3d<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib3dv != null, "pglVertexAttrib3dv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib3dv(index, (double*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib3f: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:float" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:float" />.
        /// </param>
        /// <param name="z">
        /// A <see cref="T:float" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3(uint index, float x, float y, float z)
        {
            Assert(Delegates.pglVertexAttrib3f != null, "pglVertexAttrib3f not implemented");
            Delegates.pglVertexAttrib3f(index, x, y, z);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib3fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3(uint index, float[] v)
        {
            Assert(v.Length >= 3);
            unsafe
            {
                fixed (float* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib3fv != null, "pglVertexAttrib3fv not implemented");
                    Delegates.pglVertexAttrib3fv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib3fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib3(uint index, float* v)
        {
            Assert(Delegates.pglVertexAttrib3fv != null, "pglVertexAttrib3fv not implemented");
            Delegates.pglVertexAttrib3fv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib3fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3f<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib3fv != null, "pglVertexAttrib3fv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib3fv(index, (float*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib3s: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:short" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:short" />.
        /// </param>
        /// <param name="z">
        /// A <see cref="T:short" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3(uint index, short x, short y, short z)
        {
            Assert(Delegates.pglVertexAttrib3s != null, "pglVertexAttrib3s not implemented");
            Delegates.pglVertexAttrib3s(index, x, y, z);
        }

        /// <summary>
        /// [GL4] glVertexAttrib3sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3(uint index, short[] v)
        {
            Assert(v.Length >= 3);
            unsafe
            {
                fixed (short* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib3sv != null, "pglVertexAttrib3sv not implemented");
                    Delegates.pglVertexAttrib3sv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib3sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib3(uint index, short* v)
        {
            Assert(Delegates.pglVertexAttrib3sv != null, "pglVertexAttrib3sv not implemented");
            Delegates.pglVertexAttrib3sv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib3sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib3s<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib3sv != null, "pglVertexAttrib3sv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib3sv(index, (short*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nbv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4N(uint index, sbyte[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (sbyte* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4Nbv != null, "pglVertexAttrib4Nbv not implemented");
                    Delegates.pglVertexAttrib4Nbv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nbv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4N(uint index, sbyte* v)
        {
            Assert(Delegates.pglVertexAttrib4Nbv != null, "pglVertexAttrib4Nbv not implemented");
            Delegates.pglVertexAttrib4Nbv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nbv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4Nb<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4Nbv != null, "pglVertexAttrib4Nbv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4Nbv(index, (sbyte*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Niv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4N(uint index, int[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (int* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4Niv != null, "pglVertexAttrib4Niv not implemented");
                    Delegates.pglVertexAttrib4Niv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Niv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4N(uint index, int* v)
        {
            Assert(Delegates.pglVertexAttrib4Niv != null, "pglVertexAttrib4Niv not implemented");
            Delegates.pglVertexAttrib4Niv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Niv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4Ni<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4Niv != null, "pglVertexAttrib4Niv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4Niv(index, (int*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nsv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4N(uint index, short[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (short* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4Nsv != null, "pglVertexAttrib4Nsv not implemented");
                    Delegates.pglVertexAttrib4Nsv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nsv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4N(uint index, short* v)
        {
            Assert(Delegates.pglVertexAttrib4Nsv != null, "pglVertexAttrib4Nsv not implemented");
            Delegates.pglVertexAttrib4Nsv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nsv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4Ns<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4Nsv != null, "pglVertexAttrib4Nsv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4Nsv(index, (short*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nub: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:byte" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:byte" />.
        /// </param>
        /// <param name="z">
        /// A <see cref="T:byte" />.
        /// </param>
        /// <param name="w">
        /// A <see cref="T:byte" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4N(uint index, byte x, byte y, byte z, byte w)
        {
            Assert(Delegates.pglVertexAttrib4Nub != null, "pglVertexAttrib4Nub not implemented");
            Delegates.pglVertexAttrib4Nub(index, x, y, z, w);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nubv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4N(uint index, byte[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (byte* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4Nubv != null, "pglVertexAttrib4Nubv not implemented");
                    Delegates.pglVertexAttrib4Nubv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nubv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib4N(uint index, byte* v)
        {
            Assert(Delegates.pglVertexAttrib4Nubv != null, "pglVertexAttrib4Nubv not implemented");
            Delegates.pglVertexAttrib4Nubv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nubv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4Nub<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4Nubv != null, "pglVertexAttrib4Nubv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4Nubv(index, (byte*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nuiv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4N(uint index, uint[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (uint* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4Nuiv != null, "pglVertexAttrib4Nuiv not implemented");
                    Delegates.pglVertexAttrib4Nuiv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nuiv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4N(uint index, uint* v)
        {
            Assert(Delegates.pglVertexAttrib4Nuiv != null, "pglVertexAttrib4Nuiv not implemented");
            Delegates.pglVertexAttrib4Nuiv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nuiv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4Nui<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4Nuiv != null, "pglVertexAttrib4Nuiv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4Nuiv(index, (uint*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nusv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4N(uint index, ushort[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (ushort* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4Nusv != null, "pglVertexAttrib4Nusv not implemented");
                    Delegates.pglVertexAttrib4Nusv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nusv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4N(uint index, ushort* v)
        {
            Assert(Delegates.pglVertexAttrib4Nusv != null, "pglVertexAttrib4Nusv not implemented");
            Delegates.pglVertexAttrib4Nusv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4Nusv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4Nus<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4Nusv != null, "pglVertexAttrib4Nusv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4Nusv(index, (ushort*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4bv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4(uint index, sbyte[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (sbyte* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4bv != null, "pglVertexAttrib4bv not implemented");
                    Delegates.pglVertexAttrib4bv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4bv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4(uint index, sbyte* v)
        {
            Assert(Delegates.pglVertexAttrib4bv != null, "pglVertexAttrib4bv not implemented");
            Delegates.pglVertexAttrib4bv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4bv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4b<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4bv != null, "pglVertexAttrib4bv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4bv(index, (sbyte*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4d: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:double" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:double" />.
        /// </param>
        /// <param name="z">
        /// A <see cref="T:double" />.
        /// </param>
        /// <param name="w">
        /// A <see cref="T:double" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4(uint index, double x, double y, double z, double w)
        {
            Assert(Delegates.pglVertexAttrib4d != null, "pglVertexAttrib4d not implemented");
            Delegates.pglVertexAttrib4d(index, x, y, z, w);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4(uint index, double[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (double* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4dv != null, "pglVertexAttrib4dv not implemented");
                    Delegates.pglVertexAttrib4dv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib4(uint index, double* v)
        {
            Assert(Delegates.pglVertexAttrib4dv != null, "pglVertexAttrib4dv not implemented");
            Delegates.pglVertexAttrib4dv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4dv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4d<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4dv != null, "pglVertexAttrib4dv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4dv(index, (double*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib4f: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:float" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:float" />.
        /// </param>
        /// <param name="z">
        /// A <see cref="T:float" />.
        /// </param>
        /// <param name="w">
        /// A <see cref="T:float" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4(uint index, float x, float y, float z, float w)
        {
            Assert(Delegates.pglVertexAttrib4f != null, "pglVertexAttrib4f not implemented");
            Delegates.pglVertexAttrib4f(index, x, y, z, w);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib4fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4(uint index, float[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (float* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4fv != null, "pglVertexAttrib4fv not implemented");
                    Delegates.pglVertexAttrib4fv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib4fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib4(uint index, float* v)
        {
            Assert(Delegates.pglVertexAttrib4fv != null, "pglVertexAttrib4fv not implemented");
            Delegates.pglVertexAttrib4fv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttrib4fv: Specifies the value of a generic vertex attribute
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4f<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4fv != null, "pglVertexAttrib4fv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4fv(index, (float*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4iv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4(uint index, int[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (int* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4iv != null, "pglVertexAttrib4iv not implemented");
                    Delegates.pglVertexAttrib4iv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4iv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4(uint index, int* v)
        {
            Assert(Delegates.pglVertexAttrib4iv != null, "pglVertexAttrib4iv not implemented");
            Delegates.pglVertexAttrib4iv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4iv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4i<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4iv != null, "pglVertexAttrib4iv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4iv(index, (int*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4s: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:short" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:short" />.
        /// </param>
        /// <param name="z">
        /// A <see cref="T:short" />.
        /// </param>
        /// <param name="w">
        /// A <see cref="T:short" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4(uint index, short x, short y, short z, short w)
        {
            Assert(Delegates.pglVertexAttrib4s != null, "pglVertexAttrib4s not implemented");
            Delegates.pglVertexAttrib4s(index, x, y, z, w);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4(uint index, short[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (short* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4sv != null, "pglVertexAttrib4sv not implemented");
                    Delegates.pglVertexAttrib4sv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static unsafe void VertexAttrib4(uint index, short* v)
        {
            Assert(Delegates.pglVertexAttrib4sv != null, "pglVertexAttrib4sv not implemented");
            Delegates.pglVertexAttrib4sv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4sv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program")]
        public static void VertexAttrib4s<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4sv != null, "pglVertexAttrib4sv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4sv(index, (short*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4ubv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4ub(uint index, byte[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (byte* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4ubv != null, "pglVertexAttrib4ubv not implemented");
                    Delegates.pglVertexAttrib4ubv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4ubv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4ub(uint index, byte* v)
        {
            Assert(Delegates.pglVertexAttrib4ubv != null, "pglVertexAttrib4ubv not implemented");
            Delegates.pglVertexAttrib4ubv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4ubv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4ub<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4ubv != null, "pglVertexAttrib4ubv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4ubv(index, (byte*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4uiv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4(uint index, uint[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (uint* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4uiv != null, "pglVertexAttrib4uiv not implemented");
                    Delegates.pglVertexAttrib4uiv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4uiv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4(uint index, uint* v)
        {
            Assert(Delegates.pglVertexAttrib4uiv != null, "pglVertexAttrib4uiv not implemented");
            Delegates.pglVertexAttrib4uiv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4uiv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4ui<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4uiv != null, "pglVertexAttrib4uiv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4uiv(index, (uint*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4usv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4(uint index, ushort[] v)
        {
            Assert(v.Length >= 4);
            unsafe
            {
                fixed (ushort* p_v = v)
                {
                    Assert(Delegates.pglVertexAttrib4usv != null, "pglVertexAttrib4usv not implemented");
                    Delegates.pglVertexAttrib4usv(index, p_v);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4usv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static unsafe void VertexAttrib4(uint index, ushort* v)
        {
            Assert(Delegates.pglVertexAttrib4usv != null, "pglVertexAttrib4usv not implemented");
            Delegates.pglVertexAttrib4usv(index, v);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexAttrib4usv: Specifies the value of a generic vertex attribute
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="v">
        /// For the vector commands (Gl.VertexAttrib*v), specifies a pointer to an array of values to be used for the generic
        /// vertex
        /// attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttrib4us<T>(uint index, T v) where T : struct
        {
            Assert(Delegates.pglVertexAttrib4usv != null, "pglVertexAttrib4usv not implemented");
            unsafe
            {
                TypedReference refV = __makeref(v);
                IntPtr refVPtr = *(IntPtr*) (&refV);

                Delegates.pglVertexAttrib4usv(index, (ushort*) refVPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttribPointer: define an array of generic vertex attribute data
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="size">
        /// Specifies the number of components per generic vertex attribute. Must be 1, 2, 3, 4. Additionally, the symbolic
        /// constant
        /// Gl.BGRA is accepted by Gl.VertexAttribPointer. The initial value is 4.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of each component in the array. The symbolic constants Gl.BYTE, Gl.UNSIGNED_BYTE, Gl.SHORT,
        /// Gl.UNSIGNED_SHORT, Gl.INT, and Gl.UNSIGNED_INT are accepted by Gl.VertexAttribPointer and Gl.VertexAttribIPointer.
        /// Additionally Gl.HALF_FLOAT, Gl.FLOAT, Gl.DOUBLE, Gl.FIXED, Gl.INT_2_10_10_10_REV, Gl.UNSIGNED_INT_2_10_10_10_REV and
        /// Gl.UNSIGNED_INT_10F_11F_11F_REV are accepted by Gl.VertexAttribPointer. Gl.DOUBLE is also accepted by
        /// Gl.VertexAttribLPointer and is the only token accepted by the <paramref name="type" /> parameter for that function. The
        /// initial value is Gl.FLOAT.
        /// </param>
        /// <param name="normalized">
        /// For Gl.VertexAttribPointer, specifies whether fixed-point data values should be normalized (Gl.TRUE) or converted
        /// directly as fixed-point values (Gl.FALSE) when they are accessed.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive generic vertex attributes. If <paramref name="stride" /> is 0, the
        /// generic
        /// vertex attributes are understood to be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a offset of the first component of the first generic vertex attribute in the array in the data store of the
        /// buffer currently bound to the Gl.ARRAY_BUFFER target. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttribPointer(uint index, int size, VertexAttribType type, bool normalized, int stride, IntPtr pointer)
        {
            Assert(Delegates.pglVertexAttribPointer != null, "pglVertexAttribPointer not implemented");
            Delegates.pglVertexAttribPointer(index, size, (int) type, normalized, stride, pointer);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glVertexAttribPointer: define an array of generic vertex attribute data
        ///     </para>
        /// </summary>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be modified.
        /// </param>
        /// <param name="size">
        /// Specifies the number of components per generic vertex attribute. Must be 1, 2, 3, 4. Additionally, the symbolic
        /// constant
        /// Gl.BGRA is accepted by Gl.VertexAttribPointer. The initial value is 4.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of each component in the array. The symbolic constants Gl.BYTE, Gl.UNSIGNED_BYTE, Gl.SHORT,
        /// Gl.UNSIGNED_SHORT, Gl.INT, and Gl.UNSIGNED_INT are accepted by Gl.VertexAttribPointer and Gl.VertexAttribIPointer.
        /// Additionally Gl.HALF_FLOAT, Gl.FLOAT, Gl.DOUBLE, Gl.FIXED, Gl.INT_2_10_10_10_REV, Gl.UNSIGNED_INT_2_10_10_10_REV and
        /// Gl.UNSIGNED_INT_10F_11F_11F_REV are accepted by Gl.VertexAttribPointer. Gl.DOUBLE is also accepted by
        /// Gl.VertexAttribLPointer and is the only token accepted by the <paramref name="type" /> parameter for that function. The
        /// initial value is Gl.FLOAT.
        /// </param>
        /// <param name="normalized">
        /// For Gl.VertexAttribPointer, specifies whether fixed-point data values should be normalized (Gl.TRUE) or converted
        /// directly as fixed-point values (Gl.FALSE) when they are accessed.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive generic vertex attributes. If <paramref name="stride" /> is 0, the
        /// generic
        /// vertex attributes are understood to be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a offset of the first component of the first generic vertex attribute in the array in the data store of the
        /// buffer currently bound to the Gl.ARRAY_BUFFER target. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        public static void VertexAttribPointer(uint index, int size, VertexAttribType type, bool normalized, int stride, object pointer)
        {
            GCHandle pin_pointer = GCHandle.Alloc(pointer, GCHandleType.Pinned);
            try
            {
                VertexAttribPointer(index, size, type, normalized, stride, pin_pointer.AddrOfPinnedObject());
            }
            finally
            {
                pin_pointer.Free();
            }
        }

        public static unsafe partial class Delegates
        {
            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_blend_equation_separate")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glBlendEquationSeparate(int modeRGB, int modeAlpha);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_blend_equation_separate", EntryPoint = "glBlendEquationSeparateEXT")]
            [ThreadStatic]
            public static glBlendEquationSeparate pglBlendEquationSeparate;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_draw_buffers")]
            [RequiredByFeature("GL_ATI_draw_buffers")]
            [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDrawBuffers(int n, int* bufs);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_draw_buffers", EntryPoint = "glDrawBuffersARB")]
            [RequiredByFeature("GL_ATI_draw_buffers", EntryPoint = "glDrawBuffersATI")]
            [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2", EntryPoint = "glDrawBuffersEXT")]
            [ThreadStatic]
            public static glDrawBuffers pglDrawBuffers;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ATI_separate_stencil")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glStencilOpSeparate(int face, int sfail, int dpfail, int dppass);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ATI_separate_stencil", EntryPoint = "glStencilOpSeparateATI")]
            [ThreadStatic]
            public static glStencilOpSeparate pglStencilOpSeparate;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glStencilFuncSeparate(int face, int func, int @ref, uint mask);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")] [ThreadStatic]
            public static glStencilFuncSeparate pglStencilFuncSeparate;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glStencilMaskSeparate(int face, uint mask);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")] [ThreadStatic]
            public static glStencilMaskSeparate pglStencilMaskSeparate;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glAttachShader(uint program, uint shader);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glAttachObjectARB")] [ThreadStatic]
            public static glAttachShader pglAttachShader;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glBindAttribLocation(uint program, uint index, string name);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glBindAttribLocationARB")]
            [ThreadStatic]
            public static glBindAttribLocation pglBindAttribLocation;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCompileShader(uint shader);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glCompileShaderARB")] [ThreadStatic]
            public static glCompileShader pglCompileShader;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate uint glCreateProgram();

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glCreateProgramObjectARB")]
            [ThreadStatic]
            public static glCreateProgram pglCreateProgram;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate uint glCreateShader(int type);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glCreateShaderObjectARB")]
            [ThreadStatic]
            public static glCreateShader pglCreateShader;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDeleteProgram(uint program);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glDeleteProgram pglDeleteProgram;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDeleteShader(uint shader);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glDeleteShader pglDeleteShader;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDetachShader(uint program, uint shader);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glDetachObjectARB")] [ThreadStatic]
            public static glDetachShader pglDetachShader;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDisableVertexAttribArray(uint index);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glDisableVertexAttribArrayARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glDisableVertexAttribArrayARB")]
            [ThreadStatic]
            public static glDisableVertexAttribArray pglDisableVertexAttribArray;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glEnableVertexAttribArray(uint index);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glEnableVertexAttribArrayARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glEnableVertexAttribArrayARB")]
            [ThreadStatic]
            public static glEnableVertexAttribArray pglEnableVertexAttribArray;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetActiveAttrib(uint program, uint index, int bufSize, int* length, int* size, int* type, StringBuilder name);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glGetActiveAttribARB")]
            [ThreadStatic]
            public static glGetActiveAttrib pglGetActiveAttrib;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetActiveUniform(uint program, uint index, int bufSize, int* length, int* size, int* type, StringBuilder name);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glGetActiveUniformARB")]
            [ThreadStatic]
            public static glGetActiveUniform pglGetActiveUniform;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetAttachedShaders(uint program, int maxCount, int* count, uint* shaders);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glGetAttachedShaders pglGetAttachedShaders;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate int glGetAttribLocation(uint program, string name);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glGetAttribLocationARB")]
            [ThreadStatic]
            public static glGetAttribLocation pglGetAttribLocation;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetProgramiv(uint program, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")] [ThreadStatic]
            public static glGetProgramiv pglGetProgramiv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetProgramInfoLog(uint program, int bufSize, int* length, StringBuilder infoLog);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glGetProgramInfoLog pglGetProgramInfoLog;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetShaderiv(uint shader, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glGetShaderiv pglGetShaderiv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetShaderInfoLog(uint shader, int bufSize, int* length, StringBuilder infoLog);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glGetShaderInfoLog pglGetShaderInfoLog;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetShaderSource(uint shader, int bufSize, int* length, StringBuilder source);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glGetShaderSourceARB")]
            [ThreadStatic]
            public static glGetShaderSource pglGetShaderSource;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate int glGetUniformLocation(uint program, string name);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glGetUniformLocationARB")]
            [ThreadStatic]
            public static glGetUniformLocation pglGetUniformLocation;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetUniformfv(uint program, int location, float* @params);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glGetUniformfvARB")] [ThreadStatic]
            public static glGetUniformfv pglGetUniformfv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetUniformiv(uint program, int location, int* @params);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glGetUniformivARB")] [ThreadStatic]
            public static glGetUniformiv pglGetUniformiv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetVertexAttribdv(uint index, int pname, double* @params);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glGetVertexAttribdvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glGetVertexAttribdvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glGetVertexAttribdvNV")]
            [ThreadStatic]
            public static glGetVertexAttribdv pglGetVertexAttribdv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetVertexAttribfv(uint index, int pname, float* @params);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glGetVertexAttribfvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glGetVertexAttribfvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glGetVertexAttribfvNV")]
            [ThreadStatic]
            public static glGetVertexAttribfv pglGetVertexAttribfv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetVertexAttribiv(uint index, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glGetVertexAttribivARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glGetVertexAttribivARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glGetVertexAttribivNV")]
            [ThreadStatic]
            public static glGetVertexAttribiv pglGetVertexAttribiv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetVertexAttribPointerv(uint index, int pname, IntPtr* pointer);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glGetVertexAttribPointervARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glGetVertexAttribPointervARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glGetVertexAttribPointervNV")]
            [ThreadStatic]
            public static glGetVertexAttribPointerv pglGetVertexAttribPointerv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.I1)]
            public delegate bool glIsProgram(uint program);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glIsProgram pglIsProgram;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.I1)]
            public delegate bool glIsShader(uint shader);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glIsShader pglIsShader;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glLinkProgram(uint program);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glLinkProgramARB")] [ThreadStatic]
            public static glLinkProgram pglLinkProgram;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glShaderSource(uint shader, int count, string[] @string, int* length);

            [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glShaderSourceARB")] [ThreadStatic]
            public static glShaderSource pglShaderSource;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUseProgram(uint program);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUseProgramObjectARB")]
            [ThreadStatic]
            public static glUseProgram pglUseProgram;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform1f(int location, float v0);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform1fARB")]
            [ThreadStatic]
            public static glUniform1f pglUniform1f;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform2f(int location, float v0, float v1);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform2fARB")]
            [ThreadStatic]
            public static glUniform2f pglUniform2f;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform3f(int location, float v0, float v1, float v2);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform3fARB")]
            [ThreadStatic]
            public static glUniform3f pglUniform3f;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform4f(int location, float v0, float v1, float v2, float v3);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform4fARB")]
            [ThreadStatic]
            public static glUniform4f pglUniform4f;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform1i(int location, int v0);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform1iARB")]
            [ThreadStatic]
            public static glUniform1i pglUniform1i;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform2i(int location, int v0, int v1);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform2iARB")]
            [ThreadStatic]
            public static glUniform2i pglUniform2i;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform3i(int location, int v0, int v1, int v2);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform3iARB")]
            [ThreadStatic]
            public static glUniform3i pglUniform3i;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform4i(int location, int v0, int v1, int v2, int v3);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform4iARB")]
            [ThreadStatic]
            public static glUniform4i pglUniform4i;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform1fv(int location, int count, float* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform1fvARB")]
            [ThreadStatic]
            public static glUniform1fv pglUniform1fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform2fv(int location, int count, float* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform2fvARB")]
            [ThreadStatic]
            public static glUniform2fv pglUniform2fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform3fv(int location, int count, float* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform3fvARB")]
            [ThreadStatic]
            public static glUniform3fv pglUniform3fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform4fv(int location, int count, float* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform4fvARB")]
            [ThreadStatic]
            public static glUniform4fv pglUniform4fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform1iv(int location, int count, int* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform1ivARB")]
            [ThreadStatic]
            public static glUniform1iv pglUniform1iv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform2iv(int location, int count, int* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform2ivARB")]
            [ThreadStatic]
            public static glUniform2iv pglUniform2iv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform3iv(int location, int count, int* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform3ivARB")]
            [ThreadStatic]
            public static glUniform3iv pglUniform3iv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniform4iv(int location, int count, int* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniform4ivARB")]
            [ThreadStatic]
            public static glUniform4iv pglUniform4iv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniformMatrix2fv(int location, int count, [MarshalAs(UnmanagedType.I1)] bool transpose, float* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniformMatrix2fvARB")]
            [ThreadStatic]
            public static glUniformMatrix2fv pglUniformMatrix2fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniformMatrix3fv(int location, int count, [MarshalAs(UnmanagedType.I1)] bool transpose, float* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniformMatrix3fvARB")]
            [ThreadStatic]
            public static glUniformMatrix3fv pglUniformMatrix3fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glUniformMatrix4fv(int location, int count, [MarshalAs(UnmanagedType.I1)] bool transpose, float* value);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glUniformMatrix4fvARB")]
            [ThreadStatic]
            public static glUniformMatrix4fv pglUniformMatrix4fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glValidateProgram(uint program);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_objects", EntryPoint = "glValidateProgramARB")]
            [ThreadStatic]
            public static glValidateProgram pglValidateProgram;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib1d(uint index, double x);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib1dARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib1dARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib1dNV")]
            [ThreadStatic]
            public static glVertexAttrib1d pglVertexAttrib1d;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib1dv(uint index, double* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib1dvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib1dvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib1dvNV")]
            [ThreadStatic]
            public static glVertexAttrib1dv pglVertexAttrib1dv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib1f(uint index, float x);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib1fARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib1fARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib1fNV")]
            [ThreadStatic]
            public static glVertexAttrib1f pglVertexAttrib1f;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib1fv(uint index, float* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib1fvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib1fvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib1fvNV")]
            [ThreadStatic]
            public static glVertexAttrib1fv pglVertexAttrib1fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib1s(uint index, short x);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib1sARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib1sARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib1sNV")]
            [ThreadStatic]
            public static glVertexAttrib1s pglVertexAttrib1s;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib1sv(uint index, short* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib1svARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib1svARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib1svNV")]
            [ThreadStatic]
            public static glVertexAttrib1sv pglVertexAttrib1sv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib2d(uint index, double x, double y);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib2dARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib2dARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib2dNV")]
            [ThreadStatic]
            public static glVertexAttrib2d pglVertexAttrib2d;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib2dv(uint index, double* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib2dvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib2dvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib2dvNV")]
            [ThreadStatic]
            public static glVertexAttrib2dv pglVertexAttrib2dv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib2f(uint index, float x, float y);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib2fARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib2fARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib2fNV")]
            [ThreadStatic]
            public static glVertexAttrib2f pglVertexAttrib2f;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib2fv(uint index, float* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib2fvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib2fvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib2fvNV")]
            [ThreadStatic]
            public static glVertexAttrib2fv pglVertexAttrib2fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib2s(uint index, short x, short y);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib2sARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib2sARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib2sNV")]
            [ThreadStatic]
            public static glVertexAttrib2s pglVertexAttrib2s;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib2sv(uint index, short* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib2svARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib2svARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib2svNV")]
            [ThreadStatic]
            public static glVertexAttrib2sv pglVertexAttrib2sv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib3d(uint index, double x, double y, double z);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib3dARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib3dARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib3dNV")]
            [ThreadStatic]
            public static glVertexAttrib3d pglVertexAttrib3d;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib3dv(uint index, double* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib3dvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib3dvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib3dvNV")]
            [ThreadStatic]
            public static glVertexAttrib3dv pglVertexAttrib3dv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib3f(uint index, float x, float y, float z);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib3fARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib3fARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib3fNV")]
            [ThreadStatic]
            public static glVertexAttrib3f pglVertexAttrib3f;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib3fv(uint index, float* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib3fvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib3fvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib3fvNV")]
            [ThreadStatic]
            public static glVertexAttrib3fv pglVertexAttrib3fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib3s(uint index, short x, short y, short z);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib3sARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib3sARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib3sNV")]
            [ThreadStatic]
            public static glVertexAttrib3s pglVertexAttrib3s;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib3sv(uint index, short* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib3svARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib3svARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib3svNV")]
            [ThreadStatic]
            public static glVertexAttrib3sv pglVertexAttrib3sv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4Nbv(uint index, sbyte* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4NbvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4NbvARB")]
            [ThreadStatic]
            public static glVertexAttrib4Nbv pglVertexAttrib4Nbv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4Niv(uint index, int* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4NivARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4NivARB")]
            [ThreadStatic]
            public static glVertexAttrib4Niv pglVertexAttrib4Niv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4Nsv(uint index, short* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4NsvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4NsvARB")]
            [ThreadStatic]
            public static glVertexAttrib4Nsv pglVertexAttrib4Nsv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4Nub(uint index, byte x, byte y, byte z, byte w);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4NubARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4NubARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib4ubNV")]
            [ThreadStatic]
            public static glVertexAttrib4Nub pglVertexAttrib4Nub;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4Nubv(uint index, byte* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4NubvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4NubvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib4ubvNV")]
            [ThreadStatic]
            public static glVertexAttrib4Nubv pglVertexAttrib4Nubv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4Nuiv(uint index, uint* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4NuivARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4NuivARB")]
            [ThreadStatic]
            public static glVertexAttrib4Nuiv pglVertexAttrib4Nuiv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4Nusv(uint index, ushort* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4NusvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4NusvARB")]
            [ThreadStatic]
            public static glVertexAttrib4Nusv pglVertexAttrib4Nusv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4bv(uint index, sbyte* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4bvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4bvARB")]
            [ThreadStatic]
            public static glVertexAttrib4bv pglVertexAttrib4bv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4d(uint index, double x, double y, double z, double w);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4dARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4dARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib4dNV")]
            [ThreadStatic]
            public static glVertexAttrib4d pglVertexAttrib4d;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4dv(uint index, double* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4dvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4dvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib4dvNV")]
            [ThreadStatic]
            public static glVertexAttrib4dv pglVertexAttrib4dv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4f(uint index, float x, float y, float z, float w);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4fARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4fARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib4fNV")]
            [ThreadStatic]
            public static glVertexAttrib4f pglVertexAttrib4f;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4fv(uint index, float* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4fvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4fvARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib4fvNV")]
            [ThreadStatic]
            public static glVertexAttrib4fv pglVertexAttrib4fv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4iv(uint index, int* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4ivARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4ivARB")]
            [ThreadStatic]
            public static glVertexAttrib4iv pglVertexAttrib4iv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4s(uint index, short x, short y, short z, short w);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4sARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4sARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib4sNV")]
            [ThreadStatic]
            public static glVertexAttrib4s pglVertexAttrib4s;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4sv(uint index, short* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4svARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4svARB")]
            [RequiredByFeature("GL_NV_vertex_program", EntryPoint = "glVertexAttrib4svNV")]
            [ThreadStatic]
            public static glVertexAttrib4sv pglVertexAttrib4sv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4ubv(uint index, byte* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4ubvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4ubvARB")]
            [ThreadStatic]
            public static glVertexAttrib4ubv pglVertexAttrib4ubv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4uiv(uint index, uint* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4uivARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4uivARB")]
            [ThreadStatic]
            public static glVertexAttrib4uiv pglVertexAttrib4uiv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttrib4usv(uint index, ushort* v);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttrib4usvARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttrib4usvARB")]
            [ThreadStatic]
            public static glVertexAttrib4usv pglVertexAttrib4usv;

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexAttribPointer(uint index, int size, int type, [MarshalAs(UnmanagedType.I1)] bool normalized, int stride, IntPtr pointer);

            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program", EntryPoint = "glVertexAttribPointerARB")]
            [RequiredByFeature("GL_ARB_vertex_shader", EntryPoint = "glVertexAttribPointerARB")]
            [ThreadStatic]
            public static glVertexAttribPointer pglVertexAttribPointer;
        }
    }
}