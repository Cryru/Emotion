#region Using

using System;

#endregion

namespace ImGuiNET
{
    [Flags]
    public enum ImGuiKeyModFlags
    {
        None = 0,
        Ctrl = 1,
        Shift = 2,
        Alt = 4,
        Super = 8,
    }
}