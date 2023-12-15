#region Using

using System;

#endregion

namespace ImGuiNET
{
    [Flags]
    public enum ImFontAtlasFlags
    {
        None = 0,
        NoPowerOfTwoHeight = 1,
        NoMouseCursors = 2,
        NoBakedLines = 4,
    }
}