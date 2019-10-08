#region Using

using System;

#endregion

namespace Emotion.Platform.Implementation.Win32.Wgl
{
    [Flags]
    public enum WglContextFlags
    {
        DebugBitArb = 0x00000001,
        ForwardCompatibleBitArb = 0x00000002,
        CoreProfileBitArb = 0x00000001,
        CompatibilityProfileBitArb = 0x00000002
    }
}