#region Using

using Emotion.Audio;

#endregion

namespace Emotion.Platform.Implementation.Null
{
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
}