#nullable enable

using Emotion;
using System.Runtime.InteropServices;

namespace Emotion.Core.Platform.Implementation.Win32.Native;

public static class Shell32
{
    public const string LIBRARY_NAME = "Shell32";

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-dragacceptfiles
    /// </summary>
    [DllImport(LIBRARY_NAME, CharSet = CharSet.Auto)]
    public static extern void DragAcceptFiles(
        nint hWnd,
        bool fAccept
    );
}