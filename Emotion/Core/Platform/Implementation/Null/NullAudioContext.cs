#nullable enable

using Emotion.Core.Systems.Audio;

namespace Emotion.Core.Platform.Implementation.Null;

public sealed class NullAudioContext : AudioContext
{
    public NullAudioContext(PlatformBase platform) : base(platform)
    {
    }

    public override AudioLayer CreatePlatformAudioLayer(string layerName)
    {
        return new NullAudioLayer(layerName);
    }
}