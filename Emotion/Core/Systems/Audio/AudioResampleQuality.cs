#nullable enable

using Emotion;

namespace Emotion.Core.Systems.Audio;

public enum AudioResampleQuality
{
    Auto,
    LowCubic,
    MediumHermite,
    HighHann,
    ONE_ExperimentalOptimized
}