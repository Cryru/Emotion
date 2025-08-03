#nullable enable

#region Using

using Emotion;
using Emotion.Core.Platform.Implementation.Win32.Native.Core;
using System.Runtime.InteropServices;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Core.Platform.Implementation.Win32.Native.DwmApi;

public static class DwmApi
{
    public const string LibraryName = "dwmapi";

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern HResult DwmSetWindowAttribute(nint hwnd, DwmWindowAttributeType dwAttribute,
        nint pvAttribute,
        uint attrSize);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern HResult DwmGetWindowAttribute(nint hwnd, DwmWindowAttributeType dwAttribute,
        nint pvAttribute,
        uint cbAttribute);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern HResult DwmIsCompositionEnabled(out bool pfEnabled);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool DwmDefWindowProc(nint hwnd, uint msg, nint wParam, nint lParam,
        out nint lResult);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern HResult DwmGetColorizationColor(out int pcrColorization, out bool pfOpaqueBlend);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern HResult DwmFlush();
}