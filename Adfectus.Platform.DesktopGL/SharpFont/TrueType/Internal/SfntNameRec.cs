#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.TrueType.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SfntNameRec
    {
        internal PlatformId platform_id;
        internal ushort encoding_id;
        internal ushort language_id;
        internal ushort name_id;

        internal IntPtr @string;
        internal uint string_len;
    }
}