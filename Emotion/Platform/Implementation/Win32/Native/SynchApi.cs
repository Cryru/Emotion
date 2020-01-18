#region Using

using System;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi
{
    public static class SynchApi
    {
        public const string LIBRARY_NAME = "kernel32";

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateEventW(IntPtr attributes, bool manualReset, bool initialState, string name);
    }
}