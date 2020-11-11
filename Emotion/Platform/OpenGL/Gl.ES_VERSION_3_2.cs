#pragma warning disable 649, 1572, 1573

// ReSharper disable RedundantUsingDirective

#region Using

using System;
using System.Diagnostics;
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
// ReSharper disable IdentifierTypo
namespace OpenGL
{
    public partial class Gl
    {
        /// <summary>
        /// [GLES3.2] Gl.Get: data returns a pair of values indicating the range of widths supported for lines drawn when
        /// Gl.SAMPLE_BUFFERS is one. See Gl.LineWidth.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_2_compatibility", Api = "gl|glcore")]
        public const int MULTISAMPLE_LINE_WIDTH_RANGE = 0x9381;

        /// <summary>
        /// [GL] Value of GL_MULTISAMPLE_LINE_WIDTH_GRANULARITY symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_2_compatibility", Api = "gl|glcore")]
        public const int MULTISAMPLE_LINE_WIDTH_GRANULARITY = 0x9382;

        /// <summary>
        /// [GL] Value of GL_MULTIPLY symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int MULTIPLY = 0x9294;

        /// <summary>
        /// [GL] Value of GL_SCREEN symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int SCREEN = 0x9295;

        /// <summary>
        /// [GL] Value of GL_OVERLAY symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int OVERLAY = 0x9296;

        /// <summary>
        /// [GL] Value of GL_DARKEN symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int DARKEN = 0x9297;

        /// <summary>
        /// [GL] Value of GL_LIGHTEN symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int LIGHTEN = 0x9298;

        /// <summary>
        /// [GL] Value of GL_COLORDODGE symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int COLORDODGE = 0x9299;

        /// <summary>
        /// [GL] Value of GL_COLORBURN symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int COLORBURN = 0x929A;

        /// <summary>
        /// [GL] Value of GL_HARDLIGHT symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int HARDLIGHT = 0x929B;

        /// <summary>
        /// [GL] Value of GL_SOFTLIGHT symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int SOFTLIGHT = 0x929C;

        /// <summary>
        /// [GL] Value of GL_DIFFERENCE symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int DIFFERENCE = 0x929E;

        /// <summary>
        /// [GL] Value of GL_EXCLUSION symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int EXCLUSION = 0x92A0;

        /// <summary>
        /// [GL] Value of GL_HSL_HUE symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int HSL_HUE = 0x92AD;

        /// <summary>
        /// [GL] Value of GL_HSL_SATURATION symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int HSL_SATURATION = 0x92AE;

        /// <summary>
        /// [GL] Value of GL_HSL_COLOR symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int HSL_COLOR = 0x92AF;

        /// <summary>
        /// [GL] Value of GL_HSL_LUMINOSITY symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public const int HSL_LUMINOSITY = 0x92B0;

        /// <summary>
        /// [GLES3.2] Gl.Get: data returns eight values minX, minY, minZ, minW, and maxX, maxY, maxZ, maxW corresponding to the
        /// clip
        /// space coordinates of the primitive bounding box. See Gl.PrimitiveBoundingBox.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_ES3_2_compatibility", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_primitive_bounding_box", Api = "gles2")]
        [RequiredByFeature("GL_OES_primitive_bounding_box", Api = "gles2")]
        public const int PRIMITIVE_BOUNDING_BOX = 0x92BE;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_4x4 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_4x4 = 0x93B0;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_5x4 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_5x4 = 0x93B1;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_5x5 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_5x5 = 0x93B2;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_6x5 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_6x5 = 0x93B3;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_6x6 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_6x6 = 0x93B4;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_8x5 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_8x5 = 0x93B5;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_8x6 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_8x6 = 0x93B6;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_8x8 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_8x8 = 0x93B7;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_10x5 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_10x5 = 0x93B8;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_10x6 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_10x6 = 0x93B9;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_10x8 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_10x8 = 0x93BA;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_10x10 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_10x10 = 0x93BB;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_12x10 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_12x10 = 0x93BC;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_RGBA_ASTC_12x12 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_RGBA_ASTC_12x12 = 0x93BD;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_4x4 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_4x4 = 0x93D0;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x4 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_5x4 = 0x93D1;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5x5 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_5x5 = 0x93D2;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x5 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_6x5 = 0x93D3;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6x6 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_6x6 = 0x93D4;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x5 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_8x5 = 0x93D5;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x6 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_8x6 = 0x93D6;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8x8 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_8x8 = 0x93D7;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x5 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_10x5 = 0x93D8;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x6 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_10x6 = 0x93D9;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x8 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_10x8 = 0x93DA;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10x10 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_10x10 = 0x93DB;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x10 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_12x10 = 0x93DC;

