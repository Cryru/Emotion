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
        ///     <para>
        ///     [GL4] Gl.Get: data returns a single boolean value indicating whether a fragment's RGBA color values are merged into
        ///     the
        ///     framebuffer using a logical operation. The initial value is Gl.FALSE. See Gl.LogicOp.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Enable: If enabled, apply the currently selected logical operation to the computed fragment color and
        ///     color
        ///     buffer values. See Gl.LogicOp.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns a single boolean value indicating whether logical operation on color values is
        ///     enabled.
        ///     The initial value is Gl.FALSE. See Gl.LogicOp.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        public const int COLOR_LOGIC_OP = 0x0BF2;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value. This value is multiplied by an implementation-specific value and then
        /// added to the depth value of each fragment generated when a polygon is rasterized. The initial value is 0. See
        /// Gl.PolygonOffset.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int POLYGON_OFFSET_UNITS = 0x2A00;

        /// <summary>
        /// [GL4] Gl.Get: data returns a single boolean value indicating whether polygon offset is enabled for polygons in point
        /// mode. The initial value is Gl.FALSE. See Gl.PolygonOffset.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        public const int POLYGON_OFFSET_POINT = 0x2A01;

        /// <summary>
        /// [GL4] Gl.Get: data returns a single boolean value indicating whether polygon offset is enabled for polygons in line
        /// mode. The initial value is Gl.FALSE. See Gl.PolygonOffset.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        public const int POLYGON_OFFSET_LINE = 0x2A02;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.Get: data returns a single boolean value indicating whether polygon offset is enabled for polygons in fill
        ///     mode. The initial value is Gl.FALSE. See Gl.PolygonOffset.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.Get: data returns a single boolean value indicating whether polygon offset is enabled for polygons.
        ///     The
        ///     initial value is Gl.FALSE. See Gl.PolygonOffset.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public const int POLYGON_OFFSET_FILL = 0x8037;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns one value, the scaling factor used to determine the variable offset that is added to
        /// the depth value of each fragment generated when a polygon is rasterized. The initial value is 0. See Gl.PolygonOffset.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_polygon_offset")]
        public const int POLYGON_OFFSET_FACTOR = 0x8038;

        /// <summary>
        /// [GL4] Gl.Get: data returns a single value, the name of the texture currently bound to the target Gl.TEXTURE_1D. The
        /// initial value is 0. See Gl.BindTexture.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public const int TEXTURE_BINDING_1D = 0x8068;

        /// <summary>
        /// [GL4|GLES3.2] Gl.Get: data returns a single value, the name of the texture currently bound to the target Gl.TEXTURE_2D.
        /// The initial value is 0. See Gl.BindTexture.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public const int TEXTURE_BINDING_2D = 0x8069;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetTexLevelParameter: params returns a single value, the internal format of the texture image.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        public const int TEXTURE_INTERNAL_FORMAT = 0x1003;

        /// <summary>
        ///     <para>
        ///     [GL4] Gl.GetTexLevelParameter: The internal storage resolution of an individual component. The resolution chosen by
        ///     the
        ///     GL will be a close match for the resolution requested by the user with the component argument of Gl.TexImage1D,
        ///     Gl.TexImage2D, Gl.TexImage3D, Gl.CopyTexImage1D, and Gl.CopyTexImage2D. The initial value is 0.
        ///     </para>
        ///     <para>
        ///     [GLES3.2] Gl.GetTexLevelParameter: The actual internal storage resolution of an individual component.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        public const int TEXTURE_RED_SIZE = 0x805C;

        /// <summary>
        /// [GL2.1] Gl.GetTexLevelParameter:
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        public const int TEXTURE_GREEN_SIZE = 0x805D;

        /// <summary>
        /// [GL2.1] Gl.GetTexLevelParameter:
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        public const int TEXTURE_BLUE_SIZE = 0x805E;

        /// <summary>
        /// [GL2.1] Gl.GetTexLevelParameter:
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        public const int TEXTURE_ALPHA_SIZE = 0x805F;

        /// <summary>
        /// [GL] Value of GL_DOUBLE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        public const int DOUBLE = 0x140A;

        /// <summary>
        /// [GL] Value of GL_PROXY_TEXTURE_1D symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        public const int PROXY_TEXTURE_1D = 0x8063;

        /// <summary>
        /// [GL] Value of GL_PROXY_TEXTURE_2D symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        public const int PROXY_TEXTURE_2D = 0x8064;

        /// <summary>
        /// [GL] Value of GL_R3_G3_B2 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] public const int R3_G3_B2 = 0x2A10;

        /// <summary>
        /// [GL] Value of GL_RGB4 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        public const int RGB4 = 0x804F;

        /// <summary>
        /// [GL] Value of GL_RGB5 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        public const int RGB5 = 0x8050;

        /// <summary>
        /// [GL] Value of GL_RGB8 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_rgb8_rgba8", Api = "gles1|gles2|glsc2")]
        public const int RGB8 = 0x8051;

        /// <summary>
        /// [GL] Value of GL_RGB10 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        public const int RGB10 = 0x8052;

        /// <summary>
        /// [GL] Value of GL_RGB12 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        public const int RGB12 = 0x8053;

        /// <summary>
        /// [GL] Value of GL_RGB16 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        public const int RGB16 = 0x8054;

        /// <summary>
        /// [GL] Value of GL_RGBA2 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        public const int RGBA2 = 0x8055;

        /// <summary>
        /// [GL] Value of GL_RGBA4 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        public const int RGBA4 = 0x8056;

        /// <summary>
        /// [GL] Value of GL_RGB5_A1 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        public const int RGB5_A1 = 0x8057;

        /// <summary>
        /// [GL] Value of GL_RGBA8 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_rgb8_rgba8", Api = "gles1|gles2|glsc2")]
        public const int RGBA8 = 0x8058;

        /// <summary>
        /// [GL] Value of GL_RGB10_A2 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        public const int RGB10_A2 = 0x8059;

        /// <summary>
        /// [GL] Value of GL_RGBA12 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        public const int RGBA12 = 0x805A;

        /// <summary>
        /// [GL] Value of GL_RGBA16 symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        public const int RGBA16 = 0x805B;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns one value indicating the depth of the attribute stack. The initial value is 0. See
        /// Gl.PushClientAttrib.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int CLIENT_ATTRIB_STACK_DEPTH = 0x0BB1;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Enable: If enabled, apply the currently selected logical operation to the incoming index and color
        ///     buffer
        ///     indices. See Gl.LogicOp.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether a fragment's index values are merged into
        ///     the
        ///     framebuffer using a logical operation. The initial value is Gl.FALSE. See Gl.LogicOp.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int INDEX_LOGIC_OP = 0x0BF1;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns one value indicating the maximum supported depth of the client attribute stack. See
        /// Gl.PushClientAttrib.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int MAX_CLIENT_ATTRIB_STACK_DEPTH = 0x0D3B;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns one value, the size of the feedback buffer. See Gl.FeedbackBuffer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int FEEDBACK_BUFFER_SIZE = 0x0DF1;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns one value, the type of the feedback buffer. See Gl.FeedbackBuffer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int FEEDBACK_BUFFER_TYPE = 0x0DF2;

        /// <summary>
        /// [GL2.1] Gl.Get: params return one value, the size of the selection buffer. See Gl.SelectBuffer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int SELECTION_BUFFER_SIZE = 0x0DF4;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.EnableClientState: If enabled, the vertex array is enabled for writing and used during rendering when
        ///     Gl.ArrayElement, Gl.DrawArrays, Gl.DrawElements, Gl.DrawRangeElementsGl.MultiDrawArrays, or Gl.MultiDrawElements is
        ///     called. See Gl.VertexPointer.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether the vertex array is enabled. The initial
        ///     value
        ///     is Gl.FALSE. See Gl.VertexPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.EnableClientState: If enabled, the vertex array is enabled for writing and used during rendering when
        ///     Gl.DrawArrays, or Gl.DrawElements is called. See Gl.VertexPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns a single boolean value indicating whether the vertex array is enabled. The initial
        ///     value is Gl.FALSE. See Gl.VertexPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_EXT_vertex_array")]
        [RemovedByFeature("GL_VERSION_3_2")]
        public const int VERTEX_ARRAY = 0x8074;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.EnableClientState: If enabled, the normal array is enabled for writing and used during rendering when
        ///     Gl.ArrayElement, Gl.DrawArrays, Gl.DrawElements, Gl.DrawRangeElementsGl.MultiDrawArrays, or Gl.MultiDrawElements is
        ///     called. See Gl.NormalPointer.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value, indicating whether the normal array is enabled. The initial
        ///     value
        ///     is Gl.FALSE. See Gl.NormalPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.EnableClientState: If enabled, the normal array is enabled for writing and used during rendering when
        ///     Gl.DrawArrays, or Gl.DrawElements is called. See Gl.NormalPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns a single boolean value indicating whether the normal array is enabled. The initial
        ///     value is Gl.FALSE. See Gl.NormalPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int NORMAL_ARRAY = 0x8075;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.EnableClientState: If enabled, the color array is enabled for writing and used during rendering when
        ///     Gl.ArrayElement, Gl.DrawArrays, Gl.DrawElements, Gl.DrawRangeElementsGl.MultiDrawArrays, or Gl.MultiDrawElements is
        ///     called. See Gl.ColorPointer.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether the color array is enabled. The initial
        ///     value
        ///     is Gl.FALSE. See Gl.ColorPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.EnableClientState: If enabled, the color array is enabled for writing and used during rendering when
        ///     Gl.DrawArrays, or Gl.DrawElements is called. See Gl.ColorPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns a single boolean value indicating whether the color array is enabled. The initial
        ///     value
        ///     is Gl.FALSE. See Gl.ColorPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int COLOR_ARRAY = 0x8076;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.EnableClientState: If enabled, the index array is enabled for writing and used during rendering when
        ///     Gl.ArrayElement, Gl.DrawArrays, Gl.DrawElements, Gl.DrawRangeElementsGl.MultiDrawArrays, or Gl.MultiDrawElements is
        ///     called. See Gl.IndexPointer.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether the color index array is enabled. The
        ///     initial
        ///     value is Gl.FALSE. See Gl.IndexPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int INDEX_ARRAY = 0x8077;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.EnableClientState: If enabled, the texture coordinate array is enabled for writing and used during
        ///     rendering
        ///     when Gl.ArrayElement, Gl.DrawArrays, Gl.DrawElements, Gl.DrawRangeElementsGl.MultiDrawArrays, or
        ///     Gl.MultiDrawElements is
        ///     called. See Gl.TexCoordPointer.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether the texture coordinate array is enabled.
        ///     The
        ///     initial value is Gl.FALSE. See Gl.TexCoordPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.EnableClientState: If enabled, the texture coordinate array is enabled for writing and used during
        ///     rendering when Gl.DrawArrays, or Gl.DrawElements is called. See Gl.TexCoordPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns a single boolean value indicating whether the texture coordinate array is enabled.
        ///     The
        ///     initial value is Gl.FALSE. See Gl.TexCoordPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int TEXTURE_COORD_ARRAY = 0x8078;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.EnableClientState: If enabled, the edge flag array is enabled for writing and used during rendering when
        ///     Gl.ArrayElement, Gl.DrawArrays, Gl.DrawElements, Gl.DrawRangeElementsGl.MultiDrawArrays, or Gl.MultiDrawElements is
        ///     called. See Gl.EdgeFlagPointer.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns a single boolean value indicating whether the edge flag array is enabled. The
        ///     initial
        ///     value is Gl.FALSE. See Gl.EdgeFlagPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int EDGE_FLAG_ARRAY = 0x8079;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the number of coordinates per vertex in the vertex array. The initial
        ///     value is
        ///     4. See Gl.VertexPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, number of coordinates per vertex in the vertex array. See
        ///     Gl.VertexPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int VERTEX_ARRAY_SIZE = 0x807A;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the data type of each coordinate in the vertex array. The initial value
        ///     is
        ///     Gl.FLOAT. See Gl.VertexPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, returns the data type of each coordinate in the vertex array. See
        ///     Gl.VertexPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int VERTEX_ARRAY_TYPE = 0x807B;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the byte offset between consecutive vertices in the vertex array. The
        ///     initial
        ///     value is 0. See Gl.VertexPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, the byte offset between consecutive vertexes in the vertex array. See
        ///     Gl.VertexPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int VERTEX_ARRAY_STRIDE = 0x807C;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the data type of each coordinate in the normal array. The initial value
        ///     is
        ///     Gl.FLOAT. See Gl.NormalPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, the data type of each normal in the normal array. See Gl.NormalPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int NORMAL_ARRAY_TYPE = 0x807E;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the byte offset between consecutive normals in the normal array. The
        ///     initial
        ///     value is 0. See Gl.NormalPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, the byte offset between consective normals in the normal array. See
        ///     Gl.NormalPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int NORMAL_ARRAY_STRIDE = 0x807F;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the number of components per color in the color array. The initial value
        ///     is 4.
        ///     See Gl.ColorPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, the number of components per color in the color array. See
        ///     Gl.ColorPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int COLOR_ARRAY_SIZE = 0x8081;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the data type of each component in the color array. The initial value is
        ///     Gl.FLOAT. See Gl.ColorPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, returns the data type of each component in the color array. See
        ///     Gl.ColorPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int COLOR_ARRAY_TYPE = 0x8082;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the byte offset between consecutive colors in the color array. The
        ///     initial
        ///     value is 0. See Gl.ColorPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, the byte offset between consecutive colors in the color array. See
        ///     Gl.ColorPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int COLOR_ARRAY_STRIDE = 0x8083;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns one value, the data type of indexes in the color index array. The initial value is
        /// Gl.FLOAT. See Gl.IndexPointer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int INDEX_ARRAY_TYPE = 0x8085;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns one value, the byte offset between consecutive color indexes in the color index array.
        /// The initial value is 0. See Gl.IndexPointer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int INDEX_ARRAY_STRIDE = 0x8086;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the number of coordinates per element in the texture coordinate array.
        ///     The
        ///     initial value is 4. See Gl.TexCoordPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, the number of coordinates per element in the texture coordinate array.
        ///     See
        ///     Gl.TexCoordPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int TEXTURE_COORD_ARRAY_SIZE = 0x8088;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the data type of the coordinates in the texture coordinate array. The
        ///     initial
        ///     value is Gl.FLOAT. See Gl.TexCoordPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, returns the data type of each coordinate in the texture coordinate
        ///     array.
        ///     See Gl.TexCoordPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int TEXTURE_COORD_ARRAY_TYPE = 0x8089;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.Get: params returns one value, the byte offset between consecutive elements in the texture coordinate
        ///     array.
        ///     The initial value is 0. See Gl.TexCoordPointer.
        ///     </para>
        ///     <para>
        ///     [GLES1.1] Gl.Get: params returns one value, the byte offset between consecutive elements in the texture coordinate
        ///     array. See Gl.TexCoordPointer.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int TEXTURE_COORD_ARRAY_STRIDE = 0x808A;

        /// <summary>
        /// [GL2.1] Gl.Get: params returns one value, the byte offset between consecutive edge flags in the edge flag array. The
        /// initial value is 0. See Gl.EdgeFlagPointer.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int EDGE_FLAG_ARRAY_STRIDE = 0x808C;

        /// <summary>
        /// [GL2.1] Gl.GetTexLevelParameter:
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int TEXTURE_LUMINANCE_SIZE = 0x8060;

        /// <summary>
        /// [GL2.1] Gl.GetTexLevelParameter:
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int TEXTURE_INTENSITY_SIZE = 0x8061;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.GetTexParameter: Returns the residence priority of the target texture (or the named texture bound to
        ///     it). The
        ///     initial value is 1. See Gl.PrioritizeTextures.
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexParameter: Specifies the texture residence priority of the currently bound texture. Permissible
        ///     values are
        ///     in the range 01. See Gl.PrioritizeTextures and Gl.BindTexture for more information.
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture_object")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int TEXTURE_PRIORITY = 0x8066;

        /// <summary>
        /// [GL2.1] Gl.GetTexParameter: Returns the residence status of the target texture. If the value returned in params is
        /// Gl.TRUE, the texture is resident in texture memory. See Gl.AreTexturesResident.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture_object")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int TEXTURE_RESIDENT = 0x8067;

        /// <summary>
        ///     <para>
        ///     [GL2.1] Gl.TexImage1D: Each element is a single intensity value. The GL converts it to floating point, then
        ///     assembles it
        ///     into an RGBA element by replicating the intensity value three times for red, green, blue, and alpha. Each component
        ///     is
        ///     then multiplied by the signed scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range
        ///     [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage2D: Each element is a single intensity value. The GL converts it to floating point, then
        ///     assembles it
        ///     into an RGBA element by replicating the intensity value three times for red, green, blue, and alpha. Each component
        ///     is
        ///     then multiplied by the signed scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range
        ///     [0,1] (see Gl.PixelTransfer).
        ///     </para>
        ///     <para>
        ///     [GL2.1] Gl.TexImage3D: Each element is a single intensity value. The GL converts it to floating point, then
        ///     assembles it
        ///     into an RGBA element by replicating the intensity value three times for red, green, blue, and alpha. Each component
        ///     is
        ///     then multiplied by the signed scale factor Gl.c_SCALE, added to the signed bias Gl.c_BIAS, and clamped to the range
        ///     [0,1] (see Gl.PixelTransfer).
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RemovedByFeature("GL_VERSION_3_2")]
        public const int INTENSITY = 0x8049;

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDrawArrays: render primitives from array data
        ///     </para>
        /// </summary>
        /// <param name="mode">
        /// Specifies what kind of primitives to render. Symbolic constants Gl.POINTS, Gl.LINE_STRIP, Gl.LINE_LOOP, Gl.LINES,
        /// Gl.LINE_STRIP_ADJACENCY, Gl.LINES_ADJACENCY, Gl.TRIANGLE_STRIP, Gl.TRIANGLE_FAN, Gl.TRIANGLES,
        /// Gl.TRIANGLE_STRIP_ADJACENCY, Gl.TRIANGLES_ADJACENCY and Gl.PATCHES are accepted.
        /// </param>
        /// <param name="first">
        /// Specifies the starting index in the enabled arrays.
        /// </param>
        /// <param name="count">
        /// Specifies the number of indices to be rendered.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_vertex_array")]
        public static void DrawArrays(PrimitiveType mode, int first, int count)
        {
            Assert(Delegates.pglDrawArrays != null, "pglDrawArrays not implemented");
            Delegates.pglDrawArrays((int) mode, first, count);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDrawElements: render primitives from array data
        ///     </para>
        /// </summary>
        /// <param name="mode">
        /// Specifies what kind of primitives to render. Symbolic constants Gl.POINTS, Gl.LINE_STRIP, Gl.LINE_LOOP, Gl.LINES,
        /// Gl.LINE_STRIP_ADJACENCY, Gl.LINES_ADJACENCY, Gl.TRIANGLE_STRIP, Gl.TRIANGLE_FAN, Gl.TRIANGLES,
        /// Gl.TRIANGLE_STRIP_ADJACENCY, Gl.TRIANGLES_ADJACENCY and Gl.PATCHES are accepted.
        /// </param>
        /// <param name="count">
        /// Specifies the number of elements to be rendered.
        /// </param>
        /// <param name="type">
        /// Specifies the type of the values in <paramref name="indices" />. Must be one of Gl.UNSIGNED_BYTE, Gl.UNSIGNED_SHORT, or
        /// Gl.UNSIGNED_INT.
        /// </param>
        /// <param name="indices">
        /// Specifies a pointer to the location where the indices are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void DrawElements(PrimitiveType mode, int count, DrawElementsType type, IntPtr indices)
        {
            PerformanceMetrics.RegisterDrawCall();

            Assert(Delegates.pglDrawElements != null, "pglDrawElements not implemented");
            Delegates.pglDrawElements((int) mode, count, (int) type, indices);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDrawElements: render primitives from array data
        ///     </para>
        /// </summary>
        /// <param name="mode">
        /// Specifies what kind of primitives to render. Symbolic constants Gl.POINTS, Gl.LINE_STRIP, Gl.LINE_LOOP, Gl.LINES,
        /// Gl.LINE_STRIP_ADJACENCY, Gl.LINES_ADJACENCY, Gl.TRIANGLE_STRIP, Gl.TRIANGLE_FAN, Gl.TRIANGLES,
        /// Gl.TRIANGLE_STRIP_ADJACENCY, Gl.TRIANGLES_ADJACENCY and Gl.PATCHES are accepted.
        /// </param>
        /// <param name="count">
        /// Specifies the number of elements to be rendered.
        /// </param>
        /// <param name="type">
        /// Specifies the type of the values in <paramref name="indices" />. Must be one of Gl.UNSIGNED_BYTE, Gl.UNSIGNED_SHORT, or
        /// Gl.UNSIGNED_INT.
        /// </param>
        /// <param name="indices">
        /// Specifies a pointer to the location where the indices are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void DrawElements(PrimitiveType mode, int count, DrawElementsType type, object indices)
        {
            GCHandle pin_indices = GCHandle.Alloc(indices, GCHandleType.Pinned);
            try
            {
                DrawElements(mode, count, type, pin_indices.AddrOfPinnedObject());
            }
            finally
            {
                pin_indices.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetPointerv: return the address of the specified pointer
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the pointer to be returned. Must be one of Gl.DEBUG_CALLBACK_FUNCTION or Gl.DEBUG_CALLBACK_USER_PARAM.
        /// </param>
        /// <param name="params">
        /// Returns the pointer value specified by <paramref name="pname" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_vertex_array")]
        [RequiredByFeature("GL_KHR_debug")]
        [RequiredByFeature("GL_KHR_debug", Api = "gles2")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void GetPointer(GetPointervPName pname, out IntPtr @params)
        {
            unsafe
            {
                fixed (IntPtr* p_params = &@params)
                {
                    Assert(Delegates.pglGetPointerv != null, "pglGetPointerv not implemented");
                    Delegates.pglGetPointerv((int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetPointerv: return the address of the specified pointer
        ///     </para>
        /// </summary>
        /// <param name="pname">
        /// Specifies the pointer to be returned. Must be one of Gl.DEBUG_CALLBACK_FUNCTION or Gl.DEBUG_CALLBACK_USER_PARAM.
        /// </param>
        /// <param name="params">
        /// Returns the pointer value specified by <paramref name="pname" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_vertex_array")]
        [RequiredByFeature("GL_KHR_debug")]
        [RequiredByFeature("GL_KHR_debug", Api = "gles2")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void GetPointer(GetPointervPName pname, object @params)
        {
            GCHandle pin_params = GCHandle.Alloc(@params, GCHandleType.Pinned);
            try
            {
                GetPointer(pname, pin_params.AddrOfPinnedObject());
            }
            finally
            {
                pin_params.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glPolygonOffset: set the scale and units used to calculate depth values
        ///     </para>
        /// </summary>
        /// <param name="factor">
        /// Specifies a scale factor that is used to create a variable depth offset for each polygon. The initial value is 0.
        /// </param>
        /// <param name="units">
        /// Is multiplied by an implementation-specific value to create a constant depth offset. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void PolygonOffset(float factor, float units)
        {
            Assert(Delegates.pglPolygonOffset != null, "pglPolygonOffset not implemented");
            Delegates.pglPolygonOffset(factor, units);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glCopyTexImage1D: copy pixels into a 1D texture image
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture. Must be Gl.TEXTURE_1D.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="internalformat">
        /// Specifies the internal format of the texture. Must be one of the following symbolic constants: Gl.ALPHA, Gl.ALPHA4,
        /// Gl.ALPHA8, Gl.ALPHA12, Gl.ALPHA16, Gl.COMPRESSED_ALPHA, Gl.COMPRESSED_LUMINANCE, Gl.COMPRESSED_LUMINANCE_ALPHA,
        /// Gl.COMPRESSED_INTENSITY, Gl.COMPRESSED_RGB, Gl.COMPRESSED_RGBA, Gl.DEPTH_COMPONENT, Gl.DEPTH_COMPONENT16,
        /// Gl.DEPTH_COMPONENT24, Gl.DEPTH_COMPONENT32, Gl.LUMINANCE, Gl.LUMINANCE4, Gl.LUMINANCE8, Gl.LUMINANCE12, Gl.LUMINANCE16,
        /// Gl.LUMINANCE_ALPHA, Gl.LUMINANCE4_ALPHA4, Gl.LUMINANCE6_ALPHA2, Gl.LUMINANCE8_ALPHA8, Gl.LUMINANCE12_ALPHA4,
        /// Gl.LUMINANCE12_ALPHA12, Gl.LUMINANCE16_ALPHA16, Gl.INTENSITY, Gl.INTENSITY4, Gl.INTENSITY8, Gl.INTENSITY12,
        /// Gl.INTENSITY16, Gl.RGB, Gl.R3_G3_B2, Gl.RGB4, Gl.RGB5, Gl.RGB8, Gl.RGB10, Gl.RGB12, Gl.RGB16, Gl.RGBA, Gl.RGBA2,
        /// Gl.RGBA4, Gl.RGB5_A1, Gl.RGBA8, Gl.RGB10_A2, Gl.RGBA12, Gl.RGBA16, Gl.SLUMINANCE, Gl.SLUMINANCE8, Gl.SLUMINANCE_ALPHA,
        /// Gl.SLUMINANCE8_ALPHA8, Gl.SRGB, Gl.SRGB8, Gl.SRGB_ALPHA, or Gl.SRGB8_ALPHA8.
        /// </param>
        /// <param name="x">
        /// Specify the window coordinates of the left corner of the row of pixels to be copied.
        /// </param>
        /// <param name="y">
        /// Specify the window coordinates of the left corner of the row of pixels to be copied.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture image. Must be 0 or 2n+2⁡border for some integer n. The height of the texture image
        /// is 1.
        /// </param>
        /// <param name="border">
        /// Specifies the width of the border. Must be either 0 or 1.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_EXT_copy_texture")]
        public static void CopyTexImage1D(TextureTarget target, int level, InternalFormat internalformat, int x, int y, int width, int border)
        {
            Assert(Delegates.pglCopyTexImage1D != null, "pglCopyTexImage1D not implemented");
            Delegates.pglCopyTexImage1D((int) target, level, (int) internalformat, x, y, width, border);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1] glCopyTexImage2D: copy pixels into a 2D texture image
        ///     </para>
        ///     <para>
        ///     [GLES1.1] glCopyTexImage2D: specify a two-dimensional texture image with pixels from the color buffer
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture. Must be Gl.TEXTURE_2D, Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_Y, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, or
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="internalformat">
        /// Specifies the internal format of the texture. Must be one of the following symbolic constants: Gl.ALPHA, Gl.ALPHA4,
        /// Gl.ALPHA8, Gl.ALPHA12, Gl.ALPHA16, Gl.COMPRESSED_ALPHA, Gl.COMPRESSED_LUMINANCE, Gl.COMPRESSED_LUMINANCE_ALPHA,
        /// Gl.COMPRESSED_INTENSITY, Gl.COMPRESSED_RGB, Gl.COMPRESSED_RGBA, Gl.DEPTH_COMPONENT, Gl.DEPTH_COMPONENT16,
        /// Gl.DEPTH_COMPONENT24, Gl.DEPTH_COMPONENT32, Gl.LUMINANCE, Gl.LUMINANCE4, Gl.LUMINANCE8, Gl.LUMINANCE12, Gl.LUMINANCE16,
        /// Gl.LUMINANCE_ALPHA, Gl.LUMINANCE4_ALPHA4, Gl.LUMINANCE6_ALPHA2, Gl.LUMINANCE8_ALPHA8, Gl.LUMINANCE12_ALPHA4,
        /// Gl.LUMINANCE12_ALPHA12, Gl.LUMINANCE16_ALPHA16, Gl.INTENSITY, Gl.INTENSITY4, Gl.INTENSITY8, Gl.INTENSITY12,
        /// Gl.INTENSITY16, Gl.RGB, Gl.R3_G3_B2, Gl.RGB4, Gl.RGB5, Gl.RGB8, Gl.RGB10, Gl.RGB12, Gl.RGB16, Gl.RGBA, Gl.RGBA2,
        /// Gl.RGBA4, Gl.RGB5_A1, Gl.RGBA8, Gl.RGB10_A2, Gl.RGBA12, Gl.RGBA16, Gl.SLUMINANCE, Gl.SLUMINANCE8, Gl.SLUMINANCE_ALPHA,
        /// Gl.SLUMINANCE8_ALPHA8, Gl.SRGB, Gl.SRGB8, Gl.SRGB_ALPHA, or Gl.SRGB8_ALPHA8.
        /// </param>
        /// <param name="x">
        /// Specify the window coordinates of the lower left corner of the rectangular region of pixels to be copied.
        /// </param>
        /// <param name="y">
        /// Specify the window coordinates of the lower left corner of the rectangular region of pixels to be copied.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture image. Must be 0 or 2n+2⁡border for some integer n.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture image. Must be 0 or 2m+2⁡border for some integer m.
        /// </param>
        /// <param name="border">
        /// Specifies the width of the border. Must be either 0 or 1.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_copy_texture")]
        public static void CopyTexImage2D(TextureTarget target, int level, InternalFormat internalformat, int x, int y, int width, int height, int border)
        {
            Assert(Delegates.pglCopyTexImage2D != null, "pglCopyTexImage2D not implemented");
            Delegates.pglCopyTexImage2D((int) target, level, (int) internalformat, x, y, width, height, border);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glCopyTexSubImage1D: copy a one-dimensional texture subimage
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture. Must be Gl.TEXTURE_1D.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies the texel offset within the texture array.
        /// </param>
        /// <param name="x">
        /// Specify the window coordinates of the left corner of the row of pixels to be copied.
        /// </param>
        /// <param name="y">
        /// Specify the window coordinates of the left corner of the row of pixels to be copied.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_EXT_copy_texture")]
        public static void CopyTexSubImage1D(TextureTarget target, int level, int xoffset, int x, int y, int width)
        {
            Assert(Delegates.pglCopyTexSubImage1D != null, "pglCopyTexSubImage1D not implemented");
            Delegates.pglCopyTexSubImage1D((int) target, level, xoffset, x, y, width);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1] glCopyTexSubImage2D: copy a two-dimensional texture subimage
        ///     </para>
        ///     <para>
        ///     [GLES1.1] glCopyTexSubImage2D: specify a two-dimensional texture subimage with pixels from the color buffer
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target texture. Must be Gl.TEXTURE_2D, Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_Y, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, or
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="x">
        /// Specify the window coordinates of the lower left corner of the rectangular region of pixels to be copied.
        /// </param>
        /// <param name="y">
        /// Specify the window coordinates of the lower left corner of the rectangular region of pixels to be copied.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_copy_texture")]
        public static void CopyTexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int x, int y, int width, int height)
        {
            Assert(Delegates.pglCopyTexSubImage2D != null, "pglCopyTexSubImage2D not implemented");
            Delegates.pglCopyTexSubImage2D((int) target, level, xoffset, yoffset, x, y, width, height);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTexSubImage1D: specify a one-dimensional texture subimage
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.TexSubImage1D. Must be Gl.TEXTURE_1D.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_EXT_subtexture")]
        public static void TexSubImage1D(TextureTarget target, int level, int xoffset, int width, PixelFormat format, PixelType type, IntPtr pixels)
        {
            Assert(Delegates.pglTexSubImage1D != null, "pglTexSubImage1D not implemented");
            Delegates.pglTexSubImage1D((int) target, level, xoffset, width, (int) format, (int) type, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTexSubImage1D: specify a one-dimensional texture subimage
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.TexSubImage1D. Must be Gl.TEXTURE_1D.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_EXT_subtexture")]
        public static void TexSubImage1D(TextureTarget target, int level, int xoffset, int width, PixelFormat format, PixelType type, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                TexSubImage1D(target, level, xoffset, width, format, type, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glTexSubImage2D: specify a two-dimensional texture subimage
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.TexSubImage2D. Must be Gl.TEXTURE_2D,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, or Gl.TEXTURE_1D_ARRAY.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.BGRA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_subtexture")]
        public static void TexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, IntPtr pixels)
        {
            Assert(Delegates.pglTexSubImage2D != null, "pglTexSubImage2D not implemented");
            Delegates.pglTexSubImage2D((int) target, level, xoffset, yoffset, width, height, (int) format, (int) type, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glTexSubImage2D: specify a two-dimensional texture subimage
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.TexSubImage2D. Must be Gl.TEXTURE_2D,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, or Gl.TEXTURE_1D_ARRAY.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.BGRA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_subtexture")]
        public static void TexSubImage2D(TextureTarget target, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                TexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glBindTexture: bind a named texture to a texturing target
        ///     </para>
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound. Must be one of Gl.TEXTURE_1D, Gl.TEXTURE_2D, Gl.TEXTURE_3D,
        /// Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_RECTANGLE, Gl.TEXTURE_CUBE_MAP, Gl.TEXTURE_CUBE_MAP_ARRAY,
        /// Gl.TEXTURE_BUFFER, Gl.TEXTURE_2D_MULTISAMPLE or Gl.TEXTURE_2D_MULTISAMPLE_ARRAY.
        /// </param>
        /// <param name="texture">
        /// Specifies the name of a texture.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture_object")]
        public static void BindTexture(TextureTarget target, uint texture)
        {
            Assert(Delegates.pglBindTexture != null, "pglBindTexture not implemented");
            Delegates.pglBindTexture((int) target, texture);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glDeleteTextures: delete named textures
        ///     </para>
        /// </summary>
        /// <param name="textures">
        /// Specifies an array of textures to be deleted.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static void DeleteTextures(params uint[] textures)
        {
            unsafe
            {
                fixed (uint* p_textures = textures)
                {
                    Assert(Delegates.pglDeleteTextures != null, "pglDeleteTextures not implemented");
                    Delegates.pglDeleteTextures(textures.Length, p_textures);
                }
            }

            DebugCheckErrors(null);
        }

        public unsafe static void DeleteTexture(uint texturePointer)
        {
            Assert(Delegates.pglDeleteTextures != null, "pglDeleteTextures not implemented");
            Delegates.pglDeleteTextures(1, &texturePointer);

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGenTextures: generate texture names
        ///     </para>
        /// </summary>
        /// <param name="textures">
        /// Specifies an array in which the generated texture names are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static void GenTextures(uint[] textures)
        {
            unsafe
            {
                fixed (uint* p_textures = textures)
                {
                    Assert(Delegates.pglGenTextures != null, "pglGenTextures not implemented");
                    Delegates.pglGenTextures(textures.Length, p_textures);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGenTextures: generate texture names
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        public static uint GenTexture()
        {
            uint retValue;
            unsafe
            {
                Delegates.pglGenTextures(1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glIsTexture: determine if a name corresponds to a texture
        ///     </para>
        /// </summary>
        /// <param name="texture">
        /// Specifies a value that may be the name of a texture.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        public static bool IsTexture(uint texture)
        {
            bool retValue;

            Assert(Delegates.pglIsTexture != null, "pglIsTexture not implemented");
            retValue = Delegates.pglIsTexture(texture);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        /// [GL2.1] glArrayElement: render a vertex using the specified vertex array element
        /// </summary>
        /// <param name="i">
        /// Specifies an index into the enabled vertex data arrays.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_EXT_vertex_array")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void ArrayElement(int i)
        {
            Assert(Delegates.pglArrayElement != null, "pglArrayElement not implemented");
            Delegates.pglArrayElement(i);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glColorPointer: define an array of colors
        ///     </para>
        /// </summary>
        /// <param name="size">
        /// Specifies the number of components per color. Must be 3 or 4. The initial value is 4.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of each color component in the array. Symbolic constants Gl.BYTE, Gl.UNSIGNED_BYTE, Gl.SHORT,
        /// Gl.UNSIGNED_SHORT, Gl.INT, Gl.UNSIGNED_INT, Gl.FLOAT, and Gl.DOUBLE are accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive colors. If <paramref name="stride" /> is 0, the colors are understood to
        /// be
        /// tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first component of the first color element in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void ColorPointer(int size, ColorPointerType type, int stride, IntPtr pointer)
        {
            Assert(Delegates.pglColorPointer != null, "pglColorPointer not implemented");
            Delegates.pglColorPointer(size, (int) type, stride, pointer);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glColorPointer: define an array of colors
        ///     </para>
        /// </summary>
        /// <param name="size">
        /// Specifies the number of components per color. Must be 3 or 4. The initial value is 4.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of each color component in the array. Symbolic constants Gl.BYTE, Gl.UNSIGNED_BYTE, Gl.SHORT,
        /// Gl.UNSIGNED_SHORT, Gl.INT, Gl.UNSIGNED_INT, Gl.FLOAT, and Gl.DOUBLE are accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive colors. If <paramref name="stride" /> is 0, the colors are understood to
        /// be
        /// tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first component of the first color element in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void ColorPointer(int size, ColorPointerType type, int stride, object pointer)
        {
            GCHandle pin_pointer = GCHandle.Alloc(pointer, GCHandleType.Pinned);
            try
            {
                ColorPointer(size, type, stride, pin_pointer.AddrOfPinnedObject());
            }
            finally
            {
                pin_pointer.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glDisableClientState: enable or disable client-side capability
        ///     </para>
        /// </summary>
        /// <param name="array">
        /// A <see cref="T:EnableCap" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void DisableClientState(EnableCap array)
        {
            Assert(Delegates.pglDisableClientState != null, "pglDisableClientState not implemented");
            Delegates.pglDisableClientState((int) array);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glEdgeFlagPointer: define an array of edge flags
        /// </summary>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive edge flags. If <paramref name="stride" /> is 0, the edge flags are
        /// understood to be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first edge flag in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void EdgeFlagPointer(int stride, IntPtr pointer)
        {
            Assert(Delegates.pglEdgeFlagPointer != null, "pglEdgeFlagPointer not implemented");
            Delegates.pglEdgeFlagPointer(stride, pointer);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glEdgeFlagPointer: define an array of edge flags
        /// </summary>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive edge flags. If <paramref name="stride" /> is 0, the edge flags are
        /// understood to be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first edge flag in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void EdgeFlagPointer(int stride, object pointer)
        {
            GCHandle pin_pointer = GCHandle.Alloc(pointer, GCHandleType.Pinned);
            try
            {
                EdgeFlagPointer(stride, pin_pointer.AddrOfPinnedObject());
            }
            finally
            {
                pin_pointer.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glEnableClientState: enable or disable client-side capability
        ///     </para>
        /// </summary>
        /// <param name="array">
        /// A <see cref="T:EnableCap" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void EnableClientState(EnableCap array)
        {
            Assert(Delegates.pglEnableClientState != null, "pglEnableClientState not implemented");
            Delegates.pglEnableClientState((int) array);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glIndexPointer: define an array of color indexes
        /// </summary>
        /// <param name="type">
        /// Specifies the data type of each color index in the array. Symbolic constants Gl.UNSIGNED_BYTE, Gl.SHORT, Gl.INT,
        /// Gl.FLOAT, and Gl.DOUBLE are accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive color indexes. If <paramref name="stride" /> is 0, the color indexes are
        /// understood to be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first index in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void IndexPointer(IndexPointerType type, int stride, IntPtr pointer)
        {
            Assert(Delegates.pglIndexPointer != null, "pglIndexPointer not implemented");
            Delegates.pglIndexPointer((int) type, stride, pointer);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glIndexPointer: define an array of color indexes
        /// </summary>
        /// <param name="type">
        /// Specifies the data type of each color index in the array. Symbolic constants Gl.UNSIGNED_BYTE, Gl.SHORT, Gl.INT,
        /// Gl.FLOAT, and Gl.DOUBLE are accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive color indexes. If <paramref name="stride" /> is 0, the color indexes are
        /// understood to be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first index in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void IndexPointer(IndexPointerType type, int stride, object pointer)
        {
            GCHandle pin_pointer = GCHandle.Alloc(pointer, GCHandleType.Pinned);
            try
            {
                IndexPointer(type, stride, pin_pointer.AddrOfPinnedObject());
            }
            finally
            {
                pin_pointer.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glNormalPointer: define an array of normals
        ///     </para>
        /// </summary>
        /// <param name="type">
        /// Specifies the data type of each coordinate in the array. Symbolic constants Gl.BYTE, Gl.SHORT, Gl.INT, Gl.FLOAT, and
        /// Gl.DOUBLE are accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive normals. If <paramref name="stride" /> is 0, the normals are understood
        /// to
        /// be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first coordinate of the first normal in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void NormalPointer(NormalPointerType type, int stride, IntPtr pointer)
        {
            Assert(Delegates.pglNormalPointer != null, "pglNormalPointer not implemented");
            Delegates.pglNormalPointer((int) type, stride, pointer);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glNormalPointer: define an array of normals
        ///     </para>
        /// </summary>
        /// <param name="type">
        /// Specifies the data type of each coordinate in the array. Symbolic constants Gl.BYTE, Gl.SHORT, Gl.INT, Gl.FLOAT, and
        /// Gl.DOUBLE are accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive normals. If <paramref name="stride" /> is 0, the normals are understood
        /// to
        /// be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first coordinate of the first normal in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void NormalPointer(NormalPointerType type, int stride, object pointer)
        {
            GCHandle pin_pointer = GCHandle.Alloc(pointer, GCHandleType.Pinned);
            try
            {
                NormalPointer(type, stride, pin_pointer.AddrOfPinnedObject());
            }
            finally
            {
                pin_pointer.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexCoordPointer: define an array of texture coordinates
        ///     </para>
        /// </summary>
        /// <param name="size">
        /// Specifies the number of coordinates per array element. Must be 1, 2, 3, or 4. The initial value is 4.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of each texture coordinate. Symbolic constants Gl.SHORT, Gl.INT, Gl.FLOAT, or Gl.DOUBLE are
        /// accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive texture coordinate sets. If <paramref name="stride" /> is 0, the array
        /// elements are understood to be tightly packed. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first coordinate of the first texture coordinate set in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void TexCoordPointer(int size, TexCoordPointerType type, int stride, IntPtr pointer)
        {
            Assert(Delegates.pglTexCoordPointer != null, "pglTexCoordPointer not implemented");
            Delegates.pglTexCoordPointer(size, (int) type, stride, pointer);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1|GLES1.1] glTexCoordPointer: define an array of texture coordinates
        ///     </para>
        /// </summary>
        /// <param name="size">
        /// Specifies the number of coordinates per array element. Must be 1, 2, 3, or 4. The initial value is 4.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of each texture coordinate. Symbolic constants Gl.SHORT, Gl.INT, Gl.FLOAT, or Gl.DOUBLE are
        /// accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive texture coordinate sets. If <paramref name="stride" /> is 0, the array
        /// elements are understood to be tightly packed. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first coordinate of the first texture coordinate set in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void TexCoordPointer(int size, TexCoordPointerType type, int stride, object pointer)
        {
            GCHandle pin_pointer = GCHandle.Alloc(pointer, GCHandleType.Pinned);
            try
            {
                TexCoordPointer(size, type, stride, pin_pointer.AddrOfPinnedObject());
            }
            finally
            {
                pin_pointer.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1] glVertexPointer: define an array of vertex data
        ///     </para>
        ///     <para>
        ///     [GLES1.1] glVertexPointer: define an array of vertex coordinates
        ///     </para>
        /// </summary>
        /// <param name="size">
        /// Specifies the number of coordinates per vertex. Must be 2, 3, or 4. The initial value is 4.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of each coordinate in the array. Symbolic constants Gl.SHORT, Gl.INT, Gl.FLOAT, or Gl.DOUBLE
        /// are
        /// accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive vertices. If <paramref name="stride" /> is 0, the vertices are understood
        /// to be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first coordinate of the first vertex in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void VertexPointer(int size, VertexPointerType type, int stride, IntPtr pointer)
        {
            Assert(Delegates.pglVertexPointer != null, "pglVertexPointer not implemented");
            Delegates.pglVertexPointer(size, (int) type, stride, pointer);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL2.1] glVertexPointer: define an array of vertex data
        ///     </para>
        ///     <para>
        ///     [GLES1.1] glVertexPointer: define an array of vertex coordinates
        ///     </para>
        /// </summary>
        /// <param name="size">
        /// Specifies the number of coordinates per vertex. Must be 2, 3, or 4. The initial value is 4.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of each coordinate in the array. Symbolic constants Gl.SHORT, Gl.INT, Gl.FLOAT, or Gl.DOUBLE
        /// are
        /// accepted. The initial value is Gl.FLOAT.
        /// </param>
        /// <param name="stride">
        /// Specifies the byte offset between consecutive vertices. If <paramref name="stride" /> is 0, the vertices are understood
        /// to be tightly packed in the array. The initial value is 0.
        /// </param>
        /// <param name="pointer">
        /// Specifies a pointer to the first coordinate of the first vertex in the array. The initial value is 0.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void VertexPointer(int size, VertexPointerType type, int stride, object pointer)
        {
            GCHandle pin_pointer = GCHandle.Alloc(pointer, GCHandleType.Pinned);
            try
            {
                VertexPointer(size, type, stride, pin_pointer.AddrOfPinnedObject());
            }
            finally
            {
                pin_pointer.Free();
            }
        }

        /// <summary>
        /// [GL2.1] glAreTexturesResident: determine if textures are loaded in texture memory
        /// </summary>
        /// <param name="textures">
        /// Specifies an array containing the names of the textures to be queried.
        /// </param>
        /// <param name="residences">
        /// Specifies an array in which the texture residence status is returned. The residence status of a texture named by an
        /// element of <paramref name="textures" /> is returned in the corresponding element of <paramref name="residences" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static bool AreTexturesResident(uint[] textures, [Out] byte[] residences)
        {
            bool retValue;

            unsafe
            {
                fixed (uint* p_textures = textures)
                fixed (byte* p_residences = residences)
                {
                    Assert(Delegates.pglAreTexturesResident != null, "pglAreTexturesResident not implemented");
                    retValue = Delegates.pglAreTexturesResident(textures.Length, p_textures, p_residences);
                }
            }

            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        /// [GL2.1] glPrioritizeTextures: set texture residence priority
        /// </summary>
        /// <param name="textures">
        /// Specifies an array containing the names of the textures to be prioritized.
        /// </param>
        /// <param name="priorities">
        /// Specifies an array containing the texture priorities. A priority given in an element of <paramref name="priorities" />
        /// applies to the texture named by the corresponding element of <paramref name="textures" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_EXT_texture_object")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void PrioritizeTextures(uint[] textures, params float[] priorities)
        {
            unsafe
            {
                fixed (uint* p_textures = textures)
                fixed (float* p_priorities = priorities)
                {
                    Assert(Delegates.pglPrioritizeTextures != null, "pglPrioritizeTextures not implemented");
                    Delegates.pglPrioritizeTextures(textures.Length, p_textures, p_priorities);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glIndexub: set the current color index
        /// </summary>
        /// <param name="c">
        /// Specifies the new value for the current color index.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void Index(byte c)
        {
            Assert(Delegates.pglIndexub != null, "pglIndexub not implemented");
            Delegates.pglIndexub(c);
        }

        /// <summary>
        /// [GL2.1] glIndexubv: set the current color index
        /// </summary>
        /// <param name="c">
        /// Specifies the new value for the current color index.
        /// </param>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void Index(byte[] c)
        {
            Assert(c.Length >= 1);
            unsafe
            {
                fixed (byte* p_c = c)
                {
                    Assert(Delegates.pglIndexubv != null, "pglIndexubv not implemented");
                    Delegates.pglIndexubv(p_c);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL2.1] glPopClientAttrib: push and pop the client attribute stack
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
        public static void PopClientAttrib()
        {
            Assert(Delegates.pglPopClientAttrib != null, "pglPopClientAttrib not implemented");
            Delegates.pglPopClientAttrib();
            DebugCheckErrors(null);
        }

        public static unsafe partial class Delegates
        {
            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_vertex_array")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDrawArrays(int mode, int first, int count);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_vertex_array", EntryPoint = "glDrawArraysEXT")]
            [ThreadStatic]
            public static glDrawArrays pglDrawArrays;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDrawElements(int mode, int count, int type, IntPtr indices);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glDrawElements pglDrawElements;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_EXT_vertex_array")]
            [RequiredByFeature("GL_KHR_debug")]
            [RequiredByFeature("GL_KHR_debug", Api = "gles2")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetPointerv(int pname, IntPtr* @params);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_EXT_vertex_array", EntryPoint = "glGetPointervEXT")]
            [RequiredByFeature("GL_KHR_debug")]
            [RequiredByFeature("GL_KHR_debug", Api = "gles2", EntryPoint = "glGetPointervKHR")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [ThreadStatic]
            public static glGetPointerv pglGetPointerv;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPolygonOffset(float factor, float units);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1", Profile = "common")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glPolygonOffset pglPolygonOffset;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_EXT_copy_texture")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCopyTexImage1D(int target, int level, int internalformat, int x, int y, int width, int border);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_copy_texture", EntryPoint = "glCopyTexImage1DEXT")] [ThreadStatic]
            public static glCopyTexImage1D pglCopyTexImage1D;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_copy_texture")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCopyTexImage2D(int target, int level, int internalformat, int x, int y, int width, int height, int border);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_copy_texture", EntryPoint = "glCopyTexImage2DEXT")]
            [ThreadStatic]
            public static glCopyTexImage2D pglCopyTexImage2D;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_EXT_copy_texture")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCopyTexSubImage1D(int target, int level, int xoffset, int x, int y, int width);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_copy_texture", EntryPoint = "glCopyTexSubImage1DEXT")] [ThreadStatic]
            public static glCopyTexSubImage1D pglCopyTexSubImage1D;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_copy_texture")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCopyTexSubImage2D(int target, int level, int xoffset, int yoffset, int x, int y, int width, int height);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_copy_texture", EntryPoint = "glCopyTexSubImage2DEXT")]
            [ThreadStatic]
            public static glCopyTexSubImage2D pglCopyTexSubImage2D;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_EXT_subtexture")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexSubImage1D(int target, int level, int xoffset, int width, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_subtexture", EntryPoint = "glTexSubImage1DEXT")] [ThreadStatic]
            public static glTexSubImage1D pglTexSubImage1D;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_subtexture")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexSubImage2D(int target, int level, int xoffset, int yoffset, int width, int height, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_subtexture", EntryPoint = "glTexSubImage2DEXT")]
            [ThreadStatic]
            public static glTexSubImage2D pglTexSubImage2D;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_texture_object")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glBindTexture(int target, uint texture);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_texture_object", EntryPoint = "glBindTextureEXT")]
            [ThreadStatic]
            public static glBindTexture pglBindTexture;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDeleteTextures(int n, uint* textures);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glDeleteTextures pglDeleteTextures;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGenTextures(int n, uint* textures);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [ThreadStatic]
            public static glGenTextures pglGenTextures;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.I1)]
            public delegate bool glIsTexture(uint texture);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [ThreadStatic]
            public static glIsTexture pglIsTexture;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_EXT_vertex_array")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glArrayElement(int i);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array", EntryPoint = "glArrayElementEXT")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glArrayElement pglArrayElement;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glColorPointer(int size, int type, int stride, IntPtr pointer);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glColorPointer pglColorPointer;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDisableClientState(int array);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glDisableClientState pglDisableClientState;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glEdgeFlagPointer(int stride, IntPtr pointer);

            [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glEdgeFlagPointer pglEdgeFlagPointer;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glEnableClientState(int array);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glEnableClientState pglEnableClientState;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glIndexPointer(int type, int stride, IntPtr pointer);

            [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glIndexPointer pglIndexPointer;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glInterleavedArrays(int format, int stride, IntPtr pointer);

            [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glInterleavedArrays pglInterleavedArrays;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNormalPointer(int type, int stride, IntPtr pointer);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glNormalPointer pglNormalPointer;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTexCoordPointer(int size, int type, int stride, IntPtr pointer);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glTexCoordPointer pglTexCoordPointer;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexPointer(int size, int type, int stride, IntPtr pointer);

            [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glVertexPointer pglVertexPointer;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.I1)]
            public delegate bool glAreTexturesResident(int n, uint* textures, byte* residences);

            [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glAreTexturesResident pglAreTexturesResident;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_EXT_texture_object")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPrioritizeTextures(int n, uint* textures, float* priorities);

            [RequiredByFeature("GL_VERSION_1_1")]
            [RequiredByFeature("GL_EXT_texture_object", EntryPoint = "glPrioritizeTexturesEXT")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [ThreadStatic]
            public static glPrioritizeTextures pglPrioritizeTextures;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glIndexub(byte c);

            [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glIndexub pglIndexub;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glIndexubv(byte* c);

            [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glIndexubv pglIndexubv;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPopClientAttrib();

            [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glPopClientAttrib pglPopClientAttrib;

            [RequiredByFeature("GL_VERSION_1_1")]
            [RemovedByFeature("GL_VERSION_3_2", Profile = "core")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPushClientAttrib(uint mask);

            [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2", Profile = "core")] [ThreadStatic]
            public static glPushClientAttrib pglPushClientAttrib;
        }
    }
}