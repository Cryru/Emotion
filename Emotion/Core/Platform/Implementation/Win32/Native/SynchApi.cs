#nullable enable

using Emotion;
using System.Runtime.InteropServices;

namespace Emotion.Core.Platform.Implementation.Win32.Native;

public static class SynchApi
{
    public const string LIBRARY_NAME = "kernel32";

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint CreateEventW(nint attributes, bool manualReset, bool initialState, string name);
}