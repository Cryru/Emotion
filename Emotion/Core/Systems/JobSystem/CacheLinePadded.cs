#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Core.Systems.JobSystem;

[StructLayout(LayoutKind.Explicit, Size = 128)]
internal struct CacheLinePaddedInt
{
    [FieldOffset(64)]
    public int Value;
}

[StructLayout(LayoutKind.Explicit, Size = 128)]
internal struct CacheLinePaddedLong
{
    [FieldOffset(64)]
    public long Value;
}
