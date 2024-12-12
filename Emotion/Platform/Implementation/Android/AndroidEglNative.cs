#region Using

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace Emotion.Platform.Implementation.Android;

public static class AndroidEglNative
{
    public const string LIBRARY_NAME = "libEGL.so";

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eglGetProcAddress")]
    [SuppressUnmanagedCodeSecurity]
    public static extern nint GetProcAddress(string proc);
}