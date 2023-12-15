#region Using

using System;

#endregion

namespace ImGuiNET
{
    [Flags]
    public enum ImGuiSelectableFlags
    {
        None = 0,
        DontClosePopups = 1,
        SpanAllColumns = 2,
        AllowDoubleClick = 4,
        Disabled = 8,
        AllowItemOverlap = 16,
    }
}