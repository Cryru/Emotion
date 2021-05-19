#region Using

using Emotion.Audio;

#endregion

namespace Emotion.Platform
{
    public interface IAudioAdapter
    {
        AudioLayer CreatePlatformAudioLayer(string layerName);
    }
}