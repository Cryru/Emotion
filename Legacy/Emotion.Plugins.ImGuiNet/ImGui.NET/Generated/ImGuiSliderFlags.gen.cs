#region Using

using System;

#endregion

namespace ImGuiNET
{
    [Flags]
    public enum ImGuiSliderFlags
    {
        None = 0,
        AlwaysClamp = 16,
        Logarithmic = 32,
        NoRoundToFormat = 64,
        NoInput = 128,
        InvalidMask = 1879048207,
    }
}