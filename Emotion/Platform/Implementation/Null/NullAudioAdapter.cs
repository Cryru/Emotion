#region Using

using Emotion.Audio;

#endregion

namespace Emotion.Platform.Implementation.Null
{
    public sealed class NullAudioAdapter : IAudioAdapter
    {
        public AudioLayer CreatePlatformAudioLayer(string layerName)
        {
            return new NullAudioLayer(layerName);
        }
    }
}