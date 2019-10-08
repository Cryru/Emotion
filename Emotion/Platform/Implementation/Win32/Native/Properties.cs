#region Using

using System.Runtime.InteropServices;

#endregion

namespace WinApi
{
    internal static class Properties
    {
#if !ANSI
        public const CharSet BUILD_CHAR_SET = CharSet.Unicode;
#else
        public const CharSet BuildCharSet = CharSet.Ansi;
#endif
    }
}