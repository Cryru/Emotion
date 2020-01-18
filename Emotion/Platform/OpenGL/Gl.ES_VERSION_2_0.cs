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

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer

namespace OpenGL
{
    public partial class Gl
    {
        /// <summary>
        /// [GL] Value of GL_FRAMEBUFFER_INCOMPLETE_DIMENSIONS symbol.
        /// </summary>
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        public const int FRAMEBUFFER_INCOMPLETE_DIMENSIONS = 0x8CD9;
    }
}