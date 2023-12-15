#region Using

using System;

#endregion

namespace ImGuiNET
{
    [Flags]
    public enum ImDrawListFlags
    {
        None = 0,
        AntiAliasedLines = 1,
        AntiAliasedLinesUseTex = 2,
        AntiAliasedFill = 4,
        AllowVtxOffset = 8,
    }
}