        /// <summary>
        /// [GL] Value of GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12x12 symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_hdr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_texture_compression_astc_ldr", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_compression_astc", Api = "gles2")]
        public const int COMPRESSED_SRGB8_ALPHA8_ASTC_12x12 = 0x93DD;

        /// <summary>
        /// [GLES3.2] glBlendBarrier: specifies a boundary between advanced blending passes
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        public static void BlendBarrier()
        {
            Debug.Assert(Delegates.pglBlendBarrier != null, "pglBlendBarrier not implemented");
            Delegates.pglBlendBarrier();
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GLES3.2] glPrimitiveBoundingBox: set the bounding box for a primitive
        /// </summary>
        /// <param name="minX">
        /// Specify the minimum clip space cooridnate of the bounding box. The initial value is (-1, -1, -1, -1).
        /// </param>
        /// <param name="minY">
        /// Specify the minimum clip space cooridnate of the bounding box. The initial value is (-1, -1, -1, -1).
        /// </param>
        /// <param name="minZ">
        /// Specify the minimum clip space cooridnate of the bounding box. The initial value is (-1, -1, -1, -1).
        /// </param>
        /// <param name="minW">
        /// Specify the minimum clip space cooridnate of the bounding box. The initial value is (-1, -1, -1, -1).
        /// </param>
        /// <param name="maxX">
        /// Specify the maximum clip space cooridnate of the bounding box. The initial value is (1, 1, 1, 1).
        /// </param>
        /// <param name="maxY">
        /// Specify the maximum clip space cooridnate of the bounding box. The initial value is (1, 1, 1, 1).
        /// </param>
        /// <param name="maxZ">
        /// Specify the maximum clip space cooridnate of the bounding box. The initial value is (1, 1, 1, 1).
        /// </param>
        /// <param name="maxW">
        /// Specify the maximum clip space cooridnate of the bounding box. The initial value is (1, 1, 1, 1).
        /// </param>
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_ES3_2_compatibility", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_primitive_bounding_box", Api = "gles2")]
        [RequiredByFeature("GL_OES_primitive_bounding_box", Api = "gles2")]
        public static void Primitive(float minX, float minY, float minZ, float minW, float maxX, float maxY, float maxZ, float maxW)
        {
            Debug.Assert(Delegates.pglPrimitiveBoundingBox != null, "pglPrimitiveBoundingBox not implemented");
            Delegates.pglPrimitiveBoundingBox(minX, minY, minZ, minW, maxX, maxY, maxZ, maxW);
            DebugCheckErrors(null);
        }

        public static partial class Delegates
        {
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2")]
            [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glBlendBarrier();

            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_KHR_blend_equation_advanced", Api = "gl|glcore|gles2", EntryPoint = "glBlendBarrierKHR")]
            [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2", EntryPoint = "glBlendBarrierNV")]
            [ThreadStatic]
            public static glBlendBarrier pglBlendBarrier;

            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_ES3_2_compatibility", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_primitive_bounding_box", Api = "gles2")]
            [RequiredByFeature("GL_OES_primitive_bounding_box", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glPrimitiveBoundingBox(float minX, float minY, float minZ, float minW, float maxX, float maxY, float maxZ, float maxW);

            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_ES3_2_compatibility", Api = "gl|glcore", EntryPoint = "glPrimitiveBoundingBoxARB")]
            [RequiredByFeature("GL_EXT_primitive_bounding_box", Api = "gles2", EntryPoint = "glPrimitiveBoundingBoxEXT")]
            [RequiredByFeature("GL_OES_primitive_bounding_box", Api = "gles2", EntryPoint = "glPrimitiveBoundingBoxOES")]
            [ThreadStatic]
            public static glPrimitiveBoundingBox pglPrimitiveBoundingBox;
        }
    }
}