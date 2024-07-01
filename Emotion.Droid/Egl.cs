#region Using

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Emotion.Droid
{
    public static class Egl
    {
        public const string LIBRARY_NAME = "libEGL";

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglGetProcAddress")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetProcAddress(string proc);
    }
}