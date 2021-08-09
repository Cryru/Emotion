#region Using

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Utility;
using Khronos;

#endregion

// ReSharper disable InheritdocConsiderUsage
// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable RedundantIfElseBlock
// ReSharper disable once CheckNamespace
namespace OpenGL
{
    /// <summary>
    /// Modern OpenGL bindings.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class Gl : KhronosApi
    {
        #region Versions, Extensions and Limits

        /// <summary>
        /// OpenGL version currently implemented.
        /// </summary>
        /// <remarks>
        /// Higher OpenGL versions versions cannot be requested to be implemented.
        /// </remarks>
        public static KhronosVersion CurrentVersion { get; private set; }

        /// <summary>
        /// OpenGL Shading Language version currently implemented.
        /// </summary>
        /// <remarks>
        /// Higher OpenGL Shading Language versions cannot be requested to be implemented.
        /// </remarks>
        public static GlslVersion CurrentShadingVersion { get; private set; }

        /// <summary>
        /// Get the OpenGL vendor.
        /// </summary>
        public static string CurrentVendor { get; private set; }

        /// <summary>
        /// Get the OpenGL renderer.
        /// </summary>
        public static string CurrentRenderer { get; private set; }

        /// <summary>
        /// Whether the current OpenGL renderer is a software one, such as Mesa3D's llvmpipe.
        /// </summary>
        public static bool SoftwareRenderer { get; private set; }

        /// <summary>
        /// OpenGL extension support.
        /// </summary>
        public static Extensions CurrentExtensions { get; private set; }

        /// <summary>
        /// OpenGL limits.
        /// </summary>
        public static Limits CurrentLimits { get; private set; }

        #endregion

        #region API Binding

        /// <summary>
        /// Bind OpenGL delegates.
        /// </summary>
        /// <param name="context">The Emotion graphics context.</param>
        public static void BindAPI(GraphicsContext context)
        {
            // Bind minimal API to query version, and do it.
            BindAPIFunction("glGetError", Version100, null, context);
            BindAPIFunction("glGetString", Version100, null, context);
            BindAPIFunction("glGetIntegerv", Version100, null, context);
            BindAPIFunction("glGetFloatv", Version100, null, context);

            CurrentVersion = QueryContextVersionInternal();

            // Obtain current OpenGL Shading Language version
            string glslVersion = null;

            switch (CurrentVersion.Api)
            {
                case KhronosVersion.API_GL:
                    if (CurrentVersion >= Version200 || CurrentExtensions.ShadingLanguage100_ARB)
                        glslVersion = GetString(StringName.ShadingLanguageVersion);
                    break;
                case KhronosVersion.API_GLES2:
                    glslVersion = GetString(StringName.ShadingLanguageVersion);
                    break;
            }

            if (glslVersion != null)
                CurrentShadingVersion = GlslVersion.Parse(glslVersion, CurrentVersion.Api);

            // Query OpenGL extensions (current OpenGL implementation, CurrentCaps)
            if (CurrentVersion.Major >= 3) BindAPIFunction("glGetStringi", Version300, null, context); // Try to get extensions indexed.
            CurrentExtensions = new Extensions();
            CurrentExtensions.Query();

            // Query OpenGL limits
            CurrentLimits = Limits.Query(CurrentVersion, CurrentExtensions);

            // Bind all functions.
            foreach (FieldInfo fi in _functionContext.Delegates)
            {
                BindAPIFunction(fi, CurrentVersion, CurrentExtensions, context);
            }
        }

        /// <summary>
        /// Query the OpenGL version. Requires some information lookup functions to be bound.
        /// </summary>
        /// <returns>The version of the current OpenGL context.</returns>
        private static KhronosVersion QueryContextVersionInternal()
        {
            // Parse version string (effective for detecting Desktop and ES contextes)
            string str = GetString(StringName.Version);
            KhronosVersion glVersion = KhronosVersion.Parse(str);

            // Vendor/Render information
            CurrentVendor = GetString(StringName.Vendor);
            CurrentRenderer = GetString(StringName.Renderer);
            SoftwareRenderer = CurrentRenderer.Contains("llvmpipe");

            // Context profile
            if (glVersion.Api == KhronosVersion.API_GL && glVersion >= Version320)
            {
                string glProfile;

                Get(CONTEXT_PROFILE_MASK, out int ctxProfile);

                if ((ctxProfile & CONTEXT_COMPATIBILITY_PROFILE_BIT) != 0)
                    glProfile = KhronosVersion.PROFILE_COMPATIBILITY;
                else if ((ctxProfile & CONTEXT_CORE_PROFILE_BIT) != 0)
                    glProfile = KhronosVersion.PROFILE_CORE;
                else
                    glProfile = KhronosVersion.PROFILE_COMPATIBILITY;

                return new KhronosVersion(glVersion, glProfile);
            }
            else
            {
                string profile = KhronosVersion.PROFILE_COMPATIBILITY;
                if (CurrentRenderer.Contains("WebGL")) profile = KhronosVersion.PROFILE_WEBGL;
                return new KhronosVersion(glVersion, profile);
            }
        }

        /// <summary>
        /// Query the OpenGL version without requiring any functions to be bound.
        /// </summary>
        /// <param name="procLoadFunction">
        /// The function OpenGL functions will be loaded through, in this case only glGetString
        /// only.
        /// </param>
        /// <returns>The version of the current OpenGL context.</returns>
        public static KhronosVersion QueryVersionExternal(Func<string, IntPtr> procLoadFunction)
        {
            IntPtr func = procLoadFunction("glGetString");
            if (func == IntPtr.Zero) return null;

            var getString = Marshal.GetDelegateForFunctionPointer<Delegates.glGetString>(func);
            IntPtr ptr = getString((int) StringName.Version);
            string str = NativeHelpers.StringFromPtr(ptr);
            return KhronosVersion.Parse(str);
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Whether GL errors are being suppressed.
        /// </summary>
        public static bool SuppressingErrors = false;

        /// <summary>
        /// OpenGL error checking.
        /// </summary>
        public static void CheckErrors()
        {
            ErrorCode error = GetError();

            if (error != ErrorCode.NoError)
                throw new GlException(error);
        }

        /// <summary>
        /// Flush GL errors queue.
        /// </summary>
        private static void ClearErrors()
        {
            while (GetError() != ErrorCode.NoError)
                // ReSharper disable once EmptyEmbeddedStatement
                ;
        }

        /// <summary>
        /// OpenGL error checking.
        /// </summary>
        /// <param name="returnValue">
        /// A <see cref="Object" /> that specifies the function returned value, if any.
        /// </param>
        [Conditional("DEBUG")]
        // ReSharper disable once UnusedParameter.Local
        private static void DebugCheckErrors(object returnValue)
        {
            if (!Engine.Configuration.GlDebugMode) return;

            if (SuppressingErrors) ClearErrors();

            CheckErrors();
        }

        #endregion

        #region Hand-crafted Bindings

        /// <summary>
        /// Specify a callback to receive debugging messages from the GL.
        /// </summary>
        /// <param name="source">
        /// A <see cref="DebugSource" /> that specify the source of the message.
        /// </param>
        /// <param name="type">
        /// A <see cref="DebugType" /> that specify the type of the message.
        /// </param>
        /// <param name="id">
        /// A <see cref="UInt32" /> that specify the identifier of the message.
        /// </param>
        /// <param name="severity">
        /// A <see cref="DebugSeverity" /> that specify the severity of the message.
        /// </param>
        /// <param name="length">
        /// The length of the message.
        /// </param>
        /// <param name="message">
        /// A <see cref="IntPtr" /> that specify a pointer to a null-terminated ASCII C string, representing the content of the
        /// message.
        /// </param>
        /// <param name="userParam">
        /// A <see cref="IntPtr" /> that specify the user-specified parameter.
        /// </param>
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DebugProc(DebugSource source, DebugType type, uint id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam);

        /// <summary>
        /// specify values to record in transform feedback buffers
        /// </summary>
        /// <param name="program">
        /// The name of the target program object.
        /// </param>
        /// <param name="varyings">
        /// An array of zero-terminated strings specifying the names of the varying variables to use for
        /// transform feedback.
        /// </param>
        /// <param name="bufferMode">
        /// Identifies the mode used to capture the varying variables when transform feedback is active.
        /// <paramref
        ///     name="bufferMode" />
        /// must be Gl.INTERLEAVED_ATTRIBS or Gl.SEPARATE_ATTRIBS.
        /// </param>
        /// <remarks>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Gl.INVALID_VALUE is generated if <paramref name="program" /> is not the name of a program object.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Gl.INVALID_VALUE is generated if <paramref name="bufferMode" /> is Gl.SEPARATE_ATTRIBS and the length of
        /// <paramref name="varyings" /> is
        /// greater than the implementation-dependent limit Gl.MAX_TRANSFORM_FEEDBACK_SEPARATE_ATTRIBS.
        /// </exception>
        /// <seealso cref="Gl.BeginTransformFeedback" />
        /// <seealso cref="Gl.EndTransformFeedback" />
        /// <seealso cref="Gl.GetTransformFeedbackVarying" />
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_transform_feedback")]
        public static void TransformFeedbackVaryings(uint program, IntPtr[] varyings, int bufferMode)
        {
            unsafe
            {
                fixed (IntPtr* p_varyings = varyings)
                {
                    Debug.Assert(Delegates.pglTransformFeedbackVaryings_Unmanaged != null, "pglTransformFeedbackVaryings not implemented");
                    Delegates.pglTransformFeedbackVaryings_Unmanaged(program, varyings.Length, p_varyings, bufferMode);
                }
            }

            DebugCheckErrors(null);
        }

        public static unsafe partial class Delegates
        {
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTransformFeedbackVaryings_Unmanaged(uint program, int count, IntPtr* varyings, int bufferMode);

            [RequiredByFeature("GL_VERSION_3_0", EntryPoint = "glTransformFeedbackVaryings")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2", EntryPoint = "glTransformFeedbackVaryings")]
            [RequiredByFeature("GL_EXT_transform_feedback", EntryPoint = "glTransformFeedbackVaryingsEXT")]
            [ThreadStatic]
            public static glTransformFeedbackVaryings_Unmanaged pglTransformFeedbackVaryings_Unmanaged;
        }

        #endregion

        /// <summary>
        /// Convert a draw elements type to the number of bytes that type contains.
        /// </summary>
        public static byte DrawElementTypeToByteCount(DrawElementsType elementType)
        {
            switch (elementType)
            {
                case DrawElementsType.UnsignedByte:
                    return 1;
                case DrawElementsType.UnsignedShort:
                    return 2;
                case DrawElementsType.UnsignedInt:
                    return 4;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Convert a pixel type enum to the number of bytes that type contains.
        /// </summary>
        /// <param name="pixelType">The pixel type to get the byte count of.</param>
        /// <returns>The number of bytes that pixel type contains.</returns>
        public static byte PixelTypeToByteCount(PixelType pixelType)
        {
            switch (pixelType)
            {
                case PixelType.UnsignedByte:
                case PixelType.Byte:
                    return 1;
                case PixelType.Double:
                    return 8;
                case PixelType.UnsignedInt248:
                case PixelType.Int:
                case PixelType.UnsignedInt:
                case PixelType.Float:
                    return 4;
                case PixelType.UnsignedShort:
                case PixelType.Short:
                case PixelType.HalfFloat:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the amount of components in the pixel format.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to get the components of.</param>
        /// <returns>How many components the pixel format has.</returns>
        public static byte PixelFormatToComponentCount(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Bgra:
                case PixelFormat.Rgba:
                case PixelFormat.BgraInteger:
                case PixelFormat.RgbaInteger:
                    return 4;
                case PixelFormat.Red:
                case PixelFormat.Green:
                case PixelFormat.Blue:
                case PixelFormat.Alpha:
                case PixelFormat.RedInteger:
                case PixelFormat.GreenInteger:
                case PixelFormat.BlueInteger:
                    return 1;
                case PixelFormat.DepthStencil:
                case PixelFormat.DepthComponent:
                    return 1;
                case PixelFormat.Rgb:
                case PixelFormat.RgbInteger:
                case PixelFormat.Bgr:
                    return 3;
                default:
                    return 0;
            }
        }
    }
}