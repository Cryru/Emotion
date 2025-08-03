#region Using

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Emotion.Core.Platform.Implementation.EglAngle.Native
{
    public static class Egl
    {
        public const string LIBRARY_NAME = "libEGL";

        public enum Error
        {
            Success = 0x3000
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglGetError")]
        [SuppressUnmanagedCodeSecurity]
        public static extern Error GetError();

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum Query
        {
            EGL_EXTENSIONS = 0x3055
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglQueryString")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string QueryString(nint display, Query q);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglGetDisplay")]
        [SuppressUnmanagedCodeSecurity]
        public static extern nint GetDisplay(nint nativeHandle);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglInitialize")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool Init(nint display, ref int majorVer, ref int minorVer);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglGetConfigs")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetConfigs(nint display, nint[] configs, int count, ref int total);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ConfigAttribute
        {
            EGL_COLOR_BUFFER_TYPE = 0x303f,
            EGL_SURFACE_TYPE = 0x3033,
            EGL_RENDERABLE_TYPE = 0x3040,
            EGL_ALPHA_SIZE = 0x3021,
            EGL_BLUE_SIZE = 0x3022,
            EGL_GREEN_SIZE = 0x3023,
            EGL_RED_SIZE = 0x3024,
            EGL_DEPTH_SIZE = 0x3025,
            EGL_STENCIL_SIZE = 0x3026,
            EGL_SAMPLES = 0x3031
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ConfigValue
        {
            EGL_RGB_BUFFER = 0x308e
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglGetConfigAttrib")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetConfigAttribute(nint display, nint config, ConfigAttribute attribute, ref int value);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum API
        {
            EGL_OPENGL_ES_API = 0x30a0
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglBindAPI")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool BindAPI(API api);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ContextAttribute
        {
            EGL_CONTEXT_CLIENT_VERSION = 0x3098
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglCreateContext")]
        [SuppressUnmanagedCodeSecurity]
        public static extern nint CreateContext(nint display, nint config, nint sharedContext, int[] attributes);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglMakeCurrent")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool MakeCurrent(nint display, nint surface, nint surface2, nint context);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglSwapInterval")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool SwapInterval(nint display, int interval);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglSwapBuffers")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool SwapBuffers(nint display, nint surface);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglGetProcAddress")]
        [SuppressUnmanagedCodeSecurity]
        public static extern nint GetProcAddress(string proc);

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglCreateWindowSurface")]
        [SuppressUnmanagedCodeSecurity]
        public static extern nint CreateWindowSurface(nint display, nint config, nint nativeWindow, int[] attributes);
    }
}