#nullable enable

using Emotion.Core.Systems.Audio;

namespace Emotion.Core.Platform.Implementation.Null;

public class NullAudioLayer : AudioLayer
{
    public NullAudioLayer(string name) : base(name)
    {
    }

    protected override void UpdateBackend()
    {
    }
}